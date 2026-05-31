-- ============================================================================
-- MinePress ERP — Workflow Tables Complete Seed Data
-- Version: 1.0
-- Tables: mst_workflow_template, mst_workflow_step, mst_workflow_connection, mst_workspace_config
-- Reference: docs/COMPLETE_WORKFLOW_IMPLEMENTATION_GUIDE.md (42-process workflow)
-- ============================================================================

-- ╔════════════════════════════════════════════════════════════════════════════╗
-- ║  PREREQUISITE: Ensure mst_process table has all 42 processes               ║
-- ║  Reference: docs/erp-data/mst_process.csv                                  ║
-- ║  If mst_process is empty, run the INSERT below first.                      ║
-- ╚════════════════════════════════════════════════════════════════════════════╝

-- Uncomment and run this block if mst_process table is empty:
/*
INSERT INTO press_db.mst_process (processid, processcode, processname, description, departmentid, sequenceno, ismandatory, isapprovalrequired, isclientapproval, templatecode, templatename, isactive, createdby, createdon)
VALUES
(1,  'ADV_PAY',       'Advance Payment',         'Advance payment collection',               1004,  1, FALSE, TRUE,  FALSE, 'P_ADV',         'Advance Payment',       TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(2,  'ENQ_JOB',       'Enquiry Creation',        'Customer enquiry',                         1007,  2, TRUE,  TRUE,  TRUE,  'P_ENQ_JOB',     'Enquiry Workflow',      TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(3,  'ENQ_EST',       'Estimation / Costing',    'Job costing and estimation',               1008,  3, TRUE,  TRUE,  FALSE, 'P_EST',         'Estimation',            TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(4,  'QUOT',          'Quotation Generation',    'Quotation creation',                       1008,  4, TRUE,  TRUE,  TRUE,  'P_QUOT',        'Quotation',             TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(5,  'QUOT_APPR',     'Quotation Approval',      'Quotation approval',                       1001,  5, TRUE,  TRUE,  FALSE, 'P_QUOT_APPR',   'Quotation Approval',    TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(6,  'JOB_CREATE',    'Job Creation',            'Internal job creation after quotation',   1007,  6, TRUE,  TRUE,  FALSE, 'P_JOB_CREATE',  'Job Creation',          TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(7,  'JOB_APPROVAL',  'Job Approval',            'Approval for new created jobs',            9999,  7, FALSE, TRUE,  TRUE,  'P_PRE_DES',     'Job Approval',          TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(8,  'DES_DTP',       'Designing / DTP',         'Layout and artwork',                       1009,  8, TRUE,  TRUE,  FALSE, 'P_DESIGN',      'Design',                TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(9,  'PROOF',         'Client Proof Approval',   'Client artwork approval',                  1009,  9, TRUE,  TRUE,  TRUE,  'P_PROOF',       'Proof Approval',        TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(10, 'PRE_PRESS',     'Plate Making',            'CTP plate preparation',                    1009, 10, TRUE,  FALSE, FALSE, 'P_PRE_PRESS',   'Pre Press',             TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(11, 'PROC',          'Material Procurement',    'Material planning and purchase',           1015, 11, TRUE,  TRUE,  FALSE, 'P_PROC',        'Procurement',           TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(12, 'GRN',           'Goods Receipt Note',      'Material receiving',                       1014, 12, TRUE,  TRUE,  FALSE, 'P_GRN',         'Material Receipt',      TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(13, 'QC_IN',         'Incoming QC',             'Raw material quality check',               1016, 13, TRUE,  TRUE,  FALSE, 'P_QC_IN',       'Incoming QC',           TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(14, 'STORE_ISSUE',   'Material Issue',          'Material issue to production',             1014, 14, TRUE,  FALSE, FALSE, 'P_STORE_ISSUE', 'Material Issue',        TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(15, 'JOB_PLAN',      'Production Planning',     'Machine planning',                         1010, 15, TRUE,  FALSE, FALSE, 'P_PLAN',        'Planning',              TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(16, 'JOB_SCHED',     'Job Scheduling',          'Production scheduling',                    1010, 16, TRUE,  FALSE, FALSE, 'P_SCHED',       'Scheduling',            TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(17, 'JOB_CARD',      'Job Card Generation',     'Job card generation',                      1010, 17, TRUE,  FALSE, FALSE, 'P_JOB_CARD',    'Job Card',              TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(18, 'CUT',           'Paper Cutting',           'Paper sheet preparation',                  1011, 18, TRUE,  FALSE, FALSE, 'P_CUT',         'Cutting',               TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(19, 'PRINT',         'Printing',                'Printing operation',                       1010, 19, TRUE,  FALSE, FALSE, 'P_PRINT',       'Printing',              TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(20, 'QC_PROC',       'In-Process QC',           'Production quality check',                 1016, 20, TRUE,  FALSE, FALSE, 'P_QC_PROC',     'Process QC',            TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(21, 'DRY',           'Drying',                  'Ink drying',                               1010, 21, TRUE,  FALSE, FALSE, 'P_DRY',         'Drying',                TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(22, 'POST_PRESS',    'Post Press Finishing',    'Lamination and finishing',                 1011, 22, TRUE,  FALSE, FALSE, 'P_POST',        'Post Press',            TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(23, 'FOLD',          'Folding',                 'Sheet folding',                            1011, 23, TRUE,  FALSE, FALSE, 'P_FOLD',        'Folding',               TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(24, 'BIND',          'Binding',                 'Binding operation',                        1011, 24, TRUE,  FALSE, FALSE, 'P_BIND',        'Binding',               TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(25, 'TRIM',          'Final Trim',              'Final cutting',                            1011, 25, TRUE,  FALSE, FALSE, 'P_TRIM',        'Trimming',              TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(26, 'QC_POST',       'Post Press QC',           'Final QC after finishing',                 1016, 26, TRUE,  TRUE,  FALSE, 'P_QC_POST',     'Post QC',               TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(27, 'PACK',          'Packing',                 'Packing and labeling',                     1012, 27, TRUE,  FALSE, FALSE, 'P_PACK',        'Packing',               TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(28, 'LOAD',          'Loading',                 'Loading goods',                            1013, 28, TRUE,  FALSE, FALSE, 'P_LOAD',        'Loading',               TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(29, 'CHALLAN',       'Delivery Challan',        'Delivery challan generation',              1004, 29, TRUE,  TRUE,  FALSE, 'P_CHALLAN',     'Challan',               TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(30, 'GATE_PASS',     'Gate Pass',               'Gate clearance',                           1018, 30, TRUE,  TRUE,  FALSE, 'P_GATE',        'Gate Pass',             TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(31, 'DISPATCH',      'Dispatch',                'Dispatch operation',                       1013, 31, TRUE,  FALSE, FALSE, 'P_DISPATCH',    'Dispatch',              TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(32, 'DELIVERY_CONF', 'Delivery Confirmation',   'Customer delivery confirmation',           1013, 32, TRUE,  FALSE, TRUE,  'P_DELIVERY',    'Delivery Confirmation', TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(33, 'BILL',          'Billing',                 'Invoice generation',                       1004, 33, TRUE,  TRUE,  TRUE,  'P_BILL',        'Billing',               TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(34, 'PAY_REC',       'Payment Receipt',         'Customer payment receipt',                 1004, 34, TRUE,  TRUE,  FALSE, 'P_PAY_REC',     'Payment Receipt',       TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(35, 'CREDIT_NOTE',   'Credit Note',             'Credit adjustment',                        1004, 35, FALSE, TRUE,  FALSE, 'P_CREDIT',      'Credit Note',           TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(36, 'DEBIT_NOTE',    'Debit Note',              'Debit adjustment',                         1004, 36, FALSE, TRUE,  FALSE, 'P_DEBIT',       'Debit Note',            TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(37, 'STORE_RETURN',  'Material Return',         'Return unused material',                   1014, 37, FALSE, FALSE, FALSE, 'P_RETURN',      'Material Return',       TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(38, 'WASTE_ENTRY',   'Wastage Entry',           'Production wastage record',                1014, 38, FALSE, FALSE, FALSE, 'P_WASTE',       'Wastage',               TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(39, 'COST_FINAL',    'Final Costing',           'Final job costing',                        1008, 39, TRUE,  TRUE,  FALSE, 'P_COST',        'Final Costing',         TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(40, 'PROFIT_ANALYSIS','Profit Analysis',        'Job profitability',                        1004, 40, TRUE,  FALSE, FALSE, 'P_PROFIT',      'Profit Analysis',       TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(41, 'JOB_CLOSE',     'Job Closure',             'Close job',                                1007, 41, TRUE,  FALSE, FALSE, 'P_CLOSE',       'Job Close',             TRUE, 'SYSTEM', CURRENT_TIMESTAMP),
(42, 'JOB_ARCHIVE',   'Job Archive',             'Archive job data',                         1005, 42, TRUE,  FALSE, FALSE, 'P_ARCHIVE',     'Archive',               TRUE, 'SYSTEM', CURRENT_TIMESTAMP)
ON CONFLICT (processid) DO NOTHING;
*/

