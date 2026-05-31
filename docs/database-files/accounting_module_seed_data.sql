-- ============================================================================
-- ACCOUNTING MODULE — MASTER TABLE SEED DATA
-- Schema  : press_db
-- Database: PostgreSQL
-- Covers  : Voucher Types, Transaction Types, Document Sequences,
--           Expense Categories, Cost Centers, Account Heads (Chart of Accounts),
--           Tax Components, Tax Categories, Tax Category Components, Directions
-- ============================================================================
-- NOTE: Uses ON CONFLICT DO NOTHING to avoid duplicate-key errors on re-run.
--       Run AFTER accounting_module_tables.sql and Schema Table Structure DDL.
-- ============================================================================

BEGIN;


-- ============================================================================
-- 1. DIRECTIONS (Tax flow: Output / Input)
-- ============================================================================

INSERT INTO press_db.mst_direction (id, name, is_active) VALUES
    (1, 'Output Tax',  true),
    (2, 'Input Tax',   true)
ON CONFLICT (id) DO NOTHING;


-- ============================================================================
-- 2. TRANSACTION TYPES (Referenced by trn_tax_ledger.transaction_type_id)
-- ============================================================================

INSERT INTO press_db.mst_transaction_type (name, is_active) VALUES
    ('Sales Invoice',           true),
    ('Purchase Invoice',        true),
    ('Credit Note',             true),
    ('Debit Note',              true),
    ('Expense Voucher',         true),
    ('Journal Voucher',         true),
    ('Bank Receipt',            true),
    ('Bank Payment',            true),
    ('Contra Voucher',          true),
    ('Receipt',                 true),
    ('Payment',                 true),
    ('Advance Receipt',         true),
    ('Advance Payment',         true),
    ('Purchase Order',          true),
    ('Goods Receipt Note',      true),
    ('Proforma Invoice',        true),
    ('TDS Deduction',           true)
ON CONFLICT ON CONSTRAINT uq_transaction_type_name DO NOTHING;


-- ============================================================================
-- 3. VOUCHER TYPES (Referenced for document classification & auto-numbering)
-- ============================================================================

INSERT INTO press_db.mst_voucher_type
    (voucher_code, voucher_name, transaction_nature, affects_party, affects_inventory, is_auto_numbering, prefix, suffix, last_number, is_active, sort_order)
VALUES
    -- Sales & AR
    ('SI',   'Sales Invoice',               'SALES',      true,  true,  true, 'SI/',   NULL, 0, true, 1),
    ('PI',   'Purchase Invoice',            'PURCHASE',   true,  true,  true, 'PI/',   NULL, 0, true, 2),
    ('PROF', 'Proforma Invoice',            'SALES',      true,  false, true, 'PRF/',  NULL, 0, true, 3),
    ('CN',   'Credit Note',                 'SALES',      true,  false, true, 'CN/',   NULL, 0, true, 4),
    ('DN',   'Debit Note',                  'PURCHASE',   true,  false, true, 'DN/',   NULL, 0, true, 5),

    -- Expenses
    ('EXP',  'Expense Voucher',             'EXPENSE',    true,  false, true, 'EXP/',  NULL, 0, true, 6),

    -- Journal
    ('JV',   'Journal Voucher',             'JOURNAL',    false, false, true, 'JV/',   NULL, 0, true, 7),

    -- Bank / Cash
    ('BR',   'Bank Receipt',                'RECEIPT',    true,  false, true, 'BR/',   NULL, 0, true, 8),
    ('BP',   'Bank Payment',                'PAYMENT',    true,  false, true, 'BP/',   NULL, 0, true, 9),
    ('CONT', 'Contra Voucher',              'CONTRA',     false, false, true, 'CTR/',  NULL, 0, true, 10),

    -- Receipts & Payments (existing flow)
    ('RCT',  'Receipt',                     'RECEIPT',    true,  false, true, 'RCT/',  NULL, 0, true, 11),
    ('PMT',  'Payment',                     'PAYMENT',    true,  false, true, 'PMT/',  NULL, 0, true, 12),

    -- Purchase Order & GRN
    ('PO',   'Purchase Order',              'PURCHASE',   true,  true,  true, 'PO/',   NULL, 0, true, 13),
    ('GRN',  'Goods Receipt Note',          'PURCHASE',   true,  true,  true, 'GRN/',  NULL, 0, true, 14)
ON CONFLICT (voucher_code) DO NOTHING;


