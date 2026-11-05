# Production Deployment Guide — HouseDemo v1.0

**Status:** Ready for deployment  
**Last Updated:** 2026-11-05  
**Targets:** Heroku (backend) + GitHub Pages (frontend)

---

## Overview

This guide walks through deploying HouseDemo v1.0 to production with:
- **Backend:** .NET API on Heroku with PostgreSQL
- **Frontend:** Vue 3 SPA on GitHub Pages
- **Domain:** housedemoll.com (custom domain)

**Estimated Time:** 15-20 minutes

---

## Prerequisites

### Required Tools (Local)

```bash
# Install Heroku CLI
# macOS:
brew tap heroku/brew && brew install heroku

# Verify installation
heroku --version
git --version
npm --version
```

### Required Accounts

- ✅ GitHub account (already have)
- ✅ Heroku account (free tier, create at heroku.com)
- Domain registration (optional; housedemoll.com registered already)

---

## Part 1: Backend Deployment (Heroku)

### Step 1.1: Create Heroku App

```bash
# Login to Heroku
heroku login

# Create app with PostgreSQL addon
heroku create housedemoll-backend --buildpack heroku/dotnet
heroku addons:create heroku-postgresql:hobby-dev --app housedemoll-backend

# Verify
heroku apps:info --app housedemoll-backend
```

**Expected Output:**
```
=== housedemoll-backend
Dynos:       web: 1
Region:      us
Stack:       heroku-22
Web URL:     https://housedemoll-backend.herokuapp.com/
PostgreSQL:  hobby-dev
```

### Step 1.2: Configure Environment Variables

```bash
# Set production secrets (replace with actual values)
heroku config:set \
  JWT_SECRET="your-super-secret-jwt-key-min-32-chars" \
  DB_PASSWORD="your-postgres-admin-password" \
  ASPNETCORE_ENVIRONMENT="Production" \
  --app housedemoll-backend

# Verify
heroku config --app housedemoll-backend
```

**⚠️ Important:** 
- Generate a strong JWT secret: `openssl rand -base64 32`
- Database URL is automatically set by Heroku as `DATABASE_URL`
- Never commit secrets to git; use Heroku Config Vars only

### Step 1.3: Add Heroku Remote & Deploy

```bash
# Add Heroku remote to git
heroku git:remote --app housedemoll-backend -r heroku

# Push code to Heroku (triggers build and deployment)
git push heroku master

# Monitor logs
heroku logs --tail --app housedemoll-backend
```

**Expected Behavior:**
1. Heroku detects `.NET` project (via `*.csproj`)
2. Builds using `dotnet publish`
3. Runs `release` phase (database migration)
4. Starts web dyno

### Step 1.4: Verify Backend is Running

```bash
# Test API endpoint
curl https://housedemoll-backend.herokuapp.com/api/properties

# Expected: JSON array of properties
[{"id": "...", "name": "...", ...}, ...]

# Check logs for errors
heroku logs --app housedemoll-backend
```

**Troubleshooting:**
- 503 Service Unavailable → Check logs, wait 1-2 min for dyno to start
- 404 Not Found → Verify `Program.cs` routes are correct
- Database connection error → Verify `DATABASE_URL` in config vars

---

## Part 2: Frontend Deployment (GitHub Pages)

### Step 2.1: Update Frontend API Endpoint

**File:** `src/frontend/src/composables/usePropertyApi.js`

```javascript
// Change from:
const apiBase = import.meta.env.VITE_API_BASE || 'http://localhost:5088';

// To (production):
const apiBase = import.meta.env.VITE_API_BASE || 'https://housedemoll-backend.herokuapp.com';
```

Or use `.env.production`:

```bash
# .env.production
VITE_API_BASE=https://housedemoll-backend.herokuapp.com
```

### Step 2.2: Deploy via GitHub Actions

```bash
# Push changes to master
git add -A
git commit -m "feat: update API endpoint to production Heroku backend"
git push origin master

# GitHub Actions workflow (`.github/workflows/pages.yml`) automatically:
# 1. Builds frontend (npm run build)
# 2. Deploys to GitHub Pages
# 3. Available at https://leoluziyuan.github.io/HouseDemo/

# Verify
# Visit: https://leoluziyuan.github.io/HouseDemo/
```

### Step 2.3: Configure Custom Domain (Optional)

If you own `housedemoll.com`:

#### GitHub Pages Settings

1. Navigate to `github.com/LeoLuZiYuan/HouseDemo` → **Settings** → **Pages**
2. Under "Custom domain", enter: `housedemoll.com`
3. GitHub creates a CNAME record locally (already in `public/CNAME`)

