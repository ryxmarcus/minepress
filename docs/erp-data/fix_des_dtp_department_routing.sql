-- ============================================================================
-- Fix: DES_DTP (Designing / DTP) department routing
-- Issue: DES_DTP was incorrectly routed to dept 9999 (Party) instead of 1009 (Pre-Press & Design)
-- Root cause: Seed script set department_id=9999 for all isclientapproval=TRUE processes,
--             but DES_DTP is internal work — client approval happens at the PROOF step.
-- ============================================================================

BEGIN;

-- 1. Fix mst_process: set isclientapproval = FALSE for DES_DTP
UPDATE press_db.mst_process
SET    isclientapproval = FALSE,
       modifiedby = 'SYSTEM',
       modifiedon = CURRENT_TIMESTAMP
WHERE  processcode = 'DES_DTP';

-- 2. Fix mst_process_notification_config: route DES_DTP to dept 1009 (Pre-Press & Design)
--    Update ALL DES_DTP rows (across all job types and event types) to use correct dept and recipient
UPDATE press_db.mst_process_notification_config
SET    department_id          = 1009,
       recipient_type         = 'INTERNAL',
       notify_client_sms      = FALSE,
       notify_client_whatsapp = FALSE,
       notify_client_email    = FALSE,
       modified_by            = 'SYSTEM',
       modified_on            = CURRENT_TIMESTAMP
WHERE  process_code = 'DES_DTP'
  AND  (department_id = 9999 OR recipient_type = 'PARTY');

-- 3. Fix any existing workspace tasks that were wrongly assigned to party users for DES_DTP
--    Re-assign pending/in-progress DES_DTP tasks to correct department
UPDATE press_db.trn_workspace_task
SET    department_id = 1009
WHERE  process_code = 'DES_DTP'
  AND  department_id = 9999
  AND  task_status IN ('PENDING', 'IN_PROGRESS');

COMMIT;

-- ======================== Verification Queries ========================

-- Verify notification config: all DES_DTP rows should show dept 1009 + INTERNAL
SELECT config_id, job_type_code, process_code, department_id, recipient_type,
       notify_client_whatsapp, notify_client_email, event_type_code
FROM   press_db.mst_process_notification_config
WHERE  process_code = 'DES_DTP'
ORDER  BY config_id;

-- Verify mst_process: DES_DTP should show isclientapproval = FALSE
SELECT processcode, processname, departmentid, isclientapproval
FROM   press_db.mst_process
WHERE  processcode = 'DES_DTP';

-- Verify no remaining party-routed configs for internal processes (sanity check)
-- Expected: only ENQ_JOB, JOB_APPROVAL, PROOF, QUOT should have dept 9999
SELECT DISTINCT process_code, department_id, recipient_type
FROM   press_db.mst_process_notification_config
WHERE  department_id = 9999
ORDER  BY process_code;
