Core Requirement — Designing / DTP as Task (Not Approval)

Designing / DTP must behave as a **Startable Task**, not an approval step.

## Behavior

When **Designing / DTP** task is available:

* Display it as:

text
Task Type: Start Task
Task Name: Designing / DTP


* User clicks **Start**
* A **new page** opens.

---

# Designing Selection Screen

On opening Designing/DTP page:

Display list of **Design Items** selected during:

* Enquiry
* Quotation
* Job Creation

Examples:

* Cover Page Design
* Inner Content DTP
* Artwork Preparation
* Book Layout
* Packaging Artwork

These may be:

* Single item
  OR
* Multiple items

---

# User Workflow — Designing

User must:

1. Select one design item
2. Start designing
3. Complete selected design item
4. Repeat until all design items are completed

When:

text
All Design Items = Completed


Then:

text
Designing / DTP Task = Completed


Update:

* Job Timeline
* Activity Log

---

# Parallel Execution Requirement

System must support:

text
Multiple tasks running simultaneously


Not sequential-only workflow.

---

# Example Parallel Tasks

Allow simultaneous execution of:

text
Designing Cover Page
Designing Book Content
Artwork Preparation


These tasks must run independently.

---

# CTP Dependency Logic

CTP task must start **per completed design item**, not after full design completion.

## Example Flow

text
Design: Cover Page → Completed
            ↓
CTP: Cover Page → Starts

Design: Book Content → Still Running


Meaning:

* CTP starts immediately for completed design item.
* No need to wait for all designs.

---

# Multi-Level Parallel Workflow

Support simultaneous running of:

text
Designing
CTP
Post Press


All may be:

text
Status = Running


at the same time.

---

# Example Full Workflow Scenario

Example:

text
Design Cover Page → Completed
        ↓
CTP Cover Page → Started
        ↓
Post Press Cover Page → Started

Meanwhile:

Design Book Content → Running


System must support this state:

text
Designing = Running
CTP = Running
Post Press = Running


---

# Post Press Dependency

Post Press must start:

text
After CTP completion (per item)


Not after full job completion.

---

# Task Completion Rules

Each task must be:

text
Closed Individually


Not globally.

Example:

text
Cover Page Design → Completed
CTP Cover Page → Completed
Post Press Cover Page → Completed


Meanwhile:

text
Book Content Design → Still Running


Allowed.

---

# Task Status Model

Each task must support:

text
Not Started
Running
Completed
Closed


Per item.

Not per job only.

---

# UI Requirements

Implement:

## Task Dashboard View

Display:

text
Task Name
Item Name
Status
Start Button
Complete Button
Progress Indicator


Example:

text
Designing - Cover Page        Running
Designing - Book Content      Running
CTP - Cover Page              Completed
Post Press - Cover Page       Running


---

# Visual Workflow Requirements

UI must support:

* Parallel task visualization
* Step-based progress indicators
* Per-item status tracking
* Real-time updates

---

# Data Model Requirement

Tasks must be stored:

text
Per Job
Per Item
Per Task Type


Example:

text
JobId
ItemId
TaskType (Designing / CTP / PostPress)
Status
StartTime
EndTime
AssignedUser


---

# Dependency Logic Summary

Implement these rules:

text
Design Item Completed
        ↓
CTP for that Item Starts
        ↓
CTP Completed
        ↓
Post Press Starts


All independent per item.

---

# Concurrency Requirement

System must support:

text
Multiple parallel task execution


Examples:

text
Design Cover Page
Design Book Content
CTP Cover Page
Post Press Cover Page


All allowed simultaneously.

---

# Activity Logging

Every task event must log:

text
Task Started
Task Completed
Task Closed
User Name
Timestamp
Item Name


---

# Timeline Updates

Update timeline for:

text
Design Started
Design Completed
CTP Started
CTP Completed
Post Press Started
Post Press Completed


Per item.

---

# Error Handling

Must handle:

* Partial completion
* Restart failed task
* Resume interrupted tasks

---

# Implementation Constraints

Must:

* Reuse existing workflow engine
* Reuse existing task framework
* Support multiple task instances
* Maintain job-level and item-level tracking

Must NOT:

* Force sequential execution
* Lock full job when one item is running
* Merge independent tasks

---

# Expected Output From Codex

Generate:

1. Task workflow logic
2. Parallel execution model
3. Item-level task tracking
4. UI task dashboard
5. Dependency engine (Design → CTP → PostPress)
6. Status update mechanism
7. Activity logging logic

---

# Why This Prompt Works Well

This prompt:

* Clearly defines **task vs approval**
* Enables **parallel workflow**
* Supports **real production printing workflow**
* Avoids sequential bottlenecks
* Matches real-world **Design → CTP → PostPress** operations
