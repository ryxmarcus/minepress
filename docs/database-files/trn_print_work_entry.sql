-- ────────────────────────────────────────────────────────────────────────────
-- Table : trn_print_work_entry
-- Purpose: Stores per-part printing process inputs captured in the
--          Workspace/PrintWork page for tasks where ProcessCode = 'PRINT'
-- ────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS press_db.trn_print_work_entry
(
    print_work_id           BIGSERIAL       NOT NULL,
    workspace_task_id       BIGINT          NOT NULL,
    job_id                  BIGINT,

    -- Product part details
    part_name               VARCHAR(150)    NOT NULL,
    part_sequence           INTEGER         DEFAULT 0,

    -- Printing method: OFFSET | DIGITAL | SCREEN
    printing_method         VARCHAR(30),

    -- Machine assigned for this part
    machine_id              INTEGER,
    machine_name            VARCHAR(150),

    -- Printing parameters
    number_of_colors        INTEGER         DEFAULT 0,
    number_of_plates        INTEGER         DEFAULT 0,

    -- Sheet tracking
    total_sheets_required   INTEGER         DEFAULT 0,
    total_sheets_printed    INTEGER         DEFAULT 0,
    -- balance is computed: total_sheets_required - total_sheets_printed

    -- Row selection flag (checkbox in UI)
    is_selected             BOOLEAN         NOT NULL DEFAULT FALSE,

    -- Per-row start tracking
    is_started              BOOLEAN         NOT NULL DEFAULT FALSE,
    started_on              TIMESTAMP WITHOUT TIME ZONE,
    completed_on            TIMESTAMP WITHOUT TIME ZONE,

    notes                   TEXT,

    -- Audit
    created_by              BIGINT,
    created_on              TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    modified_by             BIGINT,
    modified_on             TIMESTAMP WITHOUT TIME ZONE,

    CONSTRAINT pk_trn_print_work_entry PRIMARY KEY (print_work_id)
);

-- Foreign key to workspace task
ALTER TABLE press_db.trn_print_work_entry
    ADD CONSTRAINT fk_print_work_task
    FOREIGN KEY (workspace_task_id)
    REFERENCES press_db.trn_workspace_task (workspace_task_id)
    ON DELETE CASCADE;

-- Indexes
CREATE INDEX IF NOT EXISTS idx_print_work_task_id
    ON press_db.trn_print_work_entry (workspace_task_id);

CREATE INDEX IF NOT EXISTS idx_print_work_job_id
    ON press_db.trn_print_work_entry (job_id);

-- Comments
COMMENT ON TABLE press_db.trn_print_work_entry IS
    'Per-part printing process inputs captured in Workspace PrintWork page. One row per product part per task.';

COMMENT ON COLUMN press_db.trn_print_work_entry.printing_method IS
    'Printing method selected for this part: OFFSET, DIGITAL, or SCREEN';

COMMENT ON COLUMN press_db.trn_print_work_entry.total_sheets_required IS
    'Total sheets required for this part (pre-filled from job specs)';

COMMENT ON COLUMN press_db.trn_print_work_entry.total_sheets_printed IS
    'Actual sheets printed so far — updated during execution';
