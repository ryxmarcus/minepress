-- ============================================================
-- MinePress ERP — Menu Reset & Seed Script
-- Replaces old mst_menu rows with new menus matching _Layout.cshtml
-- Creates map_module_department table for department-based access
-- ============================================================

BEGIN;

-- ────────────────────────────────────────────────────────────
-- 1. DELETE all existing menus
-- ────────────────────────────────────────────────────────────
DELETE FROM press_db.mst_menu;

-- ────────────────────────────────────────────────────────────
-- 2. INSERT Level-1 (Top-Level Navigation Items)
--    module_id mapping preserved from original:
--      1=DASHBOARD, 2=SALES_CRM, 4=PRODUCTION, 5=INVENTORY,
--      7=DISPATCH(Outsource), 8=ACCOUNTS, 10=MASTERS,
--      12=SETTINGS(Tools), 13=HRMS, 14=MY_WORKSPACE
-- ────────────────────────────────────────────────────────────

INSERT INTO press_db.mst_menu (menuid, menucode, menuname, parentmenuid, routeurl, icon, displayorder, ismobile, isweb, isactive, menulevel, issectionheader, sectionname, badgetext, badgeclass, hasdividerbefore, iconsvg, module_id)
VALUES
-- Level-1 parents
(1,  'DASHBOARD',    'Dashboard',        NULL, '/Dashboard',  'speedometer2',     1,  true, true, true, 1, false, NULL, NULL, NULL, false, NULL, 1),
(14, 'MY_WORKSPACE', 'My Workspace',     NULL, NULL,          'kanban',           2,  true, true, true, 1, false, NULL, NULL, NULL, false, NULL, 14),
(2,  'SALES_CRM',    'CRM',              NULL, NULL,          'diagram-3',        3,  true, true, true, 1, false, NULL, NULL, NULL, false, NULL, 2),
(15, 'OUTSOURCE',    'Outsource',        NULL, NULL,          'box-arrow-up-right',4, true, true, true, 1, false, NULL, NULL, NULL, false, NULL, 7),
(5,  'STORE',        'Store',            NULL, NULL,          'boxes',            5,  true, true, true, 1, false, NULL, NULL, NULL, false, NULL, 5),
(4,  'PRODUCTION',   'Press/Production', NULL, NULL,          'printer',          6,  true, true, true, 1, false, NULL, NULL, NULL, false, NULL, 4),
(8,  'ACCOUNTS',     'Accounting',       NULL, NULL,          'calculator',       7,  true, true, true, 1, false, NULL, NULL, NULL, false, NULL, 8),
(13, 'HRMS',         'HRMS',             NULL, NULL,          'people',           8,  true, true, true, 1, false, NULL, NULL, NULL, false, NULL, 13),
(10, 'MAINTENANCE',  'Maintenance',      NULL, NULL,          'gear',             9,  true, true, true, 1, false, NULL, NULL, NULL, false, NULL, 10),
(12, 'TOOLS',        'Tools',            NULL, NULL,          'tools',            10, true, true, true, 1, false, NULL, NULL, NULL, false, NULL, 12);

-- ────────────────────────────────────────────────────────────
-- 3. INSERT Level-2 (Child Menu Items)
-- ────────────────────────────────────────────────────────────

-- ── My Workspace (parent=14, module_id=14) ──
INSERT INTO press_db.mst_menu (menuid, menucode, menuname, parentmenuid, routeurl, icon, displayorder, ismobile, isweb, isactive, menulevel, issectionheader, sectionname, badgetext, badgeclass, hasdividerbefore, iconsvg, module_id)
VALUES
(1401, 'WS_HOME',          'Workspace Home',   14, '/Workspace',              'house-door',     1, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 14),
(1402, 'WS_MY_TASKS',      'My Tasks',         14, '/Workspace/MyTasks',      'list-task',      2, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 14),
(1403, 'WS_APPROVALS',     'Approvals',        14, '/Workspace/Approvals',    'shield-check',   3, true, true, true, 2, false, NULL, NULL, NULL, true,  NULL, 14),
(1404, 'WS_CALENDAR',      'Task Calendar',    14, '/Workspace/Calendar',     'calendar3',      4, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 14),
(1405, 'WS_NOTIFICATIONS', 'Notifications',    14, '/Workspace/Notifications','bell',           5, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 14),
(1406, 'WS_HISTORY',       'Activity History',  14, '/Workspace/History',     'clock-history',  6, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 14);

