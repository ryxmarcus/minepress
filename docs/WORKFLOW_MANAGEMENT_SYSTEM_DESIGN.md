# Job Workflow Management System — Design Document
## MinePress ERP | Version 1.0

---

## 1. Executive Summary

The **Job Workflow Management System** is a visual, drag-and-drop workflow designer that enables
MinePress ERP administrators to define, manage, and automate the routing of jobs through processes,
sub-processes, approvals, and tasks — based on **Job Type** and **Product Type** combinations.

### Key Capabilities
- **Visual Workflow Designer** — SVG-based canvas with drag-and-drop nodes, connectable links,
  property panels, and zoom/pan controls.
- **Job-Type × Product-Type Matrix** — Each workflow template targets a specific combination,
  ensuring the correct processes fire for every job.
- **Automatic Routing** — Steps auto-assign to departments and users based on configurable rules
  (Auto, Manual, Round-Robin, Department Head).
- **Multi-Channel Notifications** — Per-step toggles for Email, SMS, WhatsApp, Push, and in-app
  alerts to Vendors, Suppliers, Customers, and internal Users.
- **Approval Engine** — Integrates with existing `mst_approval_type` and `mst_approval_level`
  tables for multi-level approval chains.
- **SLA & Escalation** — Configurable SLA hours per step with automatic escalation.
- **AI Suggestions** — Recommends next-best steps and optimal routing based on historical patterns.

---

