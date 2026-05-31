-- ============================================================================
-- TABLE : trn_design_work_entry
-- Purpose : Captures Design / DTP progress entered in Workspace > DesignWork
--           page. One row per design activity per workspace task.
-- Schema  : press_db
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_design_work_entry
(
    -- ── Primary Key ──────────────────────────────────────────────────────
    design_work_id          bigserial       NOT NULL,

    -- ── Task / Job Context ───────────────────────────────────────────────
    workspace_task_id       bigint          NOT NULL,
    -- FK → press_db.trn_workspace_task.workspace_task_id

    job_id                  bigint,
    -- FK → press_db.trn_job.job_id (denormalised for fast reporting)

    -- ── Design Activity ──────────────────────────────────────────────────
    activity_name           character varying(300) COLLATE pg_catalog."default" NOT NULL,
    -- e.g. "Cover Design", "Text DTP", "Image Retouching"

    activity_sequence       integer         NOT NULL DEFAULT 1,
    -- display order within a task

    -- ── Page Tracking ────────────────────────────────────────────────────
    pages_required          integer         NOT NULL DEFAULT 0,
    pages_completed         integer         NOT NULL DEFAULT 0,
    pages_pending           integer
        GENERATED ALWAYS AS (GREATEST(0, pages_required - pages_completed)) STORED,
    -- auto-computed; never set directly

    -- ── Row Status ───────────────────────────────────────────────────────
    is_completed            boolean         NOT NULL DEFAULT false,
    -- true when the individual activity Complete button was clicked

    completed_on            timestamp without time zone,

    -- ── Notes ────────────────────────────────────────────────────────────
    notes                   text COLLATE pg_catalog."default",
    -- work notes from the right-sidebar textarea (shared across all rows of the same task)

    -- ── Audit ────────────────────────────────────────────────────────────
    created_by              bigint,
    created_on              timestamp without time zone NOT NULL DEFAULT now(),
    modified_by             bigint,
    modified_on             timestamp without time zone,

    -- ── Constraints ──────────────────────────────────────────────────────
    CONSTRAINT pk_trn_design_work_entry PRIMARY KEY (design_work_id),

    CONSTRAINT fk_design_work_task
        FOREIGN KEY (workspace_task_id)
        REFERENCES press_db.trn_workspace_task (workspace_task_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,

    CONSTRAINT chk_design_work_pages_required  CHECK (pages_required  >= 0),
    CONSTRAINT chk_design_work_pages_completed CHECK (pages_completed >= 0)
);

-- ── Comments ─────────────────────────────────────────────────────────────────

COMMENT ON TABLE  press_db.trn_design_work_entry                  IS 'Per-activity Design/DTP progress captured in Workspace > DesignWork page. One row per activity per workspace task.';
COMMENT ON COLUMN press_db.trn_design_work_entry.design_work_id   IS 'Surrogate primary key — auto-incremented.';
COMMENT ON COLUMN press_db.trn_design_work_entry.workspace_task_id IS 'FK to trn_workspace_task. Identifies the parent task.';
COMMENT ON COLUMN press_db.trn_design_work_entry.job_id            IS 'Denormalised FK to trn_job for fast reporting.';
COMMENT ON COLUMN press_db.trn_design_work_entry.activity_name     IS 'Design/DTP activity label e.g. Cover Design, Text DTP.';
COMMENT ON COLUMN press_db.trn_design_work_entry.activity_sequence IS 'Display/processing order of this activity within the task.';
COMMENT ON COLUMN press_db.trn_design_work_entry.pages_required    IS 'Total pages to be designed/DTPed for this activity.';
COMMENT ON COLUMN press_db.trn_design_work_entry.pages_completed   IS 'Pages finished so far — updated on Save Progress.';
COMMENT ON COLUMN press_db.trn_design_work_entry.pages_pending     IS 'Computed column: MAX(0, pages_required - pages_completed).';
COMMENT ON COLUMN press_db.trn_design_work_entry.is_completed      IS 'True when the row-level Complete button is clicked.';
COMMENT ON COLUMN press_db.trn_design_work_entry.completed_on      IS 'Timestamp when this activity was marked completed.';
COMMENT ON COLUMN press_db.trn_design_work_entry.notes             IS 'Free-text work notes entered in the right sidebar.';
COMMENT ON COLUMN press_db.trn_design_work_entry.created_by        IS 'User ID of the person who created this record.';
COMMENT ON COLUMN press_db.trn_design_work_entry.created_on        IS 'Record creation timestamp.';
COMMENT ON COLUMN press_db.trn_design_work_entry.modified_by       IS 'User ID of the last person who modified this record.';
COMMENT ON COLUMN press_db.trn_design_work_entry.modified_on       IS 'Last modification timestamp.';

-- ── Indexes ───────────────────────────────────────────────────────────────────

CREATE INDEX IF NOT EXISTS idx_design_work_task_id
    ON press_db.trn_design_work_entry (workspace_task_id);

CREATE INDEX IF NOT EXISTS idx_design_work_job_id
    ON press_db.trn_design_work_entry (job_id);

-- ── Sample SELECT to display the saved design work entries ────────────────────
-- Use this in any report / API endpoint to show the DesignWork table data.

SELECT
    d.design_work_id,
    d.workspace_task_id,
    t.title          AS task_title,
    t.job_no,
    t.party_name,
    d.job_id,
    d.activity_sequence,
    d.activity_name,
    d.pages_required,
    d.pages_completed,
    d.pages_pending,
    d.is_completed,
    d.completed_on,
    d.notes,
    d.created_by,
    d.created_on,
    d.modified_by,
    d.modified_on
FROM  press_db.trn_design_work_entry  d
JOIN  press_db.trn_workspace_task     t  ON t.workspace_task_id = d.workspace_task_id
ORDER BY d.workspace_task_id, d.activity_sequence, d.design_work_id;