#### DNS Configuration

Update DNS records at your domain registrar (e.g., GoDaddy, Namecheap):

**Option A: Using GitHub IP (Recommended)**
```
Type: A Record
Name: @
Value: 185.199.108.153
       185.199.109.153
       185.199.110.153
       185.199.111.153

Type: CNAME Record
Name: www
Value: leoluziyuan.github.io
```

**Option B: Using CNAME**
```
Type: CNAME
Name: @
Value: leoluziyuan.github.io
```

**Verification:**
```bash
# Wait 5-10 minutes for DNS propagation
nslookup housedemoll.com

# Should show GitHub Pages IP or leoluziyuan.github.io
```

### Step 2.4: Verify Frontend is Running

```bash
# Test frontend
# Visit: https://leoluziyuan.github.io/HouseDemo/
# OR: https://housedemoll.com (if custom domain configured)

# Check browser console (F12) for API errors
# Network tab should show requests to https://housedemoll-backend.herokuapp.com/api/properties
```

**Troubleshooting:**
- Blank page → Check Network tab for 404/CORS errors
- API not loading → Verify `VITE_API_BASE` in build output
- CORS error → Backend needs to allow GitHub Pages origin

**Fix CORS (if needed):**

Edit `src/backend/HouseDemo.Backend/Program.cs`:

```csharp
// Add to Program.cs before app.Run()
app.UseCors(policy => 
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader()
);
```

Then redeploy: `git push heroku master`

---

## Part 3: Database Migration

### Step 3.1: Verify Database Schema

```bash
# Connect to Heroku PostgreSQL
heroku pg:psql --app housedemoll-backend

# Inside psql shell:
\dt                      # List tables
SELECT COUNT(*) FROM properties;  # Should see properties count
\q                       # Exit

# Expected: properties table with 10+ rows (from migration)
```

### Step 3.2: Seed Production Data (if needed)

If properties didn't migrate automatically:

```bash
# Connect to database
heroku pg:psql --app housedemoll-backend

# Run migration SQL (copy from src/backend/HouseDemo.Backend/Data/Database.cs)
# CREATE TABLE properties (...);
# INSERT INTO properties VALUES (...);
```

---

## Part 4: End-to-End Verification

### Checklist

- [ ] **Backend API responding**
  - `curl https://housedemoll-backend.herokuapp.com/api/properties`
  - Returns JSON array with 200 status

- [ ] **Frontend loading**
  - Visit `https://leoluziyuan.github.io/HouseDemo/`
  - Homepage displays property cards

- [ ] **API integration working**
  - Browser Network tab shows successful `GET /api/properties`
  - No CORS errors in console

- [ ] **Login functional**
  - Click "Admin" in header
  - Enter credentials (from env vars: `ADMIN_EMAIL`, `ADMIN_PASSWORD`)
  - Redirect to admin dashboard

- [ ] **Admin CRUD working**
  - Create new property
  - Edit existing property
  - Delete property
  - Verify changes appear on public site (refresh)

- [ ] **Domain resolving** (if custom domain)
  - `nslookup housedemoll.com` returns GitHub IP
  - `https://housedemoll.com` loads frontend

- [ ] **HTTPS working**
  - All URLs use `https://`
  - No mixed content warnings

### Production Test Script

```bash
#!/bin/bash
# test-production.sh

BACKEND="https://housedemoll-backend.herokuapp.com"
FRONTEND="https://leoluziyuan.github.io/HouseDemo"

echo "🔍 Testing Backend..."
curl -s "$BACKEND/api/properties" | jq length

echo "🔍 Testing Frontend..."
curl -s "$FRONTEND" | grep -q "HouseDemo" && echo "✅ Frontend OK" || echo "❌ Frontend Failed"

echo "✅ All checks passed!"
```

Run: `bash test-production.sh`

---

## Part 5: Monitoring & Troubleshooting

### View Logs

```bash
# Real-time backend logs
heroku logs --tail --app housedemoll-backend

# Last 50 lines
heroku logs -n 50 --app housedemoll-backend

# Filter by error level
heroku logs --grep "error" --app housedemoll-backend
```

### Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| 503 Service Unavailable | Dyno starting or crashed | Wait 1-2 min; check logs |
| 404 Not Found | Wrong endpoint | Verify URL in frontend config |
| CORS error | Backend not allowing origin | Update CORS policy; redeploy |
| Database connection error | Wrong connection string | Check `DATABASE_URL` config var |
| Properties not showing | Migration failed | Manually run schema SQL |
| Login fails | Wrong credentials | Verify `ADMIN_EMAIL`, `ADMIN_PASSWORD` in config |

