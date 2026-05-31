-- ═══════════════════════════════════════════════════════════════════════════════
-- Migration: Add workflow tracking columns to trn_workspace_task
-- Purpose: Support pre-generated workflow tasks that are processed sequentially
-- ═══════════════════════════════════════════════════════════════════════════════

-- Add new columns to trn_workspace_task
ALTER TABLE press_db.trn_workspace_task
ADD COLUMN IF NOT EXISTS sequence_no INTEGER,
ADD COLUMN IF NOT EXISTS workflow_step_id BIGINT,
ADD COLUMN IF NOT EXISTS workflow_template_id BIGINT,
ADD COLUMN IF NOT EXISTS workflow_batch_id UUID;

-- Add comments for documentation
COMMENT ON COLUMN press_db.trn_workspace_task.sequence_no IS 'Sequence number within the workflow. Used for ordering tasks in pre-generated workflow.';
COMMENT ON COLUMN press_db.trn_workspace_task.workflow_step_id IS 'Reference to the workflow step that generated this task. Null for ad-hoc tasks.';
COMMENT ON COLUMN press_db.trn_workspace_task.workflow_template_id IS 'Reference to the workflow template. Null for ad-hoc tasks.';
COMMENT ON COLUMN press_db.trn_workspace_task.workflow_batch_id IS 'Workflow batch ID to group all tasks belonging to the same workflow instance.';

-- Add foreign key constraints
ALTER TABLE press_db.trn_workspace_task
ADD CONSTRAINT fk_workspace_task_workflow_step 
    FOREIGN KEY (workflow_step_id) 
    REFERENCES press_db.mst_workflow_step(workflow_step_id) 
    ON DELETE SET NULL;

ALTER TABLE press_db.trn_workspace_task
ADD CONSTRAINT fk_workspace_task_workflow_template 
    FOREIGN KEY (workflow_template_id) 
    REFERENCES press_db.mst_workflow_template(workflow_template_id) 
    ON DELETE SET NULL;

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_workspace_task_workflow_batch 
    ON press_db.trn_workspace_task(workflow_batch_id) 
    WHERE workflow_batch_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_workspace_task_sequence 
    ON press_db.trn_workspace_task(workflow_batch_id, sequence_no) 
    WHERE workflow_batch_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_workspace_task_queued 
    ON press_db.trn_workspace_task(job_id, task_status) 
    WHERE task_status = 'QUEUED';

-- ═══════════════════════════════════════════════════════════════════════════════
-- Verification query
-- ═══════════════════════════════════════════════════════════════════════════════
-- SELECT column_name, data_type, is_nullable 
-- FROM information_schema.columns 
-- WHERE table_schema = 'press_db' AND table_name = 'trn_workspace_task'
-- AND column_name IN ('sequence_no', 'workflow_step_id', 'workflow_template_id', 'workflow_batch_id');
