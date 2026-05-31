# MinePress ERP - Tenant Management System Security Guide

## 1. Objective
Build a separate **Tenant Management System** (control portal) to manage all client tenants centrally:
- tenant onboarding/offboarding
- tenant database connections
- API keys and auto tokens
- feature flags and access controls
- audit and compliance tracking

Primary requirement: **no tenant can ever access another tenant's data**.

---

## 2. Recommended Architecture (Control Plane + Tenant Data Plane)

### 2.1 Control Plane (Admin Portal)
Use a dedicated application/module (recommended project: `erp.minepress.tenants`) for super-admin operations only.

Responsibilities:
- manage records in `minepress_db.tenants`
- manage `tenant_connections`, `tenant_encryption_keys`, `tenant_features`, `tenant_users`
- issue/rotate tenant API keys and tokens
- view centralized audit logs

### 2.2 Tenant Data Plane (Business Portal)
Your existing client-facing portal remains shared, but every request must run in a strict tenant context:
- tenant resolved from trusted source (domain/subdomain/JWT claim)
- DB connection selected by tenant
- data access filtered by tenant

### 2.3 Physical Isolation Levels
Support all three, based on client plan:
1. **Shared DB + shared schema + tenant_id filtering** (lowest cost, highest care)
2. **Shared DB + per-tenant schema** (better isolation)
3. **Dedicated DB per tenant** (highest isolation)

For high-value/regulated clients, prefer dedicated DB.

---

## 3. Existing Tables and Their Purpose

Your current SQL already has a strong base:
- `tenants`: master tenant profile + encrypted sensitive fields
- `tenant_connections`: multiple encrypted connections for HA/failover
- `tenant_encryption_keys`: key lifecycle/rotation support
- `tenant_features`: per-tenant feature flags/configuration
- `tenant_users`: tenant-scoped user mapping with encrypted PII
- `tenant_audit_logs`: compliance logging

Good design choices already present:
- UUID keys
- row-level security enabled
- FK constraints with cascade delete
- JSONB for flexible metadata/config

---

## 4. Critical Security Model (Must-Have)

### 4.1 Identity and Access
Create two security zones:
1. **Platform Admin Zone** (Tenant Management Portal)
   - only super admins
   - separate login endpoint/cookies/policy
2. **Tenant User Zone** (Normal ERP portal)
   - users belong to one tenant

Enforce:
- MFA for platform admins
- short session lifetime + sliding renewal
- lockout on failed attempts
- no shared admin and tenant accounts

### 4.2 Tenant Context Resolution
Resolve tenant in this order:
1. signed JWT claim (`tenant_id`, `tenant_key`)
2. verified subdomain mapping (`{tenant}.yourdomain.com`)
3. explicit header only for internal trusted APIs

Never trust query string for tenant identity.

### 4.3 Data Isolation Enforcement (Defense in Depth)
Apply all layers, not just one:
1. **Application layer**: every repository query filters by `tenant_id`
2. **Database layer**: RLS policies enforce `tenant_id`
3. **API layer**: middleware rejects cross-tenant resource access
4. **Token layer**: token tenant claim must match current tenant context

### 4.4 Encryption Strategy
- At rest: encrypted connection strings, admin email/phone/API key (already designed)
- In transit: TLS 1.2+
- Key handling:
  - use envelope encryption
  - keep master key outside DB (Azure Key Vault/HSM recommended)
  - rotate tenant keys periodically

### 4.5 Secrets and API Keys
- never store plain API keys or refresh tokens
- store hash for verification + encrypted full value only if retrieval is required
- rotate automatically (e.g., every 90 days)
- maintain key states: `active`, `expiring`, `revoked`
- include `last_used_at`, `created_by`, `revoked_by`

---

## 5. Recommended Extra Tables

Add these for a complete pro-grade implementation:

1. `tenant_api_credentials`
   - `tenant_id`, `key_hash`, `key_prefix`, `scopes`, `expires_at`, `last_used_at`, `is_active`
2. `tenant_token_sessions`
   - refresh token/session tracking, device info, IP, revoke support
3. `tenant_login_policies`
   - lockout policy, MFA required, password/session limits
4. `tenant_security_events`
   - suspicious logins, IP violations, token misuse
5. `tenant_rate_limits`
   - per-tenant API throttling

---

## 6. Row-Level Security Policies (PostgreSQL)

RLS is enabled; now enforce policies using session tenant id.