-- ============================================================================
-- 4. DOCUMENT SEQUENCES (Auto-numbering for each document type)
-- ============================================================================

INSERT INTO press_db.mst_document_sequence
    (process_code, process_name, prefix, suffix, current_number, padding_length, financial_year, is_active)
VALUES
    -- Sales & AR
    ('SALES_INVOICE',      'Sales Invoice',          'SI/',   NULL, 0, 5, '2025-26', true),
    ('PURCHASE_INVOICE',   'Purchase Invoice',       'PI/',   NULL, 0, 5, '2025-26', true),
    ('PROFORMA_INVOICE',   'Proforma Invoice',       'PRF/',  NULL, 0, 5, '2025-26', true),
    ('CREDIT_NOTE',        'Credit Note',            'CN/',   NULL, 0, 5, '2025-26', true),
    ('DEBIT_NOTE',         'Debit Note',             'DN/',   NULL, 0, 5, '2025-26', true),

    -- Expenses
    ('EXPENSE_VOUCHER',    'Expense Voucher',        'EXP/',  NULL, 0, 5, '2025-26', true),

    -- Journal
    ('JOURNAL_VOUCHER',    'Journal Voucher',        'JV/',   NULL, 0, 5, '2025-26', true),

    -- Bank / Cash
    ('BANK_RECEIPT',       'Bank Receipt',           'BR/',   NULL, 0, 5, '2025-26', true),
    ('BANK_PAYMENT',       'Bank Payment',           'BP/',   NULL, 0, 5, '2025-26', true),
    ('CONTRA_VOUCHER',     'Contra Voucher',         'CTR/',  NULL, 0, 5, '2025-26', true),

    -- Receipts & Payments
    ('RECEIPT',            'Receipt',                'RCT/',  NULL, 0, 5, '2025-26', true),
    ('PAYMENT',            'Payment',                'PMT/',  NULL, 0, 5, '2025-26', true),

    -- Purchase Order & GRN
    ('PURCHASE_ORDER',     'Purchase Order',         'PO/',   NULL, 0, 5, '2025-26', true),
    ('GOODS_RECEIPT',      'Goods Receipt Note',     'GRN/',  NULL, 0, 5, '2025-26', true),

    -- Bank Reconciliation
    ('BANK_RECONCILIATION','Bank Reconciliation',    'BRS/',  NULL, 0, 5, '2025-26', true)
ON CONFLICT (process_code) DO NOTHING;


-- ============================================================================
-- 5. TAX COMPONENTS (CGST, SGST, IGST, CESS, TDS, TCS)
-- ============================================================================

INSERT INTO press_db.mst_tax_component
    (code, name, description, is_percentage, is_recoverable, applicable_on, is_active, created_by)
VALUES
    ('CGST',  'Central GST',           'Central Goods and Services Tax',         true, true,  'TAXABLE_VALUE', true, 'SYSTEM'),
    ('SGST',  'State GST',             'State Goods and Services Tax',           true, true,  'TAXABLE_VALUE', true, 'SYSTEM'),
    ('IGST',  'Integrated GST',        'Integrated Goods and Services Tax',      true, true,  'TAXABLE_VALUE', true, 'SYSTEM'),
    ('CESS',  'GST Compensation Cess', 'Compensation Cess under GST',           true, false, 'TAXABLE_VALUE', true, 'SYSTEM'),
    ('TDS',   'Tax Deducted at Source', 'TDS deducted on payments',              true, false, 'TAXABLE_VALUE', true, 'SYSTEM'),
    ('TCS',   'Tax Collected at Source','TCS collected on sales',                true, false, 'TAXABLE_VALUE', true, 'SYSTEM')
ON CONFLICT (code) DO NOTHING;


-- ============================================================================
-- 6. TAX TYPES (Top-level tax classification)
-- ============================================================================

INSERT INTO press_db.mst_tax_type
    (code, name, description, is_percentage, is_recoverable, is_active)
VALUES
    ('GST',     'Goods and Services Tax',   'Indian GST — CGST+SGST or IGST',  true,  true,  true),
    ('TDS',     'Tax Deducted at Source',    'TDS under Income Tax Act',         true,  false, true),
    ('TCS',     'Tax Collected at Source',   'TCS under Income Tax Act',         true,  false, true),
    ('EXEMPT',  'Exempt',                    'Exempt from all taxes',            false, false, true)
ON CONFLICT (code) DO NOTHING;


