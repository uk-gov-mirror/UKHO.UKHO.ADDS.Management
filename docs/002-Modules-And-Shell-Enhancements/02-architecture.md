# Architecture

- **Work Package**: `002-Modules-And-Shell-Enhancements`
- **Source spec**: `docs/002-Modules-And-Shell-Enhancements/00-overview.md`

## Overall Technical Approach
- **Stack**: .NET (targets include .NET 10/.NET 9) with **Blazor** shell hosted by `UKHO.ADDS.Management.Host`.
- **Module system**: Modules implement `IModule` and contribute navigation via `IModule.Pages` (tree of `ModulePage`).
- **Navigation composition**: `ModulePageService` aggregates module pages and will be enhanced to filter based on:
  - current user roles (RBAC)
  - module health state (unhealthy modules disabled)
- **Lifecycle orchestration**: A shell lifecycle orchestrator will call async lifecycle methods on modules sequentially when deployment/config changes occur.
- **JavaScript interop**: Use named functions under `wwwroot` for interop (no `eval`) to remain CSP-friendly.

## Frontend
- **Host page**: `src/Shell/UKHO.ADDS.Management.Host/Shell/App.razor` renders base HTML and includes scripts.
- **Shell components**:
  - Navigation UI consumes `ModulePageService.Pages`.
  - `DeploymentSelector` component emits changes via `SelectedDeploymentIdChanged` and is wired into a deployment context service by its parent.
- **Pages**:
  - Sample secure page: `sample/secure` (role: `showsamplepage`)
  - Access denied page: `/access-denied`

## Backend
- Primary logic resides in the Blazor Server app services rather than separate HTTP APIs for this iteration.
- **Services**:
  - `ModulePageService`: aggregates pages from `IModule.Pages`; filters pages by RBAC and health; caches per auth-state change.
  - `DeploymentContext`: shell-scoped state service for selected deployment id.
  - Configuration reload notifier: triggers on reload of local `configuration.json` (implementation depends on current loading mechanism).
  - Module lifecycle orchestrator: sequentially invokes module lifecycle callbacks; updates module health state.
- **Authorization**:
  - Page access: `[Authorize(Roles = ...)]` on pages.
  - UI access: `AuthorizeView` for role-gated button.
  - Unauthorized redirects to `/access-denied`.
  - Unhealthy module direct URL access redirects to `/`.
