# HouseDemo — V1 Roadmap

## Executive Summary

**Project:** HouseDemo V1 — Full-stack conversion with database persistence and landlord admin panel  
**Phases:** 5  
**Mode:** Vertical MVP — Each phase delivers a complete end-to-end user capability  
**Timeline:** Exploratory, rapid iteration for learning  

---

## Phase Overview

| # | Phase | Goal | Requirements | Success Criteria | Status |
|---|-------|------|--------------|------------------|--------|
| 1 | Database & REST API | Set up PostgreSQL, data schema, backend API | REQ-PERSIST-01, REQ-PERSIST-02, REQ-PERSIST-04 | 3 | ✅ |
| 2 | Admin Authentication | Basic auth system with login | REQ-AUTH-01, REQ-AUTH-02 | 3 | ✅ |
| 3 | Admin CRUD Dashboard | Full admin panel for managing properties | REQ-ADMIN-01, REQ-ADMIN-02, REQ-ADMIN-03, REQ-ADMIN-04, REQ-ADMIN-05 | 5 | ✅ |
| 4 | API Integration | Update frontend to use backend API | REQ-PERSIST-03, REQ-PUBLIC-01, REQ-PUBLIC-02 | 3 | Pending |
| 5 | Testing & Deployment | E2E testing, deploy to Heroku, verify | All requirements validated | 3 | Pending |

**Total Requirements Mapped:** 15/15 ✓

---

## Phase Details

### Phase 1: Database & REST API

**Goal:** Establish PostgreSQL database, data schema, and foundational REST API endpoints for property data.

**Mode:** mvp

**User Story:**  
As a developer, I need a working backend API that serves property data from PostgreSQL, so I can build the admin panel and update the frontend to use persistent data.

**Requirements Addressed:**
- REQ-PERSIST-01: Landlord can view current database state
- REQ-PERSIST-02: Backend API exposes property data via REST
- REQ-PERSIST-04: Existing room data migrated to PostgreSQL

**Success Criteria:**
1. PostgreSQL database is running locally; schema created with `properties` table (id, name, area, monthlyRent, shortDesc, description, features, coverImage, images)
2. .NET Minimal API backend boots successfully; `GET /api/properties` returns all properties as JSON; `GET /api/properties/{id}` returns single property
3. Existing room data from `src/data/rooms.json` migrated to database; row count matches JSON array length; no data loss verified

**Acceptance Tests:**
- [ ] Database schema created, primary keys and constraints in place
- [ ] Sample data (from rooms.json) inserted successfully
- [ ] API returns 200 OK with valid JSON structure
- [ ] CORS configured so frontend can call backend

**Dependencies:** None (greenfield backend setup)

**Risks:**
- PostgreSQL local setup on macOS (installation, path issues) → Mitigate: document Docker Compose alternative
- JSON-to-SQL data migration fidelity → Mitigate: write validation script to compare counts

---

### Phase 2: Admin Authentication

**Goal:** Implement basic authentication system with login form and session management so only the landlord can access admin panel.

**Mode:** mvp

**User Story:**  
As a landlord, I need a secure login so my admin panel is protected and only I can manage my properties.

**Requirements Addressed:**
- REQ-AUTH-01: Landlord can log in to admin panel
- REQ-AUTH-02: Unauthorized users cannot access admin

**Success Criteria:**
1. Login endpoint `POST /api/auth/login` accepts email/password, returns JWT token; invalid credentials return 401
2. Admin routes protected by middleware that validates token in Authorization header; unauthenticated requests return 401
3. Frontend login page functional; accepts credentials, stores token in localStorage, redirects to admin dashboard on success; shows error on failure

