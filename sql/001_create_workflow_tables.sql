-- ============================================================================
-- MinePress ERP — Workflow Management Tables
-- Run against: minepress_db (PostgreSQL)
-- Schema: press_db
-- ============================================================================

-- 1. Workflow Template (links job type + product type to a workflow definition)
CREATE TABLE IF NOT EXISTS press_db.mst_workflow_template (
    workflow_template_id   BIGSERIAL PRIMARY KEY,
    workflow_code          VARCHAR(50)  NOT NULL,
    workflow_name          VARCHAR(200) NOT NULL,
    description            TEXT,
    job_type_id            INT          REFERENCES press_db.mst_job_type(jobtypeid),
    print_product_type_id  INT          REFERENCES press_db.mst_print_product_type(printproducttypeid),
    is_default             BOOLEAN      NOT NULL DEFAULT FALSE,
    version                INT          NOT NULL DEFAULT 1,
    is_active              BOOLEAN      NOT NULL DEFAULT TRUE,
    created_by             VARCHAR(100),
    created_on             TIMESTAMP    DEFAULT now(),
    modified_by            VARCHAR(100),
    modified_on            TIMESTAMP,
    CONSTRAINT uq_wf_template_code UNIQUE (workflow_code)
);

CREATE INDEX IF NOT EXISTS idx_wf_template_jobtype ON press_db.mst_workflow_template(job_type_id);
CREATE INDEX IF NOT EXISTS idx_wf_template_product ON press_db.mst_workflow_template(print_product_type_id);
CREATE INDEX IF NOT EXISTS idx_wf_template_active  ON press_db.mst_workflow_template(is_active) WHERE is_active = TRUE;

COMMENT ON TABLE press_db.mst_workflow_template IS 'Workflow definitions for job routing. Each template links a Job Type + Product Type to a sequence of steps.';

-- 2. Workflow Step (individual step in a workflow)
CREATE TABLE IF NOT EXISTS press_db.mst_workflow_step (
    workflow_step_id       BIGSERIAL PRIMARY KEY,
    workflow_template_id   BIGINT       NOT NULL REFERENCES press_db.mst_workflow_template(workflow_template_id) ON DELETE CASCADE,
    process_id             INT          REFERENCES press_db.mst_process(processid),
    sub_process_id         INT          REFERENCES press_db.mst_sub_process(subprocessid),
    step_code              VARCHAR(50)  NOT NULL,
    step_name              VARCHAR(200) NOT NULL,
    step_type              VARCHAR(30)  NOT NULL CHECK (step_type IN ('START','PROCESS','APPROVAL','TASK','NOTIFICATION','DECISION','END')),
    sequence_no            INT          NOT NULL DEFAULT 0,
    department_id          BIGINT       REFERENCES press_db.mst_department(dept_id),
    assigned_user_id       BIGINT       REFERENCES press_db.mst_user(userid),
    assignment_rule        VARCHAR(30)  CHECK (assignment_rule IN ('AUTO','MANUAL','ROUND_ROBIN','DEPT_HEAD')),
    approval_type_id       INT          REFERENCES press_db.mst_approval_type(approvaltypeid),
    approval_level_id      INT          REFERENCES press_db.mst_approval_level(approvallevelid),
    is_mandatory           BOOLEAN      NOT NULL DEFAULT FALSE,
    sla_hours              NUMERIC(8,2),
    escalate_after_hours   NUMERIC(8,2),
    escalate_to            VARCHAR(200),
    notify_vendor          BOOLEAN      NOT NULL DEFAULT FALSE,
    notify_supplier        BOOLEAN      NOT NULL DEFAULT FALSE,
    notify_customer        BOOLEAN      NOT NULL DEFAULT FALSE,
    notify_assigned_user   BOOLEAN      NOT NULL DEFAULT FALSE,
    notify_dept_head       BOOLEAN      NOT NULL DEFAULT FALSE,
    send_email             BOOLEAN      NOT NULL DEFAULT FALSE,
    send_sms               BOOLEAN      NOT NULL DEFAULT FALSE,
    send_whatsapp          BOOLEAN      NOT NULL DEFAULT FALSE,
    send_push_notification BOOLEAN      NOT NULL DEFAULT FALSE,
    canvas_x               DOUBLE PRECISION NOT NULL DEFAULT 0,
    canvas_y               DOUBLE PRECISION NOT NULL DEFAULT 0,
    node_color             VARCHAR(20),
    is_active              BOOLEAN      NOT NULL DEFAULT TRUE,
    created_by             VARCHAR(100),
    created_on             TIMESTAMP    DEFAULT now(),
    CONSTRAINT uq_wf_step_template_code UNIQUE (workflow_template_id, step_code)
);

CREATE INDEX IF NOT EXISTS idx_wf_step_template ON press_db.mst_workflow_step(workflow_template_id);
CREATE INDEX IF NOT EXISTS idx_wf_step_dept     ON press_db.mst_workflow_step(department_id);
CREATE INDEX IF NOT EXISTS idx_wf_step_type     ON press_db.mst_workflow_step(step_type);

COMMENT ON TABLE press_db.mst_workflow_step IS 'Individual steps within a workflow template. Each step has routing, assignment, notification, and visual position metadata.';

-- 3. Workflow Connection (links between steps)
CREATE TABLE IF NOT EXISTS press_db.mst_workflow_connection (
    connection_id          BIGSERIAL PRIMARY KEY,
    workflow_template_id   BIGINT       NOT NULL REFERENCES press_db.mst_workflow_template(workflow_template_id) ON DELETE CASCADE,
    from_step_id           BIGINT       NOT NULL REFERENCES press_db.mst_workflow_step(workflow_step_id) ON DELETE CASCADE,
    to_step_id             BIGINT       NOT NULL REFERENCES press_db.mst_workflow_step(workflow_step_id) ON DELETE CASCADE,
    condition_expression   TEXT,
    label                  VARCHAR(200),
    sequence_no            INT          NOT NULL DEFAULT 0,
    is_active              BOOLEAN      NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_wf_conn_template ON press_db.mst_workflow_connection(workflow_template_id);
CREATE INDEX IF NOT EXISTS idx_wf_conn_from     ON press_db.mst_workflow_connection(from_step_id);
CREATE INDEX IF NOT EXISTS idx_wf_conn_to       ON press_db.mst_workflow_connection(to_step_id);

COMMENT ON TABLE press_db.mst_workflow_connection IS 'Connections between workflow steps. Supports conditional branching via condition_expression.';
