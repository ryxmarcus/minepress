-- =============================================================================
-- Seed: Workflow Templates and Steps for Print-Only Job Types
-- Job Types: PRINT_OFFSET(1010) PRINT_DIGITAL(1011) PRINT_SCREEN(1012) PRINT_FLEX(1013) PRINT_UV(1014)
-- Source: docs/erp-data/Pint-only-job-type-workspaces.txt
-- Steps: JOB_APPROVAL(7/9999) JOB_PLAN(15/1010) PRINT(19/1010) QC_PROC(20/1016)
--        CHALLAN(29/1004) GATE_PASS(30/1018) BILL(33/1004) PAY_REC(34/1004)
--        JOB_CLOSE(41/1007) JOB_ARCHIVE(42/1005)
-- Note: JOB_CREATE excluded - in PreJobProcesses, already done at trigger time.
-- =============================================================================

-- =============================================================================
-- Seed: Workflow Templates and Steps for Print-Only Job Types
-- Job Types: PRINT_OFFSET(1010) PRINT_DIGITAL(1011) PRINT_SCREEN(1012)
--            PRINT_FLEX(1013) PRINT_UV(1014)
-- Source: docs/erp-data/Pint-only-job-type-workspaces.txt
-- Steps: JOB_APPROVAL(7/9999) JOB_PLAN(15/1010) PRINT(19/1010) QC_PROC(20/1016)
--        CHALLAN(29/1004) GATE_PASS(30/1018) BILL(33/1004) PAY_REC(34/1004)
--        JOB_CLOSE(41/1007) JOB_ARCHIVE(42/1005)
-- Note: JOB_CREATE excluded - in PreJobProcesses, already done at trigger time.
-- =============================================================================

DO $$
DECLARE
    v_tid_offset  BIGINT;
    v_tid_digital BIGINT;
    v_tid_screen  BIGINT;
    v_tid_flex    BIGINT;
    v_tid_uv      BIGINT;
BEGIN

-- ── Templates ────────────────────────────────────────────────────────────────

INSERT INTO press_db.mst_workflow_template
    (workflow_code, workflow_name, description, job_type_id, print_product_type_id,
     is_default, version, is_active, created_by, created_on)
VALUES ('WF_PRINT_OFFSET', 'Printing Only - Offset',
    'Workflow for print-only offset jobs', 1010, NULL, FALSE, 1, TRUE, 'SYSTEM', NOW())
RETURNING workflow_template_id INTO v_tid_offset;

INSERT INTO press_db.mst_workflow_template
    (workflow_code, workflow_name, description, job_type_id, print_product_type_id,
     is_default, version, is_active, created_by, created_on)
VALUES ('WF_PRINT_DIGITAL', 'Printing Only - Digital',
    'Workflow for print-only digital jobs', 1011, NULL, FALSE, 1, TRUE, 'SYSTEM', NOW())
RETURNING workflow_template_id INTO v_tid_digital;

INSERT INTO press_db.mst_workflow_template
    (workflow_code, workflow_name, description, job_type_id, print_product_type_id,
     is_default, version, is_active, created_by, created_on)
VALUES ('WF_PRINT_SCREEN', 'Printing Only - Screen',
    'Workflow for print-only screen jobs', 1012, NULL, FALSE, 1, TRUE, 'SYSTEM', NOW())
RETURNING workflow_template_id INTO v_tid_screen;

INSERT INTO press_db.mst_workflow_template
    (workflow_code, workflow_name, description, job_type_id, print_product_type_id,
     is_default, version, is_active, created_by, created_on)
VALUES ('WF_PRINT_FLEX', 'Printing Only - Flex',
    'Workflow for print-only flex jobs', 1013, NULL, FALSE, 1, TRUE, 'SYSTEM', NOW())
RETURNING workflow_template_id INTO v_tid_flex;

INSERT INTO press_db.mst_workflow_template
    (workflow_code, workflow_name, description, job_type_id, print_product_type_id,
     is_default, version, is_active, created_by, created_on)
VALUES ('WF_PRINT_UV', 'Printing Only - UV',
    'Workflow for print-only UV jobs', 1014, NULL, FALSE, 1, TRUE, 'SYSTEM', NOW())
RETURNING workflow_template_id INTO v_tid_uv;

-- ── Steps (10 per template, fanned across all 5) ─────────────────────────────