-- ============================================================================
-- 7. TAX CATEGORIES (GST slabs: 0%, 5%, 12%, 18%, 28%, Exempt, Nil, Zero-rated)
-- ============================================================================

INSERT INTO press_db.mst_tax_category
    (code, name, description, tax_type, hsn_sac_code, is_reverse_charge_applicable, is_exempt, tax_regime, is_active, created_by)
VALUES
    ('GST_0',    'GST 0%',          'Zero-rated GST',                      'GST',    NULL, false, false, 'GST', true, 'SYSTEM'),
    ('GST_5',    'GST 5%',          'GST @ 5% (CGST 2.5% + SGST 2.5%)',   'GST',    NULL, false, false, 'GST', true, 'SYSTEM'),
    ('GST_12',   'GST 12%',         'GST @ 12% (CGST 6% + SGST 6%)',      'GST',    NULL, false, false, 'GST', true, 'SYSTEM'),
    ('GST_18',   'GST 18%',         'GST @ 18% (CGST 9% + SGST 9%)',      'GST',    NULL, false, false, 'GST', true, 'SYSTEM'),
    ('GST_28',   'GST 28%',         'GST @ 28% (CGST 14% + SGST 14%)',    'GST',    NULL, false, false, 'GST', true, 'SYSTEM'),
    ('EXEMPT',   'Exempt',          'Exempt from GST',                     'EXEMPT', NULL, false, true,  'GST', true, 'SYSTEM'),
    ('NIL',      'Nil Rated',       'Nil-rated supply under GST',          'GST',    NULL, false, true,  'GST', true, 'SYSTEM'),
    ('ZERO',     'Zero Rated',      'Zero-rated export supply',            'GST',    NULL, false, false, 'GST', true, 'SYSTEM'),
    ('GST_5_RC', 'GST 5% RCM',     'GST @ 5% under Reverse Charge',      'GST',    NULL, true,  false, 'GST', true, 'SYSTEM'),
    ('GST_18_RC','GST 18% RCM',    'GST @ 18% under Reverse Charge',     'GST',    NULL, true,  false, 'GST', true, 'SYSTEM')
ON CONFLICT (code) DO NOTHING;


-- ============================================================================
-- 8. TAX CATEGORY COMPONENTS (Rate split per slab)
--    Assumes mst_tax_component IDs: CGST=1, SGST=2, IGST=3
--    Adjust IDs if your seeded component IDs differ.
-- ============================================================================

-- Helper: Use sub-selects to resolve component IDs dynamically
-- GST 5% → CGST 2.5%, SGST 2.5%, IGST 5%
INSERT INTO press_db.mst_tax_category_component
    (tax_category_id, tax_component_id, rate_percent, effective_from, is_active, created_by)
SELECT c.id, tc.tax_component_id, 2.5, '2017-07-01', true, 'SYSTEM'
FROM press_db.mst_tax_category c, press_db.mst_tax_component tc
WHERE c.code = 'GST_5' AND tc.code = 'CGST'
ON CONFLICT (tax_category_id, tax_component_id, effective_from) DO NOTHING;

INSERT INTO press_db.mst_tax_category_component
    (tax_category_id, tax_component_id, rate_percent, effective_from, is_active, created_by)
SELECT c.id, tc.tax_component_id, 2.5, '2017-07-01', true, 'SYSTEM'
FROM press_db.mst_tax_category c, press_db.mst_tax_component tc
WHERE c.code = 'GST_5' AND tc.code = 'SGST'
ON CONFLICT (tax_category_id, tax_component_id, effective_from) DO NOTHING;

INSERT INTO press_db.mst_tax_category_component
    (tax_category_id, tax_component_id, rate_percent, effective_from, is_active, created_by)
SELECT c.id, tc.tax_component_id, 5.0, '2017-07-01', true, 'SYSTEM'
FROM press_db.mst_tax_category c, press_db.mst_tax_component tc
WHERE c.code = 'GST_5' AND tc.code = 'IGST'
ON CONFLICT (tax_category_id, tax_component_id, effective_from) DO NOTHING;

-- GST 12% → CGST 6%, SGST 6%, IGST 12%
INSERT INTO press_db.mst_tax_category_component
    (tax_category_id, tax_component_id, rate_percent, effective_from, is_active, created_by)
SELECT c.id, tc.tax_component_id, 6.0, '2017-07-01', true, 'SYSTEM'
FROM press_db.mst_tax_category c, press_db.mst_tax_component tc
WHERE c.code = 'GST_12' AND tc.code = 'CGST'
ON CONFLICT (tax_category_id, tax_component_id, effective_from) DO NOTHING;

