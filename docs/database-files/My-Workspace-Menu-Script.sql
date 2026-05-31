-- ============================================================================
-- MY WORKSPACE – Menu, Module & Workspace Config Scripts
-- ============================================================================
-- Schema       : press_db
-- Dependencies : mst_menu, mst_module, mst_user (existing tables)
-- Convention   : Follows existing naming/ID patterns from mst_menu.csv
-- ============================================================================
-- ID Allocation:
--   Top-level  : 14  (MY_WORKSPACE)        — menulevel 1
--   Level 2    : 1401–1405                  — menulevel 2
--   Level 3    : 14011–14033                — menulevel 3
--   module_id  : 14  (for all workspace menu items)
-- ============================================================================

BEGIN;

-- ────────────────────────────────────────────────────────────────────────────
-- 1. INSERT INTO mst_module  (top-level workspace module)
-- ────────────────────────────────────────────────────────────────────────────

INSERT INTO press_db.mst_module
    (module_id, module_code, module_name, parent_module_id, route_url, icon,
     display_order, is_mobile, is_web, is_active, module_level,
     is_section_header, section_name, badge_text, badge_class,
     has_divider_before, icon_svg, created_by, created_on)
VALUES
    (14, 'MY_WORKSPACE', 'My Workspace', NULL, '/workspace', 'layout-dashboard',
     0, true, true, true, 1,
     false, NULL, NULL, NULL,
     false,
     '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>',
     'SYSTEM', CURRENT_TIMESTAMP)
ON CONFLICT (module_code) DO NOTHING;


-- ────────────────────────────────────────────────────────────────────────────
-- 2. INSERT INTO mst_menu  (hierarchical workspace menu items)
-- ────────────────────────────────────────────────────────────────────────────

-- ── Level 1 : My Workspace (top-level parent) ──────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (14, 'MY_WORKSPACE', 'My Workspace', NULL, '/workspace', 'layout-dashboard',
     0, true, true, true, 1,
     false, NULL, NULL, NULL,
     false,
     '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>',
     14)
ON CONFLICT (menucode) DO NOTHING;


