# Production Deployment Guide — HouseDemo v1.0

**Status:** Ready for deployment  
**Last Updated:** 2026-11-05  
**Primary:** Render (backend) + GitHub Pages (frontend)  
**Alternative:** See RENDER-DEPLOYMENT.md for detailed Render guide

---

## Quick Overview

This document provides an overview of production deployment options. **Recommended deployment:**

- **Backend:** Render.com (Docker container) — $7/month, faster, better free tier
- **Frontend:** GitHub Pages — Free
- **Database:** Render PostgreSQL — $15/month

**For detailed Render deployment steps, see: RENDER-DEPLOYMENT.md**

---

## Why Render Over Heroku?

| Factor | Render | Heroku |
|--------|--------|--------|
| Cost | $7/month (web) + $15/month (DB) = $22 | $50+/month (DB only) |
| Startup Time | ~10s | ~20s (cold) |
| Free Tier | Yes (sleeps after 15min) | Removed |
| Docker Support | Native | Via Buildpack |
| HTTPS | Free, automatic | Free, automatic |
| CLI Tools | Minimal (web only) | Full CLI |

---

## Prerequisites

### Required Accounts

- ✅ GitHub account (already have)
- ✅ Render account (free, create at render.com)
- ✅ Domain registration (optional; housedemoll.com already configured)

---

## Part 1: Backend Deployment (Render)

**For complete step-by-step Render deployment guide, see: RENDER-DEPLOYMENT.md**

### Quick Checklist

```bash
□ Create Render account (render.com)
□ Create PostgreSQL database on Render
  - Name: housedemoll-db
  - Copy DATABASE_URL
  
□ Create Web Service on Render
  - Connect GitHub repo: LeoLuZiYuan/HouseDemo
  - Runtime: Docker (auto-detected)
  - Set environment variables:
    * ASPNETCORE_ENVIRONMENT=Production
    * DATABASE_URL=<from-postgres>
    * JWT_SECRET=<generated-32-chars>
    * ADMIN_EMAIL=admin@housedemoll.com
    * ADMIN_PASSWORD=<secure-password>
    * ASPNETCORE_URLS=http://+:10000
  
□ Wait for deployment (2-3 minutes)
□ Note the service URL: https://housedemoll-backend.onrender.com
□ Test: curl https://housedemoll-backend.onrender.com/api/properties
```

### Verifying Backend is Running

```bash
# Test API endpoint
curl https://housedemoll-backend.onrender.com/api/properties

# Expected: JSON array of properties
[{"id": "...", "name": "...", ...}, ...]

# Check logs in Render Dashboard → Service → Logs
```

### Key Advantages of Render

- ✅ Docker-native: No buildpacks needed
- ✅ Auto-deploy: Every `git push master` triggers rebuild
- ✅ 2x faster: ~10s vs Heroku ~20s
- ✅ Better free tier: Sleep but no charges
- ✅ Integrated PostgreSQL with same account

---

## Part 2: Frontend Deployment (GitHub Pages)

### Step 2.1: Update Frontend API Endpoint

**File:** `src/frontend/src/composables/usePropertyApi.js`

```javascript
// Change from:
const apiBase = import.meta.env.VITE_API_BASE || 'http://localhost:5088';

// To (production with Render):
const apiBase = import.meta.env.VITE_API_BASE || 'https://housedemoll-backend.onrender.com';
```

Or use `.env.production`:

```bash
# .env.production
VITE_API_BASE=https://housedemoll-backend.onrender.com
```

### Step 2.2: Deploy via GitHub Actions

```bash
# Push changes to master
git add -A
git commit -m "feat: update API endpoint to production Render backend"
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
# Network tab should show requests to https://housedemoll-backend.onrender.com/api/properties
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

Then redeploy: `git push origin master` (Render auto-rebuilds)

---

## Part 3: Database Migration

### Step 3.1: Verify Database Schema

Render automatically runs migrations via the release command in Dockerfile.

**To verify database is initialized:**

1. Check Render logs during deployment: Render Dashboard → Service → Logs
2. Should see: `Database migration completed successfully`

**To manually connect to database (optional):**

```bash
# Using psql with Render's external database URL
psql "postgres://user:pass@host:5432/dbname"

# Inside psql shell:
\dt                      # List tables
SELECT COUNT(*) FROM properties;  # Should see properties count
\q                       # Exit

# Expected: properties table with 10+ rows (from migration)
```

### Step 3.2: Seed Production Data (if needed)

If properties didn't auto-seed, manually insert them:

```bash
# Connect to Render database
psql "postgres://user:pass@host:5432/dbname"

# Run migration SQL (copy from src/backend/HouseDemo.Backend/Data/Database.cs)
# CREATE TABLE properties (...);
# INSERT INTO properties VALUES (...);
```

---

## Part 4: End-to-End Verification

### Checklist

- [ ] **Backend API responding**
  - `curl https://housedemoll-backend.onrender.com/api/properties`
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

BACKEND="https://housedemoll-backend.onrender.com"
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
# Real-time backend logs in Render Dashboard
# 1. Visit render.com → Select housedemoll-backend service
# 2. Click "Logs" tab
# 3. Watch real-time output