-- ── CRM / Sales (parent=2, module_id=2) ──
INSERT INTO press_db.mst_menu (menuid, menucode, menuname, parentmenuid, routeurl, icon, displayorder, ismobile, isweb, isactive, menulevel, issectionheader, sectionname, badgetext, badgeclass, hasdividerbefore, iconsvg, module_id)
VALUES
(201, 'RATE_CALCULATOR',   'Calculator',           2, '/RateCalculator',       'calculator',         1,  true, true, true, 2, false, 'Rate Calculator', NULL, NULL, false, NULL, 2),
(202, 'RATE_CALC_HISTORY', 'Calculation History',  2, '/RateCalculator/List',  'clock-history',      2,  true, true, true, 2, false, 'Rate Calculator', NULL, NULL, false, NULL, 2),
(203, 'ENQUIRY_LIST',      'Enquiry List',         2, '/Enquiry',              'clipboard-data',     3,  true, true, true, 2, false, 'Enquiry',         NULL, NULL, true,  NULL, 2),
(204, 'ENQUIRY_CREATE',    'New Enquiry',          2, '/Enquiry/Create',       'plus-circle',        4,  true, true, true, 2, false, 'Enquiry',         NULL, NULL, false, NULL, 2),
(205, 'QUOTATION_LIST',    'Quotation List',       2, '/Quotation',            'file-earmark-text',  5,  true, true, true, 2, false, 'Quotation',       NULL, NULL, true,  NULL, 2),
(206, 'QUOTATION_CREATE',  'New Quotation',        2, '/Quotation/Create',     'plus-circle',        6,  true, true, true, 2, false, 'Quotation',       NULL, NULL, false, NULL, 2),
(207, 'JOB_LIST',          'Job List',             2, '/Job',                  'briefcase',          7,  true, true, true, 2, false, 'Job',             NULL, NULL, true,  NULL, 2),
(208, 'JOB_CREATE',        'New Job',              2, '/Job/Create',           'plus-circle',        8,  true, true, true, 2, false, 'Job',             NULL, NULL, false, NULL, 2),
(209, 'CHALLAN_LIST',      'Challan List',         2, '/Challan',              'truck',              9,  true, true, true, 2, false, 'Challan',         NULL, NULL, true,  NULL, 2),
(210, 'CHALLAN_CREATE',    'New Challan',          2, '/Challan/Create',       'plus-circle',        10, true, true, true, 2, false, 'Challan',         NULL, NULL, false, NULL, 2),
(211, 'GATE_PASSES',       'Gate Passes',          2, '/GatePass',             'shield-check',       11, true, true, true, 2, false, 'Challan',         NULL, NULL, false, NULL, 2);

-- ── Outsource (parent=15, module_id=7) ──
INSERT INTO press_db.mst_menu (menuid, menucode, menuname, parentmenuid, routeurl, icon, displayorder, ismobile, isweb, isactive, menulevel, issectionheader, sectionname, badgetext, badgeclass, hasdividerbefore, iconsvg, module_id)
VALUES
(1501, 'OUTSOURCE_LIST',   'Outsource List',   15, '/Outsource',        'list-ul',      1, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 7),
(1502, 'OUTSOURCE_CREATE', 'New Outsource',    15, '/Outsource/Create', 'plus-circle',  2, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 7);