INSERT INTO press_db.mst_tax_category_component
    (tax_category_id, tax_component_id, rate_percent, effective_from, is_active, created_by)
SELECT c.id, tc.tax_component_id, 6.0, '2017-07-01', true, 'SYSTEM'
FROM press_db.mst_tax_category c, press_db.mst_tax_component tc
WHERE c.code = 'GST_12' AND tc.code = 'SGST'
ON CONFLICT (tax_category_id, tax_component_id, effective_from) DO NOTHING;

INSERT INTO press_db.mst_tax_category_component
    (tax_category_id, tax_component_id, rate_percent, effective_from, is_active, created_by)
SELECT c.id, tc.tax_component_id, 12.0, '2017-07-01', true, 'SYSTEM'
FROM press_db.mst_tax_category c, press_db.mst_tax_component tc
WHERE c.code = 'GST_12' AND tc.code = 'IGST'
ON CONFLICT (tax_category_id, tax_component_id, effective_from) DO NOTHING;

-- GST 18% → CGST 9%, SGST 9%, IGST 18%
INSERT INTO press_db.mst_tax_category_component
    (tax_category_id, tax_component_id, rate_percent, effective_from, is_active, created_by)
SELECT c.id, tc.tax_component_id, 9.0, '2017-07-01', true, 'SYSTEM'
FROM press_db.mst_tax_category c, press_db.mst_tax_component tc
WHERE c.code = 'GST_18' AND tc.code = 'CGST'
ON CONFLICT (tax_category_id, tax_component_id, effective_from) DO NOTHING;

INSERT INTO press_db.mst_tax_category_component
    (tax_category_id, tax_component_id, rate_percent, effective_from, is_active, created_by)
SELECT c.id, tc.tax_component_id, 9.0, '2017-07-01', true, 'SYSTEM'
FROM press_db.mst_tax_category c, press_db.mst_tax_component tc
WHERE c.code = 'GST_18' AND tc.code = 'SGST'
ON CONFLICT (tax_category_id, tax_component_id, effective_from) DO NOTHING;

INSERT INTO press_db.mst_tax_category_component
    (tax_category_id, tax_component_id, rate_percent, effective_from, is_active, created_by)
SELECT c.id, tc.tax_component_id, 18.0, '2017-07-01', true, 'SYSTEM'
FROM press_db.mst_tax_category c, press_db.mst_tax_component tc
WHERE c.code = 'GST_18' AND tc.code = 'IGST'
ON CONFLICT (tax_category_id, tax_component_id, effective_from) DO NOTHING;

-- GST 28% → CGST 14%, SGST 14%, IGST 28%
INSERT INTO press_db.mst_tax_category_component
    (tax_category_id, tax_component_id, rate_percent, effective_from, is_active, created_by)
SELECT c.id, tc.tax_component_id, 14.0, '2017-07-01', true, 'SYSTEM'
FROM press_db.mst_tax_category c, press_db.mst_tax_component tc
WHERE c.code = 'GST_28' AND tc.code = 'CGST'
ON CONFLICT (tax_category_id, tax_component_id, effective_from) DO NOTHING;

INSERT INTO press_db.mst_tax_category_component
    (tax_category_id, tax_component_id, rate_percent, effective_from, is_active, created_by)
SELECT c.id, tc.tax_component_id, 14.0, '2017-07-01', true, 'SYSTEM'
FROM press_db.mst_tax_category c, press_db.mst_tax_component tc
WHERE c.code = 'GST_28' AND tc.code = 'SGST'
ON CONFLICT (tax_category_id, tax_component_id, effective_from) DO NOTHING;

INSERT INTO press_db.mst_tax_category_component
    (tax_category_id, tax_component_id, rate_percent, effective_from, is_active, created_by)
SELECT c.id, tc.tax_component_id, 28.0, '2017-07-01', true, 'SYSTEM'
FROM press_db.mst_tax_category c, press_db.mst_tax_component tc
WHERE c.code = 'GST_28' AND tc.code = 'IGST'
ON CONFLICT (tax_category_id, tax_component_id, effective_from) DO NOTHING;


-- ============================================================================
-- 9. CHART OF ACCOUNTS — ACCOUNT HEADS (Accounting module essentials)
--    Level-0 = Root Groups, Level-1 = Sub-Groups, Level-2 = Ledger accounts
--    account_type: ASSET, LIABILITY, INCOME, EXPENSE, EQUITY
-- ============================================================================

