# Functional Specification Document (FSD)

**Project**: UKHO ADDS Management Shell  
**Version**: v0.01  
**Date**: 2026-02-13  
**Author**: UKHO / Shell Team

---

## 1. Executive Summary

**Purpose**  
Improve baseline security and developer diagnostics within the ADDS Management Shell.

**Objective**  
- Ensure consistent authenticated user experience (default deployment selection, safe navigation fallbacks).
- Provide a development-only diagnostics page for users with the `developer` role.
- Improve behavior when users attempt to access unauthorized URLs directly.

---

## 2. System Overview

**Core Capabilities**  
- Authenticated shell experience backed by OIDC (Keycloak).
- Module navigation filtered by roles.
- Deployment selection available from the shell header.

**In Scope**  
- Login-time behaviour for deployment selection.
- A development-only diagnostics page within the Developer module for users with the `developer` role.
- Rename the existing Sample module to the Developer module (including `.csproj` rename) and role-gate it to `developer`.
- Consistent navigation outcome for unauthorized direct URL access.

**Out of Scope**  
- Redesign of the overall RBAC model.
- Changes to Keycloak realm configuration beyond what is required to support existing role-based access.

---

## 3. Architecture Overview

| Component | Description |
|-----------|-------------|
| **Architecture Style** | Modular Blazor Server UI hosted by `UKHO.ADDS.Management.Host` |
| **Communication Mechanisms** | Blazor Server UI; HTTP calls from UI using bearer token forwarding |
| **Authentication & Authorization** | OIDC (Keycloak) + cookie session; RBAC via role claims |
| **Hosting Platform** | .NET host (local Aspire orchestration during development) |

**Related component specs (this work package)**:
- `spec-shell-ui-security-diagnostics_v0.01.md`