-- ============================================================================
-- STEP 1: INSERT mst_workflow_template (Default Workflow Template)
-- ============================================================================
-- This creates ONE default workflow template that applies to all job types.
-- Additional templates per job_type_id can be added later.

INSERT INTO press_db.mst_workflow_template (
    workflow_code,
    workflow_name,
    description,
    job_type_id,
    print_product_type_id,
    is_default,
    version,
    is_active,
    created_by,
    created_on
)
VALUES (
    'WF-DEFAULT-FULL',
    'Default Full Workflow',
    'Complete 10-phase, 42-step workflow for printing ERP. Covers Enquiry → Job → Production → Dispatch → Billing → Closure. Reference: COMPLETE_WORKFLOW_IMPLEMENTATION_GUIDE.md',
    NULL,  -- NULL = applies to all job types as default
    NULL,  -- NULL = applies to all product types
    TRUE,  -- is_default = TRUE (fallback template)
    1,     -- version
    TRUE,  -- is_active
    'SYSTEM',
    CURRENT_TIMESTAMP
);

-- ============================================================================
-- STEP 2: INSERT mst_workflow_step (42 Workflow Steps)
-- ============================================================================
-- Each step maps to a process in mst_process via process_id (FK)
-- Using subqueries to resolve process_id from process_code
-- Department IDs reference: docs/COMPLETE_WORKFLOW_IMPLEMENTATION_GUIDE.md Section 3