-- ── Level 0 — Root Groups ──────────────────────────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
VALUES
    ('1000', 'Assets',                   'ASSET',     NULL, false, true,  0, 1,  true),
    ('2000', 'Liabilities',              'LIABILITY', NULL, false, true,  0, 2,  true),
    ('3000', 'Equity',                   'EQUITY',    NULL, false, true,  0, 3,  true),
    ('4000', 'Income',                   'INCOME',    NULL, false, true,  0, 4,  true),
    ('5000', 'Expenses',                 'EXPENSE',   NULL, false, true,  0, 5,  true)
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 1 — Asset Sub-Groups ─────────────────────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('1100', 'Current Assets',            'ASSET',  '1000', false, true,  1, 10),
    ('1200', 'Fixed Assets',              'ASSET',  '1000', false, true,  1, 20),
    ('1300', 'Investments',               'ASSET',  '1000', false, true,  1, 30)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 1 — Liability Sub-Groups ─────────────────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('2100', 'Current Liabilities',       'LIABILITY', '2000', false, true,  1, 10),
    ('2200', 'Long-Term Liabilities',     'LIABILITY', '2000', false, true,  1, 20),
    ('2300', 'Duties & Taxes Payable',    'LIABILITY', '2000', false, true,  1, 30)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 1 — Income Sub-Groups ────────────────────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('4100', 'Direct Income',             'INCOME', '4000', false, true,  1, 10),
    ('4200', 'Indirect Income',           'INCOME', '4000', false, true,  1, 20)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 1 — Expense Sub-Groups ───────────────────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('5100', 'Direct Expenses',           'EXPENSE', '5000', false, true,  1, 10),
    ('5200', 'Indirect Expenses',         'EXPENSE', '5000', false, true,  1, 20),
    ('5300', 'Administrative Expenses',   'EXPENSE', '5000', false, true,  1, 30),
    ('5400', 'Financial Expenses',        'EXPENSE', '5000', false, true,  1, 40)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 2 — Current Asset Ledgers ────────────────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('1101', 'Cash in Hand',              'ASSET', '1100', false, false, 2, 1),
    ('1102', 'Bank Accounts',             'ASSET', '1100', false, true,  2, 2),
    ('1103', 'Accounts Receivable',       'ASSET', '1100', true,  true,  2, 3),
    ('1104', 'Sundry Debtors',            'ASSET', '1100', true,  false, 2, 4),
    ('1105', 'Advance to Suppliers',      'ASSET', '1100', true,  false, 2, 5),
    ('1106', 'Advance to Employees',      'ASSET', '1100', false, false, 2, 6),
    ('1107', 'Input CGST Receivable',     'ASSET', '1100', false, false, 2, 7),
    ('1108', 'Input SGST Receivable',     'ASSET', '1100', false, false, 2, 8),
    ('1109', 'Input IGST Receivable',     'ASSET', '1100', false, false, 2, 9),
    ('1110', 'TDS Receivable',            'ASSET', '1100', false, false, 2, 10),
    ('1111', 'Prepaid Expenses',          'ASSET', '1100', false, false, 2, 11),
    ('1112', 'Stock in Hand',             'ASSET', '1100', false, false, 2, 12),
    ('1113', 'Work in Progress',          'ASSET', '1100', false, false, 2, 13),
    ('1114', 'Security Deposits (Asset)', 'ASSET', '1100', false, false, 2, 14),
    ('1115', 'Input CESS Receivable',     'ASSET', '1100', false, false, 2, 15)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 2 — Fixed Asset Ledgers ──────────────────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('1201', 'Plant & Machinery',         'ASSET', '1200', false, false, 2, 1),
    ('1202', 'Furniture & Fixtures',      'ASSET', '1200', false, false, 2, 2),
    ('1203', 'Vehicles',                  'ASSET', '1200', false, false, 2, 3),
    ('1204', 'Office Equipment',          'ASSET', '1200', false, false, 2, 4),
    ('1205', 'Computer & IT Equipment',   'ASSET', '1200', false, false, 2, 5),
    ('1206', 'Building',                  'ASSET', '1200', false, false, 2, 6),
    ('1207', 'Land',                      'ASSET', '1200', false, false, 2, 7),
    ('1208', 'Accumulated Depreciation',  'ASSET', '1200', false, false, 2, 8)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 2 — Current Liability Ledgers ────────────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('2101', 'Accounts Payable',          'LIABILITY', '2100', true,  true,  2, 1),
    ('2102', 'Sundry Creditors',          'LIABILITY', '2100', true,  false, 2, 2),
    ('2103', 'Advance from Customers',    'LIABILITY', '2100', true,  false, 2, 3),
    ('2104', 'Salary Payable',            'LIABILITY', '2100', false, false, 2, 4),
    ('2105', 'Expense Payable',           'LIABILITY', '2100', false, false, 2, 5),
    ('2106', 'Security Deposits (Liability)', 'LIABILITY', '2100', false, false, 2, 6),
    ('2107', 'EMD Received',              'LIABILITY', '2100', false, false, 2, 7)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 2 — Duties & Taxes Payable Ledgers ──────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('2301', 'Output CGST Payable',       'LIABILITY', '2300', false, false, 2, 1),
    ('2302', 'Output SGST Payable',       'LIABILITY', '2300', false, false, 2, 2),
    ('2303', 'Output IGST Payable',       'LIABILITY', '2300', false, false, 2, 3),
    ('2304', 'Output CESS Payable',       'LIABILITY', '2300', false, false, 2, 4),
    ('2305', 'TDS Payable',               'LIABILITY', '2300', false, false, 2, 5),
    ('2306', 'TCS Payable',               'LIABILITY', '2300', false, false, 2, 6),
    ('2307', 'Professional Tax Payable',  'LIABILITY', '2300', false, false, 2, 7),
    ('2308', 'PF Payable',                'LIABILITY', '2300', false, false, 2, 8),
    ('2309', 'ESI Payable',               'LIABILITY', '2300', false, false, 2, 9)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 2 — Equity Ledgers ───────────────────────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('3001', 'Capital Account',           'EQUITY', '3000', false, false, 1, 1),
    ('3002', 'Retained Earnings',         'EQUITY', '3000', false, false, 1, 2),
    ('3003', 'Profit & Loss Account',     'EQUITY', '3000', false, false, 1, 3),
    ('3004', 'Drawings',                  'EQUITY', '3000', false, false, 1, 4)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 2 — Income Ledgers ───────────────────────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('4101', 'Sales — Printing Jobs',     'INCOME', '4100', false, false, 2, 1),
    ('4102', 'Sales — Paper & Material',  'INCOME', '4100', false, false, 2, 2),
    ('4103', 'Sales — Design Services',   'INCOME', '4100', false, false, 2, 3),
    ('4104', 'Job Work Income',           'INCOME', '4100', false, false, 2, 4),
    ('4201', 'Interest Income',           'INCOME', '4200', false, false, 2, 1),
    ('4202', 'Discount Received',         'INCOME', '4200', false, false, 2, 2),
    ('4203', 'Commission Income',         'INCOME', '4200', false, false, 2, 3),
    ('4204', 'Round Off Income',          'INCOME', '4200', false, false, 2, 4),
    ('4205', 'Miscellaneous Income',      'INCOME', '4200', false, false, 2, 5)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 2 — Direct Expense Ledgers (Printing Press specific) ─────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('5101', 'Paper & Raw Material',      'EXPENSE', '5100', false, false, 2, 1),
    ('5102', 'Ink & Consumables',         'EXPENSE', '5100', false, false, 2, 2),
    ('5103', 'Plate & CTP Charges',       'EXPENSE', '5100', false, false, 2, 3),
    ('5104', 'Binding & Finishing',        'EXPENSE', '5100', false, false, 2, 4),
    ('5105', 'Labour / Wages',            'EXPENSE', '5100', false, false, 2, 5),
    ('5106', 'Outsource / Job Work',      'EXPENSE', '5100', false, false, 2, 6),
    ('5107', 'Packaging Material',        'EXPENSE', '5100', false, false, 2, 7),
    ('5108', 'Design & Pre-Press',        'EXPENSE', '5100', false, false, 2, 8)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 2 — Indirect Expense Ledgers ─────────────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('5201', 'Electricity & Power',       'EXPENSE', '5200', false, false, 2, 1),
    ('5202', 'Rent Expense',              'EXPENSE', '5200', false, false, 2, 2),
    ('5203', 'Repair & Maintenance',      'EXPENSE', '5200', false, false, 2, 3),
    ('5204', 'Transport & Freight',       'EXPENSE', '5200', false, false, 2, 4),
    ('5205', 'Insurance',                 'EXPENSE', '5200', false, false, 2, 5),
    ('5206', 'Depreciation',              'EXPENSE', '5200', false, false, 2, 6)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 2 — Administrative Expense Ledgers ───────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('5301', 'Salary & Wages',            'EXPENSE', '5300', false, false, 2, 1),
    ('5302', 'Office Supplies',           'EXPENSE', '5300', false, false, 2, 2),
    ('5303', 'Telephone & Internet',      'EXPENSE', '5300', false, false, 2, 3),
    ('5304', 'Travel & Conveyance',       'EXPENSE', '5300', false, false, 2, 4),
    ('5305', 'Professional Fees',         'EXPENSE', '5300', false, false, 2, 5),
    ('5306', 'Legal Expenses',            'EXPENSE', '5300', false, false, 2, 6),
    ('5307', 'Audit Fees',                'EXPENSE', '5300', false, false, 2, 7),
    ('5308', 'Printing & Stationery',     'EXPENSE', '5300', false, false, 2, 8),
    ('5309', 'Miscellaneous Expenses',    'EXPENSE', '5300', false, false, 2, 9),
    ('5310', 'Staff Welfare',             'EXPENSE', '5300', false, false, 2, 10),
    ('5311', 'Courier & Postage',         'EXPENSE', '5300', false, false, 2, 11)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;