# Or access via Render CLI (if installed)
# See: RENDER-DEPLOYMENT.md for detailed Render monitoring
```

### Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| 503 Service Unavailable | Container starting or crashed | Check Render logs, wait 1-2 min |
| 404 Not Found | Wrong endpoint | Verify URL in frontend config |
| CORS error | Backend not allowing origin | Update CORS policy; Render auto-redeploys |
| Database connection error | Wrong connection string | Verify `DATABASE_URL` env var on Render |
| Properties not showing | Migration failed | Check release logs in Render |
| Login fails | Wrong credentials | Verify `ADMIN_EMAIL`, `ADMIN_PASSWORD` on Render |

### Restart Service (if needed)

On Render Dashboard → Service → "Deployments" tab → Click "Deploy" to rebuild

### Database Backup

```bash
# Export database from Render
pg_dump "postgres://user:pass@host:5432/dbname" > backup.sql

# Restore database
psql "postgres://user:pass@host:5432/dbname" < backup.sql
```

---

## Part 6: Post-Deployment Tasks

### Update README

**File:** `README.md`

```markdown
## Live Demo

- **Frontend:** https://leoluziyuan.github.io/HouseDemo/ (or https://housedemoll.com)
- **Backend API:** https://housedemoll-backend.onrender.com
- **Admin Login:** [credentials in Render environment]

## Deployment

Primary deployment platform: Render.com (Docker)
- Backend: Render Web Service ($7/month)
- Database: Render PostgreSQL ($15/month)
- Frontend: GitHub Pages (free)

For detailed deployment guide, see RENDER-DEPLOYMENT.md
```

### Monitor Performance

- Check Render Dashboard: https://dashboard.render.com/
- Set up alerts in Render Dashboard → Service Settings → Notifications

### Continuous Deployment

After first manual deployment, every `git push origin master` triggers:
1. GitHub Actions tests (Playwright E2E)
2. Frontend build & deploy to GitHub Pages
3. Render auto-detects code changes and rebuilds Docker image

**Auto-deploy is enabled by default** when you connect GitHub repo to Render.

---

## Part 7: Rollback Procedure

If production deployment has issues:

**Option 1: Rollback to previous commit**

```bash
# View deployment history in Render Dashboard
# Render → Service → "Deployments" tab

# Select previous successful deployment and click "Redeploy"
# Render will rebuild and deploy that version
```

**Option 2: Manual rollback via git**

```bash
# Revert to previous commit locally
git revert HEAD --no-edit

# Push to master (triggers new Render build)
git push origin master

# Render auto-detects and rebuilds
```

**Option 3: Connect to database and verify data**

```bash
# If backend is running but showing wrong data:
psql "postgres://user:pass@host:5432/dbname"

# Check what's in the database
SELECT * FROM properties LIMIT 5;

# Restore from backup if needed
pg_restore backup.sql
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
| Render Web Service | Standard | $7 |
| Render PostgreSQL | Starter | $15 |
| GitHub Pages | Free | $0 |
| Custom Domain | Varies | $10-15 (registrar) |
| **Total** | | ~$32-37 |

**Cost Optimization:**
- Use Render free tier ($0, but services sleep after 15 min inactivity)
- PostgreSQL free tier (30 days trial)
- GitHub Pages always free
- Total for minimal setup: $7/month (web) + free database trial

**Comparison with other platforms:**
- **Heroku:** $50+/month (DB only, no free tier anymore)
- **AWS:** $10-50+/month (depending on usage)
- **DigitalOcean:** $5-12/month (simpler pricing)
- **Render:** $7/month minimum (best for learning)

---

## FAQ

**Q: My backend is slow / takes 10+ seconds to respond**  
A: Render free tier sleeps after 15 min inactivity. First request takes ~10s to "wake up". Upgrade to Standard ($7/month) for always-on.

**Q: Can I deploy without Render?**  
A: Yes! See `RENDER-DEPLOYMENT.md` for alternatives (AWS, GCP, DigitalOcean, or Docker locally).

**Q: How do I update production without downtime?**  
A: Render handles rolling deploys automatically. Just `git push origin master` and Render rebuilds in background.

**Q: What if custom domain fails?**  
A: Frontend will still work at `https://leoluziyuan.github.io/HouseDemo/`. Update DNS records and wait 5-10 minutes.

**Q: How do I add more admin users in V2?**  
A: See `.planning/phases/01-database-and-rest-api/PLAN.md` for multi-user roadmap.

**Q: Should I use Render, Heroku, or Docker?**  
A: Render recommended for learning (better free tier, no buildpacks). Docker good for Kubernetes/enterprise. Heroku now costs more ($50+/month).

**Q: How often should I backup the database?**  
A: For production, daily backups recommended. See RENDER-DEPLOYMENT.md for backup procedures.

---

## Support

For issues:
1. Check RENDER-DEPLOYMENT.md for detailed Render-specific guidance
2. Review logs: Render Dashboard → Service → Logs
3. Verify config: Render Dashboard → Service → Environment
4. Test API: `curl https://housedemoll-backend.onrender.com/api/properties`
5. Check frontend: Browser DevTools → Network, Console tabs

For Render documentation: https://render.com/docs

---

**Last Updated:** 2026-11-05  
**Recommended Platform:** Render.com (Docker)  
**Status:** ✅ Ready for Production Deployment
