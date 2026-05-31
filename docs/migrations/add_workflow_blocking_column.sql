-- ═══════════════════════════════════════════════════════════════════════════════
-- Migration: Add is_blocking column to workflow tables
-- Purpose: Support blocking vs non-blocking task progression in workflows
-- 
-- Business Rules:
--   - Blocking tasks: Must be completed before workflow can progress to next step
--   - Non-blocking tasks: Workflow can progress even if these tasks are pending
--   - Party-related tasks (department 9999) are typically non-blocking
--   - Approval tasks are typically blocking unless explicitly marked otherwise
-- ═══════════════════════════════════════════════════════════════════════════════

-- ─────────────────────────────────────────────────────────────────────────────────
-- Step 1: Add is_blocking column to mst_workflow_step
-- This defines the default blocking behavior for each workflow step
-- ─────────────────────────────────────────────────────────────────────────────────
ALTER TABLE press_db.mst_workflow_step
ADD COLUMN IF NOT EXISTS is_blocking BOOLEAN NOT NULL DEFAULT TRUE;

COMMENT ON COLUMN press_db.mst_workflow_step.is_blocking IS 
    'If TRUE, workflow cannot progress until this step is completed. If FALSE (non-blocking), workflow can proceed to next step even if this task is pending. Party-related tasks are typically non-blocking.';

-- ─────────────────────────────────────────────────────────────────────────────────
-- Step 2: Add is_blocking column to trn_workspace_task
-- This stores the actual blocking status for each task instance
-- ─────────────────────────────────────────────────────────────────────────────────
ALTER TABLE press_db.trn_workspace_task
ADD COLUMN IF NOT EXISTS is_blocking BOOLEAN NOT NULL DEFAULT TRUE;

COMMENT ON COLUMN press_db.trn_workspace_task.is_blocking IS 
    'If TRUE, this task blocks workflow progression. If FALSE, workflow can proceed to next step even while this task is pending. Inherited from workflow step but can be overridden.';

-- ─────────────────────────────────────────────────────────────────────────────────
-- Step 3: Create index for efficient querying of blocking tasks
-- ─────────────────────────────────────────────────────────────────────────────────
CREATE INDEX IF NOT EXISTS idx_workspace_task_blocking
    ON press_db.trn_workspace_task(workflow_batch_id, is_blocking, task_status)
    WHERE workflow_batch_id IS NOT NULL;

-- ─────────────────────────────────────────────────────────────────────────────────
-- Step 4: Update existing workflow steps to set non-blocking for party-related
-- Department ID 9999 = Party Related Activity (non-blocking by default)
-- ─────────────────────────────────────────────────────────────────────────────────
UPDATE press_db.mst_workflow_step
SET is_blocking = FALSE
WHERE department_id = 9999;

-- ─────────────────────────────────────────────────────────────────────────────────
-- Step 5: Update existing workspace tasks to set non-blocking based on department
-- ─────────────────────────────────────────────────────────────────────────────────
UPDATE press_db.trn_workspace_task
SET is_blocking = FALSE
WHERE department_id = 9999;

-- ═══════════════════════════════════════════════════════════════════════════════
-- Additional: Add source_table filter columns to mst_workflow_step for clarity
-- This helps filter which steps apply to which source (enquiry, quotation, job)
-- ═══════════════════════════════════════════════════════════════════════════════
ALTER TABLE press_db.mst_workflow_step
ADD COLUMN IF NOT EXISTS applies_to_enquiry BOOLEAN NOT NULL DEFAULT TRUE,
ADD COLUMN IF NOT EXISTS applies_to_quotation BOOLEAN NOT NULL DEFAULT TRUE,
ADD COLUMN IF NOT EXISTS applies_to_job BOOLEAN NOT NULL DEFAULT TRUE;

COMMENT ON COLUMN press_db.mst_workflow_step.applies_to_enquiry IS 
    'If TRUE, this step is included when workflow starts from an enquiry.';
COMMENT ON COLUMN press_db.mst_workflow_step.applies_to_quotation IS 
    'If TRUE, this step is included when workflow starts from a quotation.';
COMMENT ON COLUMN press_db.mst_workflow_step.applies_to_job IS 
    'If TRUE, this step is included when workflow starts directly from a job.';

-- ─────────────────────────────────────────────────────────────────────────────────
-- Step 6: Set default applicability based on process codes
-- Enquiry-related processes: Only apply to enquiry source
-- Quotation-related processes: Apply to enquiry and quotation sources
-- Job-related processes: Apply to all sources
-- ─────────────────────────────────────────────────────────────────────────────────

-- Enquiry steps (ENQ_JOB, ENQ_EST): Only for enquiry source
UPDATE press_db.mst_workflow_step ws
SET applies_to_enquiry = TRUE,
    applies_to_quotation = FALSE,
    applies_to_job = FALSE
FROM press_db.mst_process p
WHERE ws.process_id = p.processid
AND p.processcode IN ('ENQ_JOB', 'ENQ_EST');

-- Quotation steps (QUOT): For enquiry and quotation sources
UPDATE press_db.mst_workflow_step ws
SET applies_to_enquiry = TRUE,
    applies_to_quotation = TRUE,
    applies_to_job = FALSE
FROM press_db.mst_process p
WHERE ws.process_id = p.processid
AND p.processcode = 'QUOT';

-- ─────────────────────────────────────────────────────────────────────────────────
-- Step 7: ADV_PAY (Advance Payment) is NON-BLOCKING and does NOT require approval
-- Business Rule: Advance payment is optional - workflow should NOT wait for it
-- ─────────────────────────────────────────────────────────────────────────────────

-- Set ADV_PAY workflow steps as non-blocking
UPDATE press_db.mst_workflow_step ws
SET is_blocking = FALSE
FROM press_db.mst_process p
WHERE ws.process_id = p.processid
AND p.processcode = 'ADV_PAY';

-- Remove approval requirement from ADV_PAY process
UPDATE press_db.mst_process
SET isapprovalrequired = FALSE
WHERE processcode = 'ADV_PAY';

-- Update any existing ADV_PAY workspace tasks to be non-blocking
UPDATE press_db.trn_workspace_task
SET is_blocking = FALSE
WHERE process_code = 'ADV_PAY';

-- ═══════════════════════════════════════════════════════════════════════════════
-- Verification queries
-- ═══════════════════════════════════════════════════════════════════════════════
-- Check columns added to mst_workflow_step:
-- SELECT column_name, data_type, is_nullable, column_default
-- FROM information_schema.columns 
-- WHERE table_schema = 'press_db' AND table_name = 'mst_workflow_step'
-- AND column_name IN ('is_blocking', 'applies_to_enquiry', 'applies_to_quotation', 'applies_to_job');

-- Check columns added to trn_workspace_task:
-- SELECT column_name, data_type, is_nullable, column_default
-- FROM information_schema.columns 
-- WHERE table_schema = 'press_db' AND table_name = 'trn_workspace_task'
-- AND column_name = 'is_blocking';

-- Verify non-blocking steps (party-related):
-- SELECT ws.workflow_step_id, ws.step_name, ws.is_blocking, d.departmentname
-- FROM press_db.mst_workflow_step ws
-- LEFT JOIN press_db.mst_department d ON ws.department_id = d.departmentid
-- WHERE ws.is_blocking = FALSE;

-- Verify source applicability:
-- SELECT ws.step_name, p.processcode, ws.applies_to_enquiry, ws.applies_to_quotation, ws.applies_to_job
-- FROM press_db.mst_workflow_step ws
-- LEFT JOIN press_db.mst_process p ON ws.process_id = p.processid
-- ORDER BY ws.sequence_no;
