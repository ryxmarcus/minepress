-- ============================================================================
-- MinePress ERP — Workflow Tables Seed Data (Direct INSERT Version)
-- Version: 1.0 - Simplified with explicit values (no subqueries)
-- Tables: mst_workflow_template, mst_workflow_step, mst_workflow_connection
-- Reference: docs/COMPLETE_WORKFLOW_IMPLEMENTATION_GUIDE.md
-- ============================================================================
-- 
-- USAGE INSTRUCTIONS:
-- 1. Run this script AFTER mst_process table has data (42 processes)
-- 2. Run this script AFTER mst_department table has data
-- 3. Execute in order: Template → Steps → Connections
--
-- PREREQUISITE CHECK:
-- Run: SELECT COUNT(*) FROM press_db.mst_process WHERE isactive = TRUE;
-- Expected result: 42 rows
-- ============================================================================

BEGIN;

-- ============================================================================
-- CLEANUP (Optional - uncomment if re-running)
-- ============================================================================
-- DELETE FROM press_db.mst_workflow_connection;
-- DELETE FROM press_db.mst_workflow_step;
-- DELETE FROM press_db.mst_workflow_template WHERE workflow_code = 'WF-DEFAULT-FULL';

-- ============================================================================
-- 1. INSERT WORKFLOW TEMPLATE
-- ============================================================================

INSERT INTO press_db.mst_workflow_template (
    workflow_code, workflow_name, description, job_type_id, print_product_type_id,
    is_default, version, is_active, created_by, created_on
)
VALUES (
    'WF-DEFAULT-FULL',
    'Default Full Workflow',
    'Complete 10-phase, 42-step workflow for printing ERP. Ref: COMPLETE_WORKFLOW_IMPLEMENTATION_GUIDE.md',
    NULL, NULL, TRUE, 1, TRUE, 'SYSTEM', CURRENT_TIMESTAMP
)
ON CONFLICT (workflow_code) DO NOTHING;

-- ============================================================================
-- 2. INSERT WORKFLOW STEPS (42 steps)
-- ============================================================================
-- Note: workflow_template_id is fetched via subquery
-- process_id values match mst_process.processid (1-42)

INSERT INTO press_db.mst_workflow_step (
    workflow_template_id, process_id, sub_process_id, step_code, step_name,
    step_type, sequence_no, department_id, assignment_rule, is_mandatory,
    sla_hours, escalate_after_hours, escalate_to,
    notify_vendor, notify_supplier, notify_customer, notify_assigned_user, notify_dept_head,
    send_email, send_sms, send_whatsapp, send_push_notification,
    canvas_x, canvas_y, node_color, is_active, created_by, created_on
)
SELECT 
    (SELECT workflow_template_id FROM press_db.mst_workflow_template WHERE workflow_code = 'WF-DEFAULT-FULL'),
    v.process_id, v.process_id, v.step_code, v.step_name,
    v.step_type, v.seq_no, v.dept_id, v.assign_rule, v.is_mand,
    v.sla_hrs, v.esc_hrs, v.esc_to,
    v.ntfy_vend, v.ntfy_supp, v.ntfy_cust, TRUE, v.ntfy_head,
    TRUE, FALSE, v.ntfy_cust, TRUE,
    v.cx, v.cy, v.color, TRUE, 'SYSTEM', CURRENT_TIMESTAMP
