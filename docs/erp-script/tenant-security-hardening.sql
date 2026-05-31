BEGIN;

CREATE TABLE IF NOT EXISTS minepress_db.tenant_api_credentials
(
    id uuid NOT NULL DEFAULT gen_random_uuid(),
    tenant_id uuid NOT NULL,
    key_hash character varying(128) COLLATE pg_catalog."default" NOT NULL,
    key_prefix character varying(20) COLLATE pg_catalog."default" NOT NULL,
    encrypted_key character varying(700) COLLATE pg_catalog."default" NOT NULL,
    scopes jsonb NOT NULL DEFAULT '[]'::jsonb,
    expires_at timestamp with time zone NOT NULL,
    last_used_at timestamp with time zone,
    is_active boolean NOT NULL DEFAULT true,
    revoked_at timestamp with time zone,
    revoked_by character varying(100) COLLATE pg_catalog."default",
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by character varying(100) COLLATE pg_catalog."default",
    CONSTRAINT tenant_api_credentials_pkey PRIMARY KEY (id)
);

ALTER TABLE IF EXISTS minepress_db.tenant_api_credentials ENABLE ROW LEVEL SECURITY;

ALTER TABLE IF EXISTS minepress_db.tenant_api_credentials
    ADD CONSTRAINT fk_tenant_api_credentials_tenant FOREIGN KEY (tenant_id)
    REFERENCES minepress_db.tenants (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

CREATE INDEX IF NOT EXISTS idx_tenant_api_credentials_tenant
    ON minepress_db.tenant_api_credentials(tenant_id);

CREATE INDEX IF NOT EXISTS idx_tenant_api_credentials_key_hash
    ON minepress_db.tenant_api_credentials(key_hash);

CREATE TABLE IF NOT EXISTS minepress_db.tenant_token_sessions
(
    id uuid NOT NULL DEFAULT gen_random_uuid(),
    tenant_id uuid NOT NULL,
    user_id character varying(100) COLLATE pg_catalog."default" NOT NULL,
    refresh_token_hash character varying(128) COLLATE pg_catalog."default" NOT NULL,
    device_info character varying(300) COLLATE pg_catalog."default",
    source_ip inet,
    expires_at timestamp with time zone NOT NULL,
    revoked_at timestamp with time zone,
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT tenant_token_sessions_pkey PRIMARY KEY (id)
);

ALTER TABLE IF EXISTS minepress_db.tenant_token_sessions ENABLE ROW LEVEL SECURITY;

ALTER TABLE IF EXISTS minepress_db.tenant_token_sessions
    ADD CONSTRAINT fk_tenant_token_sessions_tenant FOREIGN KEY (tenant_id)
    REFERENCES minepress_db.tenants (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

CREATE INDEX IF NOT EXISTS idx_tenant_token_sessions_tenant
    ON minepress_db.tenant_token_sessions(tenant_id);

CREATE TABLE IF NOT EXISTS minepress_db.tenant_login_policies
(
    id uuid NOT NULL DEFAULT gen_random_uuid(),
    tenant_id uuid NOT NULL,
    max_failed_attempts integer NOT NULL DEFAULT 5,
    lockout_minutes integer NOT NULL DEFAULT 30,
    require_mfa boolean NOT NULL DEFAULT false,
    session_timeout_minutes integer NOT NULL DEFAULT 30,
    password_expiry_days integer NOT NULL DEFAULT 90,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone,
    CONSTRAINT tenant_login_policies_pkey PRIMARY KEY (id),
    CONSTRAINT uq_tenant_login_policies_tenant UNIQUE (tenant_id)
);

ALTER TABLE IF EXISTS minepress_db.tenant_login_policies ENABLE ROW LEVEL SECURITY;

ALTER TABLE IF EXISTS minepress_db.tenant_login_policies
    ADD CONSTRAINT fk_tenant_login_policies_tenant FOREIGN KEY (tenant_id)
    REFERENCES minepress_db.tenants (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

CREATE TABLE IF NOT EXISTS minepress_db.tenant_security_events
(
    id uuid NOT NULL DEFAULT gen_random_uuid(),
    tenant_id uuid NOT NULL,
    event_type character varying(100) COLLATE pg_catalog."default" NOT NULL,
    severity character varying(20) COLLATE pg_catalog."default" NOT NULL DEFAULT 'INFO'::character varying,
    description character varying(1000) COLLATE pg_catalog."default" NOT NULL,
    source_ip inet,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT tenant_security_events_pkey PRIMARY KEY (id)
);

ALTER TABLE IF EXISTS minepress_db.tenant_security_events ENABLE ROW LEVEL SECURITY;

ALTER TABLE IF EXISTS minepress_db.tenant_security_events
    ADD CONSTRAINT fk_tenant_security_events_tenant FOREIGN KEY (tenant_id)
    REFERENCES minepress_db.tenants (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

CREATE INDEX IF NOT EXISTS idx_tenant_security_events_tenant_created
    ON minepress_db.tenant_security_events(tenant_id, created_at DESC);

CREATE TABLE IF NOT EXISTS minepress_db.tenant_rate_limits
(
    id uuid NOT NULL DEFAULT gen_random_uuid(),
    tenant_id uuid NOT NULL,
    policy_name character varying(100) COLLATE pg_catalog."default" NOT NULL,
    requests_per_minute integer NOT NULL DEFAULT 60,
    burst_limit integer NOT NULL DEFAULT 120,
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone,
    CONSTRAINT tenant_rate_limits_pkey PRIMARY KEY (id),
    CONSTRAINT uq_tenant_rate_limit_policy UNIQUE (tenant_id, policy_name)
);

ALTER TABLE IF EXISTS minepress_db.tenant_rate_limits ENABLE ROW LEVEL SECURITY;

ALTER TABLE IF EXISTS minepress_db.tenant_rate_limits
    ADD CONSTRAINT fk_tenant_rate_limits_tenant FOREIGN KEY (tenant_id)
    REFERENCES minepress_db.tenants (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_policies
        WHERE schemaname = 'minepress_db' AND tablename = 'tenant_users' AND policyname = 'tenant_users_isolation_policy'
    ) THEN
        EXECUTE 'CREATE POLICY tenant_users_isolation_policy ON minepress_db.tenant_users USING (tenant_id = current_setting(''app.tenant_id'', true)::uuid)';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_policies
        WHERE schemaname = 'minepress_db' AND tablename = 'tenant_features' AND policyname = 'tenant_features_isolation_policy'
    ) THEN
        EXECUTE 'CREATE POLICY tenant_features_isolation_policy ON minepress_db.tenant_features USING (tenant_id = current_setting(''app.tenant_id'', true)::uuid)';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_policies
        WHERE schemaname = 'minepress_db' AND tablename = 'tenant_connections' AND policyname = 'tenant_connections_isolation_policy'
    ) THEN
        EXECUTE 'CREATE POLICY tenant_connections_isolation_policy ON minepress_db.tenant_connections USING (tenant_id = current_setting(''app.tenant_id'', true)::uuid)';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_policies
        WHERE schemaname = 'minepress_db' AND tablename = 'tenant_encryption_keys' AND policyname = 'tenant_encryption_keys_isolation_policy'
    ) THEN
        EXECUTE 'CREATE POLICY tenant_encryption_keys_isolation_policy ON minepress_db.tenant_encryption_keys USING (tenant_id = current_setting(''app.tenant_id'', true)::uuid)';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_policies
        WHERE schemaname = 'minepress_db' AND tablename = 'tenant_audit_logs' AND policyname = 'tenant_audit_logs_isolation_policy'
    ) THEN
        EXECUTE 'CREATE POLICY tenant_audit_logs_isolation_policy ON minepress_db.tenant_audit_logs USING (tenant_id = current_setting(''app.tenant_id'', true)::uuid)';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_policies
        WHERE schemaname = 'minepress_db' AND tablename = 'tenant_api_credentials' AND policyname = 'tenant_api_credentials_isolation_policy'
    ) THEN
        EXECUTE 'CREATE POLICY tenant_api_credentials_isolation_policy ON minepress_db.tenant_api_credentials USING (tenant_id = current_setting(''app.tenant_id'', true)::uuid)';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_policies
        WHERE schemaname = 'minepress_db' AND tablename = 'tenant_token_sessions' AND policyname = 'tenant_token_sessions_isolation_policy'
    ) THEN
        EXECUTE 'CREATE POLICY tenant_token_sessions_isolation_policy ON minepress_db.tenant_token_sessions USING (tenant_id = current_setting(''app.tenant_id'', true)::uuid)';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_policies
        WHERE schemaname = 'minepress_db' AND tablename = 'tenant_login_policies' AND policyname = 'tenant_login_policies_isolation_policy'
    ) THEN
        EXECUTE 'CREATE POLICY tenant_login_policies_isolation_policy ON minepress_db.tenant_login_policies USING (tenant_id = current_setting(''app.tenant_id'', true)::uuid)';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_policies
        WHERE schemaname = 'minepress_db' AND tablename = 'tenant_security_events' AND policyname = 'tenant_security_events_isolation_policy'
    ) THEN
        EXECUTE 'CREATE POLICY tenant_security_events_isolation_policy ON minepress_db.tenant_security_events USING (tenant_id = current_setting(''app.tenant_id'', true)::uuid)';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_policies
        WHERE schemaname = 'minepress_db' AND tablename = 'tenant_rate_limits' AND policyname = 'tenant_rate_limits_isolation_policy'
    ) THEN
        EXECUTE 'CREATE POLICY tenant_rate_limits_isolation_policy ON minepress_db.tenant_rate_limits USING (tenant_id = current_setting(''app.tenant_id'', true)::uuid)';
    END IF;
END
$$;

COMMIT;