-- ── Store / Inventory (parent=5, module_id=5) ──
INSERT INTO press_db.mst_menu (menuid, menucode, menuname, parentmenuid, routeurl, icon, displayorder, ismobile, isweb, isactive, menulevel, issectionheader, sectionname, badgetext, badgeclass, hasdividerbefore, iconsvg, module_id)
VALUES
(501, 'STORE_DASHBOARD',   'Dashboard',        5, '/Store',                 'speedometer2',       1,  true, true, true, 2, false, NULL,     NULL, NULL, false, NULL, 5),
(502, 'STORE_ISSUE_LIST',  'Issue List',       5, '/Store/Issue',           'box-arrow-up',       2,  true, true, true, 2, false, NULL,     NULL, NULL, true,  NULL, 5),
(503, 'STORE_ISSUE_NEW',   'New Issue',        5, '/Store/Issue/Create',    'plus-circle',        3,  true, true, true, 2, false, NULL,     NULL, NULL, false, NULL, 5),
(504, 'STORE_RECEIVE_LIST','Receive List',     5, '/Store/Receive',         'box-arrow-in-down',  4,  true, true, true, 2, false, NULL,     NULL, NULL, true,  NULL, 5),
(505, 'STORE_RECEIVE_NEW', 'New Receive',      5, '/Store/Receive/Create',  'plus-circle',        5,  true, true, true, 2, false, NULL,     NULL, NULL, false, NULL, 5),
(506, 'STORE_GRN_LIST',    'Purchase GRNs',    5, '/Store/Purchase',        'receipt',            6,  true, true, true, 2, false, NULL,     NULL, NULL, true,  NULL, 5),
(507, 'STORE_GRN_NEW',     'New GRN',          5, '/Store/Purchase/Create', 'plus-circle',        7,  true, true, true, 2, false, NULL,     NULL, NULL, false, NULL, 5),
(508, 'STOCK_LEDGER',      'Stock Ledger',     5, '/Store/StockLedger',     'journal-text',       8,  true, true, true, 2, false, NULL,     NULL, NULL, true,  NULL, 5),
(509, 'STOCK_SUMMARY',     'Stock Summary',    5, '/Store/StockSummary',    'bar-chart-line',     9,  true, true, true, 2, false, NULL,     NULL, NULL, false, NULL, 5);

-- ── Press/Production (parent=4, module_id=4) ──
INSERT INTO press_db.mst_menu (menuid, menucode, menuname, parentmenuid, routeurl, icon, displayorder, ismobile, isweb, isactive, menulevel, issectionheader, sectionname, badgetext, badgeclass, hasdividerbefore, iconsvg, module_id)
VALUES
(401, 'PROD_DASHBOARD',      'Dashboard',            4, '/Production/Dashboard',          'speedometer2',       1, true, true, true, 2, false, NULL,          NULL, NULL, false, NULL, 4),
(402, 'MACHINE_JOB_ALLOC',   'Machine Job Allocation',4,'/Production/JobAllocation',      'kanban',             2, true, true, true, 2, false, 'Planning',    NULL, NULL, true,  NULL, 4),
(403, 'MACHINE_SCHEDULING',  'Machine Scheduling',   4, '/Production/MachineScheduling',  'calendar3',          3, true, true, true, 2, false, 'Planning',    NULL, NULL, false, NULL, 4),
(404, 'PROD_TRACKING',       'Production Tracking',  4, '/Production/ProductionTracking', 'activity',           4, true, true, true, 2, false, 'Monitoring',  NULL, NULL, true,  NULL, 4),
(405, 'MACHINE_UTILIZATION', 'Machine Utilization',  4, '/Production/MachineUtilization', 'bar-chart-line',     5, true, true, true, 2, false, 'Monitoring',  NULL, NULL, false, NULL, 4),
(406, 'MACHINE_MAINTENANCE', 'Machine Maintenance',  4, '/Production/Maintenance',        'wrench-adjustable',  6, true, true, true, 2, false, 'Maintenance', NULL, NULL, true,  NULL, 4),
(407, 'MACHINE_BREAKDOWN',   'Machine Breakdown',    4, '/Production/MachineBreakdown',   'lightning-charge',   7, true, true, true, 2, false, 'Maintenance', NULL, NULL, false, NULL, 4);