FROM (VALUES
-- Phase 1: Pre-Sales (seq 1-5)
(1,  'STEP_ADV_PAY',    'Advance Payment',        'APPROVAL', 1,  1004, 'AUTO',      FALSE, 4.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, FALSE, 50,   50,  '#3498db'),
(2,  'STEP_ENQ_JOB',    'Enquiry Creation',       'START',    2,  1007, 'AUTO',      TRUE,  2.00,  1.00, 'DEPT_HEAD', FALSE, FALSE, TRUE,  TRUE,  150,  50,  '#2ecc71'),
(3,  'STEP_ENQ_EST',    'Estimation / Costing',   'PROCESS',  3,  1008, 'AUTO',      TRUE,  6.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  250,  50,  '#f39c12'),
(4,  'STEP_QUOT',       'Quotation Generation',   'APPROVAL', 4,  1008, 'AUTO',      TRUE,  4.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, TRUE,  TRUE,  350,  50,  '#f39c12'),
(5,  'STEP_QUOT_APPR',  'Quotation Approval',     'APPROVAL', 5,  1001, 'DEPT_HEAD', FALSE, 8.00,  4.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  450,  50,  '#e74c3c'),
-- Phase 2: Job Creation (seq 6-7)
(6,  'STEP_JOB_CREATE', 'Job Creation',           'PROCESS',  6,  1007, 'AUTO',      TRUE,  2.00,  1.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  550,  50,  '#2ecc71'),
(7,  'STEP_JOB_APPR',   'Job Approval',           'APPROVAL', 7,  9999, 'MANUAL',    FALSE, 4.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, TRUE,  TRUE,  650,  50,  '#e74c3c'),
-- Phase 3: Design & Pre-Press (seq 8-10)
(8,  'STEP_DES_DTP',    'Designing / DTP',        'PROCESS',  8,  1009, 'AUTO',      TRUE,  12.00, 4.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  750,  50,  '#9b59b6'),
(9,  'STEP_PROOF',      'Client Proof',           'APPROVAL', 9,  1009, 'AUTO',      TRUE,  24.00, 8.00, 'DEPT_HEAD', FALSE, FALSE, TRUE,  TRUE,  850,  50,  '#9b59b6'),
(10, 'STEP_PRE_PRESS',  'Plate Making (CTP)',     'PROCESS',  10, 1009, 'AUTO',      TRUE,  6.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, FALSE, 950,  50,  '#9b59b6'),
-- Phase 4: Procurement (seq 11-14)
(11, 'STEP_PROC',       'Procurement',            'PROCESS',  11, 1015, 'AUTO',      FALSE, 8.00,  4.00, 'DEPT_HEAD', TRUE,  TRUE,  FALSE, FALSE, 1050, 50,  '#95a5a6'),
(12, 'STEP_GRN',        'Goods Receipt',          'PROCESS',  12, 1014, 'AUTO',      FALSE, 4.00,  2.00, 'DEPT_HEAD', FALSE, TRUE,  FALSE, FALSE, 1150, 50,  '#95a5a6'),
(13, 'STEP_QC_IN',      'Incoming QC',            'APPROVAL', 13, 1016, 'AUTO',      FALSE, 4.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  1250, 50,  '#95a5a6'),
(14, 'STEP_STORE_ISSUE','Material Issue',         'PROCESS',  14, 1014, 'AUTO',      TRUE,  2.00,  1.00, NULL,        FALSE, FALSE, FALSE, FALSE, 1350, 50,  '#1abc9c'),
-- Phase 5: Planning (seq 15-17)
(15, 'STEP_JOB_PLAN',   'Production Planning',    'PROCESS',  15, 1010, 'AUTO',      TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 50,   150, '#3498db'),
(16, 'STEP_JOB_SCHED',  'Job Scheduling',         'PROCESS',  16, 1010, 'AUTO',      TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 150,  150, '#3498db'),
(17, 'STEP_JOB_CARD',   'Job Card Issue',         'PROCESS',  17, 1010, 'AUTO',      TRUE,  2.00,  1.00, NULL,        FALSE, FALSE, FALSE, FALSE, 250,  150, '#3498db'),
-- Phase 6: Production (seq 18-21)
(18, 'STEP_CUT',        'Paper Cutting',          'PROCESS',  18, 1011, 'AUTO',      TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 350,  150, '#e67e22'),
(19, 'STEP_PRINT',      'Printing',               'PROCESS',  19, 1010, 'AUTO',      TRUE,  12.00, 4.00, NULL,        FALSE, FALSE, FALSE, FALSE, 450,  150, '#e67e22'),
(20, 'STEP_QC_PROC',    'In-Process QC',          'PROCESS',  20, 1016, 'AUTO',      TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, TRUE,  550,  150, '#e74c3c'),
(21, 'STEP_DRY',        'Drying / Curing',        'PROCESS',  21, 1010, 'AUTO',      TRUE,  6.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 650,  150, '#e67e22'),
-- Phase 7: Post-Press (seq 22-26)
(22, 'STEP_POST_PRESS', 'Post-Press',             'PROCESS',  22, 1011, 'AUTO',      TRUE,  8.00,  3.00, NULL,        FALSE, FALSE, FALSE, FALSE, 750,  150, '#8e44ad'),
(23, 'STEP_FOLD',       'Folding',                'PROCESS',  23, 1011, 'AUTO',      TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 850,  150, '#8e44ad'),
(24, 'STEP_BIND',       'Binding',                'PROCESS',  24, 1011, 'AUTO',      TRUE,  8.00,  3.00, NULL,        FALSE, FALSE, FALSE, FALSE, 950,  150, '#8e44ad'),
(25, 'STEP_TRIM',       'Final Trim',             'PROCESS',  25, 1011, 'AUTO',      TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 1050, 150, '#8e44ad'),
(26, 'STEP_QC_POST',    'Post-Press QC',          'APPROVAL', 26, 1016, 'AUTO',      TRUE,  4.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  1150, 150, '#e74c3c'),
-- Phase 8: Dispatch (seq 27-32)
(27, 'STEP_PACK',       'Packing',                'PROCESS',  27, 1012, 'AUTO',      TRUE,  3.00,  1.00, NULL,        FALSE, FALSE, FALSE, FALSE, 50,   250, '#16a085'),
(28, 'STEP_LOAD',       'Loading',                'PROCESS',  28, 1013, 'AUTO',      TRUE,  2.00,  1.00, NULL,        FALSE, FALSE, FALSE, FALSE, 150,  250, '#16a085'),
(29, 'STEP_CHALLAN',    'Delivery Challan',       'APPROVAL', 29, 1004, 'AUTO',      TRUE,  2.00,  1.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  250,  250, '#3498db'),
(30, 'STEP_GATE_PASS',  'Gate Pass',              'APPROVAL', 30, 1018, 'AUTO',      TRUE,  1.00,  0.50, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  350,  250, '#7f8c8d'),
(31, 'STEP_DISPATCH',   'Dispatch',               'PROCESS',  31, 1013, 'AUTO',      TRUE,  2.00,  1.00, NULL,        FALSE, FALSE, TRUE,  FALSE, 450,  250, '#16a085'),
(32, 'STEP_DEL_CONF',   'Delivery Confirmation',  'APPROVAL', 32, 1013, 'AUTO',      TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, TRUE,  FALSE, 550,  250, '#2ecc71'),
-- Phase 9: Billing (seq 33-36)
(33, 'STEP_BILL',       'Billing / Invoice',      'APPROVAL', 33, 1004, 'AUTO',      TRUE,  4.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, TRUE,  TRUE,  650,  250, '#3498db'),
(34, 'STEP_PAY_REC',    'Payment Receipt',        'APPROVAL', 34, 1004, 'AUTO',      TRUE,  48.00, 24.00,'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  750,  250, '#3498db'),
(35, 'STEP_CREDIT_NOTE','Credit Note',            'APPROVAL', 35, 1004, 'MANUAL',    FALSE, 8.00,  4.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  850,  250, '#e74c3c'),
(36, 'STEP_DEBIT_NOTE', 'Debit Note',             'APPROVAL', 36, 1004, 'MANUAL',    FALSE, 8.00,  4.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  950,  250, '#e74c3c'),
-- Phase 10: Closure (seq 37-42)
(37, 'STEP_STORE_RET',  'Material Return',        'PROCESS',  37, 1014, 'AUTO',      FALSE, 4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 50,   350, '#1abc9c'),
(38, 'STEP_WASTE_ENT',  'Wastage Entry',          'PROCESS',  38, 1014, 'AUTO',      FALSE, 4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 150,  350, '#1abc9c'),
(39, 'STEP_COST_FINAL', 'Final Costing',          'APPROVAL', 39, 1008, 'AUTO',      TRUE,  8.00,  4.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  250,  350, '#f39c12'),
(40, 'STEP_PROFIT',     'Profit Analysis',        'PROCESS',  40, 1004, 'AUTO',      TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, TRUE,  350,  350, '#3498db'),
(41, 'STEP_JOB_CLOSE',  'Job Closure',            'PROCESS',  41, 1007, 'AUTO',      TRUE,  2.00,  1.00, NULL,        FALSE, FALSE, TRUE,  FALSE, 450,  350, '#2ecc71'),
(42, 'STEP_JOB_ARCHIVE','Job Archive',            'END',      42, 1005, 'AUTO',      TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 550,  350, '#95a5a6')
) AS v(process_id, step_code, step_name, step_type, seq_no, dept_id, assign_rule, is_mand, sla_hrs, esc_hrs, esc_to, ntfy_vend, ntfy_supp, ntfy_cust, ntfy_head, cx, cy, color)
ON CONFLICT (workflow_template_id, step_code) DO NOTHING;

-- ============================================================================
-- 3. INSERT WORKFLOW CONNECTIONS
-- ============================================================================
-- Defines step-to-step flow with optional conditions

INSERT INTO press_db.mst_workflow_connection (
    workflow_template_id, from_step_id, to_step_id, condition_expression, label, sequence_no, is_active
)
SELECT
    wt.workflow_template_id,
    fs.workflow_step_id,
    ts.workflow_step_id,
    v.cond_expr,
    v.lbl,
    v.seq,
    TRUE
FROM press_db.mst_workflow_template wt
CROSS JOIN (VALUES
-- Pre-Sales → Job
('STEP_ADV_PAY',     'STEP_ENQ_JOB',     NULL,               'Advance → Enquiry',          1),
('STEP_ENQ_JOB',     'STEP_ENQ_EST',     NULL,               'Enquiry → Estimation',       1),
('STEP_ENQ_EST',     'STEP_QUOT',        NULL,               'Estimation → Quote',         1),
('STEP_QUOT',        'STEP_QUOT_APPR',   'requires_approval','Quote → Approval',           1),
('STEP_QUOT',        'STEP_JOB_CREATE',  'auto_approve',     'Quote → Job (Direct)',       2),
('STEP_QUOT_APPR',   'STEP_JOB_CREATE',  'approved==true',   'Approved → Job',             1),
-- Job → Design
('STEP_JOB_CREATE',  'STEP_JOB_APPR',    'manual_job',       'Manual Job → Approval',      1),
('STEP_JOB_CREATE',  'STEP_DES_DTP',     'quotation_job',    'Quotation Job → Design',     2),
('STEP_JOB_APPR',    'STEP_DES_DTP',     'approved==true',   'Approved → Design',          1),
-- Design & Pre-Press
('STEP_DES_DTP',     'STEP_PROOF',       NULL,               'Design → Proof',             1),
('STEP_PROOF',       'STEP_DES_DTP',     'rejected',         'Proof Rejected → Redesign',  2),
('STEP_PROOF',       'STEP_PRE_PRESS',   'approved==true',   'Proof Approved → CTP',       1),
('STEP_PRE_PRESS',   'STEP_STORE_ISSUE', NULL,               'CTP → Material Issue',       1),
-- Procurement (optional path)
('STEP_PROC',        'STEP_GRN',         NULL,               'Procurement → GRN',          1),
('STEP_GRN',         'STEP_QC_IN',       NULL,               'GRN → QC Inward',            1),
('STEP_QC_IN',       'STEP_PROC',        'rejected',         'QC Rejected → Re-Procure',   2),
('STEP_QC_IN',       'STEP_STORE_ISSUE', 'approved==true',   'QC Approved → Issue',        1),
-- Material → Production
('STEP_STORE_ISSUE', 'STEP_JOB_PLAN',    NULL,               'Issue → Planning',           1),
('STEP_JOB_PLAN',    'STEP_JOB_SCHED',   NULL,               'Plan → Schedule',            1),
('STEP_JOB_SCHED',   'STEP_JOB_CARD',    NULL,               'Schedule → Job Card',        1),
('STEP_JOB_CARD',    'STEP_CUT',         NULL,               'Job Card → Cutting',         1),
-- Production
('STEP_CUT',         'STEP_PRINT',       NULL,               'Cutting → Printing',         1),
('STEP_PRINT',       'STEP_QC_PROC',     NULL,               'Printing → QC',              1),
('STEP_QC_PROC',     'STEP_PRINT',       'rejected',         'QC Failed → Reprint',        2),
('STEP_QC_PROC',     'STEP_DRY',         'approved==true',   'QC Passed → Drying',         1),
('STEP_DRY',         'STEP_POST_PRESS',  NULL,               'Drying → Post-Press',        1),
-- Post-Press
('STEP_POST_PRESS',  'STEP_FOLD',        'requires_folding', 'Post → Folding',             1),
('STEP_POST_PRESS',  'STEP_BIND',        'requires_binding', 'Post → Binding',             2),
('STEP_POST_PRESS',  'STEP_TRIM',        'trim_only',        'Post → Trim Only',           3),
('STEP_FOLD',        'STEP_BIND',        'requires_binding', 'Folding → Binding',          1),
('STEP_FOLD',        'STEP_TRIM',        NULL,               'Folding → Trim',             2),
('STEP_BIND',        'STEP_TRIM',        NULL,               'Binding → Trim',             1),
('STEP_TRIM',        'STEP_QC_POST',     NULL,               'Trim → QC Post',             1),
('STEP_QC_POST',     'STEP_POST_PRESS',  'rejected',         'QC Failed → Redo',           2),
('STEP_QC_POST',     'STEP_PACK',        'approved==true',   'QC Passed → Packing',        1),
-- Dispatch
('STEP_PACK',        'STEP_LOAD',        NULL,               'Packing → Loading',          1),
('STEP_LOAD',        'STEP_CHALLAN',     NULL,               'Loading → Challan',          1),
('STEP_CHALLAN',     'STEP_GATE_PASS',   NULL,               'Challan → Gate Pass',        1),
('STEP_GATE_PASS',   'STEP_DISPATCH',    NULL,               'Gate Pass → Dispatch',       1),
('STEP_DISPATCH',    'STEP_DEL_CONF',    NULL,               'Dispatch → Delivery',        1),
-- Delivery → Billing
('STEP_DEL_CONF',    'STEP_CREDIT_NOTE', 'short_delivery',   'Short → Credit Note',        2),
('STEP_DEL_CONF',    'STEP_BILL',        'full_delivery',    'Delivered → Billing',        1),
('STEP_CREDIT_NOTE', 'STEP_BILL',        NULL,               'Credit Note → Billing',      1),
('STEP_DEBIT_NOTE',  'STEP_BILL',        NULL,               'Debit Note → Billing',       1),
-- Billing → Closure
('STEP_BILL',        'STEP_PAY_REC',     NULL,               'Billing → Payment',          1),
('STEP_PAY_REC',     'STEP_STORE_RET',   NULL,               'Payment → Return',           1),
('STEP_STORE_RET',   'STEP_WASTE_ENT',   NULL,               'Return → Wastage',           1),
('STEP_WASTE_ENT',   'STEP_COST_FINAL',  NULL,               'Wastage → Costing',          1),
('STEP_COST_FINAL',  'STEP_PROFIT',      NULL,               'Costing → Profit',           1),
('STEP_PROFIT',      'STEP_JOB_CLOSE',   NULL,               'Profit → Close',             1),
('STEP_JOB_CLOSE',   'STEP_JOB_ARCHIVE', NULL,               'Close → Archive',            1)
) AS v(from_code, to_code, cond_expr, lbl, seq)
JOIN press_db.mst_workflow_step fs ON fs.step_code = v.from_code AND fs.workflow_template_id = wt.workflow_template_id
JOIN press_db.mst_workflow_step ts ON ts.step_code = v.to_code AND ts.workflow_template_id = wt.workflow_template_id
WHERE wt.workflow_code = 'WF-DEFAULT-FULL';

COMMIT;

-- ============================================================================
-- VERIFICATION
-- ============================================================================
SELECT 'mst_workflow_template' AS table_name, COUNT(*) AS row_count FROM press_db.mst_workflow_template WHERE workflow_code = 'WF-DEFAULT-FULL'
UNION ALL
SELECT 'mst_workflow_step', COUNT(*) FROM press_db.mst_workflow_step ws JOIN press_db.mst_workflow_template wt ON ws.workflow_template_id = wt.workflow_template_id WHERE wt.workflow_code = 'WF-DEFAULT-FULL'
UNION ALL
SELECT 'mst_workflow_connection', COUNT(*) FROM press_db.mst_workflow_connection wc JOIN press_db.mst_workflow_template wt ON wc.workflow_template_id = wt.workflow_template_id WHERE wt.workflow_code = 'WF-DEFAULT-FULL';

-- Expected results:
-- mst_workflow_template: 1
-- mst_workflow_step: 42
-- mst_workflow_connection: 55
