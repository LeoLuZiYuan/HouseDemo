# HouseDemo — V1 Requirements

## Overview

This document defines the scope and success criteria for HouseDemo V1: **Database-backed property listings with landlord management panel**.

**Milestone Goal:** Replace static JSON data with persistent PostgreSQL storage and enable property owners to manage listings through a backend admin interface.

---

## V1 Requirements

### Category: Data Persistence

- **[REQ-PERSIST-01]** Landlord can view current database state of all properties
  - _Acceptance:_ Admin panel displays list of all properties from PostgreSQL
  - _Test:_ Navigate to admin panel, verify all existing properties shown

- **[REQ-PERSIST-02]** Backend API exposes property data via REST endpoints
  - _Acceptance:_ GET `/api/properties` returns JSON array of all properties; GET `/api/properties/:id` returns single property
  - _Test:_ `curl` endpoint, verify JSON structure matches room.json schema

- **[REQ-PERSIST-03]** Frontend fetches property list from backend API instead of static JSON
  - _Acceptance:_ HomeView makes HTTP request to backend on page load; displays properties
  - _Test:_ Inspect network tab; verify API call succeeds

- **[REQ-PERSIST-04]** Existing room data migrated from static JSON to PostgreSQL
  - _Acceptance:_ All properties in `src/data/rooms.json` exist in database; no data loss
  - _Test:_ Count records in DB, verify matches JSON file

### Category: Landlord Authentication

- **[REQ-AUTH-01]** Landlord can log in to admin panel
  - _Acceptance:_ Login form accepts email/password; redirects to admin dashboard on success
  - _Test:_ Login with valid credentials, verify redirect; try invalid credentials, verify error

- **[REQ-AUTH-02]** Unauthorized users cannot access admin panel
  - _Acceptance:_ Unauthenticated requests to `/admin/*` redirect to login
  - _Test:_ Visit `/admin` without token, verify redirect to login

### Category: Admin Dashboard

- **[REQ-ADMIN-01]** Admin dashboard displays all properties in a table/list format
  - _Acceptance:_ Dashboard shows all properties with name, rent, area columns
  - _Test:_ Navigate to admin, verify table populated

- **[REQ-ADMIN-02]** Admin can create a new property listing
  - _Acceptance:_ Form with all room.json fields (name, area, monthlyRent, shortDesc, description, features, coverImage, images); submit button persists to DB
  - _Test:_ Fill form, submit, verify new property appears in dashboard and public site

- **[REQ-ADMIN-03]** Admin can edit existing property details
  - _Acceptance:_ Click "Edit" on property, form pre-populates with current values, submit updates DB
  - _Test:_ Edit a property, refresh public site, verify changes visible

- **[REQ-ADMIN-04]** Admin can delete a property listing
  - _Acceptance:_ Click "Delete" on property, confirm dialog, property removed from DB and public site
  - _Test:_ Delete a property, verify it no longer appears anywhere

- **[REQ-ADMIN-05]** Admin changes to properties immediately reflect on public website
  - _Acceptance:_ Create/edit/delete in admin panel, refresh public site, changes visible
  - _Test:_ CRUD cycle, verify consistency between admin and public

### Category: Public Browsing (Preserved)

- **[REQ-PUBLIC-01]** Public visitors can browse all properties without login
  - _Acceptance:_ Homepage displays all active properties, no auth required
  - _Test:_ Visit site anonymously, verify properties visible

- **[REQ-PUBLIC-02]** Visitors can view detailed property information
  - _Acceptance:_ Click property card, detail page shows all info + images
  - _Test:_ Navigate to detail page, verify all fields rendered

---

## Traceability to Roadmap

| Requirement | Phase | Status |
|-------------|-------|--------|
| REQ-PERSIST-01 → REQ-PERSIST-04 | Phase 1: Database & API | Pending |
| REQ-AUTH-01 → REQ-AUTH-02 | Phase 2: Admin Authentication | Pending |
| REQ-ADMIN-01 → REQ-ADMIN-05 | Phase 3: Admin CRUD Operations | Pending |
| REQ-PUBLIC-01 → REQ-PUBLIC-02 | Phase 1: API Integration | Pending |

---

## V2+ (Out of Scope for V1)

- Multi-landlord / multi-owner support
- OAuth or social login
- User reviews / ratings
- Search, filtering, sorting on public site
- Image upload to cloud storage (continue using external URLs)
- Booking / reservation integration
- Payment processing
- Mobile app
- Analytics dashboard
- Email notifications

---

## Definition of Done

A requirement is **Done** when:
1. ✅ Acceptance criteria all pass
2. ✅ Code reviewed and merged
3. ✅ Deployed to staging/Heroku
4. ✅ Verified end-to-end (admin + public site)
5. ✅ No regressions on existing features

---

*Last updated: 2026-11-04 after requirement gathering*