-- ── Accounting (parent=8, module_id=8) ──
INSERT INTO press_db.mst_menu (menuid, menucode, menuname, parentmenuid, routeurl, icon, displayorder, ismobile, isweb, isactive, menulevel, issectionheader, sectionname, badgetext, badgeclass, hasdividerbefore, iconsvg, module_id)
VALUES
(801, 'ACC_DASHBOARD',     'Dashboard',           8, '/Accounting',                   'speedometer2',       1,  true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 8),
(802, 'SALES_INVOICE',     'Sales Invoices',      8, '/Accounting/SalesInvoice',      'receipt-cutoff',     2,  true, true, true, 2, false, NULL, NULL, NULL, true,  NULL, 8),
(803, 'PURCHASE_INVOICE',  'Purchase Invoices',   8, '/Accounting/PurchaseInvoice',   'receipt',            3,  true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 8),
(804, 'ACC_RECEIPTS',      'Receipts',            8, '/Accounting/Receipt',           'cash-stack',         4,  true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 8),
(805, 'ACC_PAYMENTS',      'Payments',            8, '/Accounting/Payment',           'wallet2',            5,  true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 8),
(806, 'CREDIT_NOTE',       'Credit Notes',        8, '/Accounting/CreditNote',        'file-earmark-minus', 6,  true, true, true, 2, false, NULL, NULL, NULL, true,  NULL, 8),
(807, 'DEBIT_NOTE',        'Debit Notes',         8, '/Accounting/DebitNote',         'file-earmark-plus',  7,  true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 8),
(808, 'JOURNAL_VOUCHER',   'Journal Vouchers',    8, '/Accounting/JournalVoucher',    'journal-text',       8,  true, true, true, 2, false, NULL, NULL, NULL, true,  NULL, 8),
(809, 'EXPENSES',          'Expenses',            8, '/Accounting/ExpenseVoucher',    'cash-coin',          9,  true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 8),
(810, 'BANK_RECONCILIATION','Bank Reconciliation', 8, '/Accounting/BankReconciliation','bank',              10, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 8),
(811, 'AR_OUTSTANDING',    'AR Outstanding',      8, '/Accounting/ArOutstanding',     'clock-history',      11, true, true, true, 2, false, NULL, NULL, NULL, true,  NULL, 8),
(812, 'AP_OUTSTANDING',    'AP Outstanding',      8, '/Accounting/ApOutstanding',     'hourglass-split',    12, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 8),
(813, 'ACC_PURCHASE_ORDER','Purchase Orders',     8, '/Accounting/PurchaseOrder',     'cart-check',         13, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 8),
(814, 'ACC_GOODS_RECEIPT', 'Goods Receipts',      8, '/Accounting/GoodsReceipt',      'box-seam',           14, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 8);

-- ── HRMS (parent=13, module_id=13) ──
INSERT INTO press_db.mst_menu (menuid, menucode, menuname, parentmenuid, routeurl, icon, displayorder, ismobile, isweb, isactive, menulevel, issectionheader, sectionname, badgetext, badgeclass, hasdividerbefore, iconsvg, module_id)
VALUES
(1301, 'HRMS_DASHBOARD',  'HRMS Dashboard',   13, '/Hrms',                'speedometer2',    1,  true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 13),
(1302, 'HRMS_ATTENDANCE',  'Attendance',       13, '/Hrms/Attendance',     'calendar-check',  2,  true, true, true, 2, false, NULL, NULL, NULL, true,  NULL, 13),
(1303, 'HRMS_LEAVES',      'Leaves',           13, '/Hrms/Leaves',         'calendar-x',      3,  true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 13),
(1304, 'HRMS_HOLIDAYS',    'Holidays',         13, '/Hrms/Holiday',        'calendar-event',  4,  true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 13),
(1305, 'HRMS_SHIFT',       'Shift Roster',     13, '/Hrms/Shift',          'clock',           5,  true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 13),
(1306, 'HRMS_LOANS',       'Loans',            13, '/Hrms/Loan',           'bank',            6,  true, true, true, 2, false, NULL, NULL, NULL, true,  NULL, 13),
(1307, 'HRMS_ADVANCE',     'Salary Advance',   13, '/Hrms/Advance',        'cash-stack',      7,  true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 13),
(1308, 'HRMS_INCENTIVE',   'Incentives',       13, '/Hrms/Incentive',      'gift',            8,  true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 13),
(1309, 'HRMS_OVERTIME',    'Overtime',          13, '/Hrms/Overtime',       'clock-history',   9,  true, true, true, 2, false, NULL, NULL, NULL, true,  NULL, 13),
(1310, 'HRMS_MEDICAL',     'Medical Claims',   13, '/Hrms/Medical',        'heart-pulse',     10, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 13),
(1311, 'HRMS_TRAVEL',      'Travel Expenses',  13, '/Hrms/Travel',         'airplane',        11, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 13),
(1312, 'HRMS_REIMBURSE',   'Reimbursements',   13, '/Hrms/Reimbursement',  'receipt-cutoff',  12, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 13),
(1313, 'HRMS_TRANSFER',    'Transfers',        13, '/Hrms/Transfer',       'arrow-left-right',13, true, true, true, 2, false, NULL, NULL, NULL, true,  NULL, 13),
(1314, 'HRMS_RESIGNATION', 'Resignation',      13, '/Hrms/Resignation',    'box-arrow-right', 14, true, true, true, 2, false, NULL, NULL, NULL, false, NULL, 13);

