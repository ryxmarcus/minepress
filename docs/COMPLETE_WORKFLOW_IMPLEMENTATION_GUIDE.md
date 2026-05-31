# MinePress ERP — Complete Workflow Implementation Guide
## Version 1.0 | Job Workflow Management System

---

## Table of Contents
1. [Executive Summary](#1-executive-summary)
2. [Workflow Architecture Overview](#2-workflow-architecture-overview)
3. [Complete 10-Phase Workflow](#3-complete-10-phase-workflow)
4. [Job Types & Their Workflow Variations](#4-job-types--their-workflow-variations)
5. [Three Job Entry Scenarios](#5-three-job-entry-scenarios)
6. [Process Codes Reference](#6-process-codes-reference)
7. [Workflow Engine Logic](#7-workflow-engine-logic)
8. [Template-Based Routing](#8-template-based-routing)
9. [Role-Based Task Assignment](#9-role-based-task-assignment)
10. [Notification & SLA System](#10-notification--sla-system)
11. [Conditional & Parallel Flows](#11-conditional--parallel-flows)
12. [Database Schema](#12-database-schema)

---

## 1. Executive Summary

MinePress ERP implements a **dynamic, job-type-aware workflow engine** that routes jobs through configurable processes from customer enquiry to job closure. The system supports:

- **Visual Workflow Designer** — Drag-and-drop workflow creation per Job Type + Product Type
- **Three Entry Points** — Enquiry → Quotation → Job, Direct Quotation → Job, or Direct Manual Job
- **42 Process Steps** — Pre-Sales through Closure with configurable skipping
- **Role-Based Routing** — Automatic task assignment based on Process → Role → Department mappings
- **Multi-Channel Notifications** — Email, SMS, WhatsApp, Push, In-App alerts
- **SLA & Escalation** — Per-step time limits with automatic escalation
- **AI Suggestions** — Intelligent next-step recommendations based on historical data

---

## 2. Workflow Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          USER INTERFACE LAYER                           │
│  ┌──────────────────┐  ┌─────────────────┐  ┌───────────────────────┐  │
│  │ Enquiry Module   │  │ Quotation Module│  │ Job Module            │  │
│  │ - Create Enquiry │  │ - Generate Quote│  │ - Create/View Job     │  │
│  │ - Submit         │  │ - Approve       │  │ - Track Progress      │  │
│  └────────┬─────────┘  └────────┬────────┘  └───────────┬───────────┘  │
├───────────┴──────────────────────┴───────────────────────┴──────────────┤
│                        WORKSPACE ENGINE LAYER                           │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                  WorkspaceProcessEngine                          │  │
│  │  - GenerateAllWorkflowTasksAsync()  ← Pre-generates ALL tasks    │  │
│  │  - ActivateNextQueuedTaskAsync()    ← Sequential task activation │  │
│  │  - CreateWorkspaceTaskAsync()       ← Fallback single task       │  │
│  │  - GenerateNextStepTasksAsync()     ← Legacy/fallback routing    │  │
│  │  - ResolveTargetUsers()             ← Role-based user resolution │  │
│  └──────────────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────┤
│                        DATABASE / PERSISTENCE                           │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────────────┐   │
│  │ mst_process    │  │ mst_workflow_  │  │ trn_workspace_task     │   │
│  │ (42 processes) │  │ template/step  │  │ (active tasks)         │   │
│  └────────────────┘  └────────────────┘  └────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

### Pre-Generated Workflow System (NEW)

The workflow engine now supports **pre-generated workflows** where ALL tasks are created upfront at document creation time:

1. **At Creation Time** — When an Enquiry, Quotation, or Job is created, the system:
   - Finds the matching workflow template (by Job Type + Product Type)
   - Creates ALL workflow tasks from `mst_workflow_step` at once
   - First task is `PENDING` (active), all others are `QUEUED` (waiting)
   - All tasks share a common `WorkflowBatchId` (GUID) for tracking

2. **Sequential Activation** — When a task is completed:
   - System checks if all sibling tasks at the same sequence are done
   - Next `QUEUED` task in sequence is activated (`QUEUED` → `PENDING`)
   - Notifications are sent to newly assigned users

3. **Fallback Behavior** — If no workflow template is found:
   - Falls back to legacy single-task creation
   - Next task is generated on-demand via `GenerateNextStepTasksAsync()`

```
NEW WORKFLOW COLUMNS (trn_workspace_task):
├── sequence_no          → Order within workflow (1, 2, 3...)
├── workflow_step_id     → FK to mst_workflow_step
├── workflow_template_id → FK to mst_workflow_template
├── workflow_batch_id    → GUID grouping all tasks in one workflow instance
└── is_blocking          → If FALSE, workflow proceeds even if task pending (inherited from step)

NEW WORKFLOW COLUMNS (mst_workflow_step):
├── is_blocking          → If FALSE, task doesn't block workflow progression
├── applies_to_enquiry   → Include step when source is trn_enquiry (default: TRUE)
├── applies_to_quotation → Include step when source is trn_quotation (default: TRUE)
└── applies_to_job       → Include step when source is trn_job (default: TRUE)

TASK STATUS VALUES:
├── QUEUED      → Pre-created, waiting for previous step
├── PENDING     → Active, ready for user action
├── IN_PROGRESS → User has started working
├── COMPLETED   → Task finished successfully
├── APPROVED    → Approval granted
├── REJECTED    → Approval rejected
└── CANCELLED   → Task skipped/cancelled
```

### Blocking vs Non-Blocking Tasks

The workflow engine distinguishes between **blocking** and **non-blocking** tasks:

| Task Type       | Behavior                                                                 | Example                     |
|-----------------|--------------------------------------------------------------------------|-----------------------------|
| **Blocking**    | Workflow waits for completion before activating next step                | DES_DTP, PRINT, BIND        |
| **Non-Blocking**| Workflow proceeds immediately; task can complete independently           | Party approvals, customer tasks |

**Automatic Non-Blocking Assignment:**
- All tasks assigned to **Department 9999 (Party Related Activity)** are automatically set as `is_blocking = FALSE`
- Party/customer approvals run in parallel with main production workflow
- Prevents external dependencies from halting internal operations

```
BLOCKING TASK FLOW:
┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│  TASK A     │─────▸│  TASK B     │─────▸│  TASK C     │
│ (Blocking)  │      │ (Blocking)  │      │ (Blocking)  │
│ Must finish │      │ Waits for A │      │ Waits for B │
└─────────────┘      └─────────────┘      └─────────────┘

NON-BLOCKING TASK FLOW:
┌─────────────┐      ┌─────────────┐
│  TASK A     │─────▸│  TASK B     │  (Main flow continues)
│ (Blocking)  │      │ (Blocking)  │
└─────────────┘      └─────────────┘
       │
       └─────▸ ┌───────────────┐
               │ PARTY APPROVAL │  (Runs independently)
               │ (Non-Blocking) │
               └───────────────┘
```

### Source-Aware Workflow Filtering

Workflow steps are filtered based on the **entry point** (source table):

| Source Table      | Steps Included                                                         |
|-------------------|------------------------------------------------------------------------|
| `trn_enquiry`     | All steps where `applies_to_enquiry = TRUE` (full workflow)            |
| `trn_quotation`   | Steps where `applies_to_quotation = TRUE` (skips ENQ_JOB, ENQ_EST)     |
| `trn_job`         | Steps where `applies_to_job = TRUE` (skips enquiry + quotation steps)  |

**Default Configuration:**
```sql
-- Enquiry-only steps (applies_to_quotation=FALSE, applies_to_job=FALSE)
ENQ_JOB, ENQ_EST

-- Quotation steps (applies_to_job=FALSE)
QUOT

-- Job steps (applies_to_enquiry/quotation/job=TRUE by default)
JOB_APPROVAL, DES_DTP, PRINT, ... (all production steps)
```

---

## 3. Complete 10-Phase Workflow

The printing ERP follows a **10-phase workflow** with 42 process codes. Each step is assigned to a specific **department** with defined **responsibilities**.

### Department Reference
| Dept ID | Code  | Department Name               | Type       | Description                    |
|---------|-------|-------------------------------|------------|--------------------------------|
| 9999    | PTY   | Party Related Activity        | External   | Customer/Party approvals       |
| 1001    | MGT   | Top Management                | Admin      | Directors, Owners              |
| 1004    | FIN   | Accounts & Finance            | Admin      | Billing, Payments, Taxation    |
| 1005    | IT    | IT & ERP Support              | Admin      | Systems & Archive              |
| 1006    | SAL   | Sales & Marketing             | Sales      | Sales operations               |
| 1007    | CST   | Customer Service & CRM        | Sales      | Client handling                |
| 1008    | EST   | Estimation & Costing          | Sales      | Quotation & costing            |
| 1009    | PRE   | Pre-Press & Design            | Production | Design to plate making         |
| 1010    | PRT   | Printing                      | Production | All printing operations        |
| 1011    | FINP  | Post-Press & Finishing        | Production | Cutting to final finish        |
| 1012    | PKG   | Packaging                     | Production | Packing operations             |
| 1013    | DSP   | Dispatch & Logistics          | Production | Delivery & transport           |
| 1014    | INV   | Inventory & Stores            | Operations | Material storage               |
| 1015    | PUR   | Purchase                      | Operations | Procurement                    |
| 1016    | QMS   | Quality Management            | Production | Quality & inspection           |
| 1018    | SEC   | Security & Gatepass           | Operations | Security operations            |

---

### Phase 1 — Pre-Sales & Customer Initiation
| Seq | Code       | Process Name             | Department                    | Responsible Role              | Approval | Blocking | Client Approval |
|-----|------------|--------------------------|-------------------------------|-------------------------------|----------|----------|-----------------|
| 1   | `ADV_PAY`  | Advance Payment          | **FIN** (Accounts & Finance)  | Accounts Executive            | —        | ❌ Non   | —               |
| 2   | `ENQ_JOB`  | Enquiry Creation         | **CST** (Customer Service)    | CRM Executive / Sales Rep     | ✓        | ✓ Block  | ✓ (Customer)    |
| 3   | `ENQ_EST`  | Estimation / Costing     | **EST** (Estimation)          | Estimation Manager            | ✓        | ✓ Block  | —               |
| 4   | `QUOT`     | Quotation Generation     | **EST** (Estimation)          | Estimation Manager            | ✓        | ✓ Block  | ✓ (Customer)    |
| 5   | `QUOT_APPR`| Quotation Approval       | **MGT** (Top Management)      | Director / Owner              | ✓        | ✓ Block  | — *(Disabled)*  |

> **Note:** `ADV_PAY` (Advance Payment) is **non-blocking** — workflow can proceed to the next step even if advance payment task is pending. This allows jobs to progress while payment collection continues in parallel.

**Responsibility Matrix — Phase 1:**
| Role                  | Responsibilities                                                    |
|-----------------------|---------------------------------------------------------------------|
| Sales Rep / CRM Exec  | Receive enquiry, gather specs, communicate with customer            |
| Estimation Manager    | Calculate costs, prepare BOM, generate quotation                    |
| Accounts Executive    | Collect advance payment, issue receipt                              |
| Director / Owner      | Approve high-value quotations (if enabled)                          |

---

### Phase 2 — Job Creation & Approval
| Seq | Code          | Process Name    | Department                    | Responsible Role              | Approval | Client Approval |
|-----|---------------|-----------------|-------------------------------|-------------------------------|----------|-----------------|
| 6   | `JOB_CREATE`  | Job Creation    | **CST** (Customer Service)    | CRM Executive / Job Creator   | ✓        | —               |
| 7   | `JOB_APPROVAL`| Job Approval    | **PTY** (Party) / **MGT**     | Customer / Management         | ✓        | ✓ (Customer)    |

**Responsibility Matrix — Phase 2:**
| Role                  | Responsibilities                                                    |
|-----------------------|---------------------------------------------------------------------|
| CRM Executive         | Convert quotation to job, verify details, attach documents          |
| Job Creator           | Create manual jobs with full specifications                         |
| Customer (Party)      | Approve job for manual jobs requiring costing approval              |
| Management            | Final approval for high-value or special jobs                       |

---

### Phase 3 — Design & Pre-Press
| Seq | Code       | Process Name      | Department                    | Responsible Role              | Approval | Client Approval |
|-----|------------|-------------------|-------------------------------|-------------------------------|----------|-----------------|
| 8   | `DES_DTP`  | Designing / DTP   | **PRE** (Pre-Press & Design)  | DTP Operator / Designer       | ✓        | —               |
| 9   | `PROOF`    | Client Proof      | **PRE** (Pre-Press & Design)  | Proof Coordinator             | ✓        | ✓ (Customer)    |
| 10  | `PRE_PRESS`| Plate Making      | **PRE** (Pre-Press & Design)  | CTP Operator                  | —        | —               |

**Responsibility Matrix — Phase 3:**
| Role                  | Responsibilities                                                    |
|-----------------------|---------------------------------------------------------------------|
| DTP Operator          | Create layouts, typesetting, artwork preparation                    |
| Designer              | Creative design, color management, image processing                 |
| Proof Coordinator     | Generate proofs, send to customer, track approval                   |
| CTP Operator          | Prepare plates from approved artwork, verify plate quality          |
| Customer              | Review proof, approve or request changes                            |

---

### Phase 4 — Procurement & Material Handling
| Seq | Code         | Process Name      | Department                    | Responsible Role              | Approval | Client Approval |
|-----|--------------|-------------------|-------------------------------|-------------------------------|----------|-----------------|
| 11  | `PROC`       | Procurement       | **PUR** (Purchase)            | Purchase Officer              | ✓        | — *(Disabled)*  |
| 12  | `GRN`        | Goods Receipt     | **INV** (Inventory)           | Store Keeper                  | ✓        | — *(Disabled)*  |
| 13  | `QC_IN`      | Incoming QC       | **QMS** (Quality)             | QC Inspector                  | ✓        | — *(Disabled)*  |
| 14  | `STORE_ISSUE`| Material Issue    | **INV** (Inventory)           | Store Keeper                  | —        | —               |

**Responsibility Matrix — Phase 4:**
| Role                  | Responsibilities                                                    |
|-----------------------|---------------------------------------------------------------------|
| Purchase Officer      | Create PO, negotiate with vendors, track deliveries                 |
| Store Keeper          | Receive material, verify quantities, issue to production            |
| QC Inspector          | Inspect incoming material, approve/reject based on specs            |

---

### Phase 5 — Production Planning
| Seq | Code       | Process Name      | Department                    | Responsible Role              | Approval | Client Approval |
|-----|------------|-------------------|-------------------------------|-------------------------------|----------|-----------------|
| 15  | `JOB_PLAN` | Production Planning| **PRT** (Printing)           | Production Planner            | —        | —               |
| 16  | `JOB_SCHED`| Job Scheduling    | **PRT** (Printing)            | Production Scheduler          | —        | —               |
| 17  | `JOB_CARD` | Job Card Issue    | **PRT** (Printing)            | Production Supervisor         | —        | —               |

**Responsibility Matrix — Phase 5:**
| Role                  | Responsibilities                                                    |
|-----------------------|---------------------------------------------------------------------|
| Production Planner    | Plan machine allocation, estimate time, coordinate resources        |
| Production Scheduler  | Create production schedule, optimize machine utilization            |
| Production Supervisor | Generate job card, distribute to floor, brief operators             |

---

### Phase 6 — Production Execution
| Seq | Code     | Process Name      | Department                    | Responsible Role              | Approval | Client Approval |
|-----|----------|-------------------|-------------------------------|-------------------------------|----------|-----------------|
| 18  | `CUT`    | Paper Cutting     | **FINP** (Post-Press)         | Cutting Operator              | —        | —               |
| 19  | `PRINT`  | Printing          | **PRT** (Printing)            | Press Operator                | —        | —               |
| 20  | `QC_PROC`| In-Process QC     | **QMS** (Quality)             | QC Inspector                  | —        | —               |
| 21  | `DRY`    | Drying / Curing   | **PRT** (Printing)            | Press Operator / Helper       | —        | —               |

**Responsibility Matrix — Phase 6:**
| Role                  | Responsibilities                                                    |
|-----------------------|---------------------------------------------------------------------|
| Cutting Operator      | Cut paper/board to required size, maintain guillotine               |
| Press Operator        | Run printing press, maintain color density, register sheets         |
| Machine Helper        | Load paper, collect prints, assist operator                         |
| QC Inspector          | Check print quality, color matching, registration, defects          |

---

### Phase 7 — Post-Press Finishing
| Seq | Code       | Process Name      | Department                    | Responsible Role              | Approval | Client Approval |
|-----|------------|-------------------|-------------------------------|-------------------------------|----------|-----------------|
| 22  | `POST_PRESS`| Post-Press       | **FINP** (Post-Press)         | Finishing Supervisor          | —        | —               |
| 23  | `FOLD`     | Folding           | **FINP** (Post-Press)         | Folding Operator              | —        | —               |
| 24  | `BIND`     | Binding           | **FINP** (Post-Press)         | Binding Operator              | —        | —               |
| 25  | `TRIM`     | Final Trim        | **FINP** (Post-Press)         | Cutting Operator              | —        | —               |
| 26  | `QC_POST`  | Post-Press QC     | **QMS** (Quality)             | QC Inspector                  | ✓        | —               |

**Responsibility Matrix — Phase 7:**
| Role                  | Responsibilities                                                    |
|-----------------------|---------------------------------------------------------------------|
| Finishing Supervisor  | Coordinate finishing operations, allocate resources                 |
| Folding Operator      | Operate folding machine, verify fold accuracy                       |
| Binding Operator      | Perfect binding, saddle stitch, case binding operations             |
| Cutting Operator      | Final trim to exact size, three-side trim                           |
| QC Inspector          | Final inspection before packing, approve/reject batches             |

---

### Phase 8 — Packing & Dispatch
| Seq | Code          | Process Name         | Department                    | Responsible Role              | Approval | Client Approval |
|-----|---------------|----------------------|-------------------------------|-------------------------------|----------|-----------------|
| 27  | `PACK`        | Packing              | **PKG** (Packaging)           | Packing Supervisor            | —        | —               |
| 28  | `LOAD`        | Loading              | **DSP** (Dispatch)            | Loading In-charge             | —        | —               |
| 29  | `CHALLAN`     | Delivery Challan     | **FIN** (Accounts)            | Accounts / Dispatch Clerk     | ✓        | —               |
| 30  | `GATE_PASS`   | Gate Pass            | **SEC** (Security)            | Security Guard                | ✓        | —               |
| 31  | `DISPATCH`    | Dispatch             | **DSP** (Dispatch)            | Dispatch Supervisor           | —        | —               |
| 32  | `DELIVERY_CONF`| Delivery Confirmation| **DSP** (Dispatch)           | Driver / Delivery Boy         | —        | ✓ (Customer)    |

**Responsibility Matrix — Phase 8:**
| Role                  | Responsibilities                                                    |
|-----------------------|---------------------------------------------------------------------|
| Packing Supervisor    | Pack goods, apply labels, verify quantities                         |
| Loading In-charge     | Load vehicles, verify against challan, optimize load                |
| Dispatch Clerk        | Generate delivery challan, coordinate with accounts                 |
| Security Guard        | Verify gate pass, check vehicle, record exit time                   |
| Dispatch Supervisor   | Track shipments, coordinate with customers, handle issues           |
| Driver / Delivery Boy | Deliver goods, collect POD, report delivery status                  |
| Customer              | Confirm receipt, sign POD, report any issues                        |

---

### Phase 9 — Billing & Finance
| Seq | Code         | Process Name    | Department                    | Responsible Role              | Approval | Client Approval |
|-----|--------------|-----------------|-------------------------------|-------------------------------|----------|-----------------|
| 33  | `BILL`       | Billing/Invoice | **FIN** (Accounts)            | Accounts Executive            | ✓        | ✓ (Customer)    |
| 34  | `PAY_REC`    | Payment Receipt | **FIN** (Accounts)            | Accounts Executive            | ✓        | —               |
| 35  | `CREDIT_NOTE`| Credit Note     | **FIN** (Accounts)            | Accounts Manager              | ✓        | —               |
| 36  | `DEBIT_NOTE` | Debit Note      | **FIN** (Accounts)            | Accounts Manager              | ✓        | —               |

**Responsibility Matrix — Phase 9:**
| Role                  | Responsibilities                                                    |
|-----------------------|---------------------------------------------------------------------|
| Accounts Executive    | Generate invoice, apply taxes, send to customer                     |
| Accounts Manager      | Approve credit/debit notes, handle disputes, reconcile              |
| Customer              | Verify invoice, process payment, report discrepancies               |

---

### Phase 10 — Costing & Closure
| Seq | Code             | Process Name      | Department                    | Responsible Role              | Approval | Client Approval |
|-----|------------------|-------------------|-------------------------------|-------------------------------|----------|-----------------|
| 37  | `STORE_RETURN`   | Material Return   | **INV** (Inventory)           | Store Keeper                  | —        | —               |
| 38  | `WASTE_ENTRY`    | Wastage Entry     | **INV** (Inventory)           | Store Keeper                  | —        | —               |
| 39  | `COST_FINAL`     | Final Costing     | **EST** (Estimation)          | Costing Manager               | ✓        | —               |
| 40  | `PROFIT_ANALYSIS`| Profit Analysis   | **FIN** (Accounts)            | Accounts Manager / CFO        | —        | —               |
| 41  | `JOB_CLOSE`      | Job Closure       | **CST** (Customer Service)    | CRM Executive                 | —        | —               |
| 42  | `JOB_ARCHIVE`    | Job Archive       | **IT** (IT & ERP)             | System Administrator          | —        | —               |

**Responsibility Matrix — Phase 10:**
| Role                  | Responsibilities                                                    |
|-----------------------|---------------------------------------------------------------------|
| Store Keeper          | Accept returned material, record wastage                            |
| Costing Manager       | Calculate actual cost, compare with estimate, analyze variance      |
| Accounts Manager      | Review profitability, report to management                          |
| CRM Executive         | Close job, notify customer, collect feedback                        |
| System Administrator  | Archive job data, maintain records, ensure compliance               |

---

## 4. Job Types & Their Workflow Variations

Each **Job Type** can have a custom workflow template. The `mst_job_type` table defines:

### Job Type Configuration Fields
```csharp
public class MstJobType
{
    public string Jobtypecode { get; set; }        // e.g., "BOOK", "BROCHURE", "VISITING"
    public string Jobtypename { get; set; }        // Display name
    
    // Process Requirement Flags
    public bool? Isdesignrequired { get; set; }    // Needs DES_DTP step
    public bool? Isdtprequired { get; set; }       // Needs DTP step
    public bool? Isctprequired { get; set; }       // Needs PRE_PRESS/CTP step
    public bool? Isprintingrequired { get; set; } // Needs PRINT step
    public bool? Isbindingrequired { get; set; }  // Needs BIND step
    public bool? Isfinishingrequired { get; set; }// Needs POST_PRESS steps
    
    // Workflow Modifiers
    public string? Printingmode { get; set; }      // "OFFSET", "DIGITAL", "SCREEN"
    public bool? Issingleprocess { get; set; }     // Skip intermediate steps
    public bool? Isfullprocess { get; set; }       // Full workflow required
    public bool? Iscustomermaterial { get; set; }  // Customer provides material
    public bool? Isinhousematerial { get; set; }   // Use internal stock
    public bool? Isoutsourcejob { get; set; }      // Send to external vendor
    public bool? Allowadvancepayment { get; set; } // Enable ADV_PAY step
    public bool? Requirecostingapproval { get; set; }// JOB_APPROVAL required
    
    // Default Start/End Points
    public string? Defaultstartprocesscode { get; set; } // e.g., "DES_DTP"
    public string? Defaultendprocesscode { get; set; }   // e.g., "DISPATCH"
}
```

### Example Job Type Workflows

| Job Type       | Start Process | Key Steps                              | End Process  |
|----------------|---------------|----------------------------------------|--------------|
| **Book**       | DES_DTP       | Full: DTP→Proof→CTP→Print→Bind→Trim   | JOB_ARCHIVE  |
| **Brochure**   | DES_DTP       | DTP→Proof→Print→Fold                  | DISPATCH     |
| **Visiting Card** | PRE_PRESS  | CTP→Print→Cut→Pack                    | DISPATCH     |
| **Calendar**   | DES_DTP       | DTP→Proof→Print→Bind→Pack             | DISPATCH     |
| **Pamphlet**   | DES_DTP       | DTP→Print→Fold→Pack                   | DISPATCH     |
| **Digital Only** | PRINT      | Print→QC→Pack                         | DISPATCH     |
| **Customer Material** | PRINT | Print→QC→Pack (skip procurement)      | DISPATCH     |

---

## 5. Three Job Entry Scenarios

The workflow engine supports **three distinct entry paths**:

### Scenario 1: Full Flow (Enquiry → Quotation → Job)
```
┌────────────┐    ┌────────────┐    ┌────────────┐    ┌────────────┐
│ ENQ_JOB    │───▸│ ENQ_EST    │───▸│ QUOT       │───▸│ JOB_CREATE │
│ (Enquiry)  │    │ (Costing)  │    │ (Quote)    │    │ (Job)      │
└────────────┘    └────────────┘    └────────────┘    └─────┬──────┘
                                                            │
                                                            ▼
                                    ┌──────────────────────────────┐
                                    │ Continue with JOB_APPROVAL → │
                                    │ DES_DTP → PROOF → ... etc.   │
                                    └──────────────────────────────┘
```

**Source Table Flow:**
- `trn_enquiry` → `trn_quotation` → `trn_job`
- Workflow tasks transition sources as documents convert

**Step Filtering:** All steps included (`applies_to_enquiry = TRUE`)

### Scenario 2: Direct Quotation → Job (Skip Enquiry)
```
┌────────────┐    ┌────────────┐    ┌────────────────────────────┐
│ QUOT       │───▸│ JOB_CREATE │───▸│ Continue production flow    │
│ (Quote)    │    │ (Job)      │    │ DES_DTP → PROOF → ...       │
└────────────┘    └────────────┘    └────────────────────────────┘
```

**Use Case:** Repeat orders, known specifications

**Step Filtering:** Steps where `applies_to_quotation = TRUE` (ENQ_JOB, ENQ_EST skipped)

### Scenario 3: Direct Manual Job (Skip Enquiry + Quotation)
```
┌────────────┐    ┌────────────────┐    ┌────────────────────────┐
│ JOB_CREATE │───▸│ JOB_APPROVAL   │───▸│ Continue production    │
│ (Job)      │    │ (Costing Appr) │    │ DES_DTP → PROOF → ...  │
└────────────┘    └────────────────┘    └────────────────────────┘
```

**Step Filtering:** Steps where `applies_to_job = TRUE` (ENQ_JOB, ENQ_EST, QUOT skipped)

**Logic in WorkspaceProcessEngine:**
```csharp
// ── Scenario 3: Manual job (no enquiry/quotation) requires costing approval ──
if (isJobSource && completedTask.ProcessCode == WkProcessCode.JobCreate)
{
    var isManualJob = (job?.EnquiryId == null || job.EnquiryId == 0)
                   && (job?.QuotationId == null || job.QuotationId == 0);
    
    if (isManualJob)
    {
        // Manual job → route to JOB_APPROVAL for costing approval
        await CreateWorkspaceTaskAsync(
            processCode: WkProcessCode.JobApproval, // Costing approval required
            ...
        );
    }
}
```

---

## 6. Process Codes Reference

### Process Code Constants (`WkProcessCode`)
```csharp
public static class WkProcessCode
{
    // ── Pre-Sales (seq 1, optional) ──
    public const string AdvPay = "ADV_PAY";         // seq 1 — Advance Payment (optional)

    // ── Pre-Sales (skipped after Job exists) ──
    public const string EnqJob = "ENQ_JOB";         // seq 2
    public const string EnqEst = "ENQ_EST";         // seq 3
    public const string Quot = "QUOT";               // seq 4
    public const string JobCreate = "JOB_CREATE";   // seq 6

    // ── Job Flow (starts here for existing jobs) ──
    public const string JobApproval = "JOB_APPROVAL"; // seq 7
    public const string DesDtp = "DES_DTP";           // seq 8
    public const string Proof = "PROOF";              // seq 9
    public const string PrePress = "PRE_PRESS";       // seq 10
    // ... (see full list in section 3)

    // ── Disabled Processes ──
    public static readonly string[] Disabled =
        [QuotAppr, QuotApproval, QuotationApproval, Proc, Grn, QcIn];

    // ── Pre-Job Processes (skipped when source is Job) ──
    // Includes ADV_PAY since advance payment is a pre-job step
    public static readonly string[] PreJobProcesses =
        [AdvPay, EnqJob, EnqEst, Quot, QuotAppr, QuotApproval, QuotationApproval, JobCreate];
}
```

### Why Pre-Job Processes Are Skipped
Once a job exists, the workflow **never loops back** to enquiry/quotation steps:
- Prevents duplicate enquiry tasks appearing after job card
- Maintains forward-only workflow progression
- Applied in both `WorkspaceProcessEngine` and `WorkspaceController.ResolveJobProcessStepsAsync()`

---

## 7. Workflow Engine Logic

### Core Components

#### 1. Task Creation (`CreateWorkspaceTaskAsync`)
```
Input: processCode, eventTypeCode, sourceTable, sourceId, ...
  │
  ├─▸ Skip if disabled process code
  ├─▸ Resolve job type context (for job-type-aware routing)
  ├─▸ Load notification config (mst_process_notification_config)
  ├─▸ Resolve target users via role-based routing
  ├─▸ Apply SLA, priority, task type from config
  ├─▸ Create TrnWorkspaceTask records
  ├─▸ Notify assigned users
  └─▸ Create item-level tasks for parallel processes
```

#### 2. Pre-Generated Workflow (`GenerateAllWorkflowTasksAsync`)
```
On Document Creation (Enquiry/Quotation/Job):
  │
  ├─▸ Find matching workflow template (by Job Type + Product Type)
  │
  ├─▸ Filter steps by source table:
  │   └── FilterStepsBySourceTable(steps, sourceTable)
  │       ├── TRN_ENQUIRY  → applies_to_enquiry = TRUE
  │       ├── TRN_QUOTATION → applies_to_quotation = TRUE
  │       └── TRN_JOB       → applies_to_job = TRUE
  │
  ├─▸ Create ALL tasks upfront:
  │   ├── First task: Status = PENDING (active)
  │   └── Rest: Status = QUEUED (waiting)
  │
  ├─▸ Set IsBlocking for each task:
  │   ├── From step config (mst_workflow_step.is_blocking)
  │   └── Override: Department 9999 → is_blocking = FALSE
  │
  └─▸ All tasks share same WorkflowBatchId (GUID)
```

#### 3. Sequential Activation (`ActivateNextQueuedTaskAsync`)
```
On Task Completion:
  │
  ├─▸ Check if ALL BLOCKING sibling tasks at same sequence are done
  │   (Non-blocking tasks don't prevent progression)
  │
  ├─▸ Find next QUEUED task by sequence_no
  │
  ├─▸ Activate: QUEUED → PENDING
  │
  └─▸ Notify assigned users
```

#### 4. Legacy Next-Step Generation (`GenerateNextStepTasksAsync`)
```
Fallback when no pre-generated workflow exists:
  │
  ├─▸ Check if ALL sibling tasks for this step are done
  │   (parallel tasks must all complete before next step)
  │
  ├─▸ Resolve job type context
  │
  ├─▸ Handle Scenario 3: Manual job → route to JOB_APPROVAL
  │
  ├─▸ Find next process:
  │   1. Try workflow template (job-type aware)
  │   2. Fallback to global sequence (mst_process.sequenceno)
  │
  ├─▸ Skip pre-job processes if source is already a Job
  │
  └─▸ Create workspace task for next process
```

#### 3. Process Resolution Priority
```
ResolveNextProcessFromWorkflowTemplateAsync():
  1. Find workflow template for Job Type + Product Type
  2. Get current step in template
  3. Follow mst_workflow_connection to find ToStepId
  4. If no connection, use SequenceNo ordering
  5. Return linked MstProcess

Fallback (if no template):
  - Use mst_process.sequenceno > current sequence
  - Skip disabled codes and pre-job processes
```

---

## 8. Template-Based Routing

### Workflow Template Structure

```sql
mst_workflow_template
├── workflow_template_id (PK)
├── workflow_code         -- "WF-BOOK-OFFSET"
├── workflow_name         -- "Book Printing - Offset"
├── job_type_id           -- FK → mst_job_type (e.g., 1 = Book)
├── print_product_type_id -- FK → mst_print_product_type
├── is_default            -- TRUE = fallback template
└── is_active

mst_workflow_step
├── workflow_step_id (PK)
├── workflow_template_id (FK)
├── process_id (FK → mst_process)
├── step_code            -- "STEP_DTP"
├── step_name            -- "Design & DTP"
├── step_type            -- START/PROCESS/APPROVAL/TASK/END
├── sequence_no          -- Execution order
├── department_id        -- Target department
├── assignment_rule      -- AUTO/MANUAL/ROUND_ROBIN/DEPT_HEAD
├── sla_hours            -- Time limit
├── notify_* flags       -- Notification toggles
├── canvas_x/y           -- Visual designer position
├── is_blocking          -- FALSE = workflow proceeds without waiting (default: TRUE)
├── applies_to_enquiry   -- Include when source is trn_enquiry (default: TRUE)
├── applies_to_quotation -- Include when source is trn_quotation (default: TRUE)
└── applies_to_job       -- Include when source is trn_job (default: TRUE)

mst_workflow_connection
├── connection_id (PK)
├── workflow_template_id (FK)
├── from_step_id (FK → mst_workflow_step)
├── to_step_id   (FK → mst_workflow_step)
├── condition_expression -- "approved==true", "qty>1000"
├── label                -- Display label
└── sequence_no          -- Order for multiple paths
```

### Template Resolution Logic
```csharp
private async Task<long?> ResolveWorkflowTemplateIdAsync(int? jobTypeId, int? productTypeId)
{
    return await _db.MstWorkflowTemplates
        .Where(t => t.IsActive)
        .Where(t =>
            (jobTypeId.HasValue && t.JobTypeId == jobTypeId.Value) ||
            t.IsDefault)
        .OrderByDescending(t => jobTypeId.HasValue && t.JobTypeId == jobTypeId.Value)
        .ThenByDescending(t => productTypeId.HasValue && t.PrintProductTypeId == productTypeId.Value)
        .ThenByDescending(t => t.IsDefault)
        .ThenByDescending(t => t.Version)
        .Select(t => t.WorkflowTemplateId)
        .FirstOrDefaultAsync();
}
```

**Resolution Priority:**
1. Exact match: Job Type ID
2. Exact match: Job Type ID + Product Type ID
3. Fallback: IsDefault = true
4. Highest Version if multiple defaults

---

## 9. Role-Based Task Assignment

### Routing Chain
```
ProcessCode
    │
    ├─▸ mst_process_role_map (which roles can handle this process)
    │   └── RoleId(s)
    │
    ├─▸ mst_process_department_map (which departments handle this process)
    │   └── DeptId(s)
    │
    └─▸ map_user_role + mst_user
        └── Users with matching Role AND Department
```

### Code Implementation
```csharp
private async Task<List<long>> ResolveTargetUsers(string processCode, UserSessionData triggeredBy)
{
    // Get roles allowed for this process
    var roleIds = await _db.MstProcessRoleMaps
        .Where(pr => pr.ProcessCode == processCode && pr.IsActive)
        .Select(pr => pr.Roleid)
        .ToListAsync();
    
    // Get departments allowed for this process
    var deptIds = await _db.MstProcessDepartmentMaps
        .Where(pd => pd.ProcessCode == processCode && pd.IsActive)
        .Select(pd => pd.DeptId)
        .ToListAsync();
    
    // Find users with matching roles AND departments
    var userIds = await _db.MapUserRoles
        .Where(ur => roleIds.Contains(ur.Roleid) && ur.Isactive)
        .Join(
            _db.MstUsers.Where(u => u.Isactive && deptIds.Contains(u.Departmentid)),
            ur => ur.Userid,
            u => u.Userid,
            (ur, u) => u.Userid)
        .Distinct()
        .ToListAsync();
    
    return userIds.Count > 0 ? userIds : [triggeredBy.UserId]; // Fallback
}
```

### Assignment Rules
| Rule         | Description                                              |
|--------------|----------------------------------------------------------|
| `AUTO`       | System picks least-loaded user in target department      |
| `MANUAL`     | Admin manually assigns during job creation               |
| `ROUND_ROBIN`| Rotate among active department users                     |
| `DEPT_HEAD`  | Always assign to department head / manager               |

---

## 10. Notification & SLA System

### Notification Configuration (`mst_process_notification_config`)
```sql
config_id, process_code, event_type_code,
job_type_id, job_type_code,  -- Job-type-specific config
event_label, body_template,
sla_hours, escalate_after_hours, escalate_to,
priority, approval_type_id, approval_level,
notify_assignee, notify_supervisor, notify_dept_head,
notify_vendor, notify_supplier, notify_customer,
send_email, send_sms, send_whatsapp, send_push,
auto_trigger, is_active
```

### Notification Matrix
| Recipient     | Email | SMS | WhatsApp | Push | Triggers                              |
|---------------|-------|-----|----------|------|---------------------------------------|
| Assignee      | ✓     | ✓   | ✓        | ✓    | Task assigned, deadline approaching   |
| Supervisor    | ✓     | —   | —        | ✓    | Escalation, approval needed           |
| Dept Head     | ✓     | —   | —        | ✓    | Critical alerts, escalations          |
| Vendor        | ✓     | ✓   | ✓        | —    | Material needed, outsource dispatch   |
| Supplier      | ✓     | ✓   | ✓        | —    | PO raised, delivery reminder          |
| Customer      | ✓     | ✓   | ✓        | —    | Proof approval, job status, delivery  |

### SLA & Escalation
```csharp
// Applied during task creation
var slaHours = config?.SlaHours ?? 12;
var dueDate = now.AddHours((double)slaHours);

task.DueDate = dueDate;
task.SlaHours = slaHours;
task.IsOverdue = false;  // Updated by background job

// Escalation config stored in metadata
meta["escalate_after_hours"] = config.EscalateAfterHours;
meta["escalate_to"] = config.EscalateTo;  // JSON: user IDs or roles
```

---

## 11. Conditional & Parallel Flows

### Conditional Paths
| Condition                  | Flow Change                              |
|----------------------------|------------------------------------------|
| Advance payment required   | `QUOT_APPR → ADV_PAY → JOB_CREATE`       |
| Material rejected at QC    | `QC_IN → PROC (re-procurement)`          |
| Proof rejected by client   | `PROOF → DES_DTP (redesign)`             |
| QC failed in production    | `QC_PROC → PRINT (reprint)`              |
| Post-press QC failed       | `QC_POST → POST_PRESS (redo finishing)`  |
| Delivery short             | `DELIVERY_CONF → CREDIT_NOTE`            |
| Approval rejected          | `*_APPROVAL → Cancel source document`    |

### Parallel Item-Level Tasks
Certain processes support **item-level parallel execution**:

```csharp
public static class WkParallelProcessCodes
{
    public static readonly string[] Eligible =
        [DesDtp, PrePress, Print, PostPress, Bind, Fold, Trim, Pack];
    
    public static readonly Dictionary<string, string> NextProcessPerItem = new()
    {
        [DesDtp] = PrePress,
        [PrePress] = Print,
        [Print] = PostPress,
        // ...
    };
}
```

**Item Task Structure:**
```sql
trn_workspace_task_item
├── task_item_id (PK)
├── workspace_task_id (FK → parent task)
├── job_id, job_item_id
├── process_code, process_name
├── item_name, item_sequence
├── task_status           -- PENDING/IN_PROGRESS/COMPLETED
├── assigned_user_id
├── started_on, completed_on
├── work_data              -- JSON: process-specific data
└── parent_task_item_id    -- Link to upstream item task
```

---

## 12. Database Schema

### Core Tables

```
┌─────────────────────┐     ┌─────────────────────┐
│    mst_process      │     │  mst_workflow_      │
│ ─────────────────── │     │     template        │
│ processid (PK)      │     │ ─────────────────── │
│ processcode         │     │ workflow_template_id│
│ processname         │     │ job_type_id (FK)    │
│ sequenceno          │     │ is_default          │
│ departmentid (FK)   │     │                     │
│ isapprovalrequired  │     └─────────┬───────────┘
│ ismandatory         │               │
└─────────────────────┘               │
         │                  ┌─────────▼───────────┐
         │                  │  mst_workflow_step  │
         └──────────────────│ ─────────────────── │
                            │ workflow_step_id    │
                            │ process_id (FK)     │
                            │ step_type           │
                            │ sequence_no         │
                            │ sla_hours           │
                            └─────────────────────┘
                                      │
                            ┌─────────▼─────────────┐
                            │mst_workflow_connection│
                            │ ───────────────────── │
                            │ from_step_id (FK)     │
                            │ to_step_id (FK)       │
                            │ condition_expression  │
                            └───────────────────────┘

┌─────────────────────┐     ┌─────────────────────┐
│ trn_workspace_task  │     │ mst_process_        │
│ ─────────────────── │     │ notification_config │
│ workspace_task_id   │     │ ─────────────────── │
│ user_id (FK)        │     │ process_code        │
│ process_code        │     │ event_type_code     │
│ source_table        │     │ job_type_id         │
│ source_id           │     │ sla_hours           │
│ job_id (FK)         │     │ notify_* flags      │
│ task_status         │     │ auto_trigger        │
│ due_date            │     └─────────────────────┘
│ is_overdue          │
└─────────────────────┘
```

### Workflow Status Groups (Dashboard)
| Group       | Process Codes                      |
|-------------|------------------------------------|
| Pre-Sales   | ENQ_JOB → QUOT_APPR                |
| Job Setup   | JOB_CREATE → PRE_PRESS             |
| Procurement | PROC → STORE_ISSUE                 |
| Production  | CUT → DRY                          |
| Finishing   | POST_PRESS → QC_POST               |
| Dispatch    | PACK → DELIVERY_CONF               |
| Finance     | BILL → PAY_REC                     |
| Closure     | WASTE_ENTRY → JOB_ARCHIVE          |

---

## 13. RACI Matrix — Complete Workflow

**RACI Legend:**
- **R** = Responsible (Does the work)
- **A** = Accountable (Final authority)
- **C** = Consulted (Provides input)
- **I** = Informed (Kept in loop)

| Process Code | CST | EST | PRE | PRT | FINP | PKG | DSP | QMS | INV | FIN | MGT | IT | SEC | Customer |
|--------------|-----|-----|-----|-----|------|-----|-----|-----|-----|-----|-----|----|----|----------|
| ENQ_JOB      | R/A | C   | —   | —   | —    | —   | —   | —   | —   | —   | I   | —  | —  | C        |
| ENQ_EST      | C   | R/A | C   | C   | —    | —   | —   | —   | C   | C   | I   | —  | —  | —        |
| QUOT         | C   | R/A | —   | —   | —    | —   | —   | —   | —   | C   | I   | —  | —  | I        |
| JOB_CREATE   | R/A | C   | I   | I   | —    | —   | —   | —   | I   | I   | I   | —  | —  | I        |
| JOB_APPROVAL | C   | C   | —   | —   | —    | —   | —   | —   | —   | C   | A   | —  | —  | R        |
| DES_DTP      | I   | C   | R/A | —   | —    | —   | —   | —   | —   | —   | —   | —  | —  | C        |
| PROOF        | I   | —   | R/A | —   | —    | —   | —   | —   | —   | —   | —   | —  | —  | A        |
| PRE_PRESS    | —   | —   | R/A | —   | —    | —   | —   | —   | —   | —   | —   | —  | —  | —        |
| STORE_ISSUE  | —   | —   | —   | C   | —    | —   | —   | —   | R/A | —   | —   | —  | —  | —        |
| JOB_PLAN     | —   | —   | C   | R/A | C    | —   | —   | —   | C   | —   | —   | —  | —  | —        |
| JOB_SCHED    | —   | —   | —   | R/A | C    | —   | —   | —   | —   | —   | —   | —  | —  | —        |
| JOB_CARD     | —   | —   | —   | R/A | I    | —   | —   | —   | I   | —   | —   | —  | —  | —        |
| CUT          | —   | —   | —   | C   | R/A  | —   | —   | C   | —   | —   | —   | —  | —  | —        |
| PRINT        | —   | —   | —   | R/A | —    | —   | —   | C   | —   | —   | —   | —  | —  | —        |
| QC_PROC      | —   | —   | —   | C   | —    | —   | —   | R/A | —   | —   | —   | —  | —  | —        |
| DRY          | —   | —   | —   | R/A | —    | —   | —   | —   | —   | —   | —   | —  | —  | —        |
| POST_PRESS   | —   | —   | —   | —   | R/A  | —   | —   | C   | —   | —   | —   | —  | —  | —        |
| FOLD         | —   | —   | —   | —   | R/A  | —   | —   | —   | —   | —   | —   | —  | —  | —        |
| BIND         | —   | —   | —   | —   | R/A  | —   | —   | C   | —   | —   | —   | —  | —  | —        |
| TRIM         | —   | —   | —   | —   | R/A  | —   | —   | —   | —   | —   | —   | —  | —  | —        |
| QC_POST      | —   | —   | —   | —   | C    | —   | —   | R/A | —   | —   | —   | —  | —  | —        |
| PACK         | —   | —   | —   | —   | —    | R/A | C   | C   | —   | —   | —   | —  | —  | —        |
| LOAD         | —   | —   | —   | —   | —    | C   | R/A | —   | —   | —   | —   | —  | —  | —        |
| CHALLAN      | —   | —   | —   | —   | —    | —   | C   | —   | —   | R/A | —   | —  | —  | —        |
| GATE_PASS    | —   | —   | —   | —   | —    | —   | C   | —   | —   | —   | —   | —  | R/A| —        |
| DISPATCH     | —   | —   | —   | —   | —    | —   | R/A | —   | —   | I   | —   | —  | —  | I        |
| DELIVERY_CONF| I   | —   | —   | —   | —    | —   | R/A | —   | —   | I   | —   | —  | —  | A        |
| BILL         | C   | —   | —   | —   | —    | —   | —   | —   | —   | R/A | I   | —  | —  | I        |
| PAY_REC      | I   | —   | —   | —   | —    | —   | —   | —   | —   | R/A | I   | —  | —  | R        |
| STORE_RETURN | —   | —   | —   | C   | C    | —   | —   | —   | R/A | —   | —   | —  | —  | —        |
| WASTE_ENTRY  | —   | —   | —   | C   | C    | —   | —   | —   | R/A | —   | —   | —  | —  | —        |
| COST_FINAL   | —   | R/A | —   | C   | C    | —   | —   | —   | C   | C   | I   | —  | —  | —        |
| PROFIT_ANALYSIS| I | C   | —   | —   | —    | —   | —   | —   | —   | R/A | A   | —  | —  | —        |
| JOB_CLOSE    | R/A | C   | —   | —   | —    | —   | C   | —   | —   | C   | I   | —  | —  | I        |
| JOB_ARCHIVE  | I   | —   | —   | —   | —    | —   | —   | —   | —   | —   | —   | R/A| —  | —        |

---

## 14. Department Flow Diagram

```
                                    ┌─────────────────────────────────────────────────────────────────┐
                                    │                         CUSTOMER                                │
                                    │  (Enquiry → Proof Approval → Job Approval → Payment)            │
                                    └───────────────────────────────┬─────────────────────────────────┘
                                                                    │
                 ┌──────────────────────────────────────────────────┼──────────────────────────────────────────────────┐
                 │                                                  │                                                  │
                 ▼                                                  ▼                                                  ▼
    ┌────────────────────────┐                         ┌────────────────────────┐                         ┌────────────────────────┐
    │   SALES & CRM (CST)    │                         │   ESTIMATION (EST)     │                         │   ACCOUNTS (FIN)       │
    │ ────────────────────── │                         │ ────────────────────── │                         │ ────────────────────── │
    │ • ENQ_JOB (Enquiry)    │─────────────────────────▸ • ENQ_EST (Costing)    │                         │ • ADV_PAY (Advance)    │
    │ • JOB_CREATE (Job)     │◀─────────────────────────│ • QUOT (Quotation)     │                         │ • CHALLAN (DC)         │
    │ • JOB_CLOSE (Close)    │                         │ • COST_FINAL (Costing) │                         │ • BILL (Invoice)       │
    └────────────┬───────────┘                         └────────────────────────┘                         │ • PAY_REC (Payment)    │
                 │                                                                                        │ • CREDIT_NOTE          │
                 │                                                                                        │ • DEBIT_NOTE           │
                 │                                                                                        │ • PROFIT_ANALYSIS      │
                 │                                                                                        └────────────────────────┘
                 │
                 ▼
    ┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
    │                                              PRODUCTION FLOW                                                               │
    │ ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────│
    │                                                                                                                            │
    │   ┌──────────────────┐      ┌──────────────────┐      ┌──────────────────┐      ┌──────────────────┐                      │
    │   │ PRE-PRESS (PRE)  │      │ PRINTING (PRT)   │      │ POST-PRESS (FINP)│      │ PACKAGING (PKG)  │                      │
    │   │ ──────────────── │      │ ──────────────── │      │ ──────────────── │      │ ──────────────── │                      │
    │   │ • DES_DTP        │─────▸│ • JOB_PLAN       │─────▸│ • CUT            │─────▸│ • PACK           │                      │
    │   │ • PROOF          │      │ • JOB_SCHED      │      │ • POST_PRESS     │      └────────┬─────────┘                      │
    │   │ • PRE_PRESS      │      │ • JOB_CARD       │      │ • FOLD           │               │                                │
    │   └──────────────────┘      │ • PRINT          │      │ • BIND           │               │                                │
    │                             │ • DRY            │      │ • TRIM           │               │                                │
    │                             └──────────────────┘      └──────────────────┘               │                                │
    │                                                                                          │                                │
    │   ┌──────────────────┐                                                                   │                                │
    │   │ QUALITY (QMS)    │      Cross-cutting: Quality checks at multiple stages             │                                │
    │   │ ──────────────── │      • QC_PROC (In-Process QC) — after PRINT                      │                                │
    │   │ • QC_PROC        │      • QC_POST (Final QC) — after TRIM                            │                                │
    │   │ • QC_POST        │                                                                   │                                │
    │   └──────────────────┘                                                                   │                                │
    │                                                                                          │                                │
    └──────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────┘
                                                                                               │
                                                                                               ▼
    ┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
    │                                              DISPATCH & DELIVERY                                                           │
    │ ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────│
    │                                                                                                                            │
    │   ┌──────────────────┐      ┌──────────────────┐      ┌──────────────────┐      ┌──────────────────┐                      │
    │   │ DISPATCH (DSP)   │      │ SECURITY (SEC)   │      │ INVENTORY (INV)  │      │ IT & ERP (IT)    │                      │
    │   │ ──────────────── │      │ ──────────────── │      │ ──────────────── │      │ ──────────────── │                      │
    │   │ • LOAD           │─────▸│ • GATE_PASS      │      │ • STORE_ISSUE    │      │ • JOB_ARCHIVE    │                      │
    │   │ • DISPATCH       │      └──────────────────┘      │ • STORE_RETURN   │      └──────────────────┘                      │
    │   │ • DELIVERY_CONF  │                                │ • WASTE_ENTRY    │                                                │
    │   └──────────────────┘                                └──────────────────┘                                                │
    │                                                                                                                            │
    └────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 15. Process-Department Quick Reference

### By Department — Complete List

#### Customer Service & CRM (CST) — Dept ID: 1007
| Process Code  | Process Name    | Mandatory | Approval Required | Client Approval |
|---------------|-----------------|-----------|-------------------|-----------------|
| `ENQ_JOB`     | Enquiry Creation| ✓         | ✓                 | ✓               |
| `JOB_CREATE`  | Job Creation    | ✓         | ✓                 | —               |
| `JOB_CLOSE`   | Job Closure     | ✓         | —                 | —               |

#### Estimation & Costing (EST) — Dept ID: 1008
| Process Code  | Process Name        | Mandatory | Approval Required | Client Approval |
|---------------|---------------------|-----------|-------------------|-----------------|
| `ENQ_EST`     | Estimation/Costing  | ✓         | ✓                 | —               |
| `QUOT`        | Quotation Generation| ✓         | ✓                 | ✓               |
| `COST_FINAL`  | Final Costing       | ✓         | ✓                 | —               |

#### Pre-Press & Design (PRE) — Dept ID: 1009
| Process Code  | Process Name    | Mandatory | Approval Required | Client Approval |
|---------------|-----------------|-----------|-------------------|-----------------|
| `DES_DTP`     | Designing / DTP | ✓         | ✓                 | —               |
| `PROOF`       | Client Proof    | ✓         | ✓                 | ✓               |
| `PRE_PRESS`   | Plate Making    | ✓         | —                 | —               |

#### Printing (PRT) — Dept ID: 1010
| Process Code  | Process Name      | Mandatory | Approval Required | Client Approval |
|---------------|-------------------|-----------|-------------------|-----------------|
| `JOB_PLAN`    | Production Planning| ✓        | —                 | —               |
| `JOB_SCHED`   | Job Scheduling    | ✓         | —                 | —               |
| `JOB_CARD`    | Job Card Issue    | ✓         | —                 | —               |
| `PRINT`       | Printing          | ✓         | —                 | —               |
| `DRY`         | Drying            | ✓         | —                 | —               |

#### Post-Press & Finishing (FINP) — Dept ID: 1011
| Process Code  | Process Name    | Mandatory | Approval Required | Client Approval |
|---------------|-----------------|-----------|-------------------|-----------------|
| `CUT`         | Paper Cutting   | ✓         | —                 | —               |
| `POST_PRESS`  | Post Press      | ✓         | —                 | —               |
| `FOLD`        | Folding         | ✓         | —                 | —               |
| `BIND`        | Binding         | ✓         | —                 | —               |
| `TRIM`        | Final Trim      | ✓         | —                 | —               |

#### Packaging (PKG) — Dept ID: 1012
| Process Code  | Process Name    | Mandatory | Approval Required | Client Approval |
|---------------|-----------------|-----------|-------------------|-----------------|
| `PACK`        | Packing         | ✓         | —                 | —               |

#### Dispatch & Logistics (DSP) — Dept ID: 1013
| Process Code    | Process Name         | Mandatory | Approval Required | Client Approval |
|-----------------|----------------------|-----------|-------------------|-----------------|
| `LOAD`          | Loading              | ✓         | —                 | —               |
| `DISPATCH`      | Dispatch             | ✓         | —                 | —               |
| `DELIVERY_CONF` | Delivery Confirmation| ✓         | —                 | ✓               |

#### Inventory & Stores (INV) — Dept ID: 1014
| Process Code    | Process Name    | Mandatory | Approval Required | Client Approval |
|-----------------|-----------------|-----------|-------------------|-----------------|
| `STORE_ISSUE`   | Material Issue  | ✓         | —                 | —               |
| `STORE_RETURN`  | Material Return | —         | —                 | —               |
| `WASTE_ENTRY`   | Wastage Entry   | —         | —                 | —               |

#### Quality Management (QMS) — Dept ID: 1016
| Process Code  | Process Name    | Mandatory | Approval Required | Client Approval |
|---------------|-----------------|-----------|-------------------|-----------------|
| `QC_PROC`     | In-Process QC   | ✓         | —                 | —               |
| `QC_POST`     | Post-Press QC   | ✓         | ✓                 | —               |

#### Accounts & Finance (FIN) — Dept ID: 1004
| Process Code    | Process Name    | Mandatory | Approval Required | Client Approval |
|-----------------|-----------------|-----------|-------------------|-----------------|
| `ADV_PAY`       | Advance Payment | —         | ✓                 | —               |
| `CHALLAN`       | Delivery Challan| ✓         | ✓                 | —               |
| `BILL`          | Billing/Invoice | ✓         | ✓                 | ✓               |
| `PAY_REC`       | Payment Receipt | ✓         | ✓                 | —               |
| `CREDIT_NOTE`   | Credit Note     | —         | ✓                 | —               |
| `DEBIT_NOTE`    | Debit Note      | —         | ✓                 | —               |
| `PROFIT_ANALYSIS`| Profit Analysis| ✓         | —                 | —               |

#### Security (SEC) — Dept ID: 1018
| Process Code  | Process Name    | Mandatory | Approval Required | Client Approval |
|---------------|-----------------|-----------|-------------------|-----------------|
| `GATE_PASS`   | Gate Pass       | ✓         | ✓                 | —               |

#### IT & ERP (IT) — Dept ID: 1005
| Process Code  | Process Name    | Mandatory | Approval Required | Client Approval |
|---------------|-----------------|-----------|-------------------|-----------------|
| `JOB_ARCHIVE` | Job Archive     | ✓         | —                 | —               |

#### Top Management (MGT) — Dept ID: 1001
| Process Code  | Process Name       | Mandatory | Approval Required | Client Approval |
|---------------|--------------------|-----------|-------------------|-----------------|
| `QUOT_APPR`   | Quotation Approval | ✓         | ✓                 | — *(Disabled)*  |

#### Party/Customer (PTY) — Dept ID: 9999
| Process Code   | Process Name    | Mandatory | Approval Required | Client Approval |
|----------------|-----------------|-----------|-------------------|-----------------|
| `JOB_APPROVAL` | Job Approval    | —         | ✓                 | ✓               |

---

## Summary

MinePress ERP's workflow system provides:

1. **Flexible Job-Type Routing** — Different job types follow customized workflow templates
2. **Three Entry Points** — Full enquiry flow, direct quotation, or manual job creation
3. **Automatic Progression** — Tasks auto-generate as previous steps complete
4. **Role-Based Assignment** — Process → Role → Department → User mapping
5. **Multi-Channel Notifications** — Email, SMS, WhatsApp, Push per step/recipient
6. **SLA Management** — Per-step time limits with escalation
7. **Parallel Execution** — Item-level tasks for production processes
8. **Visual Designer** — Drag-and-drop workflow configuration

---

*Document Version: 1.0 | Generated: 2025 | MinePress ERP*