-- ── Level 2 — Financial Expense Ledgers ────────────────────────────────────
INSERT INTO press_db.mst_account_head
    (account_code, account_name, account_type, parent_account_id, is_party_account, is_group, level_no, sort_order, is_active)
SELECT v.account_code, v.account_name, v.account_type,
       p.account_head_id, v.is_party_account, v.is_group, v.level_no, v.sort_order, true
FROM (VALUES
    ('5401', 'Bank Charges',              'EXPENSE', '5400', false, false, 2, 1),
    ('5402', 'Interest Paid',             'EXPENSE', '5400', false, false, 2, 2),
    ('5403', 'Discount Allowed',          'EXPENSE', '5400', false, false, 2, 3),
    ('5404', 'Round Off Expense',         'EXPENSE', '5400', false, false, 2, 4),
    ('5405', 'Bad Debts',                 'EXPENSE', '5400', false, false, 2, 5)
) AS v(account_code, account_name, account_type, parent_code, is_party_account, is_group, level_no, sort_order)
JOIN press_db.mst_account_head p ON p.account_code = v.parent_code
ON CONFLICT (account_code) DO NOTHING;


-- ============================================================================
-- 10. EXPENSE CATEGORIES (New table — mst_expense_category)
--     Maps each category to an account head for automatic GL posting.
-- ============================================================================