## 2. Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                     Razor Page (UI Layer)                       │
│  ┌──────────┐  ┌─────────────────┐  ┌───────────────────────┐  │
│  │ Palette  │  │  SVG Canvas     │  │  Properties Panel     │  │
│  │ (Nodes)  │──│  (Drag & Drop)  │──│  (Config / Notify)    │  │
│  └──────────┘  └────────┬────────┘  └───────────────────────┘  │
│                         │                                       │
├─────────────────────────┼───────────────────────────────────────┤
│             WorkflowController (API)                            │
│  GET/POST/PUT/DELETE  /api/workflow/*                            │
├─────────────────────────┼───────────────────────────────────────┤
│             ApplicationDbContext (Persistence)                   │
│  ┌────────────────────┐ ┌──────────────────┐ ┌───────────────┐ │
│  │MstWorkflowTemplate │ │ MstWorkflowStep  │ │MstWorkflowConn│ │
│  └────────────────────┘ └──────────────────┘ └───────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│                  PostgreSQL (press_db)                           │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. Database Schema

### 3.1 `mst_workflow_template`
| Column                  | Type         | Description                                   |
|-------------------------|-------------|-----------------------------------------------|
| workflow_template_id    | BIGSERIAL PK | Auto-increment primary key                    |
| workflow_code           | VARCHAR(50)  | Unique business code (e.g., WF-BOOK-OFFSET)   |
| workflow_name           | VARCHAR(200) | Descriptive name                              |
| description             | TEXT         | Optional notes                                |
| job_type_id             | INT FK       | → mst_job_type.jobtypeid                      |
| print_product_type_id   | INT FK       | → mst_print_product_type.printproducttypeid   |
| is_default              | BOOLEAN      | Default workflow for this combination          |
| version                 | INT          | Version number for audit trail                |
| is_active               | BOOLEAN      | Soft-delete flag                              |
| created_by              | VARCHAR(100) | Audit: who created                            |
| created_on              | TIMESTAMP    | Audit: when created                           |
| modified_by             | VARCHAR(100) | Audit: last modifier                          |
| modified_on             | TIMESTAMP    | Audit: last modified                          |

### 3.2 `mst_workflow_step`
| Column                  | Type         | Description                                   |
|-------------------------|-------------|-----------------------------------------------|
| workflow_step_id        | BIGSERIAL PK | Auto-increment primary key                    |
| workflow_template_id    | BIGINT FK    | → mst_workflow_template                       |
| process_id              | INT FK       | → mst_process.processid (nullable)            |
| sub_process_id          | INT FK       | → mst_sub_process.subprocessid (nullable)     |
| step_code               | VARCHAR(50)  | Unique code within workflow                   |
| step_name               | VARCHAR(200) | Display name                                  |
| step_type               | VARCHAR(30)  | START / PROCESS / APPROVAL / TASK / NOTIFICATION / DECISION / END |
| sequence_no             | INT          | Execution order                               |
| department_id           | BIGINT FK    | → mst_department.dept_id                      |
| assigned_user_id        | BIGINT FK    | → mst_user.userid (optional direct assign)    |
| assignment_rule         | VARCHAR(30)  | AUTO / MANUAL / ROUND_ROBIN / DEPT_HEAD       |
| approval_type_id        | INT FK       | → mst_approval_type (for APPROVAL steps)      |
| approval_level_id       | INT FK       | → mst_approval_level (for APPROVAL steps)     |
| is_mandatory            | BOOLEAN      | Cannot be skipped                             |
| sla_hours               | DECIMAL      | Max hours to complete this step               |
| escalate_after_hours    | DECIMAL      | Hours before auto-escalation fires            |
| escalate_to             | VARCHAR(200) | JSON: escalation targets                      |
| notify_vendor           | BOOLEAN      | Send notification to vendor                   |
| notify_supplier         | BOOLEAN      | Send notification to supplier                 |
| notify_customer         | BOOLEAN      | Send notification to customer                 |
| notify_assigned_user    | BOOLEAN      | Send notification to assigned user            |
| notify_dept_head        | BOOLEAN      | Send notification to department head          |
| send_email              | BOOLEAN      | Channel: Email                                |
| send_sms                | BOOLEAN      | Channel: SMS                                  |
| send_whatsapp           | BOOLEAN      | Channel: WhatsApp                             |
| send_push_notification  | BOOLEAN      | Channel: Push/In-App                          |
| canvas_x                | DOUBLE       | Visual designer X position                    |
| canvas_y                | DOUBLE       | Visual designer Y position                    |
| node_color              | VARCHAR(20)  | Visual designer node color                    |
| is_active               | BOOLEAN      | Soft-delete flag                              |
| created_by              | VARCHAR(100) | Audit                                         |
| created_on              | TIMESTAMP    | Audit                                         |

### 3.3 `mst_workflow_connection`
| Column                  | Type         | Description                                   |
|-------------------------|-------------|-----------------------------------------------|
| connection_id           | BIGSERIAL PK | Auto-increment primary key                    |
| workflow_template_id    | BIGINT FK    | → mst_workflow_template                       |
| from_step_id            | BIGINT FK    | → mst_workflow_step (source)                  |
| to_step_id              | BIGINT FK    | → mst_workflow_step (target)                  |
| condition_expression    | TEXT         | Optional: "approved==true", "qty>1000"        |
| label                   | VARCHAR(200) | Displayed on the connection line              |
| sequence_no             | INT          | Order for multiple outbound connections       |
| is_active               | BOOLEAN      | Soft-delete                                   |

---

## 4. UI Components

### 4.1 Visual Workflow Designer Layout
```
┌─────────────────────────────────────────────────────────────────────┐
│ TOOLBAR: [Save] [Load] [Clear] [Undo] [Redo] [Zoom+] [Zoom-]     │
│          [Auto-Layout] [AI Suggest] [Export]                        │
├──────────┬──────────────────────────────────┬───────────────────────┤
│ PALETTE  │          SVG CANVAS              │   PROPERTIES PANEL    │
│          │                                  │                       │
│ ○ Start  │    ┌───────┐     ┌───────┐      │ ▸ Step Name           │
│ □ Process│    │ Start │────▸│Design │      │ ▸ Step Type           │
│ ◇ Approve│    └───────┘     └───┬───┘      │ ▸ Department          │
│ ☐ Task   │                      │          │ ▸ Assigned User       │
│ ✉ Notify │              ┌───────▼───┐      │ ▸ Assignment Rule     │
│ ◆ Decide │              │  Approval │      │ ▸ Approval Type       │
│ ● End    │              └───────┬───┘      │ ▸ SLA Hours           │
│          │                      │          │ ▸ Escalation          │
│ ─────────│              ┌───────▼───┐      │ ──── Notifications ── │
│ TEMPLATES│              │  Printing │      │ ☐ Vendor  ☐ Supplier  │
│ ─────────│              └───────┬───┘      │ ☐ Customer ☐ User     │
│ Book     │                      │          │ ── Channels ────────  │
│ Brochure │              ┌───────▼───┐      │ ☐ Email ☐ SMS         │
│ Pamphlet │              │    End    │      │ ☐ WhatsApp ☐ Push     │
│ Visiting │              └───────────┘      │                       │
│ Calendar │                                  │ [Apply Changes]       │
├──────────┴──────────────────────────────────┴───────────────────────┤
│ STATUSBAR: Template: WF-BOOK-OFFSET | Steps: 5 | Saved ✓           │
└─────────────────────────────────────────────────────────────────────┘
```

### 4.2 Node Types & Visual Styles
| Type          | Shape   | Default Color   | Icon            |
|---------------|---------|-----------------|-----------------|
| START         | Circle  | #4CAF50 (Green) | bi-play-fill    |
| PROCESS       | Rect    | #2196F3 (Blue)  | bi-gear-fill    |
| APPROVAL      | Diamond | #FF9800 (Amber) | bi-check-circle |
| TASK          | Rect    | #9C27B0 (Purple)| bi-list-task    |
| NOTIFICATION  | Rect    | #00BCD4 (Cyan)  | bi-bell-fill    |
| DECISION      | Diamond | #F44336 (Red)   | bi-signpost     |
| END           | Circle  | #607D8B (Grey)  | bi-stop-fill    |

### 4.3 Connection Types
- **Default Flow** → Solid line with arrow
- **Conditional** → Dashed line with label showing condition
- **Approval Yes** → Green solid line
- **Approval No** → Red dashed line

---

## 5. API Endpoints

| Method | Endpoint                        | Description                              |
|--------|---------------------------------|------------------------------------------|
| GET    | /api/workflow/templates         | List all workflow templates               |
| GET    | /api/workflow/templates/{id}    | Get template with steps & connections     |
| POST   | /api/workflow/templates         | Create new workflow template              |
| PUT    | /api/workflow/templates/{id}    | Update entire workflow (steps+conns)      |
| DELETE | /api/workflow/templates/{id}    | Soft-delete a workflow template           |
| POST   | /api/workflow/templates/{id}/duplicate | Clone a workflow template          |
| GET    | /api/workflow/lookups           | Fetch all lookup data for dropdowns       |
| POST   | /api/workflow/ai-suggest        | AI: suggest next steps for a job type     |

---

## 6. Notification Matrix

| Recipient     | Email | SMS | WhatsApp | Push | When Triggered                          |
|---------------|-------|-----|----------|------|-----------------------------------------|
| Vendor        | ✓     | ✓   | ✓        | —    | Material needed, outsource dispatch     |
| Supplier      | ✓     | ✓   | ✓        | —    | PO raised, delivery reminder            |
| Customer      | ✓     | ✓   | ✓        | —    | Proof approval, job status, delivery    |
| Assigned User | ✓     | ✓   | ✓        | ✓    | Task assigned, deadline approaching     |
| Dept Head     | ✓     | —   | —        | ✓    | Escalation, approval needed             |
| All           | ✓     | —   | —        | ✓    | Critical alerts, system notifications   |

---

## 7. Auto-Assignment Rules

| Rule         | Description                                                    |
|--------------|----------------------------------------------------------------|
| AUTO         | System picks the least-loaded user in the target department    |
| MANUAL       | Admin manually assigns during job creation                     |
| ROUND_ROBIN  | Rotate assignment among active department users                |
| DEPT_HEAD    | Always assign to the department head / reporting manager       |

---

## 8. AI Suggestions Engine

The AI suggestion feature analyzes:
1. **Historical job data** — Which processes were used for similar job types
2. **Completion times** — Average SLA adherence per department
3. **Resource availability** — Current workload of departments/users
4. **Success patterns** — Most efficient workflow paths

Output: Recommended step sequence with confidence scores.

---

## 9. Files Created/Modified

### New Files
| File                                                              | Purpose                          |
|-------------------------------------------------------------------|----------------------------------|
| `persistence/Models/MstWorkflowTemplate.cs`                      | Template entity                  |
| `persistence/Models/MstWorkflowStep.cs`                          | Step entity                      |
| `persistence/Models/MstWorkflowConnection.cs`                    | Connection entity                |
| `web/Controllers/WorkflowController.cs`                          | API endpoints                    |
| `web/Pages/Workflow/Index.cshtml`                                 | Visual designer page             |
| `web/Pages/Workflow/Index.cshtml.cs`                              | Page model                       |
| `web/wwwroot/css/workflow.css`                                    | Designer styles                  |
| `web/wwwroot/js/workflow-designer.js`                             | Designer logic                   |
| `sql/001_create_workflow_tables.sql`                              | Database migration               |

### Modified Files
| File                                                              | Change                           |
|-------------------------------------------------------------------|----------------------------------|
| `persistence/Context/ApplicationDbContext.cs`                     | Add DbSets                      |
| `web/Pages/Shared/_Layout.cshtml`                                 | Add nav link                     |

---

## 10. Technology Stack

- **Backend**: .NET 9, Razor Pages, EF Core (PostgreSQL)
- **Frontend**: SVG Canvas, jQuery, Bootstrap 5 (Tabler theme), Bootstrap Icons
- **Notifications**: Existing `INotificationDispatcher` service
- **Storage**: PostgreSQL with JSONB for flexible config fields

---

*Document Version: 1.0 | Created: 2025 | Author: MinePress ERP Team*
