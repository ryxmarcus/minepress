-- ============================================================
-- Table : press_db.trn_plate_making_entry
-- Purpose: Per-activity Plate Making progress for Workspace tasks
-- Mirrors : trn_design_work_entry with plate-specific columns
-- ============================================================

CREATE TABLE press_db.trn_plate_making_entry (
    plate_making_id     BIGSERIAL       NOT NULL,
    workspace_task_id   BIGINT          NOT NULL,
    job_id              BIGINT          NULL,

    -- Activity / item description
    activity_name       VARCHAR(300)    NOT NULL,
    activity_sequence   INT             NOT NULL DEFAULT 1,

    -- Part info (populated from hyb_job_rate_calculator productParts)
    part_name           VARCHAR(200)    NULL,

    -- Plate-specific fields
    plate_type          VARCHAR(100)    NULL,       -- e.g. CTP, Conventional, Violet, Thermal
    number_of_colors    INT             NOT NULL DEFAULT 0,
    number_of_plates    INT             NOT NULL DEFAULT 0,   -- plates required
    plates_made         INT             NOT NULL DEFAULT 0,   -- plates completed so far

    -- Computed: GREATEST(0, number_of_plates - plates_made)
    plates_pending      INT             GENERATED ALWAYS AS (GREATEST(0, number_of_plates - plates_made)) STORED,

    -- Status
    is_completed        BOOLEAN         NOT NULL DEFAULT FALSE,
    completed_on        TIMESTAMP       NULL,

    -- Notes
    notes               TEXT            NULL,

    -- Audit
    created_by          BIGINT          NULL,
    created_on          TIMESTAMP       NOT NULL DEFAULT now(),
    modified_by         BIGINT          NULL,
    modified_on         TIMESTAMP       NULL,

    CONSTRAINT pk_trn_plate_making_entry PRIMARY KEY (plate_making_id),
    CONSTRAINT fk_plate_making_task
        FOREIGN KEY (workspace_task_id)
        REFERENCES press_db.trn_workspace_task (workspace_task_id)
        ON DELETE CASCADE
);

-- Indexes
CREATE INDEX idx_plate_making_task_id ON press_db.trn_plate_making_entry (workspace_task_id);
CREATE INDEX idx_plate_making_job_id  ON press_db.trn_plate_making_entry (job_id);

-- Comments
COMMENT ON TABLE  press_db.trn_plate_making_entry IS 'Per-activity Plate Making progress captured in Workspace > PlateMaking page.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.plate_making_id    IS 'Surrogate primary key — auto-incremented.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.workspace_task_id  IS 'FK to trn_workspace_task.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.job_id             IS 'Denormalised FK to trn_job for fast reporting.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.activity_name      IS 'Plate making activity label e.g. Cover Plates, Text Plates.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.activity_sequence  IS 'Display/processing order within the task.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.part_name          IS 'Product part name from job config (e.g. Cover, Text).';
COMMENT ON COLUMN press_db.trn_plate_making_entry.plate_type         IS 'Plate technology: CTP, Conventional, Violet, Thermal, etc.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.number_of_colors   IS 'Number of ink colors for this activity.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.number_of_plates   IS 'Total plates to be made.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.plates_made        IS 'Plates finished so far.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.plates_pending     IS 'Computed: GREATEST(0, number_of_plates - plates_made).';
COMMENT ON COLUMN press_db.trn_plate_making_entry.is_completed       IS 'True when row-level Complete button clicked.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.completed_on       IS 'Timestamp when activity was marked completed.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.notes              IS 'Free-text work notes.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.created_by         IS 'User ID who created the record.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.created_on         IS 'Record creation timestamp.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.modified_by        IS 'User ID of last modifier.';
COMMENT ON COLUMN press_db.trn_plate_making_entry.modified_on        IS 'Last modification timestamp.';
