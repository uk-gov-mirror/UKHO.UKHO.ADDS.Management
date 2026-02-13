# Authentication and authorization overview (Shell + Modules)

This document describes how authentication and authorization are currently applied in the ADDS Management **Shell Host** and the associated **Blazor modules**.

## Scope

- Host (Blazor Server): `src/Shell/UKHO.ADDS.Management.Host`
- Shell shared services (navigation filtering, health, configuration): `src/Shell/UKHO.ADDS.Management.Shell`
- Modules (examples):
  - Samples: `src/Modules/UKHO.ADDS.Management.Modules.Samples`
  - FileShare: `src/Modules/UKHO.ADDS.Management.Modules.FileShare`
  - Permit: `src/Modules/UKHO.ADDS.Management.Modules.Permit`

This is an implementation-focused overview (what the code does today), not a future RBAC model.

## Authentication (host)

Authentication is configured in `src/Shell/UKHO.ADDS.Management.Host/Program.cs`.

### Protocol and identity provider

- Uses **OpenID Connect**.
- Configured against **Keycloak**, realm `ADDSManagement`.
- OIDC authorization code flow (`ResponseType = code`).

### Schemes and session

- Default authentication scheme: `OpenIdConnectDefaults.AuthenticationScheme`.
- Uses a cookie session for sign-in:
  - `options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme`
  - Adds cookie auth via `.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)`
- Tokens are persisted to the auth session:
  - `options.SaveTokens = true`

### Scopes

- The OIDC client adds the scope `addsmanagement:all`.

### Blazor authentication state

- `builder.Services.AddCascadingAuthenticationState()` is used so Blazor components can access the current `AuthenticationState`.

## Login and logout endpoints

Login/logout endpoints are mapped in `src/Shell/UKHO.ADDS.Management.Host/Extensions/LoginLogoutEndpointRouteBuilderExtensions.cs` and activated from `Program.cs` via `app.MapLoginAndLogout()`.

### Routes

- `GET /authentication/login`
  - Issues an auth challenge.
  - Uses `RedirectUri = "/"` so successful login returns to the shell home.
  - Marked `AllowAnonymous()`.

- `GET /authentication/logout` and `POST /authentication/logout`
  - Signs out both:
    - `CookieAuthenticationDefaults.AuthenticationScheme`
    - `OpenIdConnectDefaults.AuthenticationScheme`
  - Uses `RedirectUri = "/"`.

## Authorization (host + UI routing)

### ASP.NET Core authorization services

- Authorization services are registered in `Program.cs` using `builder.Services.AddAuthorization()`.

> Note: No custom policies are configured in the host code shown; authorization decisions are currently driven primarily by per-page attributes (e.g. `[Authorize(Roles=...)]`) and Blazor’s built-in role checks.

### Route-level authorization

The shell router is implemented in `src/Shell/UKHO.ADDS.Management.Host/Shell/AppRouter.razor`.

- Routes are rendered via `AuthorizeRouteView`.
- When a route is not authorized, the router renders the `AccessDenied` UI within `ShellLayout`.

Access denied page UI:
- `src/Shell/UKHO.ADDS.Management.Host/Shell/Pages/AccessDenied.razor` (`@page "/access-denied"`)

## Authorization in navigation (module pages)

The left-hand navigation menu is populated from `ModulePageService` in the shared shell project.

### Page model

- `src/Shell/UKHO.ADDS.Management.Shell/Modules/ModulePage.cs`
- Each `ModulePage` can declare `RequiredRoles`.

`ModulePage.UserHasAccess(principal)` logic:
- If `RequiredRoles` is null/empty => access allowed.
- Otherwise => access allowed if the user is in **any** role listed (`any-of` semantics).

### Navigation filtering

- `src/Shell/UKHO.ADDS.Management.Shell/Services/ModulePageService.cs`
- Builds a full page list (`_allPages`) including a default `Home` entry (`Path = "/"`).
- Filters the full list to `_filteredPages` based on:
  1. `ModulePage.UserHasAccess(principal)` (role gating)
  2. Module health (`ModuleHealthService`) which can mark items disabled (unhealthy modules remain visible but disabled)

Effect:
- Users without roles should still see the shell and any pages that do not specify `RequiredRoles`.
- Pages with `RequiredRoles` are hidden from navigation for users lacking those roles.

## Module-level authorization patterns

### 1) Role-gated pages (route protection)

Modules can protect routes using the Blazor/ASP.NET Core `[Authorize]` attribute.

Example:
- `src/Modules/UKHO.ADDS.Management.Modules.Samples/Pages/SampleSecurePage.razor`
  - `@attribute [Microsoft.AspNetCore.Authorization.Authorize(Roles = "showsamplepage")]`

Expected behavior:
- If the user navigates directly to a secured route without the required role, the route is considered not authorized and the shell shows the access denied UI.

### 2) Role-gated UI elements (in-page)

Modules can hide/show UI fragments using `AuthorizeView`.

Example:
- `SampleSecurePage.razor`:
  - `<AuthorizeView Roles="showsamplebutton"> ... </AuthorizeView>`

Effect:
- The page can be accessible (or not) independently of whether certain buttons/sections render.

### 3) Navigation role gating (menu visibility)

In addition to protecting the route, modules should declare the required roles on the corresponding `ModulePage` so the nav item is hidden for users without the role.

Example:
- `src/Modules/UKHO.ADDS.Management.Modules.Samples/SampleModule.cs`
  - Defines a child navigation node:
    - `Path = "/sample/secure"`
    - `RequiredRoles = ["showsamplepage"]`

## Token forwarding for downstream calls

`src/Shell/UKHO.ADDS.Management.Host/Extensions/AuthorizationHandler.cs` implements a `DelegatingHandler` that:

- Reads the current user’s `access_token` from the ASP.NET Core auth session:
  - `httpContext.GetTokenAsync("access_token")`
- If present, forwards it as:
  - `Authorization: Bearer <token>`

This supports calling downstream HTTP APIs as the current user.

## Roles currently referenced in the repository

These roles are used as examples/demonstrators in the current codebase:

- `showsamplepage`
  - Used to protect the Sample secure page route and to hide/show its nav item.

- `showsamplebutton`
  - Used to hide/show a button within the secure sample page.

- `fileshareuser`
  - Used to gate the File Share module navigation entry (`/fileshare`).

- `permitserviceuser`
  - Used to gate the Permit module navigation entry (`/permit`).

See:
- `docs/002-Modules-And-Shell-Enhancements/sample-module-rbac-ui-spec.md`

## Configuration and secrets guidance

Configuration files such as `configuration.json` and `deployments.json` are intended to contain non-secret topology/deployment metadata.

Do not store secrets (credentials, connection strings, API tokens) in these files.

Reference:
- `docs/notes/configuration-json-guidance.md`