INSERT INTO press_db.mst_workflow_step (
    workflow_template_id, process_id, step_code, step_name, step_type,
    sequence_no, department_id, assignment_rule, is_mandatory, sla_hours,
    is_blocking, applies_to_enquiry, applies_to_quotation, applies_to_job,
    notify_vendor, notify_supplier, notify_customer,
    notify_assigned_user, notify_dept_head,
    send_email, send_sms, send_whatsapp, send_push_notification,
    canvas_x, canvas_y, is_active, created_by, created_on
)
SELECT
    t.tid,
    s.process_id,
    s.step_code,
    s.step_name,
    s.step_type,
    s.sequence_no,
    s.department_id,
    s.assignment_rule,
    TRUE,    -- is_mandatory
    s.sla_hours,
    s.is_blocking,
    FALSE,   -- applies_to_enquiry  (print-only jobs start from job only)
    FALSE,   -- applies_to_quotation
    TRUE,    -- applies_to_job
    FALSE,   -- notify_vendor
    FALSE,   -- notify_supplier
    FALSE,   -- notify_customer
    TRUE,    -- notify_assigned_user
    TRUE,    -- notify_dept_head
    FALSE,   -- send_email
    FALSE,   -- send_sms
    FALSE,   -- send_whatsapp
    FALSE,   -- send_push_notification
    0,       -- canvas_x
    0,       -- canvas_y
    TRUE,    -- is_active
    'SYSTEM',
    NOW()
FROM (VALUES
    (v_tid_offset),
    (v_tid_digital),
    (v_tid_screen),
    (v_tid_flex),
    (v_tid_uv)
) AS t(tid)
CROSS JOIN (VALUES
--  seq  pid  step_code             step_name              step_type   dept   rule          sla  blocking
    ( 1,  7,  'S01_JOB_APPROVAL',  'Job Approval',        'APPROVAL', 9999,  'MANUAL',      4,  FALSE),
    ( 2, 15,  'S02_JOB_PLAN',      'Production Planning', 'TASK',     1010,  'AUTO',        8,  TRUE),
    ( 3, 19,  'S03_PRINT',         'Printing',            'TASK',     1010,  'AUTO',        24, TRUE),
    ( 4, 20,  'S04_QC_PROC',       'In-Process QC',       'TASK',     1016,  'AUTO',        4,  TRUE),
    ( 5, 29,  'S05_CHALLAN',       'Delivery Challan',    'TASK',     1004,  'AUTO',        4,  TRUE),
    ( 6, 30,  'S06_GATE_PASS',     'Gate Pass',           'TASK',     1018,  'AUTO',        2,  TRUE),
    ( 7, 33,  'S07_BILL',          'Billing',             'TASK',     1004,  'AUTO',        24, TRUE),
    ( 8, 34,  'S08_PAY_REC',       'Payment Receipt',     'TASK',     1004,  'AUTO',        48, TRUE),
    ( 9, 41,  'S09_JOB_CLOSE',     'Job Closure',         'TASK',     1007,  'AUTO',        8,  TRUE),
    (10, 42,  'S10_JOB_ARCHIVE',   'Job Archive',         'TASK',     1005,  'AUTO',        4,  TRUE)
) AS s(sequence_no, process_id, step_code, step_name, step_type,
       department_id, assignment_rule, sla_hours, is_blocking);

RAISE NOTICE 'Done. OFFSET=%, DIGITAL=%, SCREEN=%, FLEX=%, UV=%',
    v_tid_offset, v_tid_digital, v_tid_screen, v_tid_flex, v_tid_uv;

END $$;

-- =============================================================================
-- Verification — expect 5 rows each with step_count = 10
-- =============================================================================
SELECT
    t.workflow_template_id,
    t.workflow_code,
    t.job_type_id,
    COUNT(s.workflow_step_id) AS step_count
FROM   press_db.mst_workflow_template t
JOIN   press_db.mst_workflow_step s
       ON s.workflow_template_id = t.workflow_template_id
WHERE  t.workflow_code IN (
           'WF_PRINT_OFFSET', 'WF_PRINT_DIGITAL', 'WF_PRINT_SCREEN',
           'WF_PRINT_FLEX', 'WF_PRINT_UV')
GROUP  BY t.workflow_template_id, t.workflow_code, t.job_type_id
ORDER  BY t.job_type_id;