-- ── Maintenance / Masters (parent=10, module_id=10) ──
INSERT INTO press_db.mst_menu (menuid, menucode, menuname, parentmenuid, routeurl, icon, displayorder, ismobile, isweb, isactive, menulevel, issectionheader, sectionname, badgetext, badgeclass, hasdividerbefore, iconsvg, module_id)
VALUES
(1001, 'PARTY_LIST',       'All Parties',       10, '/Maintenance/Party',           'people',       1, true, true, true, 2, false, 'Party Management',   NULL, NULL, false, NULL, 10),
(1002, 'PARTY_CREATE',     'New Party',         10, '/Maintenance/Party/Create',    'person-plus',  2, true, true, true, 2, false, 'Party Management',   NULL, NULL, false, NULL, 10),
(1003, 'USER_MANAGEMENT',  'Users & Roles',     10, '/Maintenance/UserManagement',  'shield-lock',  3, true, true, true, 2, false, 'User Management',    NULL, NULL, true,  NULL, 10),
(1004, 'ITEM_MANAGEMENT',  'All Items',         10, '/Maintenance/ItemManagement',  'box-seam',     4, true, true, true, 2, false, 'Item Management',    NULL, NULL, true,  NULL, 10);

-- ── Tools / Settings (parent=12, module_id=12) ──
INSERT INTO press_db.mst_menu (menuid, menucode, menuname, parentmenuid, routeurl, icon, displayorder, ismobile, isweb, isactive, menulevel, issectionheader, sectionname, badgetext, badgeclass, hasdividerbefore, iconsvg, module_id)
VALUES
(1201, 'REPORT_BUILDER',  'Report Builder',    12, '/Reports',     'bar-chart-line', 1, true, true, true, 2, false, NULL,    NULL, NULL, false, NULL, 12),
(1202, 'WORKFLOW',         'Workflow',          12, '/Workflow',    'diagram-3',      2, true, true, true, 2, false, NULL,    NULL, NULL, false, NULL, 12),
(1203, 'AI_ASSISTANT',     'AI Assistant',      12, '/AgenticAi',  'robot',          3, true, true, true, 2, false, NULL,    NULL, NULL, false, NULL, 12),
(1204, 'DB_MANAGER',       'Database Manager',  12, '/DbManager',  'database-gear',  4, true, true, true, 2, false, 'Admin', NULL, NULL, true,  NULL, 12);

-- ────────────────────────────────────────────────────────────
-- 4. Reset serial sequence to avoid PK conflicts
-- ────────────────────────────────────────────────────────────
SELECT setval('press_db.mst_menu_menuid_seq', (SELECT MAX(menuid) FROM press_db.mst_menu));

-- ────────────────────────────────────────────────────────────
-- 5. CREATE map_module_department table
-- ────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS press_db.map_module_department (
    id              serial      PRIMARY KEY,
    department_id   bigint      NOT NULL,
    module_id       integer     NOT NULL,
    is_active       boolean     NOT NULL DEFAULT true,
    created_by      varchar(50),
    created_on      timestamp   NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_dept_module UNIQUE (department_id, module_id)
);

COMMENT ON TABLE  press_db.map_module_department IS 'Maps departments to allowed navigation modules. Controls which top-level menu groups are visible per department.';
COMMENT ON COLUMN press_db.map_module_department.department_id IS 'FK → mst_department.dept_id';
COMMENT ON COLUMN press_db.map_module_department.module_id    IS 'Module identifier matching mst_menu.module_id (top-level grouping)';

-- ────────────────────────────────────────────────────────────
-- 6. SEED map_module_department
--    Department IDs from DepartmentCode enum:
--      1001=MGT, 1002=ADM, 1003=HR, 1004=FIN, 1005=IT,
--      1006=SAL, 1007=CST, 1008=EST, 1009=PRE, 1010=PRT,
--      1011=FINP, 1012=PKG, 1013=DSP, 1014=INV, 1015=PUR,
--      1016=QMS, 1017=MNT, 1018=SEC
--    Module IDs:
--      1=Dashboard, 14=Workspace, 2=CRM, 7=Outsource/Dispatch,
--      5=Store, 4=Production, 8=Accounts, 13=HRMS,
--      10=Maintenance, 12=Tools
-- ────────────────────────────────────────────────────────────
DELETE FROM press_db.map_module_department;

