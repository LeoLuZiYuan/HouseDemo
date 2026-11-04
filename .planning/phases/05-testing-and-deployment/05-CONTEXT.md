# Phase 5 Context — Testing & Deployment Decisions

**Phase:** 05-testing-and-deployment  
**Date:** 2026-11-04  
**Decision Owner:** User (@LeoLuZiYuan)  
**Status:** ✅ All Gray Areas Resolved

---

## Executive Summary

Phase 5 will validate the full end-to-end application and deploy it to production. All architectural, tooling, and configuration decisions have been locked to guide research and planning phases.

---

## 1️⃣ E2E Testing Strategy

### Decision: Full-Suite Playwright Testing with CI/CD Automation

**Locked Decisions:**

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Test Framework** | Playwright | Fast, cross-browser, no external dependencies, excellent async handling |
| **Test Coverage** | Complete flow | Login → Create → Verify on public → Delete (end-to-end validation) |
| **Test Environment** | Local (localhost) | localhost:5174 (frontend) + localhost:5088 (backend) |
| **CI/CD Automation** | GitHub Actions | Run E2E tests on every push to verify code quality before merge |
| **Test Execution Timing** | Pre-merge gate | Fail merge if E2E tests fail |

### Test Scenarios

**Core E2E Flow (Must-Have):**
1. Admin login with valid credentials → redirected to dashboard
2. Admin creates property with all fields → verify ID returned
3. HomeView loads and displays new property → check card render
4. Click property card → RoomDetailView loads correct data
5. Admin edits property (name/price) → public site reflects changes (after refresh)
6. Admin deletes property → property no longer appears on public site

**Validation Points:**
- No 404s in network requests
- No console errors
- Load times within acceptable range
- Database changes persist across page refreshes

### Implementation Notes

- Use Playwright test runner (not CLI runner) for better debugging
- Run headless in CI/CD; headful locally for development
- Generate HTML report for failed tests
- Tag tests: `@critical`, `@smoke`, `@integration` for selective runs

---

## 2️⃣ Heroku Backend Deployment

### Decision: Heroku PostgreSQL with Procfile + Config Vars

**Locked Decisions:**

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Database** | Heroku PostgreSQL add-on | Integrated, auto-backup, managed credentials in DATABASE_URL |
| **Runtime** | .NET 8 | Current stable LTS, no compatibility issues expected |
| **Environment Variables** | Heroku Config Vars | Simple UI, secure, integrated with app, no extra tooling |
| **DB Initialization** | Procfile `release` phase | Automatic schema creation on first deploy, idempotent |
| **API Base URL** | Heroku auto-generated | e.g., `https://housedemo-backend-{random}.herokuapp.com/api` |

### Heroku Configuration

**Procfile:**
```
web: dotnet HouseDemo.Backend.dll
release: dotnet HouseDemo.Backend migrate-db
```

**Required Environment Variables (set via Heroku dashboard or CLI):**
- `DATABASE_URL` — auto-populated by PostgreSQL add-on
- `JWT_SECRET` — random 32+ char string for token signing
- `CORS_ALLOWED_ORIGINS` — `https://housedemoll.com` (set once domain is purchased)
- `NODE_ENV` — `production`

**Deployment Steps:**
1. Install Heroku CLI
2. Create Heroku app: `heroku create housedemo-backend`
3. Add PostgreSQL: `heroku addons:create heroku-postgresql:hobby-dev`
4. Set config vars: `heroku config:set JWT_SECRET=...`
5. Deploy: `git push heroku main`
6. Run migrations: `heroku run dotnet migrate-db`

### Post-Deployment Validation

- [ ] API responds on `https://housedemo-backend-{name}.herokuapp.com/api/properties`
- [ ] Database is populated with seed data
- [ ] Authentication endpoint working: `POST /api/auth/login`
- [ ] Admin CRUD endpoints protected (401 without token)

---

## 3️⃣ Domain & CORS Configuration

### Decision: Separate Domains with CORS Allow-Specific

**Locked Decisions:**

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Domain Status** | Not yet purchased | housedemoll.com must be purchased before Phase 5 execution |
| **Architecture** | Separate domains | Frontend: housedemoll.com (GitHub Pages) / Backend: Heroku auto-URL |
| **CORS Policy** | AllowSpecificOrigin | Only allow requests from `https://housedemoll.com` in production |
| **Admin URL** | Same domain hash route | `https://housedemoll.com/#/admin` (no separate admin subdomain) |
| **Heroku URL** | Auto-generated | No custom subdomain (e.g., api.housedemoll.com) needed |