INSERT INTO press_db.mst_workflow_step (
    workflow_template_id,
    process_id,
    sub_process_id,
    step_code,
    step_name,
    step_type,
    sequence_no,
    department_id,
    assigned_user_id,
    assignment_rule,
    approval_type_id,
    approval_level_id,
    is_mandatory,
    sla_hours,
    escalate_after_hours,
    escalate_to,
    notify_vendor,
    notify_supplier,
    notify_customer,
    notify_assigned_user,
    notify_dept_head,
    send_email,
    send_sms,
    send_whatsapp,
    send_push_notification,
    canvas_x,
    canvas_y,
    node_color,
    is_active,
    created_by,
    created_on
)
SELECT
    wt.workflow_template_id,
    p.processid,
    p.processid,  -- sub_process_id same as process_id for main processes
    steps.step_code,
    steps.step_name,
    steps.step_type,
    steps.sequence_no,
    steps.department_id,
    NULL,  -- assigned_user_id (resolved at runtime)
    steps.assignment_rule,
    NULL,  -- approval_type_id
    NULL,  -- approval_level_id
    steps.is_mandatory,
    steps.sla_hours,
    steps.escalate_hours,
    steps.escalate_to,
    steps.notify_vendor,
    steps.notify_supplier,
    steps.notify_customer,
    TRUE,  -- notify_assigned_user
    steps.notify_dept_head,
    TRUE,  -- send_email
    FALSE, -- send_sms
    steps.notify_customer,  -- send_whatsapp (same as customer flag)
    TRUE,  -- send_push_notification
    steps.canvas_x,
    steps.canvas_y,
    steps.node_color,
    TRUE,  -- is_active
    'SYSTEM',
    CURRENT_TIMESTAMP
