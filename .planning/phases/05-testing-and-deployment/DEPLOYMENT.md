# HouseDemo Deployment Runbook

**Last Updated:** 2026-11-05  
**Audience:** DevOps, SRE, Release Engineers  
**Duration:** ~15 minutes (first time), ~5 minutes (subsequent deploys)

---

## Table of Contents

1. [Pre-Deployment Checklist](#pre-deployment-checklist)
2. [Heroku Backend Deployment](#heroku-backend-deployment)
3. [GitHub Pages Frontend Deployment](#github-pages-frontend-deployment)
4. [Verification & Testing](#verification--testing)
5. [Rollback Procedures](#rollback-procedures)
6. [Troubleshooting](#troubleshooting)

---

## Pre-Deployment Checklist

### Prerequisites

- [ ] Heroku CLI installed (`brew install heroku` on macOS)
- [ ] Logged into Heroku: `heroku login`
- [ ] Git configured with default branch `master` or `main`
- [ ] All E2E tests passing locally: `npm run test:e2e`
- [ ] Production build succeeds: `npm run build` (frontend) + backend build
- [ ] Domain registered: `housedemoll.com` (or update CNAME file)
- [ ] GitHub Pages repository settings accessible

### Environment Variables

**Heroku Config Vars Required:**
```bash
JWT_SECRET=<random-32-char-string>           # Generate: openssl rand -base64 32
CORS_ALLOWED_ORIGINS=https://housedemoll.com
NODE_ENV=production
DATABASE_URL=<auto-set-by-postgres-addon>
```

**Frontend .env.production:**
```bash
VITE_API_BASE=https://housedemo-backend-<app-name>.herokuapp.com/api
```

---

## Heroku Backend Deployment

### Step 1: Create Heroku App (First Time Only)

```bash
# Create app (choose unique name)
heroku create housedemo-backend-prod

# Add PostgreSQL
heroku addons:create heroku-postgresql:hobby-dev --app housedemo-backend-prod

# Verify addon attached
heroku config --app housedemo-backend-prod | grep DATABASE_URL
```

### Step 2: Set Environment Variables

```bash
# Generate JWT secret
JWT_SECRET=$(openssl rand -base64 32)

# Set all config vars
heroku config:set \
  JWT_SECRET="$JWT_SECRET" \
  CORS_ALLOWED_ORIGINS="https://housedemoll.com" \
  NODE_ENV="production" \
  --app housedemo-backend-prod

# Verify
heroku config --app housedemo-backend-prod
```

### Step 3: Deploy Backend

```bash
# Add Heroku remote
heroku git:remote --app housedemo-backend-prod

# Deploy (pushes current branch to Heroku)
git push heroku master

# (or if using 'main' branch)
git push heroku main:master
```

### Step 4: Monitor Deployment

```bash
# Watch logs in real-time
heroku logs --tail --app housedemo-backend-prod

# Check app status
heroku open --app housedemo-backend-prod
```

### Step 5: Run Release Phase (Migrations)

```bash
# Release phase runs automatically on first deploy
# (defined in Procfile: "release: dotnet HouseDemo.Backend.dll migrate-db")

# Verify database migrations ran
heroku logs --app housedemo-backend-prod | grep -i "migrat"
```

---

## GitHub Pages Frontend Deployment

### Step 1: Verify GitHub Pages Settings

In repository Settings > Pages:
- **Source:** Deploy from a branch
- **Branch:** `gh-pages`
- **Folder:** `/ (root)`
- **Custom domain:** `housedemoll.com` (if DNS configured; otherwise leave blank for auto-domain)

### Step 2: Update Production Configuration

```bash
# Edit .env.production with actual Heroku URL
echo "VITE_API_BASE=https://housedemo-backend-prod.herokuapp.com/api" > .env.production

# Commit
git add .env.production
git commit -m "chore: update production API URL"
git push origin master
```

### Step 3: GitHub Actions Workflow Triggers Automatically

The workflow `.github/workflows/deploy.yml` runs on push to `master`:

```yaml
on:
  push:
    branches:
      - master  # ← Pushes to this branch trigger deployment
```

**Workflow steps:**
1. Checkout code
2. Install Node 22
3. Run `npm ci` (clean install)
4. Run `npm run build` (produces `dist/` folder)
5. Upload `dist/` as artifact
6. Deploy artifact to `gh-pages` branch
7. GitHub Pages serves from `gh-pages` branch

### Step 4: Monitor Deployment

1. Go to repository **Actions** tab
2. Click the workflow run corresponding to your push
3. Watch build and deploy steps complete (usually < 2 minutes)
4. Check **Environments** > **github-pages** for deployment history

### Step 5: Verify DNS Configuration (Custom Domain)

```bash
# If using custom domain, verify CNAME records
dig housedemoll.com

# Should point to GitHub Pages:
# housedemoll.com. CNAME <your-org>.github.io.
```

---

## Verification & Testing

### Backend Verification

```bash
# Get Heroku app URL
APP_URL=$(heroku info --json --app housedemo-backend-prod | jq -r '.web_url')

# Test API endpoints
curl $APP_URL/api/properties | jq '.[0]'                    # Get all properties
curl $APP_URL/api/properties/1 | jq '.'                     # Get property 1
curl -X POST $APP_URL/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"landlord@housedemoll.com","password":"demo123456"}' | jq '.token'

# Test admin endpoint (should return 401 without token)
curl $APP_URL/api/admin/properties
```

### Frontend Verification

```bash
# Visit deployed site
open https://housedemoll.com  # or auto-assigned GitHub Pages URL

# Verify in browser:
# ✓ Homepage loads with property list
# ✓ Property detail pages load correctly
# ✓ Admin login page accessible at /#/login
# ✓ API requests go to correct Heroku backend
# ✓ Console has no CORS errors
```

### E2E Production Tests

```bash
# Run integration tests against production
API_BASE=https://housedemo-backend-prod.herokuapp.com/api \
  npx playwright test e2e/tests/integration.spec.ts

# Expected: 2/2 tests pass
```

---

## Rollback Procedures

### Rollback Frontend (GitHub Pages)

```bash
# GitHub Pages runs off the gh-pages branch
# To rollback, revert the gh-pages branch to prior commit

# Find prior commit on gh-pages
git log origin/gh-pages --oneline -5

# Force gh-pages to a prior commit
git push origin <prior-commit>:gh-pages --force

# Site will redeploy from that commit
```

### Rollback Backend (Heroku)

```bash
# View release history
heroku releases --app housedemo-backend-prod

# Rollback to previous release
heroku rollback v<N> --app housedemo-backend-prod

# (e.g., heroku rollback v42 --app housedemo-backend-prod)

# Monitor logs
heroku logs --tail --app housedemo-backend-prod
```

### Database Rollback (PostgreSQL)

```bash
# Heroku PostgreSQL backups are automatic
# View backups
heroku pg:backups --app housedemo-backend-prod

# Restore from backup
heroku pg:backups:restore <backup-id> DATABASE --app housedemo-backend-prod

# (Requires confirmation)
```

---

## Troubleshooting

### Issue: Deployment hangs or times out

**Heroku Backend:**
```bash
# Cancel current deployment
heroku run:detached "exit" --app housedemo-backend-prod

# Clear buildpacks and redeploy
heroku buildpacks:clear --app housedemo-backend-prod
git push heroku master
```

**GitHub Pages:**
- Check **Actions** tab for workflow errors
- GitHub rate-limits may delay deploy; wait 2-3 minutes
- Clear browser cache (Cmd+Shift+R on macOS)

---

### Issue: API returns 401 Unauthorized

**Cause:** JWT_SECRET not set correctly  
**Fix:**
```bash
# Verify JWT_SECRET is set
heroku config --app housedemo-backend-prod | grep JWT_SECRET

# If blank, set it
JWT_SECRET=$(openssl rand -base64 32)
heroku config:set JWT_SECRET="$JWT_SECRET" --app housedemo-backend-prod

# Redeploy
git push heroku master
```

---

### Issue: CORS errors in browser console

**Cause:** `CORS_ALLOWED_ORIGINS` doesn't match frontend URL  
**Fix:**
```bash
# Check current CORS setting
heroku config --app housedemo-backend-prod | grep CORS

# Update if needed (must match GitHub Pages URL)
heroku config:set CORS_ALLOWED_ORIGINS="https://housedemoll.com" --app housedemo-backend-prod

# Restart app
heroku restart --app housedemo-backend-prod
```

---

### Issue: Database connection error

**Cause:** PostgreSQL addon not attached or DATABASE_URL not set  
**Fix:**
```bash
# Verify addon exists
heroku addons --app housedemo-backend-prod | grep postgres

# If not attached, create it
heroku addons:create heroku-postgresql:hobby-dev --app housedemo-backend-prod

# Restart app
heroku restart --app housedemo-backend-prod

# Check logs
heroku logs --app housedemo-backend-prod | grep -i "database"
```

---

### Issue: Frontend cannot reach backend API

**Cause:** .env.production has wrong API URL  
**Fix:**
```bash
# Get correct Heroku backend URL
heroku info --app housedemo-backend-prod

# Update .env.production
echo "VITE_API_BASE=https://housedemo-backend-prod.herokuapp.com/api" > .env.production

# Commit and push to trigger GitHub Actions
git add .env.production
git commit -m "fix: update backend API URL"
git push origin master

# Wait for GitHub Actions to redeploy
```

---

## Post-Deployment Monitoring

### Health Checks (daily)

```bash
# Backend uptime
curl -s https://housedemo-backend-prod.herokuapp.com/api/properties | jq 'length'

# Frontend loads
curl -s https://housedemoll.com | head -100 | grep -q "HouseDemo" && echo "Frontend OK"

# E2E tests (weekly)
npx playwright test e2e/tests/integration.spec.ts
```

### Logs & Alerts

```bash
# View recent errors
heroku logs --app housedemo-backend-prod | grep -i "error" | head -20

# Stream logs in real-time
heroku logs --tail --app housedemo-backend-prod
```

---

## Support & Escalation

**For issues:**
1. Check logs: `heroku logs --tail --app housedemo-backend-prod`
2. Run health checks (see above)
3. Review troubleshooting section
4. File issue with logs and steps to reproduce

**Heroku Support:** https://help.heroku.com  
**GitHub Pages Support:** https://docs.github.com/en/pages

---

**End of Runbook**