### CORS Configuration (Backend)

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("https://housedemoll.com", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("AllowSpecificOrigin");
```

### Frontend API URL Management

**Development (.env.local):**
```
VITE_API_BASE=http://localhost:5088/api
```

**Production (.env.production):**
```
VITE_API_BASE=https://housedemo-backend-{name}.herokuapp.com/api
```

### Domain Purchase & Setup

**Before Phase 5 Execution:**
1. Purchase housedemoll.com from domain registrar
2. Point nameservers to GitHub Pages DNS (if using GitHub Pages for frontend)
3. OR configure CNAME to point to `{username}.github.io`
4. Add CNAME file to `public/CNAME`
5. Enable HTTPS (automatic via GitHub Pages + Let's Encrypt)

**Pre-Deployment Checklist:**
- [ ] Domain purchased and DNS configured
- [ ] CNAME file in place
- [ ] GitHub Pages custom domain setting enabled
- [ ] HTTPS working (green lock icon)
- [ ] API URL updated in environment variables

### API Endpoint Access Pattern

**Public Properties (No Auth):**
```
GET https://housedemo-backend-{name}.herokuapp.com/api/properties
GET https://housedemo-backend-{name}.herokuapp.com/api/properties/{id}
```

**Admin Properties (Requires Auth):**
```
POST https://housedemo-backend-{name}.herokuapp.com/api/admin/properties
PUT https://housedemo-backend-{name}.herokuapp.com/api/admin/properties/{id}
DELETE https://housedemo-backend-{name}.herokuapp.com/api/admin/properties/{id}
```

---

## 4️⃣ GitHub Pages Frontend Deployment

### Decision: Auto-Deploy with Base Path `'/'` and `.env` Config

**Locked Decisions:**

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Base Path** | `'/'` | Optimized for custom domain housedemoll.com |
| **CI/CD** | GitHub Actions auto-deploy | Every push to master triggers build + deploy automatically |
| **CNAME File** | Check if exists, create if missing | Required for custom domain routing |
| **API URL Source** | `.env` environment variables | Already implemented; supports dev/prod separation |
| **Build Optimization** | Vite defaults | No custom optimizations needed; Vite handles tree-shaking + code splitting |
| **HTTPS** | GitHub Pages auto (Let's Encrypt) | No manual configuration needed |

### GitHub Actions Workflow

**Expected Workflow File:** `.github/workflows/deploy.yml`

```yaml
name: Deploy to GitHub Pages

on:
  push:
    branches: [master]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '22'
      - run: npm install
      - run: npm run build
      - uses: actions/upload-artifact@v3
        with:
          name: dist
          path: dist/
      - uses: peaceiris/actions-gh-pages@v3
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: ./dist
          cname: housedemoll.com
```

### Deployment Environment Variables

**.env.production (in repository):**
```
VITE_API_BASE=https://housedemo-backend-{name}.herokuapp.com/api
```

When domain is purchased, update this once with the actual Heroku backend URL.

### Pre-Deployment Checklist

- [ ] CNAME file exists in `public/CNAME` with content: `housedemoll.com`
- [ ] GitHub Pages settings → Source: Deploy from branch (master)
- [ ] GitHub Pages settings → Custom domain: `housedemoll.com`
- [ ] `.env.production` updated with Heroku API base URL
- [ ] GitHub Actions workflow exists and passes
- [ ] Verify production build locally: `npm run build && npm run preview`

### Post-Deployment Validation

- [ ] https://housedemoll.com loads without errors
- [ ] Properties display from API
- [ ] /#/admin route accessible (redirects to /#/login if not authenticated)
- [ ] Login works with test credentials
- [ ] Create property → refreshes → appears on public list
- [ ] Page load time < 2 seconds

---

## 5️⃣ Production Monitoring & Stability

### Not Discussed (Deferred to Post-Phase-5)

The following topics were not prioritized and can be addressed in a future iteration:

- Application performance monitoring (APM)
- Error logging/tracking (Sentry integration)
- Database backup strategy
- Rate limiting and DDoS protection
- Analytics and usage tracking

**Recommendation:** Implement these after Phase 5 validates the core functionality in production.

---

## Cross-Phase Dependencies

**Locked Dependencies:**
- ✅ Phase 1: Database & REST API (backend ready)
- ✅ Phase 2: Admin Authentication (JWT + login working)
- ✅ Phase 3: Admin CRUD Dashboard (create/read/update/delete verified)
- ✅ Phase 4: API Integration & Frontend (public site fetches from API)

**All prerequisites complete.** Phase 5 can proceed.

---

## Risk Mitigation

| Risk | Mitigation |
|------|-----------|
| E2E tests fail in CI/CD | Run tests locally first; fix flaky tests before commit |
| Heroku PostgreSQL connection fails | Test locally with PostgreSQL first; verify DB_URL format |
| CORS blocks API requests | Pre-test CORS headers with curl before deployment |
| Domain not ready | Use Heroku auto-URL for testing; update DNS after domain purchase |
| GitHub Actions fails silently | Monitor Actions runs in GitHub UI; check logs if deploy doesn't appear |

---

## Phase 5 Execution Plan

**Expected Tasks (To be detailed in PLAN.md):**

1. **Task 1: E2E Test Suite Setup**
   - Install Playwright
   - Write tests for complete flow
   - Configure CI/CD trigger

2. **Task 2: Heroku Backend Deployment**
   - Create Heroku app
   - Set up PostgreSQL add-on
   - Deploy and test API

3. **Task 3: GitHub Pages Frontend Deployment**
   - Update CNAME file
   - Configure GitHub Pages
   - Verify custom domain

4. **Task 4: Production Integration Testing**
   - Run E2E tests against production
   - Validate end-to-end flow
   - Performance checks

---

## Sign-Off

**Context Document:** ✅ APPROVED  
**All Decisions Locked:** ✅ Yes  
**Ready for Researcher:** ✅ Yes  
**Ready for Planner:** ✅ Yes  

**Date:** 2026-11-04  
**Reviewer:** Copilot

Next Step: `/gsd-plan-phase 5` to create detailed execution plan.