INSERT INTO press_db.map_module_department (department_id, module_id, created_by) VALUES
-- MGT (1001) — Top Management — ALL modules
(1001, 1,  'SYSTEM'), (1001, 14, 'SYSTEM'), (1001, 2,  'SYSTEM'), (1001, 7,  'SYSTEM'),
(1001, 5,  'SYSTEM'), (1001, 4,  'SYSTEM'), (1001, 8,  'SYSTEM'), (1001, 13, 'SYSTEM'),
(1001, 10, 'SYSTEM'), (1001, 12, 'SYSTEM'),

-- ADM (1002) — Administration
(1002, 1,  'SYSTEM'), (1002, 14, 'SYSTEM'), (1002, 10, 'SYSTEM'), (1002, 12, 'SYSTEM'),
(1002, 13, 'SYSTEM'),

-- HR (1003) — Human Resource
(1003, 1,  'SYSTEM'), (1003, 14, 'SYSTEM'), (1003, 13, 'SYSTEM'),

-- FIN (1004) — Accounts & Finance
(1004, 1,  'SYSTEM'), (1004, 14, 'SYSTEM'), (1004, 8,  'SYSTEM'), (1004, 2,  'SYSTEM'),

-- IT (1005) — IT & ERP Support — ALL modules
(1005, 1,  'SYSTEM'), (1005, 14, 'SYSTEM'), (1005, 2,  'SYSTEM'), (1005, 7,  'SYSTEM'),
(1005, 5,  'SYSTEM'), (1005, 4,  'SYSTEM'), (1005, 8,  'SYSTEM'), (1005, 13, 'SYSTEM'),
(1005, 10, 'SYSTEM'), (1005, 12, 'SYSTEM'),

-- SAL (1006) — Sales & Marketing
(1006, 1,  'SYSTEM'), (1006, 14, 'SYSTEM'), (1006, 2,  'SYSTEM'),

-- CST (1007) — Customer Service & CRM
(1007, 1,  'SYSTEM'), (1007, 14, 'SYSTEM'), (1007, 2,  'SYSTEM'),

-- EST (1008) — Estimation & Costing
(1008, 1,  'SYSTEM'), (1008, 14, 'SYSTEM'), (1008, 2,  'SYSTEM'),

-- PRE (1009) — Pre-Press & Design
(1009, 1,  'SYSTEM'), (1009, 14, 'SYSTEM'), (1009, 2,  'SYSTEM'), (1009, 4,  'SYSTEM'),

-- PRT (1010) — Printing
(1010, 1,  'SYSTEM'), (1010, 14, 'SYSTEM'), (1010, 4,  'SYSTEM'),

-- FINP (1011) — Post-Press & Finishing
(1011, 1,  'SYSTEM'), (1011, 14, 'SYSTEM'), (1011, 4,  'SYSTEM'),

-- PKG (1012) — Packaging
(1012, 1,  'SYSTEM'), (1012, 14, 'SYSTEM'), (1012, 4,  'SYSTEM'),

-- DSP (1013) — Dispatch & Logistics
(1013, 1,  'SYSTEM'), (1013, 14, 'SYSTEM'), (1013, 2,  'SYSTEM'), (1013, 7,  'SYSTEM'),

-- INV (1014) — Inventory & Stores
(1014, 1,  'SYSTEM'), (1014, 14, 'SYSTEM'), (1014, 5,  'SYSTEM'),

-- PUR (1015) — Purchase
(1015, 1,  'SYSTEM'), (1015, 14, 'SYSTEM'), (1015, 5,  'SYSTEM'), (1015, 8,  'SYSTEM'),

-- QMS (1016) — Quality Management
(1016, 1,  'SYSTEM'), (1016, 14, 'SYSTEM'), (1016, 4,  'SYSTEM'),

-- MNT (1017) — Maintenance & Utilities
(1017, 1,  'SYSTEM'), (1017, 14, 'SYSTEM'), (1017, 4,  'SYSTEM'),

-- SEC (1018) — Security & Gatepass
(1018, 1,  'SYSTEM'), (1018, 14, 'SYSTEM'), (1018, 2,  'SYSTEM');

COMMIT;