**Acceptance Tests:**
- [ ] POST /api/auth/login with valid credentials returns JWT token
- [ ] POST /api/auth/login with invalid credentials returns 401
- [ ] GET /api/admin/* without token returns 401
- [ ] Frontend login form submits and redirects to /admin on success
- [ ] Token persists in localStorage; survives page refresh
- [ ] Router guard prevents access to /admin routes without token

**Plans:**
- [x] 02-01-PLAN.md — End-to-end authentication (tracer: login → JWT → protected endpoint)

**Dependencies:** Phase 1 (API foundation)

**Risks:**
- JWT token expiry/refresh → Mitigate: 24-hour expiration for MVP; refresh mechanism in Phase 2.1
- Frontend token XSS vulnerability → Mitigate: localStorage (acceptable for learning project); HttpOnly cookies in Phase 2.1

---

### Phase 3: Admin CRUD Dashboard

**Goal:** Build the landlord admin panel UI and backend endpoints for creating, reading, updating, and deleting property listings.

**Mode:** mvp

**User Story:**  
As a landlord, I need an admin dashboard where I can add new properties, edit details, and remove listings so I can manage my portfolio.

**Requirements Addressed:**
- REQ-ADMIN-01: Admin dashboard displays all properties
- REQ-ADMIN-02: Admin can create new property
- REQ-ADMIN-03: Admin can edit property details
- REQ-ADMIN-04: Admin can delete property
- REQ-ADMIN-05: Changes immediately reflect on public site

**Success Criteria:**
1. Backend CRUD endpoints fully functional: `GET /api/admin/properties`, `POST /api/admin/properties`, `PUT /api/admin/properties/{id}`, `DELETE /api/admin/properties/{id}` with proper validation
2. Vue admin dashboard component displays properties in table format; each row has Edit and Delete buttons
3. Create/Edit form with all fields (name, area, monthlyRent, shortDesc, description, features list, image URLs); form validation; submit persists to DB
4. Delete operation shows confirmation, then removes property; property no longer appears on public site immediately after refresh

**Acceptance Tests:**
- [ ] Backend POST /api/admin/properties creates property with all fields
- [ ] Backend PUT updates property successfully
- [ ] Backend DELETE removes property, returns 204
- [ ] Frontend admin table populates and refreshes after CRUD operations
- [ ] Form validation rejects incomplete submissions
- [ ] Changes visible immediately on public site

**Dependencies:** Phase 1 (API), Phase 2 (Authentication)

**Risks:**
- Form complexity (features array handling, multiple image URLs) → Mitigate: start simple, add complexity iteratively
- Concurrent edits or race conditions → Mitigate: add optimistic locking or timestamps for V2

---

### Phase 4: API Integration & Frontend

**Goal:** Update the public-facing frontend to fetch property data from the backend API instead of static JSON, completing the full-stack integration.

**Mode:** mvp

**User Story:**  
As a visitor, I want to see the same rental listings as before, but now they're powered by the database so the landlord can manage them in real-time.

**Requirements Addressed:**
- REQ-PERSIST-03: Frontend fetches from backend API
- REQ-PUBLIC-01: Public browsing preserved
- REQ-PUBLIC-02: Detail pages work with API data

**Success Criteria:**
1. HomeView fetches property list from `GET /api/properties` on mount; displays properties in grid as before
2. RoomDetailView fetches single property from `GET /api/properties/{id}` by URL param; renders all details and images
3. No static JSON file loaded by frontend; all data comes from backend API
4. Public site works identically to before (visually/functionally); no breaking changes

**Acceptance Tests:**
- [ ] Frontend inspects network tab, confirms API calls not static JSON
- [ ] All properties display correctly from database
- [ ] Detail pages load and render all fields
- [ ] No console errors
- [ ] Page load time acceptable (no major performance regression)

**Dependencies:** Phase 1 (API endpoints available)

**Risks:**
- API latency on first load → Mitigate: add loading states in UI
- API failure graceful handling → Mitigate: display error message, fall back if needed

---

### Phase 5: Testing & Deployment

**Goal:** Validate the full end-to-end application (admin + public site), ensure quality, and deploy to production (GitHub Pages + Heroku).

**Mode:** mvp

**User Story:**  
As a project owner, I want the full application tested and deployed to production so users can access the live site with a working backend.

**Requirements Addressed:** All (validation pass)

**Success Criteria:**
1. E2E test suite covers: login → CRUD property → verify public site reflects changes
2. Frontend deployed to GitHub Pages; backend deployed to Heroku; both accessible via custom domain (housedemoll.com)
3. All 15 V1 requirements verified end-to-end; acceptance tests pass

**Acceptance Tests:**
- [ ] Run full E2E flow: admin login → create property → verify on public site → edit → delete
- [ ] Public site loads from housedemoll.com; properties display
- [ ] Admin panel accessible from admin.housedemoll.com (or separate URL)
- [ ] No 404s or unhandled errors
- [ ] Performance acceptable (page load < 2s)

**Dependencies:** Phase 1, 2, 3, 4 (all phases complete)

**Risks:**
- CORS/domain setup complexity → Mitigate: document DNS and CNAME configuration
- Heroku deployment quirks (.NET runtime, PostgreSQL add-on) → Mitigate: use Procfile, test locally first

---

## Requirement Traceability

All V1 requirements mapped and validated:

| REQ-ID | Category | Phase | Priority |
|--------|----------|-------|----------|
| REQ-PERSIST-01 | Data Persistence | Phase 1 | Must Have |
| REQ-PERSIST-02 | Data Persistence | Phase 1 | Must Have |
| REQ-PERSIST-03 | Data Persistence | Phase 4 | Must Have |
| REQ-PERSIST-04 | Data Persistence | Phase 1 | Must Have |
| REQ-AUTH-01 | Admin Auth | Phase 2 | Must Have |
| REQ-AUTH-02 | Admin Auth | Phase 2 | Must Have |
| REQ-ADMIN-01 | Admin Panel | Phase 3 | Must Have |
| REQ-ADMIN-02 | Admin Panel | Phase 3 | Must Have |
| REQ-ADMIN-03 | Admin Panel | Phase 3 | Must Have |
| REQ-ADMIN-04 | Admin Panel | Phase 3 | Must Have |
| REQ-ADMIN-05 | Admin Panel | Phase 3 | Must Have |
| REQ-PUBLIC-01 | Public Browsing | Phase 4 | Must Have |
| REQ-PUBLIC-02 | Public Browsing | Phase 4 | Must Have |

**Coverage:** 13/13 requirements ✓

---

## Next Steps

1. ✅ **Approved Roadmap** — Proceed to Phase 1 Planning
2. Run `/gsd-plan-phase 1` to create detailed PLAN.md for Phase 1
3. Execute Phase 1 with `/gsd-execute-phase 1`
4. Validate with `/gsd-verify-work`
5. Loop for subsequent phases

---

*Last updated: 2026-11-04 after roadmap creation*
