-- ============================================================================
-- Seed: press_db.mst_process_notification_config (job-type + process flag driven)
-- Uses logic from mst_process and mst_job_type column values.
-- Party workspace rule: client-facing/party approval routes to department 9999.
-- Events seeded: PROC_START, TASK_ASSIGNED, TASK_OVERDUE, TASK_COMPLETED
-- ============================================================================

BEGIN;

DELETE FROM press_db.mst_process_notification_config
WHERE event_type_code IN ('PROC_START', 'TASK_ASSIGNED', 'TASK_OVERDUE', 'TASK_COMPLETED');

WITH process_alias AS (
    SELECT * FROM (VALUES
        ('ENQUIRY','ENQ_JOB'),('DESIGN','DES_DTP'),('DTP','DES_DTP'),('PREPRESS','PRE_PRESS'),
        ('PRINTING','PRINT'),('BINDING','BIND'),('FINISHING','POST_PRESS'),('PURCHASE','PROC'),
        ('RECEIPT','GRN'),('CLOSURE','JOB_CLOSE'),('DISPATCH','DISPATCH')
    ) AS a(alias_code, process_code)
),
active_job_type AS (
    SELECT
        jt.jobtypeid, jt.jobtypecode, jt.jobtypename,
        jt.isdesignrequired, jt.isdtprequired, jt.isctprequired,
        jt.isprintingrequired, jt.isbindingrequired, jt.isfinishingrequired,
        jt.printingmode, jt.issingleprocess, jt.isfullprocess,
        jt.iscustomermaterial, jt.isinhousematerial, jt.isoutsourcejob,
        jt.allowadvancepayment, jt.requirecostingapproval,
        jt.defaultstartprocesscode, jt.defaultendprocesscode
    FROM press_db.mst_job_type jt
    WHERE jt.isactive = TRUE
),
job_type_bounds AS (
    SELECT
        j.*,
        COALESCE(ps.sequenceno, 2) AS start_seq,
        COALESCE(pe.sequenceno, 31) AS end_seq
    FROM active_job_type j
    LEFT JOIN process_alias sa ON UPPER(COALESCE(j.defaultstartprocesscode, '')) = sa.alias_code
    LEFT JOIN process_alias ea ON UPPER(COALESCE(j.defaultendprocesscode, '')) = ea.alias_code
    LEFT JOIN press_db.mst_process ps ON ps.processcode = COALESCE(sa.process_code, j.defaultstartprocesscode)
    LEFT JOIN press_db.mst_process pe ON pe.processcode = COALESCE(ea.process_code, j.defaultendprocesscode)
),
active_process AS (
    SELECT
        p.processid, p.processcode, p.processname, p.departmentid,
        p.sequenceno, p.ismandatory, p.isapprovalrequired, p.isclientapproval,
        p.templatecode, p.templatename
    FROM press_db.mst_process p
    WHERE p.isactive = TRUE
),
job_process_matrix AS (
    SELECT
        j.jobtypeid, j.jobtypecode, j.jobtypename,
        p.processid, p.processcode, p.processname, p.departmentid, p.sequenceno,
        p.ismandatory, p.isapprovalrequired, p.isclientapproval, p.templatecode, p.templatename,
        j.printingmode, j.issingleprocess, j.isfullprocess, j.iscustomermaterial,
        j.isinhousematerial, j.isoutsourcejob, j.allowadvancepayment, j.requirecostingapproval,
        CASE
            WHEN p.processcode = 'ADV_PAY' THEN COALESCE(j.allowadvancepayment, FALSE)
            WHEN p.processcode IN ('ENQ_JOB','ENQ_EST','QUOT','QUOT_APPR','JOB_CREATE','JOB_APPROVAL') THEN TRUE
            WHEN p.processcode = 'PRE_DES' THEN COALESCE(j.isdesignrequired, FALSE)
            WHEN p.processcode = 'DES_DTP' THEN COALESCE(j.isdesignrequired, FALSE) OR COALESCE(j.isdtprequired, FALSE)
            WHEN p.processcode = 'PROOF' THEN COALESCE(j.isdesignrequired, FALSE) OR COALESCE(j.isdtprequired, FALSE) OR j.jobtypecode = 'PROOF_ONLY'
            WHEN p.processcode = 'PRE_PRESS' THEN COALESCE(j.isctprequired, FALSE)
            WHEN p.processcode IN ('JOB_PLAN','JOB_SCHED','JOB_CARD','CUT','PRINT','QC_PROC','DRY') THEN COALESCE(j.isprintingrequired, FALSE)
            WHEN p.processcode = 'BIND' THEN COALESCE(j.isbindingrequired, FALSE)
            WHEN p.processcode IN ('POST_PRESS','FOLD','TRIM') THEN COALESCE(j.isfinishingrequired, FALSE)
            WHEN p.processcode = 'QC_POST' THEN COALESCE(j.isbindingrequired, FALSE) OR COALESCE(j.isfinishingrequired, FALSE)
            WHEN p.processcode IN ('PROC','GRN','QC_IN') THEN COALESCE(j.isoutsourcejob, FALSE)
            WHEN p.processcode = 'COST_FINAL' THEN COALESCE(j.requirecostingapproval, FALSE)
            WHEN p.processcode IN ('PACK','LOAD','CHALLAN','GATE_PASS','DISPATCH') THEN
                COALESCE(j.isprintingrequired, FALSE) OR COALESCE(j.isbindingrequired, FALSE) OR
                COALESCE(j.isfinishingrequired, FALSE) OR COALESCE(j.isoutsourcejob, FALSE) OR
                j.jobtypecode = 'JOB_WORK'
            ELSE FALSE
        END AS include_by_job_nature
    FROM job_type_bounds j
    CROSS JOIN active_process p
    WHERE p.sequenceno BETWEEN LEAST(j.start_seq, j.end_seq) AND GREATEST(j.start_seq, j.end_seq)
),
selected_matrix AS (
    SELECT *
    FROM job_process_matrix
    WHERE include_by_job_nature = TRUE
),
events AS (
    SELECT * FROM (VALUES
        ('PROC_START'::varchar(30),    1, ' Started'::varchar(20),   'PENDING'::varchar(30),   TRUE),
        ('TASK_ASSIGNED'::varchar(30), 2, ' Assigned',               'PENDING',                 TRUE),
        ('TASK_OVERDUE'::varchar(30),  3, ' Overdue',                'OVERDUE',                 FALSE),
        ('TASK_COMPLETED'::varchar(30),4, ' Completed',              'COMPLETED',               FALSE)
    ) e(event_type_code, event_offset, label_suffix, trigger_on_status, auto_trigger)
),
final_rows AS (
    SELECT
        m.jobtypeid AS job_type_id,
        m.jobtypecode AS job_type_code,
        m.processid AS process_id,
        m.processid AS subprocess_id,
        m.processcode AS process_code,
        m.processcode AS subprocess_code,
        CASE WHEN (COALESCE(m.isclientapproval, FALSE) OR m.processcode = 'JOB_APPROVAL')
                  AND m.processcode NOT IN ('DES_DTP', 'PRE_DES')
             THEN 9999 ELSE m.departmentid::bigint END AS department_id,
        e.event_type_code,
        (m.processname || e.label_suffix)::varchar(100) AS event_label,
        NULL::integer AS approval_type_id,
        CASE WHEN COALESCE(m.isapprovalrequired, FALSE) THEN 1 ELSE 0 END AS approval_level,
        CASE WHEN (COALESCE(m.isclientapproval, FALSE) OR m.processcode = 'JOB_APPROVAL')
                  AND m.processcode NOT IN ('DES_DTP', 'PRE_DES')
             THEN 'PARTY' ELSE 'INTERNAL' END::varchar(20) AS recipient_type,
        TRUE AS notify_assignee,
        COALESCE(m.isapprovalrequired, FALSE) AS notify_dept_head,
        FALSE AS notify_supervisor,
        COALESCE(m.isapprovalrequired, FALSE) AS notify_approver,
        FALSE AS notify_client_sms,
        COALESCE(m.isclientapproval, FALSE) AS notify_client_whatsapp,
        COALESCE(m.isclientapproval, FALSE) AS notify_client_email,
        FALSE AS notify_internal_sms,
        FALSE AS notify_internal_whatsapp,
        TRUE AS notify_internal_email,
        TRUE AS notify_push,
        FALSE AS notify_topup_alert,
        COALESCE(m.templatecode, ('TPL_' || m.processcode || '_' || e.event_type_code))::varchar(50) AS template_code,
        ('[' || m.jobtypecode || '] ' || COALESCE(m.templatename, m.processname) || ' [' || e.event_type_code || '] - {job_no}')::varchar(300) AS subject_template,
        ('Process: ' || m.processname || ' (' || m.processcode || ')' || E'\n' ||
         'Event: ' || e.event_type_code || E'\n' ||
         'Job No: {job_no}' || E'\n' ||
         'Work Note: Update step status and part-wise completion in workspace.')::text AS body_template,
        (CASE m.processcode
            WHEN 'ENQ_JOB' THEN 2.00 WHEN 'ENQ_EST' THEN 6.00 WHEN 'QUOT' THEN 4.00
            WHEN 'QUOT_APPR' THEN 8.00 WHEN 'JOB_CREATE' THEN 2.00 WHEN 'JOB_APPROVAL' THEN 4.00
            WHEN 'DES_DTP' THEN 12.00 WHEN 'PROOF' THEN 24.00 WHEN 'PRE_PRESS' THEN 6.00
            WHEN 'JOB_PLAN' THEN 4.00 WHEN 'JOB_SCHED' THEN 4.00 WHEN 'JOB_CARD' THEN 2.00
            WHEN 'CUT' THEN 4.00 WHEN 'PRINT' THEN 12.00 WHEN 'QC_PROC' THEN 4.00 WHEN 'DRY' THEN 6.00
            WHEN 'POST_PRESS' THEN 8.00 WHEN 'FOLD' THEN 4.00 WHEN 'BIND' THEN 8.00 WHEN 'TRIM' THEN 4.00
            WHEN 'QC_POST' THEN 4.00 WHEN 'PROC' THEN 8.00 WHEN 'GRN' THEN 4.00 WHEN 'QC_IN' THEN 4.00
            WHEN 'PACK' THEN 3.00 WHEN 'LOAD' THEN 2.00 WHEN 'CHALLAN' THEN 2.00 WHEN 'GATE_PASS' THEN 1.00
            WHEN 'DISPATCH' THEN 2.00 ELSE 4.00 END)::numeric(6,2) AS sla_hours,
        (CASE WHEN m.processcode IN ('GATE_PASS','CHALLAN','DISPATCH') THEN 1.00 ELSE 2.00 END)::numeric(6,2) AS escalate_after_hours,
        CASE WHEN COALESCE(m.isapprovalrequired, FALSE) THEN 'DEPT_HEAD' ELSE NULL END::varchar(50) AS escalate_to,
        2.00::numeric(6,2) AS overdue_reminder_interval_hours,
        e.trigger_on_status,
        e.auto_trigger,
        NULL::text AS trigger_condition,
        jsonb_build_object(
            'jobTypeCode', m.jobtypecode,
            'jobTypeName', m.jobtypename,
            'processCode', m.processcode,
            'eventType', e.event_type_code,
            'printingMode', m.printingmode,
            'isSingleProcess', m.issingleprocess,
            'isFullProcess', m.isfullprocess,
            'isCustomerMaterial', m.iscustomermaterial,
            'isInhouseMaterial', m.isinhousematerial,
            'isOutsourceJob', m.isoutsourcejob,
            'allowAdvancePayment', m.allowadvancepayment,
            'requireCostingApproval', m.requirecostingapproval,
            'channels', jsonb_build_object('workspace', true, 'email', true, 'push', true)
        ) AS payload_config,
        jsonb_build_object(
            'enabled', true,
            'model', 'agentic-workspace-v1',
            'purpose', 'process-work-guidance'
        ) AS ai_config,
        jsonb_build_object(
            'seededFrom', 'mst_process + mst_job_type + mst_department',
            'taskTypeLogic', 'process.isapprovalrequired + client approval + job-type nature',
            'partyRoutingDepartment', 9999
        ) AS meta,
        (CASE
            WHEN COALESCE(m.isapprovalrequired, FALSE) THEN 'HIGH'
            WHEN m.processcode IN ('PRINT','QC_POST','CHALLAN','GATE_PASS') THEN 'HIGH'
            ELSE 'NORMAL'
        END)::varchar(20) AS priority,
        COALESCE(m.ismandatory, FALSE) AS is_mandatory,
        TRUE AS is_active,
        (m.sequenceno * 10 + e.event_offset) AS sequence_no,
        'SYSTEM'::varchar(50) AS created_by,
        CURRENT_TIMESTAMP AS created_on,
        NULL::varchar(50) AS modified_by,
        CURRENT_TIMESTAMP AS modified_on
    FROM selected_matrix m
    CROSS JOIN events e
)
INSERT INTO press_db.mst_process_notification_config (
    job_type_id, job_type_code, process_id, subprocess_id, process_code, subprocess_code,
    department_id, event_type_code, event_label, approval_type_id, approval_level, recipient_type,
    notify_assignee, notify_dept_head, notify_supervisor, notify_approver,
    notify_client_sms, notify_client_whatsapp, notify_client_email,
    notify_internal_sms, notify_internal_whatsapp, notify_internal_email, notify_push, notify_topup_alert,
    template_code, subject_template, body_template,
    sla_hours, escalate_after_hours, escalate_to, overdue_reminder_interval_hours,
    trigger_on_status, auto_trigger, trigger_condition,
    payload_config, ai_config, meta,
    priority, is_mandatory, is_active, sequence_no,
    created_by, created_on, modified_by, modified_on
)
SELECT
    job_type_id, job_type_code, process_id, subprocess_id, process_code, subprocess_code,
    department_id, event_type_code, event_label, approval_type_id, approval_level, recipient_type,
    notify_assignee, notify_dept_head, notify_supervisor, notify_approver,
    notify_client_sms, notify_client_whatsapp, notify_client_email,
    notify_internal_sms, notify_internal_whatsapp, notify_internal_email, notify_push, notify_topup_alert,
    template_code, subject_template, body_template,
    sla_hours, escalate_after_hours, escalate_to, overdue_reminder_interval_hours,
    trigger_on_status, auto_trigger, trigger_condition,
    payload_config, ai_config, meta,
    priority, is_mandatory, is_active, sequence_no,
    created_by, created_on, modified_by, modified_on
FROM final_rows
ORDER BY job_type_id, sequence_no;

COMMIT;

-- Validation
-- SELECT job_type_id, job_type_code, process_code, department_id, event_type_code, approval_level, recipient_type, is_mandatory
-- FROM press_db.mst_process_notification_config
-- ORDER BY job_type_id, sequence_no;