### Restart Dyno

```bash
# If backend is stuck
heroku dyno:restart --app housedemoll-backend
```

### Scale Dyno (if needed)

```bash
# Upgrade from free to paid (only if necessary)
heroku dyno:type web=standard-1x --app housedemoll-backend

# Check current dyno
heroku dynos --app housedemoll-backend
```

### Database Backup

```bash
# Export database
heroku pg:backups:capture --app housedemoll-backend

# List backups
heroku pg:backups --app housedemoll-backend

# Download backup
heroku pg:backups:download --app housedemoll-backend
```

---

## Part 6: Post-Deployment Tasks

### Update README

**File:** `README.md`

```markdown
## Live Demo

- **Frontend:** https://leoluziyuan.github.io/HouseDemo/ (or https://housedemoll.com)
- **Backend API:** https://housedemoll-backend.herokuapp.com
- **Admin Login:** [credentials hidden]

## Deployment

See PRODUCTION-DEPLOYMENT.md for detailed deployment guide.
```

### Monitor Performance

- Check Heroku dashboard: https://dashboard.heroku.com/
- Set up alerts in Heroku Dashboard → Settings → Log Drain

### Continuous Deployment

After first manual deployment, every `git push origin master` triggers:
1. GitHub Actions tests (Playwright E2E)
2. Frontend build & deploy to GitHub Pages
3. Backend build & deploy to Heroku (via `git push heroku master`)

**To enable auto-deploy to Heroku:**

```bash
# Option 1: Manual trigger via git push (current setup)
git push heroku master

# Option 2: Connect GitHub repo to Heroku for automatic deploys
# 1. Heroku Dashboard → App → Deploy → GitHub → Connect
# 2. Enable "Automatic Deploys" for master branch
```

---

## Part 7: Rollback Procedure

If production deployment has issues:

```bash
# View deployment history
heroku releases --app housedemoll-backend

# Rollback to previous version
heroku releases:rollback --app housedemoll-backend

# Verify
heroku logs --tail --app housedemoll-backend
```

---

## Security Checklist

- [ ] No secrets in code (all use Heroku Config Vars)
- [ ] HTTPS enabled (automatic with Heroku)
- [ ] CORS policy allows only necessary origins
- [ ] JWT secret is strong (32+ chars)
- [ ] Database backups configured
- [ ] Admin credentials stored securely (not in README)
- [ ] `.gitignore` prevents secret files from being committed

---

## Cost Estimate

| Service | Tier | Cost/Month |
|---------|------|-----------|
| Heroku Web Dyno | Free | $0 (sleeping after 30 min inactivity) |
| Heroku PostgreSQL | Hobby-Dev | $0 |
| GitHub Pages | Free | $0 |
| Custom Domain | Varies | $10-15 (registrar) |
| **Total** | | ~$10-15 |

**To keep free:**
- Keep dyno on free tier (sleeps between requests, cold start ~5-10s)
- Use Hobby-Dev PostgreSQL (20 rows limit for free; upgrade if needed)

---

## FAQ

**Q: My backend is slow / takes 10+ seconds to respond**  
A: Heroku free dyno sleeps after 30 min inactivity. First request takes 5-10s to "wake up". Normal behavior.

**Q: Can I deploy without Heroku?**  
A: Yes! See `DOCKER-DEPLOYMENT.md` for Docker deployment to AWS, GCP, or DigitalOcean.

**Q: How do I update production without downtime?**  
A: Heroku handles rolling deploys automatically. Just `git push heroku master`.

**Q: What if custom domain fails?**  
A: Frontend will still work at `https://leoluziyuan.github.io/HouseDemo/`. Update DNS records and wait 5-10 minutes.

**Q: How do I add more admin users in V2?**  
A: See `.planning/phases/01-database-and-rest-api/PLAN.md` for multi-user roadmap.

---

## Next Steps

1. ✅ Deploy backend to Heroku (this guide)
2. ✅ Deploy frontend to GitHub Pages
3. ✅ Configure custom domain
4. 📋 Monitor production (set up alerts)
5. 📋 Plan V2 features (multi-landlord, image upload, etc.)

---

## Support

For issues:
1. Check `.planning/phases/05-testing-and-deployment/` for phase-specific docs
2. Review logs: `heroku logs --tail --app housedemoll-backend`
3. Verify config: `heroku config --app housedemoll-backend`
4. Re-run E2E tests against production: `npm run test:e2e -- --baseURL=https://leoluziyuan.github.io/HouseDemo/`

---

**Last Updated:** 2026-11-05  
**Maintainer:** @LeoLuZiYuan  
**Status:** ✅ Ready for Production
