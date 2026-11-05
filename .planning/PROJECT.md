# HouseDemo — Project Context

## What This Is

HouseDemo is a **rental property showcase website** that presents house/apartment listings to potential tenants. Currently a pure frontend application (Vue 3 + Vite), it's evolving into a **full-stack application** with a .NET backend and PostgreSQL database to enable property management capabilities.

**Project Type:** Educational side project for learning AI-assisted full-stack development with rapid iteration.

---

## Core Value

Enable a property owner to:
1. **Display their rental listings** to anyone who knows the website URL
2. **Manage listings dynamically** through a backend admin panel (create, edit, delete property information)
3. Replace static data storage with persistent database-backed infrastructure

---

## Context

### Current State
- **Frontend:** Vue 3 + Vite + Tailwind CSS v4
- **Data:** Static JSON file (`src/data/rooms.json`)
- **Images:** External Cloudinary/placeholder URLs
- **Hosting:** GitHub Pages (static deployment)
- **Features:** Read-only property browsing, image galleries, detail pages

### Target State (V1)
- **Frontend:** Vue 3 + Vite (preserved)
- **Backend:** .NET Minimal APIs + Dapper ORM
- **Database:** PostgreSQL
- **API Style:** REST
- **Hosting:** Frontend on GitHub Pages + Backend on Heroku
- **Features:** Database-backed listings + landlord admin panel

### User Personas
1. **Visitor:** Anyone with the website URL — views property listings, no login required
2. **Landlord:** Property owner — logs in to admin panel, manages (CRUD) property information

---

## Tech Stack & Constraints

| Layer | Technology | Rationale |
|-------|-----------|-----------|
| Frontend | Vue 3 + Vite + Tailwind v4 | Existing stack; no migration needed |
| Backend | .NET Minimal APIs | Lightweight, fast iteration |
| ORM | Dapper | Lightweight SQL mapping; quick learning curve |
| Database | PostgreSQL | Robust, open-source, excellent .NET support |
| Deployment | GitHub Pages + Heroku | Low cost, learning-friendly, GitHub native |

### Key Constraints
- Pure frontend, no backend (current) → transitioning to backend-driven
- No existing user authentication system (will be needed for admin panel)
- Images sourced externally (continue this pattern)
- Single landlord (no multi-tenant complexity in V1)

---

## Scope & Requirements

### V1 Requirements (Must Have)

#### Data Persistence
- **[REQ-01]** Migrate room/property data from static JSON to PostgreSQL database
- **[REQ-02]** Backend API exposes room data via REST endpoints (list, get by ID)
- **[REQ-03]** Frontend continues to display properties, now fetching from backend API

#### Landlord Admin Panel
- **[REQ-04]** Landlord can log in to admin panel (basic auth, no OAuth for V1)
- **[REQ-05]** Landlord can view all property listings in admin dashboard
- **[REQ-06]** Landlord can create new property listing (form with all room.json fields)
- **[REQ-07]** Landlord can edit existing property details
- **[REQ-08]** Landlord can delete property listings
- **[REQ-09]** Admin changes immediately reflect on public-facing website

### Out of Scope (V2+)
- Multi-landlord / multi-owner support
- User authentication for visitors (browsing remains public)
- Payment/booking integration
- Advanced search, filters, or sorting
- Image upload/management (continue using external URLs in V1)
- Mobile app
- Analytics or reporting

---

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Keep Vue 3 + Vite | Stable, proven stack; focus iteration on backend | Frontend changes minimal |
| Use Dapper instead of EF Core | Lighter weight, faster to learn, explicit SQL | Slight complexity trade-off for speed |
| PostgreSQL over SQLite | Better for future scaling; cloud-friendly | Setup overhead acceptable for learning |
| REST API (not GraphQL) | Simpler for learning; aligns with V1 scope | Standard CRUD endpoints only |
| Heroku for backend | Free tier available; GitHub native integration | Supports .NET containers |
| No image upload in V1 | External URLs reduce backend complexity | Images managed outside the app (for now) |
| Basic auth for admin | Simplest MVP auth; not production-grade | Suitable for single-owner learning project |

---

## Success Criteria

✓ **V1 Launch:** Backend API running, data persists to PostgreSQL, landlord can CRUD listings, public site fetches from backend  
✓ **Developer Experience:** Fast iteration cycles, clear code structure for learning  
✓ **Deployment:** Automated CI/CD for both frontend and backend  

---

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---

*Last updated: 2026-11-04 after initialization*