-- ── Level 2 : My Tasks ─────────────────────────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (1401, 'WS_MY_TASKS', 'My Tasks', 14, '/workspace/my-tasks', 'clipboard-list',
     1, true, true, true, 2,
     false, 'Tasks', NULL, NULL,
     false, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;

-- ── Level 3 : My Tasks → Pending Tasks ─────────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (14011, 'WS_PENDING_TASKS', 'Pending Tasks', 1401, '/workspace/my-tasks/pending', 'clock',
     1, true, true, true, 3,
     false, NULL, NULL, 'badge-warning',
     false, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;

-- ── Level 3 : My Tasks → Completed Tasks ───────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (14012, 'WS_COMPLETED_TASKS', 'Completed Tasks', 1401, '/workspace/my-tasks/completed', 'circle-check',
     2, true, true, true, 3,
     false, NULL, NULL, 'badge-success',
     false, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;

-- ── Level 3 : My Tasks → Assigned Tasks ────────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (14013, 'WS_ASSIGNED_TASKS', 'Assigned Tasks', 1401, '/workspace/my-tasks/assigned', 'user-check',
     3, true, true, true, 3,
     false, NULL, NULL, 'badge-info',
     false, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;


-- ── Level 2 : Approvals ────────────────────────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (1402, 'WS_APPROVALS', 'Approvals', 14, '/workspace/approvals', 'checkup-list',
     2, true, true, true, 2,
     false, 'Approvals', NULL, NULL,
     true, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;

-- ── Level 3 : Approvals → Pending Approvals ────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (14021, 'WS_PENDING_APPROVALS', 'Pending Approvals', 1402, '/workspace/approvals/pending', 'hourglass',
     1, true, true, true, 3,
     false, NULL, NULL, 'badge-warning',
     false, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;

-- ── Level 3 : Approvals → Approved ─────────────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (14022, 'WS_APPROVED', 'Approved', 1402, '/workspace/approvals/approved', 'thumb-up',
     2, true, true, true, 3,
     false, NULL, NULL, 'badge-success',
     false, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;

-- ── Level 3 : Approvals → Rejected ─────────────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (14023, 'WS_REJECTED', 'Rejected', 1402, '/workspace/approvals/rejected', 'thumb-down',
     3, true, true, true, 3,
     false, NULL, NULL, 'badge-danger',
     false, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;


-- ── Level 2 : Calendar ─────────────────────────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (1403, 'WS_CALENDAR', 'Calendar', 14, '/workspace/calendar', 'calendar',
     3, true, true, true, 2,
     false, 'Schedule', NULL, NULL,
     true, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;

-- ── Level 3 : Calendar → Daily View ────────────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (14031, 'WS_CALENDAR_DAILY', 'Daily View', 1403, '/workspace/calendar/daily', 'calendar-event',
     1, true, true, true, 3,
     false, NULL, NULL, NULL,
     false, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;

-- ── Level 3 : Calendar → Weekly View ───────────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (14032, 'WS_CALENDAR_WEEKLY', 'Weekly View', 1403, '/workspace/calendar/weekly', 'calendar-stats',
     2, true, true, true, 3,
     false, NULL, NULL, NULL,
     false, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;

-- ── Level 3 : Calendar → Monthly View ──────────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (14033, 'WS_CALENDAR_MONTHLY', 'Monthly View', 1403, '/workspace/calendar/monthly', 'calendar-time',
     3, true, true, true, 3,
     false, NULL, NULL, NULL,
     false, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;


-- ── Level 2 : Notifications ────────────────────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (1404, 'WS_NOTIFICATIONS', 'Notifications', 14, '/workspace/notifications', 'bell',
     4, true, true, true, 2,
     false, 'Activity', NULL, 'badge-danger',
     true, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;


-- ── Level 2 : My History ───────────────────────────────────────────────────

INSERT INTO press_db.mst_menu
    (menuid, menucode, menuname, parentmenuid, routeurl, icon,
     displayorder, ismobile, isweb, isactive, menulevel,
     issectionheader, sectionname, badgetext, badgeclass,
     hasdividerbefore, iconsvg, module_id)
VALUES
    (1405, 'WS_MY_HISTORY', 'My History', 14, '/workspace/my-history', 'history',
     5, true, true, true, 2,
     false, 'Activity', NULL, NULL,
     false, NULL, 14)
ON CONFLICT (menucode) DO NOTHING;


-- ────────────────────────────────────────────────────────────────────────────
-- 3. CREATE TABLE : mst_workspace_config
--    Per-user workspace preferences (widget layout, pinned items, filters)
-- ────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS press_db.mst_workspace_config
(
    config_id           bigserial       NOT NULL,
    user_id             bigint          NOT NULL,

    -- Widget / Panel visibility
    show_pending_tasks      boolean     NOT NULL DEFAULT true,
    show_completed_tasks    boolean     NOT NULL DEFAULT true,
    show_assigned_tasks     boolean     NOT NULL DEFAULT true,
    show_approvals          boolean     NOT NULL DEFAULT true,
    show_calendar           boolean     NOT NULL DEFAULT true,
    show_notifications      boolean     NOT NULL DEFAULT true,
    show_history            boolean     NOT NULL DEFAULT true,

    -- Calendar default view
    default_calendar_view   character varying(20) COLLATE pg_catalog."default"
                            NOT NULL DEFAULT 'WEEKLY'::character varying,
    -- Values: DAILY, WEEKLY, MONTHLY

    -- Task default filter
    default_task_filter     character varying(20) COLLATE pg_catalog."default"
                            NOT NULL DEFAULT 'PENDING'::character varying,
    -- Values: PENDING, COMPLETED, ASSIGNED, ALL

    -- Approval default filter
    default_approval_filter character varying(20) COLLATE pg_catalog."default"
                            NOT NULL DEFAULT 'PENDING'::character varying,
    -- Values: PENDING, APPROVED, REJECTED, ALL

    -- Notification preferences for workspace
    notify_on_task_assign       boolean NOT NULL DEFAULT true,
    notify_on_task_overdue      boolean NOT NULL DEFAULT true,
    notify_on_approval_request  boolean NOT NULL DEFAULT true,
    notify_on_approval_complete boolean NOT NULL DEFAULT true,

    -- Dashboard widget order (JSON array of widget codes)
    widget_order            jsonb       DEFAULT '["PENDING_TASKS","APPROVALS","CALENDAR","NOTIFICATIONS","HISTORY"]'::jsonb,

    -- Pinned / favourite items
    pinned_jobs             jsonb       DEFAULT '[]'::jsonb,
    pinned_processes        jsonb       DEFAULT '[]'::jsonb,

    -- History retention
    history_days            integer     NOT NULL DEFAULT 30,

    -- Auto-refresh interval (seconds, 0 = disabled)
    auto_refresh_seconds    integer     NOT NULL DEFAULT 60,

    -- Theme / layout
    compact_mode            boolean     NOT NULL DEFAULT false,
    items_per_page          integer     NOT NULL DEFAULT 20,

    -- Audit
    is_active               boolean     NOT NULL DEFAULT true,
    created_by              character varying(100) COLLATE pg_catalog."default",
    created_on              timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by             character varying(100) COLLATE pg_catalog."default",
    modified_on             timestamp without time zone,

    CONSTRAINT mst_workspace_config_pkey PRIMARY KEY (config_id),
    CONSTRAINT mst_workspace_config_user_id_key UNIQUE (user_id)
);

COMMENT ON TABLE press_db.mst_workspace_config
    IS 'Per-user workspace configuration. Controls widget visibility, default filters, calendar view, notification preferences, pinned items and layout options for the My Workspace dashboard.';

COMMENT ON COLUMN press_db.mst_workspace_config.default_calendar_view
    IS 'Default calendar view when workspace loads: DAILY, WEEKLY, MONTHLY';

COMMENT ON COLUMN press_db.mst_workspace_config.widget_order
    IS 'JSON array defining display order of workspace widgets: PENDING_TASKS, APPROVALS, CALENDAR, NOTIFICATIONS, HISTORY';

COMMENT ON COLUMN press_db.mst_workspace_config.pinned_jobs
    IS 'JSON array of job_ids that user has pinned for quick access on workspace';

COMMENT ON COLUMN press_db.mst_workspace_config.pinned_processes
    IS 'JSON array of process_ids pinned by user for quick navigation';

COMMENT ON COLUMN press_db.mst_workspace_config.auto_refresh_seconds
    IS 'Auto-refresh interval in seconds for workspace widgets. 0 disables auto-refresh.';


-- ── Foreign key: user_id → mst_user ────────────────────────────────────────

ALTER TABLE IF EXISTS press_db.mst_workspace_config
    ADD CONSTRAINT fk_workspace_config_user
    FOREIGN KEY (user_id)
    REFERENCES press_db.mst_user (userid) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

-- ── Index on user_id for fast lookup ───────────────────────────────────────

CREATE INDEX IF NOT EXISTS idx_workspace_config_user_id
    ON press_db.mst_workspace_config(user_id);


-- ────────────────────────────────────────────────────────────────────────────
-- 4. CREATE TABLE : trn_workspace_task_view
--    Consolidated view-backing table for user's workspace tasks
--    (materialised from multiple source tables for fast querying)
-- ────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS press_db.trn_workspace_task
(
    workspace_task_id   bigserial       NOT NULL,
    user_id             bigint          NOT NULL,

    -- Source reference
    source_table        character varying(50) COLLATE pg_catalog."default" NOT NULL,
    -- Values: trn_job, trn_enquiry, trn_quotation, trn_challan, trn_purchase_order, etc.
    source_id           bigint          NOT NULL,
    source_no           character varying(50) COLLATE pg_catalog."default",

    -- Task details
    task_type           character varying(30) COLLATE pg_catalog."default" NOT NULL,
    -- Values: TASK, APPROVAL, REVIEW, FOLLOW_UP
    task_status         character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'PENDING'::character varying,
    -- Values: PENDING, IN_PROGRESS, COMPLETED, OVERDUE, CANCELLED, REJECTED, APPROVED

    title               character varying(300) COLLATE pg_catalog."default" NOT NULL,
    description         text COLLATE pg_catalog."default",

    -- Process context
    process_id          integer,
    subprocess_id       integer,
    process_code        character varying(30) COLLATE pg_catalog."default",
    subprocess_code     character varying(30) COLLATE pg_catalog."default",
    department_id       integer,

    -- Assignment
    assigned_by         bigint,
    assigned_on         timestamp without time zone,

    -- SLA tracking
    priority            character varying(20) COLLATE pg_catalog."default" DEFAULT 'NORMAL'::character varying,
    -- Values: LOW, NORMAL, HIGH, URGENT, CRITICAL
    due_date            timestamp without time zone,
    sla_hours           numeric(8, 2),
    is_overdue          boolean NOT NULL DEFAULT false,

    -- Approval specific
    approval_type_id    integer,
    approval_level      integer,

    -- Completion
    completed_by        bigint,
    completed_on        timestamp without time zone,
    completion_remarks  text COLLATE pg_catalog."default",

    -- Navigation
    action_url          character varying(300) COLLATE pg_catalog."default",

    -- Job context (if applicable)
    job_id              bigint,
    job_no              character varying(50) COLLATE pg_catalog."default",
    party_name          character varying(200) COLLATE pg_catalog."default",

    -- Metadata
    metadata            jsonb DEFAULT '{}'::jsonb,

    -- Audit
    is_read             boolean NOT NULL DEFAULT false,
    read_at             timestamp without time zone,
    is_archived         boolean NOT NULL DEFAULT false,
    created_on          timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_on         timestamp without time zone,

    CONSTRAINT trn_workspace_task_pkey PRIMARY KEY (workspace_task_id)
);

COMMENT ON TABLE press_db.trn_workspace_task
    IS 'Consolidated workspace task table aggregating tasks, approvals and follow-ups from all ERP modules. Provides a single source for the My Workspace dashboard. Rows are created by triggers or application logic when tasks/approvals are assigned.';

COMMENT ON COLUMN press_db.trn_workspace_task.source_table
    IS 'Origin table: trn_job, trn_enquiry, trn_quotation, trn_challan, trn_purchase_order, trn_sales_invoice, etc.';

COMMENT ON COLUMN press_db.trn_workspace_task.task_type
    IS 'TASK: assigned work item, APPROVAL: pending approval request, REVIEW: QC/review item, FOLLOW_UP: CRM/payment follow-up';

COMMENT ON COLUMN press_db.trn_workspace_task.task_status
    IS 'PENDING, IN_PROGRESS, COMPLETED, OVERDUE, CANCELLED, REJECTED, APPROVED';


-- ── Foreign keys ───────────────────────────────────────────────────────────

ALTER TABLE IF EXISTS press_db.trn_workspace_task
    ADD CONSTRAINT fk_workspace_task_user
    FOREIGN KEY (user_id)
    REFERENCES press_db.mst_user (userid) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_workspace_task
    ADD CONSTRAINT fk_workspace_task_assigned_by
    FOREIGN KEY (assigned_by)
    REFERENCES press_db.mst_user (userid) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_workspace_task
    ADD CONSTRAINT fk_workspace_task_process
    FOREIGN KEY (process_id)
    REFERENCES press_db.mst_process (process_id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_workspace_task
    ADD CONSTRAINT fk_workspace_task_department
    FOREIGN KEY (department_id)
    REFERENCES press_db.mst_department (department_id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;


-- ── Indexes for workspace queries ──────────────────────────────────────────

CREATE INDEX IF NOT EXISTS idx_ws_task_user_status
    ON press_db.trn_workspace_task(user_id, task_status);

CREATE INDEX IF NOT EXISTS idx_ws_task_user_type
    ON press_db.trn_workspace_task(user_id, task_type);

CREATE INDEX IF NOT EXISTS idx_ws_task_user_priority
    ON press_db.trn_workspace_task(user_id, priority);

CREATE INDEX IF NOT EXISTS idx_ws_task_due_date
    ON press_db.trn_workspace_task(due_date)
    WHERE task_status IN ('PENDING', 'IN_PROGRESS');

CREATE INDEX IF NOT EXISTS idx_ws_task_overdue
    ON press_db.trn_workspace_task(user_id)
    WHERE is_overdue = true AND task_status NOT IN ('COMPLETED', 'CANCELLED');

CREATE INDEX IF NOT EXISTS idx_ws_task_source
    ON press_db.trn_workspace_task(source_table, source_id);

CREATE INDEX IF NOT EXISTS idx_ws_task_job
    ON press_db.trn_workspace_task(job_id)
    WHERE job_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_ws_task_created
    ON press_db.trn_workspace_task(user_id, created_on DESC);


COMMIT;


-- ════════════════════════════════════════════════════════════════════════════
-- MENU HIERARCHY SUMMARY
-- ════════════════════════════════════════════════════════════════════════════
--
-- My Workspace  (14)  │ menulevel=1 │ /workspace
-- │
-- ├── My Tasks  (1401) │ menulevel=2 │ /workspace/my-tasks
-- │     ├── Pending Tasks    (14011) │ menulevel=3 │ /workspace/my-tasks/pending
-- │     ├── Completed Tasks  (14012) │ menulevel=3 │ /workspace/my-tasks/completed
-- │     └── Assigned Tasks   (14013) │ menulevel=3 │ /workspace/my-tasks/assigned
-- │
-- ├── Approvals  (1402) │ menulevel=2 │ /workspace/approvals
-- │     ├── Pending Approvals (14021) │ menulevel=3 │ /workspace/approvals/pending
-- │     ├── Approved          (14022) │ menulevel=3 │ /workspace/approvals/approved
-- │     └── Rejected          (14023) │ menulevel=3 │ /workspace/approvals/rejected
-- │
-- ├── Calendar  (1403) │ menulevel=2 │ /workspace/calendar
-- │     ├── Daily View   (14031) │ menulevel=3 │ /workspace/calendar/daily
-- │     ├── Weekly View  (14032) │ menulevel=3 │ /workspace/calendar/weekly
-- │     └── Monthly View (14033) │ menulevel=3 │ /workspace/calendar/monthly
-- │
-- ├── Notifications  (1404) │ menulevel=2 │ /workspace/notifications
-- │
-- └── My History  (1405) │ menulevel=2 │ /workspace/my-history
--
-- ════════════════════════════════════════════════════════════════════════════
-- TABLES CREATED
-- ════════════════════════════════════════════════════════════════════════════
--
-- 1. mst_workspace_config  — Per-user workspace preferences
--    └── FK → mst_user(userid)
--
-- 2. trn_workspace_task    — Consolidated workspace task/approval feed
--    ├── FK → mst_user(userid)        [user_id]
--    ├── FK → mst_user(userid)        [assigned_by]
--    ├── FK → mst_process(process_id)
--    └── FK → mst_department(department_id)
--
-- ════════════════════════════════════════════════════════════════════════════
-- EXISTING TABLES REUSED (no changes needed)
-- ════════════════════════════════════════════════════════════════════════════
--
-- • trn_user_notification  — Powers "Notifications" tab
-- • trn_user_activity_log  — Powers "My History" tab
-- • mst_menu               — Navigation menu (rows inserted above)
-- • mst_module             — Module registry (row inserted above)
--
-- ════════════════════════════════════════════════════════════════════════════