INSERT INTO press_db.mst_expense_category
    (category_code, category_name, parent_category_id, account_head_id, description, is_reimbursable, requires_approval, approval_limit, is_active, created_by)
SELECT
    v.category_code, v.category_name, NULL,
    ah.account_head_id, v.description,
    v.is_reimbursable, v.requires_approval, v.approval_limit, true, 'SYSTEM'
FROM (VALUES
    ('OFFICE',     'Office Supplies',       '5302', 'Stationery, toner, paper for office use',         false, true,  5000.00),
    ('TRAVEL',     'Travel & Conveyance',   '5304', 'Staff travel, local conveyance, fuel',            true,  true,  10000.00),
    ('UTILITIES',  'Utilities',             '5201', 'Electricity, water, generator fuel',               false, true,  25000.00),
    ('REPAIRS',    'Repairs & Maintenance',  '5203', 'Machine repair, building maintenance, AMC',       false, true,  50000.00),
    ('SALARY',     'Salary & Wages',        '5301', 'Monthly salaries, overtime, bonus',                false, true,  0.00),
    ('RENT',       'Rent',                  '5202', 'Office/factory/godown rent',                       false, true,  100000.00),
    ('PRINTING',   'Printing & Stationery', '5308', 'Printing, visiting cards, letterheads',            false, false, 5000.00),
    ('TRANSPORT',  'Transport & Freight',   '5204', 'Delivery, courier, freight charges',               false, true,  15000.00),
    ('TELECOM',    'Telephone & Internet',  '5303', 'Mobile, landline, broadband, SIM plans',           false, false, 5000.00),
    ('INSURANCE',  'Insurance',             '5205', 'Fire, marine, vehicle, health insurance',          false, true,  100000.00),
    ('LEGAL',      'Legal & Professional',  '5305', 'Lawyer fees, consultant fees, CA charges',         false, true,  50000.00),
    ('BANK',       'Bank Charges',          '5401', 'Bank fees, NEFT/RTGS charges, annual maintenance', false, false, 5000.00),
    ('WELFARE',    'Staff Welfare',         '5310', 'Tea, snacks, medical aid, gifts',                  false, false, 10000.00),
    ('COURIER',    'Courier & Postage',     '5311', 'Speed post, courier, stamps',                      false, false, 5000.00),
    ('MISC',       'Miscellaneous',         '5309', 'All other expenses not classified above',          false, true,  10000.00)
) AS v(category_code, category_name, account_code, description, is_reimbursable, requires_approval, approval_limit)
JOIN press_db.mst_account_head ah ON ah.account_code = v.account_code
ON CONFLICT (category_code) DO NOTHING;


