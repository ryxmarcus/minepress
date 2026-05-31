# 📘 context.md – MinePress ERP (Execution + Planning Guide)

## 🎯 Objective
This document is designed for **GitHub Copilot + Developers** to:
- Understand folder structure
- Generate projects automatically
- Follow a **step-by-step execution plan**
- Build a **scalable Printing Press ERP**

Architecture: **Clean Architecture + Modular Monolith (Microservice-ready)**

Database : postgres (Dbcontext)
PS C:\erp.minepress\erp.minepress\src\ERP.MinePress.Persistence> dotnet ef dbcontext scaffold "Host=localhost;Database=minepress_db;Username=postgres;Password=minepress@123456" Npgsql.EntityFrameworkCore.PostgreSQL --schema press_db --output-dir Models --context ApplicationDbContext --context-dir Context --force --verbose


---

# 🧭 PHASE-WISE EXECUTION PLAN

## 🟢 Phase 1: Solution & Project Setup

Create solution:
```
dotnet new sln -n MinePressERP
```

Create projects:
```
dotnet new classlib -n erp.minepress.domain
dotnet new classlib -n erp.minepress.application
dotnet new classlib -n erp.minepress.infrastructure
dotnet new classlib -n erp.minepress.persistence
dotnet new classlib -n erp.minepress.frameworks
dotnet new classlib -n erp.minepress.notification
dotnet new classlib -n erp.minepress.tenants
dotnet new classlib -n erp.minepress.printingcostingengine

# AI Layer the whole project is based on agentic AI logic (for Agentic AI user can prompt or voice)
dotnet new classlib -n erp.minepress.agentic.ai

# API + UI
dotnet new webapi -n erp.minepress.webapi
dotnet new web -n erp.minepress.web

# BFF
dotnet new webapi -n erp.minepress.bff
dotnet new classlib -n erp.minepress.bff.service

# App Host
dotnet new console -n erp.minepress.app
```

Add to solution:
```
dotnet sln add **/*.csproj
```

---

## 🟢 Phase 2: Project Responsibilities

### 🔹 erp.minepress.domain
Core business logic
- Entities: Job, PrintOrder, Customer, Machine, Paper
- Value Objects: Size, Cost, Quantity
- Enums: JobStatus, PrintType

RULE: No dependencies

---

### 🔹 erp.minepress.application
Use cases (CQRS)
- Commands / Queries
- DTOs
- Validators
- Interfaces

Example:
- CreatePrintJobCommand
- CalculateCostQuery

---

### 🔹 erp.minepress.printingcostingengine
⚠️ CRITICAL MODULE

- Cost formulas (offset, digital)
- Paper calculation
- Ink usage
- Machine costing
- 

RULE:
- Pure logic
- No DB dependency

---

### 🔹 erp.minepress.agentic.ai
AI + automation

- Cost prediction
- Smart scheduling
- Job recommendations
- LLM integrations
- AI agenti for prompting and voice commands 
- Agentic AI logic for automating tasks based on user prompts and voice commands
- Integrations with LLMs for natural language understanding and generation
- AI-driven insights for optimizing printing workflows and cost efficiency
- AI agents that can autonomously perform tasks such as scheduling, cost estimation, and job management based on user input and system data
- AI-powered recommendations for job prioritization, resource allocation, and workflow optimization
- AI-driven automation of routine tasks, freeing up human resources for more complex decision-making and creative work
- AI agents that can learn from historical data to improve cost estimation accuracy and scheduling efficiency over time
- AI-powered natural language interfaces for interacting with the ERP system, allowing users to query data, generate reports, and manage jobs using conversational language
- AI-driven insights and analytics for identifying trends, bottlenecks, and opportunities for improvement in the printing process
- AI agents that can assist with troubleshooting and problem-solving by analyzing system data and providing recommendations for resolving issues in the printing workflow
- AI-powered automation of customer interactions, such as sending notifications, responding to inquiries, and managing customer relationships based on user prompts and system data
- AI-driven optimization of printing processes, such as adjusting machine settings, optimizing material usage, and improving scheduling based on real-time data and AI analysis
- AI agents that can autonomously manage inventory levels, reorder supplies, and optimize resource allocation based on historical data and real-time monitoring of inventory levels and usage patterns
- AI-powered predictive maintenance for printing equipment, analyzing machine data to predict potential failures and schedule maintenance proactively, minimizing downtime and maximizing productivity
- AI-driven insights for improving cost efficiency, such as identifying cost-saving opportunities, optimizing resource usage, and providing recommendations for reducing waste in the printing process
  - AI agents that can assist with decision-making by analyzing data, generating insights, and providing recommendations for optimizing printing workflows, improving cost efficiency, and enhancing overall operational performance based on user prompts and system data
  - AI-powered automation of routine tasks, freeing up human resources for more complex decision-making and creative work
  - AI-driven insights for continuous improvement, identifying areas for optimization and innovation in printing processes
  - AI agents that can assist with training and onboarding new employees by providing guidance, answering questions, and offering personalized learning experiences based on user prompts and system data
  - 
- 

---

### 🔹 erp.minepress.persistence
Database layer

- DbContext
- EF Core config
- Repositories

---

### 🔹 erp.minepress.infrastructure
External systems

- File storage
- Email/SMS
- API integrations

---

### 🔹 erp.minepress.frameworks
Shared utilities

- Logging
- Middleware helpers
- Base classes

---

### 🔹 erp.minepress.notification
Notification system

- Email
- SMS
- WhatsApp

---

### 🔹 erp.minepress.tenants
Multi-tenancy

- Tenant resolver
- Tenant DB switching

---

### 🔹 erp.minepress.webapi
Main backend API

- Controllers
- Auth (JWT)
- Middleware

---

### 🔹 erp.minepress.bff
Frontend aggregator

---

### 🔹 erp.minepress.bff.service
BFF logic layer

---

### 🔹 erp.minepress.web
Frontend UI

---

### 🔹 erp.minepress.app
App bootstrapper

---

# 🔗 PHASE 3: DEPENDENCY STRUCTURE

```
web / bff / webapi
        ↓
application
        ↓
domain
        ↓
persistence + infrastructure
```

Special:
- printingcostingengine → used by application
- agentic.ai → used by application

---

# 🧱 PHASE 4: PRINTING ERP MODULES

## Core Modules
- Customer Management
- Job Management
- Prepress
- Printing
- Postpress
- Inventory (Paper, Ink, Plates)
- Cost Estimation

---

# ⚙️ PHASE 5: CODING RULES (COPILOT MUST FOLLOW)

1. Use Clean Architecture strictly
2. Use CQRS pattern
3. Keep controllers thin
4. Business logic ONLY in Application
5. Domain = pure C#
6. Use async/await
7. Use DI everywhere
8. Use interfaces first

---

# 📁 INTERNAL FOLDER STRUCTURE (PER PROJECT)

Example (application):
```
application/
 └── jobs/
     ├── commands/
     ├── queries/
     ├── handlers/
     └── dto/
```

Example (domain):
```
domain/
 └── job/
     ├── job.cs
     ├── jobstatus.cs
     └── events/
```

---

# 📌 FINAL NOTE FOR COPILOT

This is a **printing industry ERP**.

Focus areas:
- Offset printing workflow
- Prepress → Press → Postpress lifecycle
- Accurate costing engine (MOST IMPORTANT)

Always prioritize:
1. Cost calculation accuracy
2. Modular architecture
3. Scalability

---

✅ This file is the **single source of truth** for generating consistent code across the MinePress ERP solution.

