# Tenant Integration Guide for MinePress ERP

## 1. Goal
Integrate tenant logic into the ERP so one shared application can safely serve multiple clients, while ensuring strict data isolation.

---

## 2. Current Components in Your Solution

### 2.1 Tenant Library
Project: `src/erp.minepress.tenants`

Already includes:
- `ITenantResolver` + `DefaultTenantResolver`
- `ITenantContextAccessor` + `TenantContextAccessor`
- `ITenantSecurityService` + `TenantSecurityService`
- `ITenantManagementService` + `TenantManagementService`

### 2.2 Web App (Razor Pages)
Project: `src/erp.minepress.web`

Already includes:
- `TenantConnectionContextMiddleware`
- Tenant admin pages under `Pages/TenantAdmin/*`

### 2.3 Database Scripts
- Base tenant schema: `docs/erp-script/minepress-db-tenant.sql`
- Hardening script: `docs/erp-script/tenant-security-hardening.sql`

---

## 3. Integration Architecture

Use two logical zones:

1. **Control Plane** (Platform Admin)
- URL area: `/TenantAdmin`
- Manage tenants, connections, feature flags, API keys
- Only system admins

2. **Tenant Data Plane** (Main ERP)
- All business pages/API
- Tenant resolved per request
- Data access restricted by tenant context + DB policies

---

## 4. Request Flow (How Tenant Logic Works)

1. User/API request arrives
2. Authentication runs
3. `TenantContextMiddleware` resolves tenant from:
   - JWT claim (`tenant_key`)
   - `X-Tenant-Key` header (trusted/internal)
   - subdomain fallback
4. Middleware validates tenant activity and claim consistency
5. Middleware stores tenant in request context
   - `TenantId`
   - `TenantKey`
   - `TenantConnectionString`
6. Repositories/services query data for that tenant
7. DB RLS policies enforce tenant isolation

---

## 5. Startup Wiring (Required)

In `Program.cs`:

- Register tenant services:
  - `builder.Services.AddTenantServices(builder.Configuration);`
- Add middleware in pipeline after auth:
  - `app.UseAuthentication();`
  - `app.UseMiddleware<TenantConnectionContextMiddleware>();`
  - `app.UseAuthorization();`

- Configure connection strings centrally:
  - `ConnectionStrings:TenantCatalogConnection` in appsettings
  - avoid hardcoded defaults in code

Do this in both:
- `src/erp.minepress.web/Program.cs`
- `src/erp.minepress.webapi/Program.cs`

---

## 6. Data Isolation Rules

### 6.1 Application Layer
- Every tenant-owned query must filter by tenant id/key
- Never load cross-tenant data in service methods

### 6.2 Database Layer
Execute:
- `docs/erp-script/minepress-db-tenant.sql`
- `docs/erp-script/tenant-security-hardening.sql`

This creates tenant security tables and RLS policies.

### 6.3 Access Layer
- Reject request if tenant mismatch between token and resolved tenant
- Reject inactive/suspended tenant access

---

## 7. Tenant Admin Module Usage

Main pages:
- `/TenantAdmin/Tenants` (list)
- `/TenantAdmin/Tenants/Create` (create)
- `/TenantAdmin/Tenants/Details/{tenantId}` (update details)

From details page, admin can:
- Update tenant settings
- Add tenant connection
- Add/update feature flags
- Issue/revoke API keys
- Review security events

---

## 8. ERP Module Integration Pattern

For each ERP module (Enquiry, Job, Quotation, Accounting, etc.):

1. Inject `ITenantContextAccessor` in service
2. Read current tenant context
3. Validate tenant exists in context
4. Include tenant constraint in all reads/writes
5. Log tenant-aware audit/security events

Example flow:
- Create Job -> save `tenant_id`
- Query Jobs -> `WHERE tenant_id = currentTenantId`

---

## 9. JWT Integration Requirements

Ensure token includes:
- `tenant_id`
- `tenant_key`

During login/token issue:
- attach tenant claims
- enforce claim checks in middleware

---

## 10. API Key and Token Practices

Use tenant-managed credentials:
- Store hash for validation
- Encrypt key material at rest
- Set expiry and revoke support
- Track last usage and security events

Tables used:
- `tenant_api_credentials`
- `tenant_token_sessions`
- `tenant_security_events`

---

## 11. Deployment Sequence

1. Deploy DB scripts
2. Deploy tenant service project changes
3. Deploy web/webapi middleware changes
4. Configure appsettings secrets (master key, connection)
5. Create first tenant from `/TenantAdmin`
6. Test isolation with two tenants
7. Verify connection mode from diagnostics endpoint: `/api/tenant-diagnostics/connection`

---

## 12. Go-Live Validation Checklist

- [ ] Tenant A cannot read Tenant B data in UI
- [ ] Tenant A cannot read Tenant B data in API
- [ ] Tenant mismatch token is rejected
- [ ] Inactive tenant is blocked
- [ ] API key issue/revoke works per tenant
- [ ] Security events are logged
- [ ] RLS policies are active

---

## 13. Recommended Next Enhancements

1. Set DB session variable `app.tenant_id` per request for full RLS runtime enforcement
2. Add MFA for `/TenantAdmin` login
3. Add edit/delete options for connections/features
4. Add tenant user management (lock/unlock/reset)
5. Add per-tenant rate limit enforcement in middleware

---

## 14. Troubleshooting Notes

- If new interface methods show `ENC0023`, restart debug session (Edit-and-Continue limitation)
- PostgreSQL scripts may show SQL parser warnings in Visual Studio SQL tools (T-SQL parser mismatch)
- Validate actual execution directly on PostgreSQL server
- Use `/api/tenant-diagnostics/connection` to confirm whether app is using `tenant` or `catalog` connection mode