FROM press_db.mst_workflow_template wt
CROSS JOIN (VALUES
    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 1: Pre-Sales & Customer Initiation (seq 1-5)
    -- ══════════════════════════════════════════════════════════════════════════════
    ('ADV_PAY',    'STEP_ADV_PAY',    'Advance Payment',         'APPROVAL',  1,  1004, 'AUTO',       FALSE, 4.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, FALSE, 50,   50,  '#3498db'),
    ('ENQ_JOB',    'STEP_ENQ_JOB',    'Enquiry Creation',        'START',     2,  1007, 'AUTO',       TRUE,  2.00,  1.00, 'DEPT_HEAD', FALSE, FALSE, TRUE,  TRUE,  150,  50,  '#2ecc71'),
    ('ENQ_EST',    'STEP_ENQ_EST',    'Estimation / Costing',    'PROCESS',   3,  1008, 'AUTO',       TRUE,  6.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  250,  50,  '#f39c12'),
    ('QUOT',       'STEP_QUOT',       'Quotation Generation',    'APPROVAL',  4,  1008, 'AUTO',       TRUE,  4.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, TRUE,  TRUE,  350,  50,  '#f39c12'),
    ('QUOT_APPR',  'STEP_QUOT_APPR',  'Quotation Approval',      'APPROVAL',  5,  1001, 'DEPT_HEAD',  FALSE, 8.00,  4.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  450,  50,  '#e74c3c'),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 2: Job Creation & Approval (seq 6-7)
    -- ══════════════════════════════════════════════════════════════════════════════
    ('JOB_CREATE', 'STEP_JOB_CREATE', 'Job Creation',            'PROCESS',   6,  1007, 'AUTO',       TRUE,  2.00,  1.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  550,  50,  '#2ecc71'),
    ('JOB_APPROVAL','STEP_JOB_APPR',  'Job Approval',            'APPROVAL',  7,  9999, 'MANUAL',     FALSE, 4.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, TRUE,  TRUE,  650,  50,  '#e74c3c'),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 3: Design & Pre-Press (seq 8-10)
    -- ══════════════════════════════════════════════════════════════════════════════
    ('DES_DTP',    'STEP_DES_DTP',    'Designing / DTP',         'PROCESS',   8,  1009, 'AUTO',       TRUE,  12.00, 4.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  750,  50,  '#9b59b6'),
    ('PROOF',      'STEP_PROOF',      'Client Proof',            'APPROVAL',  9,  1009, 'AUTO',       TRUE,  24.00, 8.00, 'DEPT_HEAD', FALSE, FALSE, TRUE,  TRUE,  850,  50,  '#9b59b6'),
    ('PRE_PRESS',  'STEP_PRE_PRESS',  'Plate Making (CTP)',      'PROCESS',   10, 1009, 'AUTO',       TRUE,  6.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, FALSE, 950,  50,  '#9b59b6'),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 4: Procurement & Material Handling (seq 11-14) - Partially Disabled
    -- ══════════════════════════════════════════════════════════════════════════════
    ('PROC',       'STEP_PROC',       'Procurement',             'PROCESS',   11, 1015, 'AUTO',       FALSE, 8.00,  4.00, 'DEPT_HEAD', TRUE,  TRUE,  FALSE, FALSE, 1050, 50,  '#95a5a6'),
    ('GRN',        'STEP_GRN',        'Goods Receipt',           'PROCESS',   12, 1014, 'AUTO',       FALSE, 4.00,  2.00, 'DEPT_HEAD', FALSE, TRUE,  FALSE, FALSE, 1150, 50,  '#95a5a6'),
    ('QC_IN',      'STEP_QC_IN',      'Incoming QC',             'APPROVAL',  13, 1016, 'AUTO',       FALSE, 4.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  1250, 50,  '#95a5a6'),
    ('STORE_ISSUE','STEP_STORE_ISSUE','Material Issue',          'PROCESS',   14, 1014, 'AUTO',       TRUE,  2.00,  1.00, NULL,        FALSE, FALSE, FALSE, FALSE, 1350, 50,  '#1abc9c'),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 5: Production Planning (seq 15-17)
    -- ══════════════════════════════════════════════════════════════════════════════
    ('JOB_PLAN',   'STEP_JOB_PLAN',   'Production Planning',     'PROCESS',   15, 1010, 'AUTO',       TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 50,   150, '#3498db'),
    ('JOB_SCHED',  'STEP_JOB_SCHED',  'Job Scheduling',          'PROCESS',   16, 1010, 'AUTO',       TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 150,  150, '#3498db'),
    ('JOB_CARD',   'STEP_JOB_CARD',   'Job Card Issue',          'PROCESS',   17, 1010, 'AUTO',       TRUE,  2.00,  1.00, NULL,        FALSE, FALSE, FALSE, FALSE, 250,  150, '#3498db'),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 6: Production Execution (seq 18-21)
    -- ══════════════════════════════════════════════════════════════════════════════
    ('CUT',        'STEP_CUT',        'Paper Cutting',           'PROCESS',   18, 1011, 'AUTO',       TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 350,  150, '#e67e22'),
    ('PRINT',      'STEP_PRINT',      'Printing',                'PROCESS',   19, 1010, 'AUTO',       TRUE,  12.00, 4.00, NULL,        FALSE, FALSE, FALSE, FALSE, 450,  150, '#e67e22'),
    ('QC_PROC',    'STEP_QC_PROC',    'In-Process QC',           'PROCESS',   20, 1016, 'AUTO',       TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, TRUE,  550,  150, '#e74c3c'),
    ('DRY',        'STEP_DRY',        'Drying / Curing',         'PROCESS',   21, 1010, 'AUTO',       TRUE,  6.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 650,  150, '#e67e22'),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 7: Post-Press Finishing (seq 22-26)
    -- ══════════════════════════════════════════════════════════════════════════════
    ('POST_PRESS', 'STEP_POST_PRESS', 'Post-Press',              'PROCESS',   22, 1011, 'AUTO',       TRUE,  8.00,  3.00, NULL,        FALSE, FALSE, FALSE, FALSE, 750,  150, '#8e44ad'),
    ('FOLD',       'STEP_FOLD',       'Folding',                 'PROCESS',   23, 1011, 'AUTO',       TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 850,  150, '#8e44ad'),
    ('BIND',       'STEP_BIND',       'Binding',                 'PROCESS',   24, 1011, 'AUTO',       TRUE,  8.00,  3.00, NULL,        FALSE, FALSE, FALSE, FALSE, 950,  150, '#8e44ad'),
    ('TRIM',       'STEP_TRIM',       'Final Trim',              'PROCESS',   25, 1011, 'AUTO',       TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 1050, 150, '#8e44ad'),
    ('QC_POST',    'STEP_QC_POST',    'Post-Press QC',           'APPROVAL',  26, 1016, 'AUTO',       TRUE,  4.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  1150, 150, '#e74c3c'),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 8: Packing & Dispatch (seq 27-32)
    -- ══════════════════════════════════════════════════════════════════════════════
    ('PACK',       'STEP_PACK',       'Packing',                 'PROCESS',   27, 1012, 'AUTO',       TRUE,  3.00,  1.00, NULL,        FALSE, FALSE, FALSE, FALSE, 50,   250, '#16a085'),
    ('LOAD',       'STEP_LOAD',       'Loading',                 'PROCESS',   28, 1013, 'AUTO',       TRUE,  2.00,  1.00, NULL,        FALSE, FALSE, FALSE, FALSE, 150,  250, '#16a085'),
    ('CHALLAN',    'STEP_CHALLAN',    'Delivery Challan',        'APPROVAL',  29, 1004, 'AUTO',       TRUE,  2.00,  1.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  250,  250, '#3498db'),
    ('GATE_PASS',  'STEP_GATE_PASS',  'Gate Pass',               'APPROVAL',  30, 1018, 'AUTO',       TRUE,  1.00,  0.50, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  350,  250, '#7f8c8d'),
    ('DISPATCH',   'STEP_DISPATCH',   'Dispatch',                'PROCESS',   31, 1013, 'AUTO',       TRUE,  2.00,  1.00, NULL,        FALSE, FALSE, TRUE,  FALSE, 450,  250, '#16a085'),
    ('DELIVERY_CONF','STEP_DEL_CONF', 'Delivery Confirmation',   'APPROVAL',  32, 1013, 'AUTO',       TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, TRUE,  FALSE, 550,  250, '#2ecc71'),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 9: Billing & Finance (seq 33-36)
    -- ══════════════════════════════════════════════════════════════════════════════
    ('BILL',       'STEP_BILL',       'Billing / Invoice',       'APPROVAL',  33, 1004, 'AUTO',       TRUE,  4.00,  2.00, 'DEPT_HEAD', FALSE, FALSE, TRUE,  TRUE,  650,  250, '#3498db'),
    ('PAY_REC',    'STEP_PAY_REC',    'Payment Receipt',         'APPROVAL',  34, 1004, 'AUTO',       TRUE,  48.00, 24.00,'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  750,  250, '#3498db'),
    ('CREDIT_NOTE','STEP_CREDIT_NOTE','Credit Note',             'APPROVAL',  35, 1004, 'MANUAL',     FALSE, 8.00,  4.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  850,  250, '#e74c3c'),
    ('DEBIT_NOTE', 'STEP_DEBIT_NOTE', 'Debit Note',              'APPROVAL',  36, 1004, 'MANUAL',     FALSE, 8.00,  4.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  950,  250, '#e74c3c'),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 10: Costing & Closure (seq 37-42)
    -- ══════════════════════════════════════════════════════════════════════════════
    ('STORE_RETURN','STEP_STORE_RET', 'Material Return',         'PROCESS',   37, 1014, 'AUTO',       FALSE, 4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 50,   350, '#1abc9c'),
    ('WASTE_ENTRY','STEP_WASTE_ENT',  'Wastage Entry',           'PROCESS',   38, 1014, 'AUTO',       FALSE, 4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 150,  350, '#1abc9c'),
    ('COST_FINAL', 'STEP_COST_FINAL', 'Final Costing',           'APPROVAL',  39, 1008, 'AUTO',       TRUE,  8.00,  4.00, 'DEPT_HEAD', FALSE, FALSE, FALSE, TRUE,  250,  350, '#f39c12'),
    ('PROFIT_ANALYSIS','STEP_PROFIT', 'Profit Analysis',         'PROCESS',   40, 1004, 'AUTO',       TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, TRUE,  350,  350, '#3498db'),
    ('JOB_CLOSE',  'STEP_JOB_CLOSE',  'Job Closure',             'PROCESS',   41, 1007, 'AUTO',       TRUE,  2.00,  1.00, NULL,        FALSE, FALSE, TRUE,  FALSE, 450,  350, '#2ecc71'),
    ('JOB_ARCHIVE','STEP_JOB_ARCHIVE','Job Archive',             'END',       42, 1005, 'AUTO',       TRUE,  4.00,  2.00, NULL,        FALSE, FALSE, FALSE, FALSE, 550,  350, '#95a5a6')
) AS steps(
    process_code, step_code, step_name, step_type, sequence_no, department_id,
    assignment_rule, is_mandatory, sla_hours, escalate_hours, escalate_to,
    notify_vendor, notify_supplier, notify_customer, notify_dept_head,
    canvas_x, canvas_y, node_color
)
JOIN press_db.mst_process p ON p.processcode = steps.process_code AND p.isactive = TRUE
WHERE wt.workflow_code = 'WF-DEFAULT-FULL';

-- ============================================================================
-- STEP 3: INSERT mst_workflow_connection (Step Connections)
-- ============================================================================
-- Defines the flow between steps. Each connection links from_step_id → to_step_id
-- condition_expression allows conditional branching (e.g., "approved==true")

INSERT INTO press_db.mst_workflow_connection (
    workflow_template_id,
    from_step_id,
    to_step_id,
    condition_expression,
    label,
    sequence_no,
    is_active
)
SELECT
    wt.workflow_template_id,
    from_step.workflow_step_id,
    to_step.workflow_step_id,
    conn.condition_expr,
    conn.label,
    conn.seq,
    TRUE
FROM press_db.mst_workflow_template wt
CROSS JOIN (VALUES
    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 1 → PHASE 2: Pre-Sales to Job Creation
    -- ══════════════════════════════════════════════════════════════════════════════
    ('STEP_ADV_PAY',     'STEP_ENQ_JOB',     NULL,               'Advance → Enquiry',     1),
    ('STEP_ENQ_JOB',     'STEP_ENQ_EST',     NULL,               'Enquiry → Estimation',  1),
    ('STEP_ENQ_EST',     'STEP_QUOT',        NULL,               'Estimation → Quote',    1),
    ('STEP_QUOT',        'STEP_QUOT_APPR',   'requires_approval','Quote → Approval',      1),
    ('STEP_QUOT',        'STEP_JOB_CREATE',  'auto_approve',     'Quote → Job (Direct)',  2),
    ('STEP_QUOT_APPR',   'STEP_JOB_CREATE',  'approved==true',   'Approved → Job',        1),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 2 → PHASE 3: Job Creation to Design
    -- ══════════════════════════════════════════════════════════════════════════════
    ('STEP_JOB_CREATE',  'STEP_JOB_APPR',    'manual_job',       'Job → Approval',        1),
    ('STEP_JOB_CREATE',  'STEP_DES_DTP',     'quotation_job',    'Job → Design (Direct)', 2),
    ('STEP_JOB_APPR',    'STEP_DES_DTP',     'approved==true',   'Approved → Design',     1),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 3: Design & Pre-Press Flow
    -- ══════════════════════════════════════════════════════════════════════════════
    ('STEP_DES_DTP',     'STEP_PROOF',       NULL,               'Design → Proof',        1),
    ('STEP_PROOF',       'STEP_DES_DTP',     'rejected',         'Proof Rejected → Redesign', 2),
    ('STEP_PROOF',       'STEP_PRE_PRESS',   'approved==true',   'Proof Approved → CTP',  1),
    ('STEP_PRE_PRESS',   'STEP_STORE_ISSUE', NULL,               'CTP → Material Issue',  1),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 4: Procurement (Optional/Disabled) to Material Issue
    -- ══════════════════════════════════════════════════════════════════════════════
    ('STEP_PROC',        'STEP_GRN',         NULL,               'Procurement → GRN',     1),
    ('STEP_GRN',         'STEP_QC_IN',       NULL,               'GRN → QC Inward',       1),
    ('STEP_QC_IN',       'STEP_PROC',        'rejected',         'QC Rejected → Re-Procure', 2),
    ('STEP_QC_IN',       'STEP_STORE_ISSUE', 'approved==true',   'QC Approved → Issue',   1),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 5 → PHASE 6: Planning to Production
    -- ══════════════════════════════════════════════════════════════════════════════
    ('STEP_STORE_ISSUE', 'STEP_JOB_PLAN',    NULL,               'Issue → Planning',      1),
    ('STEP_JOB_PLAN',    'STEP_JOB_SCHED',   NULL,               'Plan → Schedule',       1),
    ('STEP_JOB_SCHED',   'STEP_JOB_CARD',    NULL,               'Schedule → Job Card',   1),
    ('STEP_JOB_CARD',    'STEP_CUT',         NULL,               'Job Card → Cutting',    1),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 6: Production Execution
    -- ══════════════════════════════════════════════════════════════════════════════
    ('STEP_CUT',         'STEP_PRINT',       NULL,               'Cutting → Printing',    1),
    ('STEP_PRINT',       'STEP_QC_PROC',     NULL,               'Printing → QC',         1),
    ('STEP_QC_PROC',     'STEP_PRINT',       'rejected',         'QC Failed → Reprint',   2),
    ('STEP_QC_PROC',     'STEP_DRY',         'approved==true',   'QC Passed → Drying',    1),
    ('STEP_DRY',         'STEP_POST_PRESS',  NULL,               'Drying → Post-Press',   1),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 7: Post-Press Finishing
    -- ══════════════════════════════════════════════════════════════════════════════
    ('STEP_POST_PRESS',  'STEP_FOLD',        'requires_folding', 'Post → Folding',        1),
    ('STEP_POST_PRESS',  'STEP_BIND',        'requires_binding', 'Post → Binding',        2),
    ('STEP_POST_PRESS',  'STEP_TRIM',        'trim_only',        'Post → Trim Only',      3),
    ('STEP_FOLD',        'STEP_BIND',        'requires_binding', 'Folding → Binding',     1),
    ('STEP_FOLD',        'STEP_TRIM',        NULL,               'Folding → Trim',        2),
    ('STEP_BIND',        'STEP_TRIM',        NULL,               'Binding → Trim',        1),
    ('STEP_TRIM',        'STEP_QC_POST',     NULL,               'Trim → QC Post',        1),
    ('STEP_QC_POST',     'STEP_POST_PRESS',  'rejected',         'QC Failed → Redo',      2),
    ('STEP_QC_POST',     'STEP_PACK',        'approved==true',   'QC Passed → Packing',   1),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 8: Packing & Dispatch
    -- ══════════════════════════════════════════════════════════════════════════════
    ('STEP_PACK',        'STEP_LOAD',        NULL,               'Packing → Loading',     1),
    ('STEP_LOAD',        'STEP_CHALLAN',     NULL,               'Loading → Challan',     1),
    ('STEP_CHALLAN',     'STEP_GATE_PASS',   NULL,               'Challan → Gate Pass',   1),
    ('STEP_GATE_PASS',   'STEP_DISPATCH',    NULL,               'Gate Pass → Dispatch',  1),
    ('STEP_DISPATCH',    'STEP_DEL_CONF',    NULL,               'Dispatch → Delivery',   1),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 8 → PHASE 9: Delivery to Billing
    -- ══════════════════════════════════════════════════════════════════════════════
    ('STEP_DEL_CONF',    'STEP_CREDIT_NOTE', 'short_delivery',   'Short → Credit Note',   2),
    ('STEP_DEL_CONF',    'STEP_BILL',        'full_delivery',    'Delivered → Billing',   1),
    ('STEP_CREDIT_NOTE', 'STEP_BILL',        NULL,               'Credit Note → Billing', 1),
    ('STEP_DEBIT_NOTE',  'STEP_BILL',        NULL,               'Debit Note → Billing',  1),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 9: Billing & Finance
    -- ══════════════════════════════════════════════════════════════════════════════
    ('STEP_BILL',        'STEP_PAY_REC',     NULL,               'Billing → Payment',     1),

    -- ══════════════════════════════════════════════════════════════════════════════
    -- PHASE 9 → PHASE 10: Finance to Closure
    -- ══════════════════════════════════════════════════════════════════════════════
    ('STEP_PAY_REC',     'STEP_STORE_RET',   NULL,               'Payment → Return',      1),
    ('STEP_STORE_RET',   'STEP_WASTE_ENT',   NULL,               'Return → Wastage',      1),
    ('STEP_WASTE_ENT',   'STEP_COST_FINAL',  NULL,               'Wastage → Costing',     1),
    ('STEP_COST_FINAL',  'STEP_PROFIT',      NULL,               'Costing → Profit',      1),
    ('STEP_PROFIT',      'STEP_JOB_CLOSE',   NULL,               'Profit → Close',        1),
    ('STEP_JOB_CLOSE',   'STEP_JOB_ARCHIVE', NULL,               'Close → Archive',       1)
) AS conn(from_step_code, to_step_code, condition_expr, label, seq)
JOIN press_db.mst_workflow_step from_step 
    ON from_step.step_code = conn.from_step_code 
    AND from_step.workflow_template_id = wt.workflow_template_id
JOIN press_db.mst_workflow_step to_step 
    ON to_step.step_code = conn.to_step_code 
    AND to_step.workflow_template_id = wt.workflow_template_id
WHERE wt.workflow_code = 'WF-DEFAULT-FULL';

-- ============================================================================
-- STEP 4: INSERT mst_workspace_config (Default User Configuration - OPTIONAL)
-- ============================================================================
-- This table stores per-user workspace preferences.
-- Usually populated when user first accesses workspace.
-- Below is a sample for user_id = 1 (admin/system user).

-- Note: Only insert if you have a user with user_id = 1
-- Comment out if not applicable

INSERT INTO press_db.mst_workspace_config (
    user_id,
    show_pending_tasks,
    show_completed_tasks,
    show_assigned_tasks,
    show_approvals,
    show_calendar,
    show_notifications,
    show_history,
    default_calendar_view,
    default_task_filter,
    default_approval_filter,
    notify_on_task_assign,
    notify_on_task_overdue,
    notify_on_approval_request,
    notify_on_approval_complete,
    widget_order,
    pinned_jobs,
    pinned_processes,
    history_days,
    auto_refresh_seconds,
    compact_mode,
    items_per_page,
    is_active,
    created_by,
    created_on
)
SELECT
    u.userid,
    TRUE,  -- show_pending_tasks
    TRUE,  -- show_completed_tasks
    TRUE,  -- show_assigned_tasks
    TRUE,  -- show_approvals
    TRUE,  -- show_calendar
    TRUE,  -- show_notifications
    TRUE,  -- show_history
    'WEEKLY',  -- default_calendar_view
    'PENDING', -- default_task_filter
    'PENDING', -- default_approval_filter
    TRUE,  -- notify_on_task_assign
    TRUE,  -- notify_on_task_overdue
    TRUE,  -- notify_on_approval_request
    TRUE,  -- notify_on_approval_complete
    '["PENDING_TASKS", "APPROVALS", "CALENDAR", "NOTIFICATIONS", "HISTORY"]'::jsonb,
    '[]'::jsonb,  -- pinned_jobs
    '[]'::jsonb,  -- pinned_processes
    30,    -- history_days
    60,    -- auto_refresh_seconds
    FALSE, -- compact_mode
    20,    -- items_per_page
    TRUE,  -- is_active
    'SYSTEM',
    CURRENT_TIMESTAMP
FROM press_db.mst_user u
WHERE u.userid = 1  -- Admin user
AND NOT EXISTS (
    SELECT 1 FROM press_db.mst_workspace_config wc WHERE wc.user_id = u.userid
);

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================
-- Run these queries to verify the seed data was inserted correctly:

-- 1. Check workflow template
-- SELECT * FROM press_db.mst_workflow_template WHERE workflow_code = 'WF-DEFAULT-FULL';

-- 2. Check workflow steps (should be 42 rows)
-- SELECT step_code, step_name, step_type, sequence_no, department_id 
-- FROM press_db.mst_workflow_step 
-- WHERE workflow_template_id = (SELECT workflow_template_id FROM press_db.mst_workflow_template WHERE workflow_code = 'WF-DEFAULT-FULL')
-- ORDER BY sequence_no;

-- 3. Check workflow connections
-- SELECT 
--     from_s.step_code AS from_step,
--     to_s.step_code AS to_step,
--     c.label,
--     c.condition_expression
-- FROM press_db.mst_workflow_connection c
-- JOIN press_db.mst_workflow_step from_s ON from_s.workflow_step_id = c.from_step_id
-- JOIN press_db.mst_workflow_step to_s ON to_s.workflow_step_id = c.to_step_id
-- WHERE c.workflow_template_id = (SELECT workflow_template_id FROM press_db.mst_workflow_template WHERE workflow_code = 'WF-DEFAULT-FULL')
-- ORDER BY from_s.sequence_no, c.sequence_no;

-- 4. Check workspace config
-- SELECT * FROM press_db.mst_workspace_config;

-- ============================================================================
-- SUMMARY
-- ============================================================================
-- This script creates:
--   1. mst_workflow_template: 1 default workflow template (WF-DEFAULT-FULL)
--   2. mst_workflow_step: 42 steps covering all 10 phases
--   3. mst_workflow_connection: ~55 connections defining the flow
--   4. mst_workspace_config: 1 default user config (optional, for user_id=1)
--
-- Department IDs used:
--   9999 = Party (Customer Approvals)
--   1001 = Top Management
--   1004 = Accounts & Finance
--   1005 = IT & ERP Support
--   1007 = Customer Service & CRM
--   1008 = Estimation & Costing
--   1009 = Pre-Press & Design
--   1010 = Printing
--   1011 = Post-Press & Finishing
--   1012 = Packaging
--   1013 = Dispatch & Logistics
--   1014 = Inventory & Stores
--   1015 = Purchase
--   1016 = Quality Management
--   1018 = Security & Gatepass
--
-- Reference: docs/COMPLETE_WORKFLOW_IMPLEMENTATION_GUIDE.md
-- ============================================================================
