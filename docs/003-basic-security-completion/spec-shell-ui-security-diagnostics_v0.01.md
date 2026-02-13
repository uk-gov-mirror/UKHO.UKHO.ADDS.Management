# Functional Specification Document (FSD)

**Project**: UKHO ADDS Management Shell  
**Version**: v0.01  
**Date**: 2026-02-13  
**Author**: UKHO / Shell Team

---

## 1. Executive Summary

**Purpose**  
Define functional and technical requirements to complete the shell’s baseline security behaviours and introduce a development-only diagnostic view.

**Objective**  
- Default the selected deployment to `Dev` on login.
- Provide a `Development` page for users with the `developer` role in development environments.
- Ensure unauthorized direct URL access results in a safe and predictable navigation experience.

---

## 2. System Overview

**User Types**  
- Authenticated users (all) – must be able to use the shell home page.
- Role-bearing users – may see additional module pages based on roles.
- Developer users – users with the `developer` role; can access the Developer module and `Development` diagnostics page.

**Key Behaviours**  
- Users must be authenticated to access shell pages.
- Development-only diagnostics must be accessible to users with the `developer` role.

---

## 3. Architecture Overview

| Component | Description |
|-----------|-------------|
| **Shell UI Routing** | Blazor router using `AuthorizeRouteView` to enforce authentication and handle unauthorized routes |
| **Deployment Selection** | Header selector backed by `DeploymentsJsonLoader`, `DeploymentSelectionStorage`, and `DeploymentContext` |
| **Navigation Filtering** | `ModulePageService` filters module navigation entries based on roles |
| **Developer Module** | The existing Sample module, renamed to the Developer module (including `.csproj` rename) and role-gated to `developer` |
| **Diagnostics Page** | A new shell page and route rendered only in Development environment and only for users with the `developer` role |

---

## 4. Functional Requirements

### 4.1 Deployment selection defaults

| ID | Requirement Description | Priority |
|----|--------------------------|----------|
| FR1 | When a user logs in, the default deployment selection must be set to `Dev` by matching deployment `id == "Dev"`. | High |
| FR2 | The header deployment dropdown must reflect the selected default deployment (`Dev`). | High |
| FR3 | `Dev` is always available. If any deployment list is loaded, `Dev` must be selected as the default. | High |
| FR3a | The default deployment selection must be applied immediately after login (when the user first becomes authenticated), not continuously enforced on every page refresh. | Medium |

### 4.2 Development page

| ID | Requirement Description | Priority |
|----|--------------------------|----------|
| FR4 | A new shell page named `Development` must be available only to users with the `developer` role. | High |
| FR5 | The logic currently used to list user roles/claims (diagnostics) must be moved from Home to the `Development` page. | High |
| FR6 | The Home page must no longer display debug/auth diagnostic information. | High |
| FR7 | The `Development` page must be within the Developer module and must only be accessible to users with the `developer` role. | High |

### 4.3 Developer role and module access

| ID | Requirement Description | Priority |
|----|--------------------------|----------|
| FR15 | A new application role named `developer` must be supported for authorization decisions in the shell. | High |
| FR16 | The existing Sample module must be renamed to the Developer module. | High |
| FR17 | Renaming the Sample module must include renaming its `.csproj` to match the Developer module name. | High |
| FR18 | All pages/routes within the Developer module must require the `developer` role. | High |
| FR19 | The `Development` page must live within the Developer module (not in the shell host project). | High |

### 4.4 Unauthorized direct URL behaviour

| ID | Requirement Description | Priority |
|----|--------------------------|----------|
| FR10 | If a user pastes a URL for a page they do not have permission to access, the user must be redirected to the Home page (`/`). | High |
| FR11 | This behaviour must apply to module routes protected by roles and/or authorization attributes. | High |
| FR12 | The user must not see a generic browser error like “This page can’t be found” for unauthorized access. | High |
| FR13 | If a user navigates to a URL that does not exist (no matching route), the user must be redirected to the Home page (`/`). | High |
| FR14 | The user must not see a generic browser error like “This page can’t be found” for non-existent routes. | High |

---

## 5. Non-Functional Requirements

| ID | Category | Description | Target |
|----|----------|-------------|--------|
| NFR1 | Usability | The shell must provide a clear, predictable outcome for unauthorized navigation attempts. | No dead-ends |
| NFR2 | Observability | Development diagnostics must assist troubleshooting auth/role issues. | Visible in Development only |

---

## 6. Security Requirements

| ID | Category | Description | Implementation |
|----|----------|-------------|----------------|
| SR1 | Authentication | All shell pages require authenticated users. | `[Authorize]` at Pages scope + `AuthorizeRouteView` |
| SR2 | Authorization | Role-gated module pages enforce roles via `[Authorize(Roles=...)]` and navigation filtering. | Roles via Keycloak -> ClaimTypes.Role mapping |
| SR3 | Developer role gating | Developer module pages and diagnostics must not be accessible without the `developer` role. | `[Authorize(Roles = "developer")]` + navigation filtering |

---

## 7. Data Model Overview

_No new persistent data model entities are introduced by this work._

---

## 8. Deployment Strategy

_No environment-based feature gating is introduced by this work package._

---

## 9. Known Issues / Decisions Pending

| ID | Topic | Description | Status | Owner | Target Date |
|----|-------|-------------|--------|-------|-------------|
| KI1 | Development diagnostics content | Confirm exact diagnostics to include on Development page. | Pending | Product/Dev | TBD |
| KI2 | Unauthorized URL redirect | Confirm whether to redirect silently or show a message/toast before redirect. | Pending | Product/Dev | TBD |

