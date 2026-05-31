-- ═══════════════════════════════════════════════════════════════════════════
-- trn_workspace_task_item — Item-level parallel task tracking
-- Supports: Design → CTP → PostPress per-item independent execution
-- Each job item gets its own task row per process stage, enabling parallel work.
-- ═══════════════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS press_db.trn_workspace_task_item
(
    task_item_id        bigserial       NOT NULL,
    workspace_task_id   bigint          NOT NULL,
    job_id              bigint          NOT NULL,
    job_item_id         bigint          NOT NULL,

    -- Process context
    process_code        character varying(30) COLLATE pg_catalog."default" NOT NULL,
    process_name        character varying(200) COLLATE pg_catalog."default",

    -- Item identity (denormalized for fast display)
    item_name           character varying(300) COLLATE pg_catalog."default" NOT NULL,
    item_description    text COLLATE pg_catalog."default",
    item_sequence       integer NOT NULL DEFAULT 1,

    -- Status tracking
    task_status         character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'NOT_STARTED'::character varying,
    -- Values: NOT_STARTED, RUNNING, COMPLETED, CLOSED

    -- Assignment
    assigned_user_id    bigint,
    assigned_on         timestamp without time zone,

    -- Execution timestamps
    started_on          timestamp without time zone,
    started_by          bigint,
    completed_on        timestamp without time zone,
    completed_by        bigint,

    -- Work data
    remarks             text COLLATE pg_catalog."default",
    work_data           jsonb DEFAULT '{}'::jsonb,

    -- Dependency: which upstream item task triggered this one
    parent_task_item_id bigint,

    -- Audit
    created_on          timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_on         timestamp without time zone,

    CONSTRAINT trn_workspace_task_item_pkey PRIMARY KEY (task_item_id)
);

-- ── Indexes ──
CREATE INDEX IF NOT EXISTS idx_wti_workspace_task ON press_db.trn_workspace_task_item (workspace_task_id);
CREATE INDEX IF NOT EXISTS idx_wti_job_item ON press_db.trn_workspace_task_item (job_id, job_item_id);
CREATE INDEX IF NOT EXISTS idx_wti_process_status ON press_db.trn_workspace_task_item (process_code, task_status);
CREATE INDEX IF NOT EXISTS idx_wti_assigned_user ON press_db.trn_workspace_task_item (assigned_user_id, task_status);

-- ── Foreign Keys ──
ALTER TABLE IF EXISTS press_db.trn_workspace_task_item
    ADD CONSTRAINT fk_wti_workspace_task
    FOREIGN KEY (workspace_task_id)
    REFERENCES press_db.trn_workspace_task (workspace_task_id) MATCH SIMPLE
    ON UPDATE NO ACTION ON DELETE CASCADE;

ALTER TABLE IF EXISTS press_db.trn_workspace_task_item
    ADD CONSTRAINT fk_wti_job
    FOREIGN KEY (job_id)
    REFERENCES press_db.trn_job (job_id) MATCH SIMPLE
    ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_workspace_task_item
    ADD CONSTRAINT fk_wti_job_item
    FOREIGN KEY (job_item_id)
    REFERENCES press_db.trn_job_item (job_item_id) MATCH SIMPLE
    ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_workspace_task_item
    ADD CONSTRAINT fk_wti_assigned_user
    FOREIGN KEY (assigned_user_id)
    REFERENCES press_db.mst_user (userid) MATCH SIMPLE
    ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_workspace_task_item
    ADD CONSTRAINT fk_wti_parent_task_item
    FOREIGN KEY (parent_task_item_id)
    REFERENCES press_db.trn_workspace_task_item (task_item_id) MATCH SIMPLE
    ON UPDATE NO ACTION ON DELETE SET NULL;

-- ── Comments ──
COMMENT ON TABLE press_db.trn_workspace_task_item
    IS 'Item-level task tracking for parallel execution. Each job item (e.g. Cover Page, Book Content) gets independent task rows per process (Design, CTP, PostPress), enabling simultaneous work across items.';

COMMENT ON COLUMN press_db.trn_workspace_task_item.task_status
    IS 'NOT_STARTED, RUNNING, COMPLETED, CLOSED — tracked per item independently';

COMMENT ON COLUMN press_db.trn_workspace_task_item.parent_task_item_id
    IS 'Links to the upstream item task that triggered this one (e.g. Design Cover → CTP Cover)';
