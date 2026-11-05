# Git Push HTTP 400 Issue — Phase 5 Shipping

## Problem

Attempted to push Phase 5 work to remote `origin/master` multiple times. All push attempts fail with:

```
error: RPC failed; HTTP 400 curl 56 The requested URL returned error: 400
fatal: the remote end hung up unexpectedly
```

## Local State

✅ **All Phase 5 work is locally committed and safe:**
- 36 commits ahead of origin/master (including gitignore cleanup)
- Latest commit: `3e56eb2 chore: add playwright-report to gitignore`
- Branch: `phase-05-testing-deployment` (created from master with all work)
- Working tree: Clean (no uncommitted changes)

## Diagnostic Evidence

### 1. Connectivity ✓
```
PING github.com — 3/3 packets successful, avg RTT 33.76ms
```

### 2. Authentication ✓
```
GitHub CLI: Logged in to LeoLuZiYuan (keyring, HTTPS protocol)
Tokens scopes: gist, read:org, repo, workflow
GitHub API: User profile accessible
Repository: Admin/maintain/push/pull permissions confirmed
```

### 3. Repository Status ✓
```
Repository: LeoLuZiYuan/HouseDemo
Archived: false
Disabled: false
Permissions: push=true, admin=true
Last remote activity: 2026-04-11T15:32:26Z
```

### 4. Git Configuration ✓
```
Remote URL: https://github.com/LeoLuZiYuan/HouseDemo.git
User: LeoLu <leolu@ddsc.com.tw>
Git version: 2.19.2
```

### 5. Push Attempts Made

| Attempt | Command | Protocol | Result |
|---------|---------|----------|--------|
| 1 | `git push origin master` | HTTPS | HTTP 400 |
| 2 | `git push -u origin master` | HTTPS | HTTP 400 |
| 3 | `git remote set-url origin git@github.com:...` | SSH | Permission denied (publickey) |
| 4 | Switch back to HTTPS + retry | HTTPS | HTTP 400 |
| 5 | After removing playwright-report (558 deletions) | HTTPS | HTTP 400 |
| 6 | Curl verbose push | HTTPS | HTTP/2 400 response, empty body |

### 6. HTTP/2 Response Details

```
POST /LeoLuZiYuan/HouseDemo.git/git-receive-pack HTTP/2
Authorization: Basic [encoded token]
Content-Type: application/x-git-receive-pack-request

Response:
< HTTP/2 400
< server: GitHub-Babel/3.0
< content-length: 0
< x-github-request-id: C5D0:AD0EF:38D8:5D0E:6AEC8208
< x-github-edge-region: japaneast
```

**Analysis:**
- Empty response body (0 content-length)
- Request-ID suggests server-side processing
- GitHub Babel server (CDN/load balancer)
- Region: Japan East
- Not a client-side auth or protocol error

## Potential Causes

1. **Repository-level webhook failure** — GitHub webhook rejecting commits
2. **Push size limits** — Payload too large (even after removing 23 files / 558 deletions)
3. **GitHub API rate limiting** — Not indicated in response headers, but possible
4. **Temporary GitHub infrastructure issue** — HTTP 400 with empty body suggests server error
5. **Branch protection rules** — Not configured; push to master should be allowed
6. **Pre-receive hook** — GitHub Enterprise or custom validation (unlikely for free tier)

## Workarounds Tested

- ✗ Remove large files (playwright-report/) and retry
- ✗ Switch to SSH (failed at authentication layer, not server)
- ✗ Use `-u` flag for tracking setup
- ✗ Verbose git/curl tracing (shows auth is correct)
- ✗ Verify GitHub API access (works; can read repo details)

## Recommended Next Steps

### Option 1: Retry in New Session (Recommended)
1. Update LOCAL git credentials: `git credential approve` or `gh auth login --refresh`
2. Retry: `git push origin master`
3. If still fails, escalate to GitHub Support with request-id: `C5D0:AD0EF:38D8:5D0E:6AEC8208`

### Option 2: Force Push (Use Caution)
```bash
git push --force-with-lease origin phase-05-testing-deployment
```
This bypasses some server-side checks but may not resolve HTTP 400.

### Option 3: Create PR via GitHub UI
1. Manually create PR on GitHub.com:
   - Base: master
   - Compare: phase-05-testing-deployment (local branch, unpushed)
2. Add PR description from template below

### Option 4: Escalate to GitHub Support
Contact support with:
- Request-ID: C5D0:AD0EF:38D8:5D0E:6AEC8208
- Repository: LeoLuZiYuan/HouseDemo
- Branch: phase-05-testing-deployment
- Commits: 36 ahead of origin/master
- Error: Consistent HTTP 400 on all push attempts since 2026-11-05 21:18 UTC+8

## PR Description Template

If PR must be created manually:

```markdown
## Phase 5: Testing & Deployment Complete

**Accomplishments:**
- E2E test suite (Playwright) with 9 passing tests
- Heroku deployment configuration (Procfile, PostgreSQL addon)
- GitHub Pages CI/CD workflow (auto-deploy on push)
- Custom domain setup (CNAME for housedemoll.com)
- Production environment configuration

**Requirements Validated:** 13/13 (100%)
- Persistence: 4/4 ✓
- Authentication: 2/2 ✓
- Admin Panel: 5/5 ✓
- Public Browsing: 2/2 ✓

**Test Results:**
- E2E Tests: 9/9 passing (100%)
- Build Time: 0.57s
- API Response: < 2s average
- No CORS errors, no 404s

**UAT Report:** .planning/phases/05-testing-and-deployment/05-UAT.md ✅ 13/13 verified

**Commits:** 36 total (36 ahead of origin/master)
```

## Files Affected

See Phase 5 UAT report for complete list:
- .planning/phases/05-testing-and-deployment/05-UAT.md

## Status

**Local Work:** ✅ Complete and committed  
**Remote Push:** ❌ Blocked (HTTP 400)  
**Impact:** Cannot create PR or merge; work is safe locally  
**Priority:** Medium — resolve in next session before proceeding to v1.1 milestone

---

*Generated: 2026-11-05 21:18 UTC+8*  
*Session: Phase 5 Shipping Attempt*  
*Branch: phase-05-testing-deployment*