Pattern:
- set request tenant in DB session parameter: `app.tenant_id`
- policy checks `tenant_id = current_setting('app.tenant_id')::uuid`

Apply to every tenant-scoped table:
- `tenant_users`
- `tenant_features`
- business transaction tables (orders, jobs, invoices, etc.)

Important:
- platform-admin connection should be separate and tightly restricted
- tenant app role must not bypass RLS

---

## 7. Tenant Management Portal (Razor Pages) - Screen Blueprint

Create a separate area/app for super admins.

### 7.1 Authentication Screens
1. `Login`
2. `MFA Verify`
3. `Forgot Password`
4. `Reset Password`

### 7.2 Core Tenant Screens
1. `Tenant List`
   - status, plan, users, last activity, suspension state
2. `Create Tenant`
3. `Tenant Details`
4. `Edit Tenant`
5. `Suspend/Reactivate Tenant`

### 7.3 Security & Access Screens
1. `Tenant Connections`
   - add/edit/priority/failover test
2. `Encryption Keys`
   - rotate, activate/deactivate, expiry monitor
3. `API Keys & Tokens`
   - issue/revoke/rotate/view usage
4. `Tenant Users`
   - roles, lock/unlock, password reset, MFA enforce
5. `Feature Management`
   - enable/disable feature flags

### 7.4 Monitoring Screens
1. `Audit Logs`
2. `Security Events`
3. `Usage & Limits`
4. `Data Retention & Compliance`

---

## 8. API and Middleware Design

### 8.1 Middleware Pipeline (Business Portal)
1. authenticate token
2. resolve tenant context
3. validate token tenant claim == resolved tenant
4. set tenant context in request scope
5. set DB session variable `app.tenant_id`
6. continue request

### 8.2 Guard Rules
- reject if tenant not found/inactive/suspended
- reject if token tenant mismatch
- reject if IP restriction enabled and IP not allowed
- reject if plan limit exceeded (optional soft/hard enforcement)

---

## 9. Operational Security Controls

1. **Audit Everywhere**
   - all admin actions, key rotations, login failures, tenant status changes
2. **Backups**
   - encrypted backups + tested restore per tenant isolation model
3. **Monitoring**
   - alerts for unusual login, token spikes, cross-tenant query attempts
4. **Incident Response**
   - quick tenant isolation switch (suspend + revoke keys/tokens)
5. **Compliance**
   - retention and deletion workflows for GDPR-like requests

---

## 10. Tenant Lifecycle Process

### Onboarding
1. create tenant record
2. generate encryption key reference
3. store encrypted connection(s)
4. seed default features and policies
5. create admin user for tenant
6. issue initial API credential (optional)
7. write audit entries

### Offboarding / Suspension
1. suspend login and API keys
2. lock connections and rotate secrets
3. export/backup data if contract requires
4. anonymize/delete PII by policy
5. finalize audit trail

---

## 11. Implementation Phases

### Phase 1 - Foundation
- create dedicated tenant management login
- build tenant context middleware
- enforce tenant claim checks
- add RLS policies for all tenant tables

### Phase 2 - Security Hardening
- MFA for platform admins
- API key/token management module
- key rotation scheduler
- security event logging and alerts

### Phase 3 - Management Portal Completion
- Razor Pages for all tenant admin screens
- feature/plan limit controls
- audit dashboards and exports

### Phase 4 - Compliance & Automation
- retention automation
- incident playbooks
- penetration test and isolation validation

---

## 12. Validation Checklist (Go-Live)

A. Isolation tests
- [ ] tenant A cannot read tenant B data via UI
- [ ] tenant A cannot read tenant B data via API
- [ ] direct DB query from tenant role cannot bypass RLS

B. Credential security
- [ ] no plain API keys in DB logs/tables
- [ ] token revocation works instantly
- [ ] key rotation has zero-downtime path

C. Admin security
- [ ] MFA required for control portal
- [ ] all admin operations audited
- [ ] brute-force and lockout controls active

D. Recovery
- [ ] encrypted backup verified
- [ ] tenant-level restore tested

---

## 13. Suggested Next Step
After this document, implement in this order:
1. tenant management auth + admin role policies
2. tenant resolution middleware + strict claim validation
3. RLS policy rollout to all tenant-owned data tables
4. Razor Pages tenant management module (connections, keys, API tokens)

This sequence gives the fastest path to a secure and production-ready multi-tenant system.