-- ============================================================================
-- 11. COST CENTERS (New table — mst_cost_center)
--     Linked to mst_department where applicable. Printing Press specific.
-- ============================================================================

INSERT INTO press_db.mst_cost_center
    (center_code, center_name, parent_center_id, department_id, description, is_active, created_by)
VALUES
    ('CC-ADMIN',   'Administration',      NULL, NULL, 'General administration and management',          true, 'SYSTEM'),
    ('CC-PREPR',   'Pre-Press',           NULL, NULL, 'Design, plate-making, CTP, proofing',            true, 'SYSTEM'),
    ('CC-PRESS',   'Press / Printing',    NULL, NULL, 'Offset, digital, screen printing machines',      true, 'SYSTEM'),
    ('CC-POSTPR',  'Post-Press',          NULL, NULL, 'Binding, lamination, cutting, folding',          true, 'SYSTEM'),
    ('CC-STORE',   'Stores & Inventory',  NULL, NULL, 'Raw material store, finished goods warehouse',   true, 'SYSTEM'),
    ('CC-SALES',   'Sales & Marketing',   NULL, NULL, 'Sales team, marketing, customer relations',      true, 'SYSTEM'),
    ('CC-ACCT',    'Accounts & Finance',  NULL, NULL, 'Accounting, billing, collections, payments',     true, 'SYSTEM'),
    ('CC-HR',      'Human Resources',     NULL, NULL, 'HR, payroll, attendance, recruitment',            true, 'SYSTEM'),
    ('CC-IT',      'IT & Systems',        NULL, NULL, 'IT infra, software, network, ERP support',       true, 'SYSTEM'),
    ('CC-QC',      'Quality Control',     NULL, NULL, 'Quality check, inspection, color matching',      true, 'SYSTEM'),
    ('CC-DISPATCH','Dispatch & Delivery', NULL, NULL, 'Packing, dispatch, logistics, delivery',         true, 'SYSTEM'),
    ('CC-MAINT',   'Maintenance',         NULL, NULL, 'Machine maintenance, electrical, plumbing',      true, 'SYSTEM')
ON CONFLICT (center_code) DO NOTHING;


COMMIT;

-- ============================================================================
-- SEED DATA SUMMARY
-- ============================================================================
-- 1.  mst_direction             —  2 rows  (Output Tax, Input Tax)
-- 2.  mst_transaction_type      — 17 rows  (All accounting transaction types)
-- 3.  mst_voucher_type          — 14 rows  (SI, PI, PROF, CN, DN, EXP, JV, BR, BP, CONT, RCT, PMT, PO, GRN)
-- 4.  mst_document_sequence     — 15 rows  (Auto-numbering for all document types)
-- 5.  mst_tax_component         —  6 rows  (CGST, SGST, IGST, CESS, TDS, TCS)
-- 6.  mst_tax_type              —  4 rows  (GST, TDS, TCS, Exempt)
-- 7.  mst_tax_category          — 10 rows  (GST 0/5/12/18/28%, Exempt, Nil, Zero, RCM slabs)
-- 8.  mst_tax_category_component— 12 rows  (CGST+SGST+IGST splits for 5/12/18/28%)
-- 9.  mst_account_head          — 90+ rows (Full chart of accounts: Assets, Liabilities, Equity, Income, Expenses)
-- 10. mst_expense_category      — 15 rows  (Office, Travel, Utilities, Repairs, Salary, Rent, etc.)
-- 11. mst_cost_center           — 12 rows  (Admin, Pre-Press, Press, Post-Press, Stores, Sales, etc.)
-- ============================================================================



