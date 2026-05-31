-- ============================================================================
-- ACCOUNTING MODULE — COMPLETE TABLE CREATION SCRIPT
-- Schema  : press_db
-- Database: PostgreSQL
-- Covers  : Accounts Receivable, Accounts Payable, All Invoice Types,
--           Expenses, Journal Vouchers, Bank Transactions, Reconciliation,
--           Credit/Debit Notes, Tax Ledger, AR/AP Outstanding
-- ============================================================================
-- NOTE: Existing tables NOT recreated here:
--   trn_sales_invoice, trn_sales_invoice_item,
--   trn_receipt, trn_receipt_allocation,
--   trn_payment, trn_payment_allocation,
--   trn_ledger, trn_account_ledger, trn_advance_ledger,
--   mst_account_head, mst_bank_account, mst_voucher_type,
--   mst_financial_year, mst_payment_term
-- ============================================================================

BEGIN;

-- ============================================================================
-- 1. PURCHASE INVOICE (Accounts Payable — Supplier Invoices)
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_purchase_invoice
(
    purchase_invoice_id   bigserial       NOT NULL,
    invoice_no            character varying(50)  COLLATE pg_catalog."default" NOT NULL,
    invoice_date          date            NOT NULL DEFAULT CURRENT_DATE,
    due_date              date,
    company_id            integer         NOT NULL,
    location_id           integer,
    fin_year_id           integer,
    party_id              integer         NOT NULL,
    supplier_id           integer,
    supplier_invoice_no   character varying(100) COLLATE pg_catalog."default",
    supplier_invoice_date date,
    billing_address_id    integer,
    shipping_address_id   integer,
    currency_id           integer,
    exchange_rate         numeric(10, 4)  DEFAULT 1,
    payment_term_id       integer,
    place_of_supply       character varying(150) COLLATE pg_catalog."default",
    is_import             boolean         DEFAULT false,
    is_reverse_charge     boolean         DEFAULT false,
    po_no                 character varying(100) COLLATE pg_catalog."default",
    po_date               date,
    grn_no                character varying(100) COLLATE pg_catalog."default",
    grn_date              date,
    subtotal_amount       numeric(18, 2)  DEFAULT 0,
    discount_amount       numeric(18, 2)  DEFAULT 0,
    taxable_amount        numeric(18, 2)  DEFAULT 0,
    cgst_amount           numeric(18, 2)  DEFAULT 0,
    sgst_amount           numeric(18, 2)  DEFAULT 0,
    igst_amount           numeric(18, 2)  DEFAULT 0,
    cess_amount           numeric(18, 2)  DEFAULT 0,
    total_tax_amount      numeric(18, 2)  DEFAULT 0,
    tds_amount            numeric(18, 2)  DEFAULT 0,
    round_off             numeric(10, 2)  DEFAULT 0,
    grand_total           numeric(18, 2)  DEFAULT 0,
    paid_amount           numeric(18, 2)  DEFAULT 0,
    balance_amount        numeric(18, 2)  DEFAULT 0,
    status                character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'DRAFT'::character varying,
    is_cancelled          boolean         DEFAULT false,
    cancelled_by          bigint,
    cancelled_on          timestamp without time zone,
    cancel_reason         text            COLLATE pg_catalog."default",
    is_posted_to_gl       boolean         DEFAULT false,
    gl_posted_on          timestamp without time zone,
    gl_posted_by          bigint,
    internal_notes        text            COLLATE pg_catalog."default",
    attachments_json      jsonb           DEFAULT '[]'::jsonb,
    created_by            bigint          NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT trn_purchase_invoice_pkey PRIMARY KEY (purchase_invoice_id),
    CONSTRAINT uq_purchase_invoice_no UNIQUE (invoice_no)
);

COMMENT ON TABLE press_db.trn_purchase_invoice
    IS 'Purchase invoice header for goods/services received from suppliers. Supports GST (CGST/SGST/IGST), reverse charge, TDS, import purchases.';

CREATE TABLE IF NOT EXISTS press_db.trn_purchase_invoice_item
(
    purchase_item_id      bigserial       NOT NULL,
    purchase_invoice_id   bigint          NOT NULL,
    item_sequence         integer         NOT NULL DEFAULT 1,
    item_id               bigint,
    account_head_id       bigint,
    description           text            COLLATE pg_catalog."default" NOT NULL,
    hsn_sac_code          character varying(20) COLLATE pg_catalog."default",
    uom_id                integer,
    quantity              numeric(14, 4)  DEFAULT 0,
    unit_rate             numeric(14, 4)  DEFAULT 0,
    discount_percent      numeric(6, 3)   DEFAULT 0,
    discount_amount       numeric(14, 2)  DEFAULT 0,
    taxable_value         numeric(14, 2)  DEFAULT 0,
    tax_category_id       integer,
    cgst_percent          numeric(6, 3)   DEFAULT 0,
    cgst_amount           numeric(14, 2)  DEFAULT 0,
    sgst_percent          numeric(6, 3)   DEFAULT 0,
    sgst_amount           numeric(14, 2)  DEFAULT 0,
    igst_percent          numeric(6, 3)   DEFAULT 0,
    igst_amount           numeric(14, 2)  DEFAULT 0,
    cess_percent          numeric(6, 3)   DEFAULT 0,
    cess_amount           numeric(14, 2)  DEFAULT 0,
    total_tax_amount      numeric(14, 2)  DEFAULT 0,
    line_total            numeric(14, 2)  DEFAULT 0,
    cost_center_id        integer,
    job_id                bigint,
    remarks               text            COLLATE pg_catalog."default",
    CONSTRAINT trn_purchase_invoice_item_pkey PRIMARY KEY (purchase_item_id)
);

COMMENT ON TABLE press_db.trn_purchase_invoice_item
    IS 'Purchase invoice line items with full GST breakup (CGST/SGST/IGST/CESS) per item.';


-- ============================================================================
-- 2. PROFORMA INVOICE
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_proforma_invoice
(
    proforma_invoice_id   bigserial       NOT NULL,
    proforma_no           character varying(50)  COLLATE pg_catalog."default" NOT NULL,
    proforma_date         date            NOT NULL DEFAULT CURRENT_DATE,
    valid_till            date,
    company_id            integer         NOT NULL,
    location_id           integer,
    fin_year_id           integer,
    party_id              integer         NOT NULL,
    billing_address_id    integer,
    shipping_address_id   integer,
    job_id                bigint,
    quotation_id          bigint,
    currency_id           integer,
    exchange_rate         numeric(10, 4)  DEFAULT 1,
    payment_term_id       integer,
    sales_person          character varying(200) COLLATE pg_catalog."default",
    place_of_supply       character varying(150) COLLATE pg_catalog."default",
    is_export             boolean         DEFAULT false,
    po_no                 character varying(100) COLLATE pg_catalog."default",
    po_date               date,
    subtotal_amount       numeric(18, 2)  DEFAULT 0,
    discount_amount       numeric(18, 2)  DEFAULT 0,
    taxable_amount        numeric(18, 2)  DEFAULT 0,
    cgst_amount           numeric(18, 2)  DEFAULT 0,
    sgst_amount           numeric(18, 2)  DEFAULT 0,
    igst_amount           numeric(18, 2)  DEFAULT 0,
    cess_amount           numeric(18, 2)  DEFAULT 0,
    total_tax_amount      numeric(18, 2)  DEFAULT 0,
    round_off             numeric(10, 2)  DEFAULT 0,
    grand_total           numeric(18, 2)  DEFAULT 0,
    converted_to_invoice  boolean         DEFAULT false,
    sales_invoice_id      bigint,
    status                character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'DRAFT'::character varying,
    terms_conditions      text            COLLATE pg_catalog."default",
    internal_notes        text            COLLATE pg_catalog."default",
    attachments_json      jsonb           DEFAULT '[]'::jsonb,
    created_by            bigint          NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT trn_proforma_invoice_pkey PRIMARY KEY (proforma_invoice_id),
    CONSTRAINT uq_proforma_no UNIQUE (proforma_no)
);

COMMENT ON TABLE press_db.trn_proforma_invoice
    IS 'Proforma invoice issued to customer before delivery/final sales invoice. Can be converted to sales invoice. Does NOT post to GL.';

CREATE TABLE IF NOT EXISTS press_db.trn_proforma_invoice_item
(
    proforma_item_id      bigserial       NOT NULL,
    proforma_invoice_id   bigint          NOT NULL,
    item_sequence         integer         NOT NULL DEFAULT 1,
    item_id               bigint,
    account_head_id       bigint,
    description           text            COLLATE pg_catalog."default" NOT NULL,
    hsn_sac_code          character varying(20) COLLATE pg_catalog."default",
    uom_id                integer,
    quantity              numeric(14, 4)  DEFAULT 0,
    unit_rate             numeric(14, 4)  DEFAULT 0,
    discount_percent      numeric(6, 3)   DEFAULT 0,
    discount_amount       numeric(14, 2)  DEFAULT 0,
    taxable_value         numeric(14, 2)  DEFAULT 0,
    tax_category_id       integer,
    cgst_percent          numeric(6, 3)   DEFAULT 0,
    cgst_amount           numeric(14, 2)  DEFAULT 0,
    sgst_percent          numeric(6, 3)   DEFAULT 0,
    sgst_amount           numeric(14, 2)  DEFAULT 0,
    igst_percent          numeric(6, 3)   DEFAULT 0,
    igst_amount           numeric(14, 2)  DEFAULT 0,
    cess_percent          numeric(6, 3)   DEFAULT 0,
    cess_amount           numeric(14, 2)  DEFAULT 0,
    total_tax_amount      numeric(14, 2)  DEFAULT 0,
    line_total            numeric(14, 2)  DEFAULT 0,
    job_id                bigint,
    remarks               text            COLLATE pg_catalog."default",
    CONSTRAINT trn_proforma_invoice_item_pkey PRIMARY KEY (proforma_item_id)
);

COMMENT ON TABLE press_db.trn_proforma_invoice_item
    IS 'Proforma invoice line items with full GST breakup.';


-- ============================================================================
-- 3. CREDIT NOTE (Sales Return / Adjustment — reduces AR)
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_credit_note
(
    credit_note_id        bigserial       NOT NULL,
    credit_note_no        character varying(50)  COLLATE pg_catalog."default" NOT NULL,
    credit_note_date      date            NOT NULL DEFAULT CURRENT_DATE,
    credit_note_type      character varying(30)  COLLATE pg_catalog."default" NOT NULL DEFAULT 'SALES_RETURN'::character varying,
    company_id            integer         NOT NULL,
    location_id           integer,
    fin_year_id           integer,
    party_id              integer         NOT NULL,
    original_invoice_id   bigint,
    original_invoice_no   character varying(50) COLLATE pg_catalog."default",
    original_invoice_date date,
    reason                text            COLLATE pg_catalog."default",
    billing_address_id    integer,
    currency_id           integer,
    exchange_rate         numeric(10, 4)  DEFAULT 1,
    place_of_supply       character varying(150) COLLATE pg_catalog."default",
    subtotal_amount       numeric(18, 2)  DEFAULT 0,
    discount_amount       numeric(18, 2)  DEFAULT 0,
    taxable_amount        numeric(18, 2)  DEFAULT 0,
    cgst_amount           numeric(18, 2)  DEFAULT 0,
    sgst_amount           numeric(18, 2)  DEFAULT 0,
    igst_amount           numeric(18, 2)  DEFAULT 0,
    cess_amount           numeric(18, 2)  DEFAULT 0,
    total_tax_amount      numeric(18, 2)  DEFAULT 0,
    round_off             numeric(10, 2)  DEFAULT 0,
    grand_total           numeric(18, 2)  DEFAULT 0,
    adjusted_amount       numeric(18, 2)  DEFAULT 0,
    unadjusted_amount     numeric(18, 2)  DEFAULT 0,
    status                character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'DRAFT'::character varying,
    is_cancelled          boolean         DEFAULT false,
    cancelled_by          bigint,
    cancelled_on          timestamp without time zone,
    cancel_reason         text            COLLATE pg_catalog."default",
    is_posted_to_gl       boolean         DEFAULT false,
    gl_posted_on          timestamp without time zone,
    gl_posted_by          bigint,
    e_invoice_irn         character varying(100) COLLATE pg_catalog."default",
    e_invoice_ack_no      character varying(50)  COLLATE pg_catalog."default",
    e_invoice_ack_date    timestamp without time zone,
    internal_notes        text            COLLATE pg_catalog."default",
    attachments_json      jsonb           DEFAULT '[]'::jsonb,
    created_by            bigint          NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT trn_credit_note_pkey PRIMARY KEY (credit_note_id),
    CONSTRAINT uq_credit_note_no UNIQUE (credit_note_no)
);

COMMENT ON TABLE press_db.trn_credit_note
    IS 'Credit note issued to customer for sales returns, rate difference, or post-sale adjustments. Reduces Accounts Receivable. Supports GST and e-Invoice.';

COMMENT ON COLUMN press_db.trn_credit_note.credit_note_type
    IS 'SALES_RETURN, RATE_DIFFERENCE, QUALITY_ISSUE, DISCOUNT_AFTER_SALE, OTHER';

CREATE TABLE IF NOT EXISTS press_db.trn_credit_note_item
(
    credit_note_item_id   bigserial       NOT NULL,
    credit_note_id        bigint          NOT NULL,
    item_sequence         integer         NOT NULL DEFAULT 1,
    original_invoice_item_id bigint,
    item_id               bigint,
    account_head_id       bigint,
    description           text            COLLATE pg_catalog."default" NOT NULL,
    hsn_sac_code          character varying(20) COLLATE pg_catalog."default",
    uom_id                integer,
    quantity              numeric(14, 4)  DEFAULT 0,
    unit_rate             numeric(14, 4)  DEFAULT 0,
    discount_percent      numeric(6, 3)   DEFAULT 0,
    discount_amount       numeric(14, 2)  DEFAULT 0,
    taxable_value         numeric(14, 2)  DEFAULT 0,
    tax_category_id       integer,
    cgst_percent          numeric(6, 3)   DEFAULT 0,
    cgst_amount           numeric(14, 2)  DEFAULT 0,
    sgst_percent          numeric(6, 3)   DEFAULT 0,
    sgst_amount           numeric(14, 2)  DEFAULT 0,
    igst_percent          numeric(6, 3)   DEFAULT 0,
    igst_amount           numeric(14, 2)  DEFAULT 0,
    cess_percent          numeric(6, 3)   DEFAULT 0,
    cess_amount           numeric(14, 2)  DEFAULT 0,
    total_tax_amount      numeric(14, 2)  DEFAULT 0,
    line_total            numeric(14, 2)  DEFAULT 0,
    remarks               text            COLLATE pg_catalog."default",
    CONSTRAINT trn_credit_note_item_pkey PRIMARY KEY (credit_note_item_id)
);

COMMENT ON TABLE press_db.trn_credit_note_item
    IS 'Credit note line items with full GST breakup per item.';


-- ============================================================================
-- 4. DEBIT NOTE (Purchase Return / Adjustment — reduces AP)
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_debit_note
(
    debit_note_id         bigserial       NOT NULL,
    debit_note_no         character varying(50)  COLLATE pg_catalog."default" NOT NULL,
    debit_note_date       date            NOT NULL DEFAULT CURRENT_DATE,
    debit_note_type       character varying(30)  COLLATE pg_catalog."default" NOT NULL DEFAULT 'PURCHASE_RETURN'::character varying,
    company_id            integer         NOT NULL,
    location_id           integer,
    fin_year_id           integer,
    party_id              integer         NOT NULL,
    supplier_id           integer,
    original_invoice_id   bigint,
    original_invoice_no   character varying(50) COLLATE pg_catalog."default",
    original_invoice_date date,
    reason                text            COLLATE pg_catalog."default",
    billing_address_id    integer,
    currency_id           integer,
    exchange_rate         numeric(10, 4)  DEFAULT 1,
    place_of_supply       character varying(150) COLLATE pg_catalog."default",
    subtotal_amount       numeric(18, 2)  DEFAULT 0,
    discount_amount       numeric(18, 2)  DEFAULT 0,
    taxable_amount        numeric(18, 2)  DEFAULT 0,
    cgst_amount           numeric(18, 2)  DEFAULT 0,
    sgst_amount           numeric(18, 2)  DEFAULT 0,
    igst_amount           numeric(18, 2)  DEFAULT 0,
    cess_amount           numeric(18, 2)  DEFAULT 0,
    total_tax_amount      numeric(18, 2)  DEFAULT 0,
    round_off             numeric(10, 2)  DEFAULT 0,
    grand_total           numeric(18, 2)  DEFAULT 0,
    adjusted_amount       numeric(18, 2)  DEFAULT 0,
    unadjusted_amount     numeric(18, 2)  DEFAULT 0,
    status                character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'DRAFT'::character varying,
    is_cancelled          boolean         DEFAULT false,
    cancelled_by          bigint,
    cancelled_on          timestamp without time zone,
    cancel_reason         text            COLLATE pg_catalog."default",
    is_posted_to_gl       boolean         DEFAULT false,
    gl_posted_on          timestamp without time zone,
    gl_posted_by          bigint,
    internal_notes        text            COLLATE pg_catalog."default",
    attachments_json      jsonb           DEFAULT '[]'::jsonb,
    created_by            bigint          NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT trn_debit_note_pkey PRIMARY KEY (debit_note_id),
    CONSTRAINT uq_debit_note_no UNIQUE (debit_note_no)
);

COMMENT ON TABLE press_db.trn_debit_note
    IS 'Debit note issued to supplier for purchase returns, rate difference, or quality issues. Reduces Accounts Payable. Supports GST.';

COMMENT ON COLUMN press_db.trn_debit_note.debit_note_type
    IS 'PURCHASE_RETURN, RATE_DIFFERENCE, QUALITY_ISSUE, SHORT_SUPPLY, OTHER';

CREATE TABLE IF NOT EXISTS press_db.trn_debit_note_item
(
    debit_note_item_id    bigserial       NOT NULL,
    debit_note_id         bigint          NOT NULL,
    item_sequence         integer         NOT NULL DEFAULT 1,
    original_invoice_item_id bigint,
    item_id               bigint,
    account_head_id       bigint,
    description           text            COLLATE pg_catalog."default" NOT NULL,
    hsn_sac_code          character varying(20) COLLATE pg_catalog."default",
    uom_id                integer,
    quantity              numeric(14, 4)  DEFAULT 0,
    unit_rate             numeric(14, 4)  DEFAULT 0,
    discount_percent      numeric(6, 3)   DEFAULT 0,
    discount_amount       numeric(14, 2)  DEFAULT 0,
    taxable_value         numeric(14, 2)  DEFAULT 0,
    tax_category_id       integer,
    cgst_percent          numeric(6, 3)   DEFAULT 0,
    cgst_amount           numeric(14, 2)  DEFAULT 0,
    sgst_percent          numeric(6, 3)   DEFAULT 0,
    sgst_amount           numeric(14, 2)  DEFAULT 0,
    igst_percent          numeric(6, 3)   DEFAULT 0,
    igst_amount           numeric(14, 2)  DEFAULT 0,
    cess_percent          numeric(6, 3)   DEFAULT 0,
    cess_amount           numeric(14, 2)  DEFAULT 0,
    total_tax_amount      numeric(14, 2)  DEFAULT 0,
    line_total            numeric(14, 2)  DEFAULT 0,
    remarks               text            COLLATE pg_catalog."default",
    CONSTRAINT trn_debit_note_item_pkey PRIMARY KEY (debit_note_item_id)
);

COMMENT ON TABLE press_db.trn_debit_note_item
    IS 'Debit note line items with full GST breakup per item.';


-- ============================================================================
-- 5. EXPENSE VOUCHER
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_expense_voucher
(
    expense_voucher_id    bigserial       NOT NULL,
    voucher_no            character varying(50)  COLLATE pg_catalog."default" NOT NULL,
    voucher_date          date            NOT NULL DEFAULT CURRENT_DATE,
    company_id            integer         NOT NULL,
    location_id           integer,
    fin_year_id           integer,
    expense_category      character varying(50) COLLATE pg_catalog."default",
    party_id              integer,
    employee_id           bigint,
    payment_mode          character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'CASH'::character varying,
    bank_account_id       integer,
    cheque_no             character varying(30) COLLATE pg_catalog."default",
    cheque_date           date,
    reference_no          character varying(100) COLLATE pg_catalog."default",
    reference_date        date,
    subtotal_amount       numeric(18, 2)  DEFAULT 0,
    taxable_amount        numeric(18, 2)  DEFAULT 0,
    cgst_amount           numeric(18, 2)  DEFAULT 0,
    sgst_amount           numeric(18, 2)  DEFAULT 0,
    igst_amount           numeric(18, 2)  DEFAULT 0,
    cess_amount           numeric(18, 2)  DEFAULT 0,
    total_tax_amount      numeric(18, 2)  DEFAULT 0,
    tds_amount            numeric(18, 2)  DEFAULT 0,
    grand_total           numeric(18, 2)  DEFAULT 0,
    narration             text            COLLATE pg_catalog."default",
    status                character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'DRAFT'::character varying,
    is_approved           boolean         DEFAULT false,
    approved_by           bigint,
    approved_on           timestamp without time zone,
    is_cancelled          boolean         DEFAULT false,
    cancelled_by          bigint,
    cancelled_on          timestamp without time zone,
    cancel_reason         text            COLLATE pg_catalog."default",
    is_posted_to_gl       boolean         DEFAULT false,
    gl_posted_on          timestamp without time zone,
    gl_posted_by          bigint,
    attachments_json      jsonb           DEFAULT '[]'::jsonb,
    created_by            bigint          NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT trn_expense_voucher_pkey PRIMARY KEY (expense_voucher_id),
    CONSTRAINT uq_expense_voucher_no UNIQUE (voucher_no)
);

COMMENT ON TABLE press_db.trn_expense_voucher
    IS 'Expense voucher for direct business expenses (rent, utilities, travel, repairs, etc.). Supports multi-line with GST and TDS. Approval workflow.';

COMMENT ON COLUMN press_db.trn_expense_voucher.expense_category
    IS 'OFFICE, TRAVEL, UTILITIES, REPAIRS, SALARY, RENT, PRINTING, TRANSPORT, MISC';

CREATE TABLE IF NOT EXISTS press_db.trn_expense_voucher_item
(
    expense_item_id       bigserial       NOT NULL,
    expense_voucher_id    bigint          NOT NULL,
    item_sequence         integer         NOT NULL DEFAULT 1,
    account_head_id       bigint          NOT NULL,
    description           text            COLLATE pg_catalog."default" NOT NULL,
    hsn_sac_code          character varying(20) COLLATE pg_catalog."default",
    amount                numeric(14, 2)  DEFAULT 0,
    tax_category_id       integer,
    cgst_percent          numeric(6, 3)   DEFAULT 0,
    cgst_amount           numeric(14, 2)  DEFAULT 0,
    sgst_percent          numeric(6, 3)   DEFAULT 0,
    sgst_amount           numeric(14, 2)  DEFAULT 0,
    igst_percent          numeric(6, 3)   DEFAULT 0,
    igst_amount           numeric(14, 2)  DEFAULT 0,
    cess_percent          numeric(6, 3)   DEFAULT 0,
    cess_amount           numeric(14, 2)  DEFAULT 0,
    total_tax_amount      numeric(14, 2)  DEFAULT 0,
    line_total            numeric(14, 2)  DEFAULT 0,
    cost_center_id        integer,
    job_id                bigint,
    remarks               text            COLLATE pg_catalog."default",
    CONSTRAINT trn_expense_voucher_item_pkey PRIMARY KEY (expense_item_id)
);

COMMENT ON TABLE press_db.trn_expense_voucher_item
    IS 'Expense voucher line items. Each line debits a different expense account head with optional GST breakup.';


-- ============================================================================
-- 6. JOURNAL VOUCHER (General Journal Entry)
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_journal_voucher
(
    journal_id            bigserial       NOT NULL,
    journal_no            character varying(50)  COLLATE pg_catalog."default" NOT NULL,
    journal_date          date            NOT NULL DEFAULT CURRENT_DATE,
    journal_type          character varying(30)  COLLATE pg_catalog."default" NOT NULL DEFAULT 'GENERAL'::character varying,
    company_id            integer         NOT NULL,
    location_id           integer,
    fin_year_id           integer,
    reference_no          character varying(100) COLLATE pg_catalog."default",
    reference_date        date,
    total_debit           numeric(18, 2)  DEFAULT 0,
    total_credit          numeric(18, 2)  DEFAULT 0,
    narration             text            COLLATE pg_catalog."default",
    status                character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'DRAFT'::character varying,
    is_auto_generated     boolean         DEFAULT false,
    source_voucher_type   character varying(50) COLLATE pg_catalog."default",
    source_voucher_id     bigint,
    source_voucher_no     character varying(50) COLLATE pg_catalog."default",
    is_reversing_entry    boolean         DEFAULT false,
    original_journal_id   bigint,
    reversal_date         date,
    is_cancelled          boolean         DEFAULT false,
    cancelled_by          bigint,
    cancelled_on          timestamp without time zone,
    cancel_reason         text            COLLATE pg_catalog."default",
    is_posted             boolean         DEFAULT false,
    posted_on             timestamp without time zone,
    posted_by             bigint,
    created_by            bigint          NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT trn_journal_voucher_pkey PRIMARY KEY (journal_id),
    CONSTRAINT uq_journal_no UNIQUE (journal_no)
);

COMMENT ON TABLE press_db.trn_journal_voucher
    IS 'General journal voucher header. Supports manual entries, auto-generated GL postings from invoices/payments, and reversing entries. Debit must equal Credit.';

COMMENT ON COLUMN press_db.trn_journal_voucher.journal_type
    IS 'GENERAL, OPENING, CLOSING, ADJUSTMENT, DEPRECIATION, PROVISION, REVERSAL, AUTO';

CREATE TABLE IF NOT EXISTS press_db.trn_journal_voucher_line
(
    journal_line_id       bigserial       NOT NULL,
    journal_id            bigint          NOT NULL,
    line_no               integer         NOT NULL DEFAULT 1,
    account_head_id       bigint          NOT NULL,
    party_id              integer,
    debit_amount          numeric(18, 2)  DEFAULT 0,
    credit_amount         numeric(18, 2)  DEFAULT 0,
    narration             text            COLLATE pg_catalog."default",
    cost_center_id        integer,
    reference_type        character varying(50) COLLATE pg_catalog."default",
    reference_id          bigint,
    reference_no          character varying(100) COLLATE pg_catalog."default",
    CONSTRAINT trn_journal_voucher_line_pkey PRIMARY KEY (journal_line_id)
);

COMMENT ON TABLE press_db.trn_journal_voucher_line
    IS 'Journal voucher debit/credit lines. Each line posts to one account head. Sum of debits must equal sum of credits within a journal.';


-- ============================================================================
-- 7. BANK RECEIPT (Cash/Cheque/NEFT received via bank)
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_bank_receipt
(
    bank_receipt_id       bigserial       NOT NULL,
    receipt_no            character varying(50)  COLLATE pg_catalog."default" NOT NULL,
    receipt_date          date            NOT NULL DEFAULT CURRENT_DATE,
    company_id            integer         NOT NULL,
    location_id           integer,
    fin_year_id           integer,
    bank_account_id       integer         NOT NULL,
    party_id              integer,
    received_from         character varying(200) COLLATE pg_catalog."default",
    payment_mode          character varying(30) COLLATE pg_catalog."default" NOT NULL,
    cheque_no             character varying(30) COLLATE pg_catalog."default",
    cheque_date           date,
    transaction_ref_no    character varying(100) COLLATE pg_catalog."default",
    amount                numeric(18, 2)  NOT NULL,
    tds_amount            numeric(18, 2)  DEFAULT 0,
    net_amount            numeric(18, 2)  DEFAULT 0,
    narration             text            COLLATE pg_catalog."default",
    account_head_id       bigint,
    is_advance            boolean         DEFAULT false,
    status                character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'POSTED'::character varying,
    is_reconciled         boolean         DEFAULT false,
    reconciled_on         date,
    is_cancelled          boolean         DEFAULT false,
    cancelled_by          bigint,
    cancelled_on          timestamp without time zone,
    cancel_reason         text            COLLATE pg_catalog."default",
    is_posted_to_gl       boolean         DEFAULT false,
    gl_posted_on          timestamp without time zone,
    gl_posted_by          bigint,
    created_by            bigint          NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT trn_bank_receipt_pkey PRIMARY KEY (bank_receipt_id),
    CONSTRAINT uq_bank_receipt_no UNIQUE (receipt_no)
);

COMMENT ON TABLE press_db.trn_bank_receipt
    IS 'Bank receipt voucher for money received into company bank account. Supports cheque, NEFT, RTGS, UPI. Links to party and optional AR allocation.';

CREATE TABLE IF NOT EXISTS press_db.trn_bank_receipt_allocation
(
    allocation_id         bigserial       NOT NULL,
    bank_receipt_id       bigint          NOT NULL,
    allocation_against    character varying(30) COLLATE pg_catalog."default" NOT NULL,
    ref_id                bigint          NOT NULL,
    ref_no                character varying(50) COLLATE pg_catalog."default",
    ref_date              date,
    allocated_amount      numeric(18, 2)  NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT trn_bank_receipt_allocation_pkey PRIMARY KEY (allocation_id)
);

COMMENT ON TABLE press_db.trn_bank_receipt_allocation
    IS 'Allocation of bank receipt against sales invoices or advance adjustments.';

COMMENT ON COLUMN press_db.trn_bank_receipt_allocation.allocation_against
    IS 'SALES_INVOICE, CREDIT_NOTE, ADVANCE, OTHER';


-- ============================================================================
-- 8. BANK PAYMENT (Payment made from bank)
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_bank_payment
(
    bank_payment_id       bigserial       NOT NULL,
    payment_no            character varying(50)  COLLATE pg_catalog."default" NOT NULL,
    payment_date          date            NOT NULL DEFAULT CURRENT_DATE,
    company_id            integer         NOT NULL,
    location_id           integer,
    fin_year_id           integer,
    bank_account_id       integer         NOT NULL,
    party_id              integer,
    paid_to               character varying(200) COLLATE pg_catalog."default",
    payment_mode          character varying(30) COLLATE pg_catalog."default" NOT NULL,
    cheque_no             character varying(30) COLLATE pg_catalog."default",
    cheque_date           date,
    transaction_ref_no    character varying(100) COLLATE pg_catalog."default",
    amount                numeric(18, 2)  NOT NULL,
    tds_amount            numeric(18, 2)  DEFAULT 0,
    net_amount            numeric(18, 2)  DEFAULT 0,
    narration             text            COLLATE pg_catalog."default",
    account_head_id       bigint,
    is_advance            boolean         DEFAULT false,
    status                character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'POSTED'::character varying,
    is_reconciled         boolean         DEFAULT false,
    reconciled_on         date,
    is_cancelled          boolean         DEFAULT false,
    cancelled_by          bigint,
    cancelled_on          timestamp without time zone,
    cancel_reason         text            COLLATE pg_catalog."default",
    is_posted_to_gl       boolean         DEFAULT false,
    gl_posted_on          timestamp without time zone,
    gl_posted_by          bigint,
    created_by            bigint          NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT trn_bank_payment_pkey PRIMARY KEY (bank_payment_id),
    CONSTRAINT uq_bank_payment_no UNIQUE (payment_no)
);

COMMENT ON TABLE press_db.trn_bank_payment
    IS 'Bank payment voucher for money paid from company bank account to supplier/vendor. Supports cheque, NEFT, RTGS, UPI.';

CREATE TABLE IF NOT EXISTS press_db.trn_bank_payment_allocation
(
    allocation_id         bigserial       NOT NULL,
    bank_payment_id       bigint          NOT NULL,
    allocation_against    character varying(30) COLLATE pg_catalog."default" NOT NULL,
    ref_id                bigint          NOT NULL,
    ref_no                character varying(50) COLLATE pg_catalog."default",
    ref_date              date,
    allocated_amount      numeric(18, 2)  NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT trn_bank_payment_allocation_pkey PRIMARY KEY (allocation_id)
);

COMMENT ON TABLE press_db.trn_bank_payment_allocation
    IS 'Allocation of bank payment against purchase invoices or advance adjustments.';

COMMENT ON COLUMN press_db.trn_bank_payment_allocation.allocation_against
    IS 'PURCHASE_INVOICE, DEBIT_NOTE, ADVANCE, EXPENSE, OTHER';


-- ============================================================================
-- 9. CONTRA VOUCHER (Fund transfer between Cash ↔ Bank or Bank ↔ Bank)
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_contra_voucher
(
    contra_id             bigserial       NOT NULL,
    contra_no             character varying(50)  COLLATE pg_catalog."default" NOT NULL,
    contra_date           date            NOT NULL DEFAULT CURRENT_DATE,
    company_id            integer         NOT NULL,
    location_id           integer,
    fin_year_id           integer,
    transfer_from_type    character varying(10)  COLLATE pg_catalog."default" NOT NULL,
    transfer_from_id      integer         NOT NULL,
    transfer_to_type      character varying(10)  COLLATE pg_catalog."default" NOT NULL,
    transfer_to_id        integer         NOT NULL,
    amount                numeric(18, 2)  NOT NULL,
    reference_no          character varying(100) COLLATE pg_catalog."default",
    narration             text            COLLATE pg_catalog."default",
    status                character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'POSTED'::character varying,
    is_cancelled          boolean         DEFAULT false,
    cancelled_by          bigint,
    cancelled_on          timestamp without time zone,
    cancel_reason         text            COLLATE pg_catalog."default",
    is_posted_to_gl       boolean         DEFAULT false,
    gl_posted_on          timestamp without time zone,
    gl_posted_by          bigint,
    created_by            bigint          NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT trn_contra_voucher_pkey PRIMARY KEY (contra_id),
    CONSTRAINT uq_contra_no UNIQUE (contra_no)
);

COMMENT ON TABLE press_db.trn_contra_voucher
    IS 'Contra voucher for fund transfers: Cash→Bank, Bank→Cash, Bank→Bank. No party involved.';

COMMENT ON COLUMN press_db.trn_contra_voucher.transfer_from_type
    IS 'CASH or BANK';

COMMENT ON COLUMN press_db.trn_contra_voucher.transfer_to_type
    IS 'CASH or BANK';

COMMENT ON COLUMN press_db.trn_contra_voucher.transfer_from_id
    IS 'FK to mst_bank_account.bank_account_id (for BANK) or 0 (for CASH)';

COMMENT ON COLUMN press_db.trn_contra_voucher.transfer_to_id
    IS 'FK to mst_bank_account.bank_account_id (for BANK) or 0 (for CASH)';


-- ============================================================================
-- 10. BANK RECONCILIATION
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_bank_reconciliation
(
    reconciliation_id     bigserial       NOT NULL,
    reconciliation_no     character varying(50)  COLLATE pg_catalog."default" NOT NULL,
    company_id            integer         NOT NULL,
    bank_account_id       integer         NOT NULL,
    fin_year_id           integer,
    statement_date        date            NOT NULL,
    statement_balance     numeric(18, 2)  NOT NULL DEFAULT 0,
    book_balance          numeric(18, 2)  DEFAULT 0,
    reconciled_balance    numeric(18, 2)  DEFAULT 0,
    difference_amount     numeric(18, 2)  DEFAULT 0,
    total_items           integer         DEFAULT 0,
    reconciled_items      integer         DEFAULT 0,
    pending_items         integer         DEFAULT 0,
    status                character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'IN_PROGRESS'::character varying,
    completed_by          bigint,
    completed_on          timestamp without time zone,
    remarks               text            COLLATE pg_catalog."default",
    created_by            bigint          NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT trn_bank_reconciliation_pkey PRIMARY KEY (reconciliation_id),
    CONSTRAINT uq_bank_reconciliation_no UNIQUE (reconciliation_no)
);

COMMENT ON TABLE press_db.trn_bank_reconciliation
    IS 'Bank reconciliation header. Matches book entries with bank statement for a given bank account and statement date.';

CREATE TABLE IF NOT EXISTS press_db.trn_bank_reconciliation_item
(
    recon_item_id         bigserial       NOT NULL,
    reconciliation_id     bigint          NOT NULL,
    voucher_type          character varying(50) COLLATE pg_catalog."default" NOT NULL,
    voucher_id            bigint          NOT NULL,
    voucher_no            character varying(50) COLLATE pg_catalog."default",
    voucher_date          date,
    cheque_no             character varying(30) COLLATE pg_catalog."default",
    debit_amount          numeric(18, 2)  DEFAULT 0,
    credit_amount         numeric(18, 2)  DEFAULT 0,
    bank_date             date,
    is_reconciled         boolean         DEFAULT false,
    reconciled_on         timestamp without time zone,
    remarks               text            COLLATE pg_catalog."default",
    CONSTRAINT trn_bank_reconciliation_item_pkey PRIMARY KEY (recon_item_id)
);

COMMENT ON TABLE press_db.trn_bank_reconciliation_item
    IS 'Bank reconciliation line items. Each row maps a book voucher entry to its bank statement clearance date.';


-- ============================================================================
-- 11. TAX LEDGER (GST Input/Output tracking for returns)
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_tax_ledger
(
    tax_ledger_id         bigserial       NOT NULL,
    company_id            integer         NOT NULL,
    fin_year_id           integer,
    tax_period            character varying(10) COLLATE pg_catalog."default",
    posting_date          date            NOT NULL,
    transaction_type_id   integer,
    direction_id          integer         NOT NULL,
    voucher_type          character varying(50) COLLATE pg_catalog."default" NOT NULL,
    voucher_id            bigint          NOT NULL,
    voucher_no            character varying(50) COLLATE pg_catalog."default",
    voucher_date          date,
    party_id              integer,
    party_gstin           character varying(20) COLLATE pg_catalog."default",
    place_of_supply       character varying(150) COLLATE pg_catalog."default",
    hsn_sac_code          character varying(20) COLLATE pg_catalog."default",
    taxable_value         numeric(18, 2)  DEFAULT 0,
    tax_component_id      integer         NOT NULL,
    tax_rate              numeric(8, 4)   DEFAULT 0,
    tax_amount            numeric(18, 2)  DEFAULT 0,
    is_reverse_charge     boolean         DEFAULT false,
    is_nil_rated          boolean         DEFAULT false,
    is_exempt             boolean         DEFAULT false,
    itc_eligible          boolean         DEFAULT true,
    itc_category          character varying(30) COLLATE pg_catalog."default",
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT trn_tax_ledger_pkey PRIMARY KEY (tax_ledger_id)
);

COMMENT ON TABLE press_db.trn_tax_ledger
    IS 'Tax ledger for GST compliance. One row per tax component per voucher line. Powers GSTR-1, GSTR-2B, GSTR-3B, and ITC reports. direction_id 1=Output (payable), 2=Input (ITC).';

COMMENT ON COLUMN press_db.trn_tax_ledger.tax_period
    IS 'GST return period in MMYYYY format e.g. 072025 for July 2025.';

COMMENT ON COLUMN press_db.trn_tax_ledger.itc_category
    IS 'ITC category: INPUTS, CAPITAL_GOODS, INPUT_SERVICES, INELIGIBLE';


-- ============================================================================
-- 12. ACCOUNTS RECEIVABLE OUTSTANDING
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_ar_outstanding
(
    ar_id                 bigserial       NOT NULL,
    company_id            integer         NOT NULL,
    party_id              integer         NOT NULL,
    fin_year_id           integer,
    document_type         character varying(30) COLLATE pg_catalog."default" NOT NULL,
    document_id           bigint          NOT NULL,
    document_no           character varying(50) COLLATE pg_catalog."default" NOT NULL,
    document_date         date            NOT NULL,
    due_date              date,
    currency_id           integer,
    original_amount       numeric(18, 2)  NOT NULL DEFAULT 0,
    paid_amount           numeric(18, 2)  DEFAULT 0,
    adjusted_amount       numeric(18, 2)  DEFAULT 0,
    write_off_amount      numeric(18, 2)  DEFAULT 0,
    outstanding_amount    numeric(18, 2)  GENERATED ALWAYS AS ((original_amount - paid_amount - adjusted_amount - write_off_amount)) STORED,
    overdue_days          integer         DEFAULT 0,
    aging_bucket          character varying(20) COLLATE pg_catalog."default",
    is_fully_settled      boolean         DEFAULT false,
    last_payment_date     date,
    last_reminder_date    date,
    reminder_count        integer         DEFAULT 0,
    status                character varying(30) COLLATE pg_catalog."default" DEFAULT 'OPEN'::character varying,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_on           timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT trn_ar_outstanding_pkey PRIMARY KEY (ar_id)
);

COMMENT ON TABLE press_db.trn_ar_outstanding
    IS 'Accounts Receivable outstanding tracker. One row per sales invoice/credit note. Updated on receipt/allocation. Powers AR aging report, customer statement, collection follow-up.';

COMMENT ON COLUMN press_db.trn_ar_outstanding.document_type
    IS 'SALES_INVOICE, CREDIT_NOTE, DEBIT_NOTE';

COMMENT ON COLUMN press_db.trn_ar_outstanding.aging_bucket
    IS 'CURRENT, 1-30, 31-60, 61-90, 91-120, 120+';


-- ============================================================================
-- 13. ACCOUNTS PAYABLE OUTSTANDING
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_ap_outstanding
(
    ap_id                 bigserial       NOT NULL,
    company_id            integer         NOT NULL,
    party_id              integer         NOT NULL,
    supplier_id           integer,
    fin_year_id           integer,
    document_type         character varying(30) COLLATE pg_catalog."default" NOT NULL,
    document_id           bigint          NOT NULL,
    document_no           character varying(50) COLLATE pg_catalog."default" NOT NULL,
    document_date         date            NOT NULL,
    due_date              date,
    currency_id           integer,
    original_amount       numeric(18, 2)  NOT NULL DEFAULT 0,
    paid_amount           numeric(18, 2)  DEFAULT 0,
    adjusted_amount       numeric(18, 2)  DEFAULT 0,
    tds_amount            numeric(18, 2)  DEFAULT 0,
    write_off_amount      numeric(18, 2)  DEFAULT 0,
    outstanding_amount    numeric(18, 2)  GENERATED ALWAYS AS ((original_amount - paid_amount - adjusted_amount - tds_amount - write_off_amount)) STORED,
    overdue_days          integer         DEFAULT 0,
    aging_bucket          character varying(20) COLLATE pg_catalog."default",
    is_fully_settled      boolean         DEFAULT false,
    last_payment_date     date,
    status                character varying(30) COLLATE pg_catalog."default" DEFAULT 'OPEN'::character varying,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_on           timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT trn_ap_outstanding_pkey PRIMARY KEY (ap_id)
);

COMMENT ON TABLE press_db.trn_ap_outstanding
    IS 'Accounts Payable outstanding tracker. One row per purchase invoice/debit note. Updated on payment/allocation. Powers AP aging report, vendor statement, payment scheduling.';

COMMENT ON COLUMN press_db.trn_ap_outstanding.document_type
    IS 'PURCHASE_INVOICE, DEBIT_NOTE, CREDIT_NOTE';

COMMENT ON COLUMN press_db.trn_ap_outstanding.aging_bucket
    IS 'CURRENT, 1-30, 31-60, 61-90, 91-120, 120+';


-- ============================================================================
-- 14. PURCHASE ORDER (AP flow — before Purchase Invoice)
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_purchase_order
(
    purchase_order_id     bigserial       NOT NULL,
    po_no                 character varying(50)  COLLATE pg_catalog."default" NOT NULL,
    po_date               date            NOT NULL DEFAULT CURRENT_DATE,
    company_id            integer         NOT NULL,
    location_id           integer,
    fin_year_id           integer,
    party_id              integer         NOT NULL,
    supplier_id           integer,
    billing_address_id    integer,
    shipping_address_id   integer,
    currency_id           integer,
    exchange_rate         numeric(10, 4)  DEFAULT 1,
    payment_term_id       integer,
    expected_delivery_date date,
    subtotal_amount       numeric(18, 2)  DEFAULT 0,
    discount_amount       numeric(18, 2)  DEFAULT 0,
    taxable_amount        numeric(18, 2)  DEFAULT 0,
    cgst_amount           numeric(18, 2)  DEFAULT 0,
    sgst_amount           numeric(18, 2)  DEFAULT 0,
    igst_amount           numeric(18, 2)  DEFAULT 0,
    cess_amount           numeric(18, 2)  DEFAULT 0,
    total_tax_amount      numeric(18, 2)  DEFAULT 0,
    round_off             numeric(10, 2)  DEFAULT 0,
    grand_total           numeric(18, 2)  DEFAULT 0,
    status                character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'DRAFT'::character varying,
    is_approved           boolean         DEFAULT false,
    approved_by           bigint,
    approved_on           timestamp without time zone,
    is_cancelled          boolean         DEFAULT false,
    cancelled_by          bigint,
    cancelled_on          timestamp without time zone,
    cancel_reason         text            COLLATE pg_catalog."default",
    terms_conditions      text            COLLATE pg_catalog."default",
    internal_notes        text            COLLATE pg_catalog."default",
    attachments_json      jsonb           DEFAULT '[]'::jsonb,
    created_by            bigint          NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT trn_purchase_order_pkey PRIMARY KEY (purchase_order_id),
    CONSTRAINT uq_po_no UNIQUE (po_no)
);

COMMENT ON TABLE press_db.trn_purchase_order
    IS 'Purchase order header. Part of AP flow: PO → GRN → Purchase Invoice → Payment. Supports GST and approval workflow.';

CREATE TABLE IF NOT EXISTS press_db.trn_purchase_order_item
(
    po_item_id            bigserial       NOT NULL,
    purchase_order_id     bigint          NOT NULL,
    item_sequence         integer         NOT NULL DEFAULT 1,
    item_id               bigint,
    description           text            COLLATE pg_catalog."default" NOT NULL,
    hsn_sac_code          character varying(20) COLLATE pg_catalog."default",
    uom_id                integer,
    quantity              numeric(14, 4)  DEFAULT 0,
    received_quantity     numeric(14, 4)  DEFAULT 0,
    pending_quantity      numeric(14, 4)  DEFAULT 0,
    unit_rate             numeric(14, 4)  DEFAULT 0,
    discount_percent      numeric(6, 3)   DEFAULT 0,
    discount_amount       numeric(14, 2)  DEFAULT 0,
    taxable_value         numeric(14, 2)  DEFAULT 0,
    tax_category_id       integer,
    cgst_percent          numeric(6, 3)   DEFAULT 0,
    cgst_amount           numeric(14, 2)  DEFAULT 0,
    sgst_percent          numeric(6, 3)   DEFAULT 0,
    sgst_amount           numeric(14, 2)  DEFAULT 0,
    igst_percent          numeric(6, 3)   DEFAULT 0,
    igst_amount           numeric(14, 2)  DEFAULT 0,
    cess_percent          numeric(6, 3)   DEFAULT 0,
    cess_amount           numeric(14, 2)  DEFAULT 0,
    total_tax_amount      numeric(14, 2)  DEFAULT 0,
    line_total            numeric(14, 2)  DEFAULT 0,
    status                character varying(30) COLLATE pg_catalog."default" DEFAULT 'OPEN'::character varying,
    remarks               text            COLLATE pg_catalog."default",
    CONSTRAINT trn_purchase_order_item_pkey PRIMARY KEY (po_item_id)
);

COMMENT ON TABLE press_db.trn_purchase_order_item
    IS 'Purchase order line items with GST breakup. Tracks received vs pending quantities.';


-- ============================================================================
-- 15. GOODS RECEIPT NOTE (GRN — links PO to Purchase Invoice)
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_goods_receipt
(
    grn_id                bigserial       NOT NULL,
    grn_no                character varying(50)  COLLATE pg_catalog."default" NOT NULL,
    grn_date              date            NOT NULL DEFAULT CURRENT_DATE,
    company_id            integer         NOT NULL,
    location_id           integer,
    party_id              integer         NOT NULL,
    supplier_id           integer,
    purchase_order_id     bigint,
    po_no                 character varying(50) COLLATE pg_catalog."default",
    supplier_challan_no   character varying(100) COLLATE pg_catalog."default",
    supplier_challan_date date,
    vehicle_no            character varying(50) COLLATE pg_catalog."default",
    total_quantity        numeric(14, 2)  DEFAULT 0,
    total_accepted_qty    numeric(14, 2)  DEFAULT 0,
    total_rejected_qty    numeric(14, 2)  DEFAULT 0,
    status                character varying(30) COLLATE pg_catalog."default" NOT NULL DEFAULT 'DRAFT'::character varying,
    is_quality_checked    boolean         DEFAULT false,
    quality_checked_by    bigint,
    quality_checked_on    timestamp without time zone,
    remarks               text            COLLATE pg_catalog."default",
    created_by            bigint          NOT NULL,
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT trn_goods_receipt_pkey PRIMARY KEY (grn_id),
    CONSTRAINT uq_grn_no UNIQUE (grn_no)
);

COMMENT ON TABLE press_db.trn_goods_receipt
    IS 'Goods Receipt Note (GRN) for material received from suppliers. Links PO to purchase invoice. Supports quality check.';

CREATE TABLE IF NOT EXISTS press_db.trn_goods_receipt_item
(
    grn_item_id           bigserial       NOT NULL,
    grn_id                bigint          NOT NULL,
    po_item_id            bigint,
    item_sequence         integer         NOT NULL DEFAULT 1,
    item_id               bigint,
    description           text            COLLATE pg_catalog."default" NOT NULL,
    uom_id                integer,
    ordered_quantity      numeric(14, 4)  DEFAULT 0,
    received_quantity     numeric(14, 4)  DEFAULT 0,
    accepted_quantity     numeric(14, 4)  DEFAULT 0,
    rejected_quantity     numeric(14, 4)  DEFAULT 0,
    unit_rate             numeric(14, 4)  DEFAULT 0,
    amount                numeric(14, 2)  DEFAULT 0,
    batch_no              character varying(50) COLLATE pg_catalog."default",
    expiry_date           date,
    quality_status        character varying(30) COLLATE pg_catalog."default" DEFAULT 'PENDING'::character varying,
    rejection_reason      text            COLLATE pg_catalog."default",
    remarks               text            COLLATE pg_catalog."default",
    CONSTRAINT trn_goods_receipt_item_pkey PRIMARY KEY (grn_item_id)
);

COMMENT ON TABLE press_db.trn_goods_receipt_item
    IS 'GRN line items with accepted/rejected quantities and quality check status.';


-- ============================================================================
-- 16. TDS LEDGER (Tax Deducted at Source tracking)
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.trn_tds_ledger
(
    tds_id                bigserial       NOT NULL,
    company_id            integer         NOT NULL,
    fin_year_id           integer,
    party_id              integer         NOT NULL,
    tds_section           character varying(20) COLLATE pg_catalog."default" NOT NULL,
    tds_rate              numeric(6, 3)   NOT NULL DEFAULT 0,
    voucher_type          character varying(50) COLLATE pg_catalog."default" NOT NULL,
    voucher_id            bigint          NOT NULL,
    voucher_no            character varying(50) COLLATE pg_catalog."default",
    voucher_date          date            NOT NULL,
    base_amount           numeric(18, 2)  NOT NULL DEFAULT 0,
    tds_amount            numeric(18, 2)  NOT NULL DEFAULT 0,
    surcharge_amount      numeric(18, 2)  DEFAULT 0,
    education_cess        numeric(18, 2)  DEFAULT 0,
    total_tds_amount      numeric(18, 2)  DEFAULT 0,
    is_deposited          boolean         DEFAULT false,
    deposit_challan_no    character varying(50) COLLATE pg_catalog."default",
    deposit_date          date,
    bsr_code              character varying(20) COLLATE pg_catalog."default",
    certificate_no        character varying(50) COLLATE pg_catalog."default",
    is_return_filed       boolean         DEFAULT false,
    quarter               character varying(10) COLLATE pg_catalog."default",
    narration             text            COLLATE pg_catalog."default",
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT trn_tds_ledger_pkey PRIMARY KEY (tds_id)
);

COMMENT ON TABLE press_db.trn_tds_ledger
    IS 'TDS (Tax Deducted at Source) ledger. Tracks TDS deducted on payments to suppliers/vendors. Powers TDS return filing (26Q/27Q), certificate generation.';

COMMENT ON COLUMN press_db.trn_tds_ledger.tds_section
    IS 'TDS section: 194C (Contractor), 194J (Professional), 194I (Rent), 194H (Commission), 194A (Interest), etc.';

COMMENT ON COLUMN press_db.trn_tds_ledger.quarter
    IS 'TDS quarter: Q1 (Apr-Jun), Q2 (Jul-Sep), Q3 (Oct-Dec), Q4 (Jan-Mar)';


-- ============================================================================
-- 17. EXPENSE CATEGORY MASTER
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.mst_expense_category
(
    expense_category_id   serial          NOT NULL,
    category_code         character varying(30) COLLATE pg_catalog."default" NOT NULL,
    category_name         character varying(150) COLLATE pg_catalog."default" NOT NULL,
    parent_category_id    integer,
    account_head_id       bigint,
    description           text            COLLATE pg_catalog."default",
    is_reimbursable       boolean         DEFAULT false,
    requires_approval     boolean         DEFAULT true,
    approval_limit        numeric(18, 2)  DEFAULT 0,
    tax_category_id       integer,
    is_active             boolean         DEFAULT true,
    created_by            character varying(100) COLLATE pg_catalog."default",
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT mst_expense_category_pkey PRIMARY KEY (expense_category_id),
    CONSTRAINT uq_expense_category_code UNIQUE (category_code)
);

COMMENT ON TABLE press_db.mst_expense_category
    IS 'Master table for expense categories: Office, Travel, Utilities, Repairs, Rent, Salary, Transport, Printing, Misc. Maps to account head for GL posting.';


-- ============================================================================
-- 18. COST CENTER MASTER
-- ============================================================================

CREATE TABLE IF NOT EXISTS press_db.mst_cost_center
(
    cost_center_id        serial          NOT NULL,
    center_code           character varying(30) COLLATE pg_catalog."default" NOT NULL,
    center_name           character varying(150) COLLATE pg_catalog."default" NOT NULL,
    parent_center_id      integer,
    department_id         bigint,
    description           text            COLLATE pg_catalog."default",
    is_active             boolean         DEFAULT true,
    created_by            character varying(100) COLLATE pg_catalog."default",
    created_on            timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_by           character varying(100) COLLATE pg_catalog."default",
    modified_on           timestamp without time zone,
    CONSTRAINT mst_cost_center_pkey PRIMARY KEY (cost_center_id),
    CONSTRAINT uq_cost_center_code UNIQUE (center_code)
);

COMMENT ON TABLE press_db.mst_cost_center
    IS 'Cost center master for departmental/project-wise expense tracking. Referenced by journal lines, expense items, invoice items.';


-- ============================================================================
-- FOREIGN KEY CONSTRAINTS
-- ============================================================================

-- Purchase Invoice
ALTER TABLE IF EXISTS press_db.trn_purchase_invoice
    ADD CONSTRAINT fk_pi_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_purchase_invoice
    ADD CONSTRAINT fk_pi_party FOREIGN KEY (party_id)
    REFERENCES press_db.mst_party (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_purchase_invoice
    ADD CONSTRAINT fk_pi_fin_year FOREIGN KEY (fin_year_id)
    REFERENCES press_db.mst_financial_year (fin_year_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_purchase_invoice
    ADD CONSTRAINT fk_pi_payment_term FOREIGN KEY (payment_term_id)
    REFERENCES press_db.mst_payment_term (payment_term_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_purchase_invoice_item
    ADD CONSTRAINT fk_pi_item_header FOREIGN KEY (purchase_invoice_id)
    REFERENCES press_db.trn_purchase_invoice (purchase_invoice_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE CASCADE;

ALTER TABLE IF EXISTS press_db.trn_purchase_invoice_item
    ADD CONSTRAINT fk_pi_item_account FOREIGN KEY (account_head_id)
    REFERENCES press_db.mst_account_head (account_head_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

-- Proforma Invoice
ALTER TABLE IF EXISTS press_db.trn_proforma_invoice
    ADD CONSTRAINT fk_prof_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_proforma_invoice
    ADD CONSTRAINT fk_prof_party FOREIGN KEY (party_id)
    REFERENCES press_db.mst_party (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_proforma_invoice
    ADD CONSTRAINT fk_prof_sales_invoice FOREIGN KEY (sales_invoice_id)
    REFERENCES press_db.trn_sales_invoice (sales_invoice_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_proforma_invoice_item
    ADD CONSTRAINT fk_prof_item_header FOREIGN KEY (proforma_invoice_id)
    REFERENCES press_db.trn_proforma_invoice (proforma_invoice_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE CASCADE;

-- Credit Note
ALTER TABLE IF EXISTS press_db.trn_credit_note
    ADD CONSTRAINT fk_cn_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_credit_note
    ADD CONSTRAINT fk_cn_party FOREIGN KEY (party_id)
    REFERENCES press_db.mst_party (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_credit_note
    ADD CONSTRAINT fk_cn_original_invoice FOREIGN KEY (original_invoice_id)
    REFERENCES press_db.trn_sales_invoice (sales_invoice_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_credit_note_item
    ADD CONSTRAINT fk_cn_item_header FOREIGN KEY (credit_note_id)
    REFERENCES press_db.trn_credit_note (credit_note_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE CASCADE;

-- Debit Note
ALTER TABLE IF EXISTS press_db.trn_debit_note
    ADD CONSTRAINT fk_dn_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_debit_note
    ADD CONSTRAINT fk_dn_party FOREIGN KEY (party_id)
    REFERENCES press_db.mst_party (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_debit_note
    ADD CONSTRAINT fk_dn_original_invoice FOREIGN KEY (original_invoice_id)
    REFERENCES press_db.trn_purchase_invoice (purchase_invoice_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_debit_note_item
    ADD CONSTRAINT fk_dn_item_header FOREIGN KEY (debit_note_id)
    REFERENCES press_db.trn_debit_note (debit_note_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE CASCADE;

-- Expense Voucher
ALTER TABLE IF EXISTS press_db.trn_expense_voucher
    ADD CONSTRAINT fk_ev_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_expense_voucher
    ADD CONSTRAINT fk_ev_bank FOREIGN KEY (bank_account_id)
    REFERENCES press_db.mst_bank_account (bank_account_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_expense_voucher
    ADD CONSTRAINT fk_ev_employee FOREIGN KEY (employee_id)
    REFERENCES press_db.mst_employee (employee_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_expense_voucher_item
    ADD CONSTRAINT fk_ev_item_header FOREIGN KEY (expense_voucher_id)
    REFERENCES press_db.trn_expense_voucher (expense_voucher_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE CASCADE;

ALTER TABLE IF EXISTS press_db.trn_expense_voucher_item
    ADD CONSTRAINT fk_ev_item_account FOREIGN KEY (account_head_id)
    REFERENCES press_db.mst_account_head (account_head_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

-- Journal Voucher
ALTER TABLE IF EXISTS press_db.trn_journal_voucher
    ADD CONSTRAINT fk_jv_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_journal_voucher
    ADD CONSTRAINT fk_jv_original FOREIGN KEY (original_journal_id)
    REFERENCES press_db.trn_journal_voucher (journal_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_journal_voucher_line
    ADD CONSTRAINT fk_jvl_header FOREIGN KEY (journal_id)
    REFERENCES press_db.trn_journal_voucher (journal_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE CASCADE;

ALTER TABLE IF EXISTS press_db.trn_journal_voucher_line
    ADD CONSTRAINT fk_jvl_account FOREIGN KEY (account_head_id)
    REFERENCES press_db.mst_account_head (account_head_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

-- Bank Receipt
ALTER TABLE IF EXISTS press_db.trn_bank_receipt
    ADD CONSTRAINT fk_br_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_bank_receipt
    ADD CONSTRAINT fk_br_bank FOREIGN KEY (bank_account_id)
    REFERENCES press_db.mst_bank_account (bank_account_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_bank_receipt_allocation
    ADD CONSTRAINT fk_bra_receipt FOREIGN KEY (bank_receipt_id)
    REFERENCES press_db.trn_bank_receipt (bank_receipt_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE CASCADE;

-- Bank Payment
ALTER TABLE IF EXISTS press_db.trn_bank_payment
    ADD CONSTRAINT fk_bp_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_bank_payment
    ADD CONSTRAINT fk_bp_bank FOREIGN KEY (bank_account_id)
    REFERENCES press_db.mst_bank_account (bank_account_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_bank_payment_allocation
    ADD CONSTRAINT fk_bpa_payment FOREIGN KEY (bank_payment_id)
    REFERENCES press_db.trn_bank_payment (bank_payment_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE CASCADE;

-- Contra Voucher
ALTER TABLE IF EXISTS press_db.trn_contra_voucher
    ADD CONSTRAINT fk_contra_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

-- Bank Reconciliation
ALTER TABLE IF EXISTS press_db.trn_bank_reconciliation
    ADD CONSTRAINT fk_recon_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_bank_reconciliation
    ADD CONSTRAINT fk_recon_bank FOREIGN KEY (bank_account_id)
    REFERENCES press_db.mst_bank_account (bank_account_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_bank_reconciliation_item
    ADD CONSTRAINT fk_recon_item_header FOREIGN KEY (reconciliation_id)
    REFERENCES press_db.trn_bank_reconciliation (reconciliation_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE CASCADE;

-- Tax Ledger
ALTER TABLE IF EXISTS press_db.trn_tax_ledger
    ADD CONSTRAINT fk_tl_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_tax_ledger
    ADD CONSTRAINT fk_tl_direction FOREIGN KEY (direction_id)
    REFERENCES press_db.mst_direction (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_tax_ledger
    ADD CONSTRAINT fk_tl_transaction_type FOREIGN KEY (transaction_type_id)
    REFERENCES press_db.mst_transaction_type (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_tax_ledger
    ADD CONSTRAINT fk_tl_tax_component FOREIGN KEY (tax_component_id)
    REFERENCES press_db.mst_tax_component (tax_component_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

-- AR Outstanding
ALTER TABLE IF EXISTS press_db.trn_ar_outstanding
    ADD CONSTRAINT fk_ar_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_ar_outstanding
    ADD CONSTRAINT fk_ar_party FOREIGN KEY (party_id)
    REFERENCES press_db.mst_party (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

-- AP Outstanding
ALTER TABLE IF EXISTS press_db.trn_ap_outstanding
    ADD CONSTRAINT fk_ap_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_ap_outstanding
    ADD CONSTRAINT fk_ap_party FOREIGN KEY (party_id)
    REFERENCES press_db.mst_party (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

-- Purchase Order
ALTER TABLE IF EXISTS press_db.trn_purchase_order
    ADD CONSTRAINT fk_po_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_purchase_order
    ADD CONSTRAINT fk_po_party FOREIGN KEY (party_id)
    REFERENCES press_db.mst_party (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_purchase_order_item
    ADD CONSTRAINT fk_poi_header FOREIGN KEY (purchase_order_id)
    REFERENCES press_db.trn_purchase_order (purchase_order_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE CASCADE;

-- Goods Receipt
ALTER TABLE IF EXISTS press_db.trn_goods_receipt
    ADD CONSTRAINT fk_grn_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_goods_receipt
    ADD CONSTRAINT fk_grn_party FOREIGN KEY (party_id)
    REFERENCES press_db.mst_party (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_goods_receipt
    ADD CONSTRAINT fk_grn_po FOREIGN KEY (purchase_order_id)
    REFERENCES press_db.trn_purchase_order (purchase_order_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_goods_receipt_item
    ADD CONSTRAINT fk_grni_header FOREIGN KEY (grn_id)
    REFERENCES press_db.trn_goods_receipt (grn_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE CASCADE;

-- TDS Ledger
ALTER TABLE IF EXISTS press_db.trn_tds_ledger
    ADD CONSTRAINT fk_tds_company FOREIGN KEY (company_id)
    REFERENCES press_db.mst_company (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.trn_tds_ledger
    ADD CONSTRAINT fk_tds_party FOREIGN KEY (party_id)
    REFERENCES press_db.mst_party (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

-- Expense Category
ALTER TABLE IF EXISTS press_db.mst_expense_category
    ADD CONSTRAINT fk_expcat_parent FOREIGN KEY (parent_category_id)
    REFERENCES press_db.mst_expense_category (expense_category_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.mst_expense_category
    ADD CONSTRAINT fk_expcat_account FOREIGN KEY (account_head_id)
    REFERENCES press_db.mst_account_head (account_head_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

-- Cost Center
ALTER TABLE IF EXISTS press_db.mst_cost_center
    ADD CONSTRAINT fk_cc_parent FOREIGN KEY (parent_center_id)
    REFERENCES press_db.mst_cost_center (cost_center_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE IF EXISTS press_db.mst_cost_center
    ADD CONSTRAINT fk_cc_dept FOREIGN KEY (department_id)
    REFERENCES press_db.mst_department (dept_id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;


-- ============================================================================
-- INDEXES
-- ============================================================================

-- Purchase Invoice
CREATE INDEX IF NOT EXISTS idx_pi_party ON press_db.trn_purchase_invoice(party_id);
CREATE INDEX IF NOT EXISTS idx_pi_date ON press_db.trn_purchase_invoice(invoice_date);
CREATE INDEX IF NOT EXISTS idx_pi_status ON press_db.trn_purchase_invoice(status);
CREATE INDEX IF NOT EXISTS idx_pi_company ON press_db.trn_purchase_invoice(company_id);
CREATE INDEX IF NOT EXISTS idx_pi_item_header ON press_db.trn_purchase_invoice_item(purchase_invoice_id);

-- Proforma Invoice
CREATE INDEX IF NOT EXISTS idx_prof_party ON press_db.trn_proforma_invoice(party_id);
CREATE INDEX IF NOT EXISTS idx_prof_date ON press_db.trn_proforma_invoice(proforma_date);
CREATE INDEX IF NOT EXISTS idx_prof_status ON press_db.trn_proforma_invoice(status);
CREATE INDEX IF NOT EXISTS idx_prof_item_header ON press_db.trn_proforma_invoice_item(proforma_invoice_id);

-- Credit Note
CREATE INDEX IF NOT EXISTS idx_cn_party ON press_db.trn_credit_note(party_id);
CREATE INDEX IF NOT EXISTS idx_cn_date ON press_db.trn_credit_note(credit_note_date);
CREATE INDEX IF NOT EXISTS idx_cn_status ON press_db.trn_credit_note(status);
CREATE INDEX IF NOT EXISTS idx_cn_original_inv ON press_db.trn_credit_note(original_invoice_id);
CREATE INDEX IF NOT EXISTS idx_cn_item_header ON press_db.trn_credit_note_item(credit_note_id);

-- Debit Note
CREATE INDEX IF NOT EXISTS idx_dn_party ON press_db.trn_debit_note(party_id);
CREATE INDEX IF NOT EXISTS idx_dn_date ON press_db.trn_debit_note(debit_note_date);
CREATE INDEX IF NOT EXISTS idx_dn_status ON press_db.trn_debit_note(status);
CREATE INDEX IF NOT EXISTS idx_dn_original_inv ON press_db.trn_debit_note(original_invoice_id);
CREATE INDEX IF NOT EXISTS idx_dn_item_header ON press_db.trn_debit_note_item(debit_note_id);

-- Expense Voucher
CREATE INDEX IF NOT EXISTS idx_ev_company ON press_db.trn_expense_voucher(company_id);
CREATE INDEX IF NOT EXISTS idx_ev_date ON press_db.trn_expense_voucher(voucher_date);
CREATE INDEX IF NOT EXISTS idx_ev_status ON press_db.trn_expense_voucher(status);
CREATE INDEX IF NOT EXISTS idx_ev_employee ON press_db.trn_expense_voucher(employee_id);
CREATE INDEX IF NOT EXISTS idx_ev_item_header ON press_db.trn_expense_voucher_item(expense_voucher_id);

-- Journal Voucher
CREATE INDEX IF NOT EXISTS idx_jv_company ON press_db.trn_journal_voucher(company_id);
CREATE INDEX IF NOT EXISTS idx_jv_date ON press_db.trn_journal_voucher(journal_date);
CREATE INDEX IF NOT EXISTS idx_jv_status ON press_db.trn_journal_voucher(status);
CREATE INDEX IF NOT EXISTS idx_jvl_header ON press_db.trn_journal_voucher_line(journal_id);
CREATE INDEX IF NOT EXISTS idx_jvl_account ON press_db.trn_journal_voucher_line(account_head_id);

-- Bank Receipt
CREATE INDEX IF NOT EXISTS idx_br_bank ON press_db.trn_bank_receipt(bank_account_id);
CREATE INDEX IF NOT EXISTS idx_br_date ON press_db.trn_bank_receipt(receipt_date);
CREATE INDEX IF NOT EXISTS idx_br_party ON press_db.trn_bank_receipt(party_id);
CREATE INDEX IF NOT EXISTS idx_bra_receipt ON press_db.trn_bank_receipt_allocation(bank_receipt_id);

-- Bank Payment
CREATE INDEX IF NOT EXISTS idx_bp_bank ON press_db.trn_bank_payment(bank_account_id);
CREATE INDEX IF NOT EXISTS idx_bp_date ON press_db.trn_bank_payment(payment_date);
CREATE INDEX IF NOT EXISTS idx_bp_party ON press_db.trn_bank_payment(party_id);
CREATE INDEX IF NOT EXISTS idx_bpa_payment ON press_db.trn_bank_payment_allocation(bank_payment_id);

-- Contra Voucher
CREATE INDEX IF NOT EXISTS idx_contra_company ON press_db.trn_contra_voucher(company_id);
CREATE INDEX IF NOT EXISTS idx_contra_date ON press_db.trn_contra_voucher(contra_date);

-- Bank Reconciliation
CREATE INDEX IF NOT EXISTS idx_recon_bank ON press_db.trn_bank_reconciliation(bank_account_id);
CREATE INDEX IF NOT EXISTS idx_recon_date ON press_db.trn_bank_reconciliation(statement_date);
CREATE INDEX IF NOT EXISTS idx_recon_item_header ON press_db.trn_bank_reconciliation_item(reconciliation_id);

-- Tax Ledger
CREATE INDEX IF NOT EXISTS idx_tl_company_period ON press_db.trn_tax_ledger(company_id, tax_period);
CREATE INDEX IF NOT EXISTS idx_tl_voucher ON press_db.trn_tax_ledger(voucher_type, voucher_id);
CREATE INDEX IF NOT EXISTS idx_tl_party ON press_db.trn_tax_ledger(party_id);
CREATE INDEX IF NOT EXISTS idx_tl_direction ON press_db.trn_tax_ledger(direction_id);
CREATE INDEX IF NOT EXISTS idx_tl_hsn ON press_db.trn_tax_ledger(hsn_sac_code);

-- AR Outstanding
CREATE INDEX IF NOT EXISTS idx_ar_party ON press_db.trn_ar_outstanding(party_id);
CREATE INDEX IF NOT EXISTS idx_ar_status ON press_db.trn_ar_outstanding(status);
CREATE INDEX IF NOT EXISTS idx_ar_due_date ON press_db.trn_ar_outstanding(due_date);
CREATE INDEX IF NOT EXISTS idx_ar_company ON press_db.trn_ar_outstanding(company_id);

-- AP Outstanding
CREATE INDEX IF NOT EXISTS idx_ap_party ON press_db.trn_ap_outstanding(party_id);
CREATE INDEX IF NOT EXISTS idx_ap_status ON press_db.trn_ap_outstanding(status);
CREATE INDEX IF NOT EXISTS idx_ap_due_date ON press_db.trn_ap_outstanding(due_date);
CREATE INDEX IF NOT EXISTS idx_ap_company ON press_db.trn_ap_outstanding(company_id);

-- Purchase Order
CREATE INDEX IF NOT EXISTS idx_po_party ON press_db.trn_purchase_order(party_id);
CREATE INDEX IF NOT EXISTS idx_po_date ON press_db.trn_purchase_order(po_date);
CREATE INDEX IF NOT EXISTS idx_po_status ON press_db.trn_purchase_order(status);
CREATE INDEX IF NOT EXISTS idx_poi_header ON press_db.trn_purchase_order_item(purchase_order_id);

-- Goods Receipt
CREATE INDEX IF NOT EXISTS idx_grn_party ON press_db.trn_goods_receipt(party_id);
CREATE INDEX IF NOT EXISTS idx_grn_date ON press_db.trn_goods_receipt(grn_date);
CREATE INDEX IF NOT EXISTS idx_grn_po ON press_db.trn_goods_receipt(purchase_order_id);
CREATE INDEX IF NOT EXISTS idx_grni_header ON press_db.trn_goods_receipt_item(grn_id);

-- TDS Ledger
CREATE INDEX IF NOT EXISTS idx_tds_party ON press_db.trn_tds_ledger(party_id);
CREATE INDEX IF NOT EXISTS idx_tds_company ON press_db.trn_tds_ledger(company_id);
CREATE INDEX IF NOT EXISTS idx_tds_voucher ON press_db.trn_tds_ledger(voucher_type, voucher_id);
CREATE INDEX IF NOT EXISTS idx_tds_section ON press_db.trn_tds_ledger(tds_section);


COMMIT;

-- ============================================================================
-- TABLE SUMMARY
-- ============================================================================
-- MASTER TABLES (2 new):
--   mst_expense_category     — Expense category master with account mapping
--   mst_cost_center          — Cost center for departmental tracking
--
-- TRANSACTION TABLES (30 new):
--   trn_purchase_invoice          — Purchase invoice header (AP)
--   trn_purchase_invoice_item     — Purchase invoice line items
--   trn_proforma_invoice          — Proforma invoice header
--   trn_proforma_invoice_item     — Proforma invoice line items
--   trn_credit_note               — Credit note header (AR adjustment)
--   trn_credit_note_item          — Credit note line items
--   trn_debit_note                — Debit note header (AP adjustment)
--   trn_debit_note_item           — Debit note line items
--   trn_expense_voucher           — Expense voucher header
--   trn_expense_voucher_item      — Expense voucher line items
--   trn_journal_voucher           — Journal voucher header (GL entries)
--   trn_journal_voucher_line      — Journal voucher debit/credit lines
--   trn_bank_receipt              — Bank receipt voucher
--   trn_bank_receipt_allocation   — Bank receipt allocation vs invoices
--   trn_bank_payment              — Bank payment voucher
--   trn_bank_payment_allocation   — Bank payment allocation vs invoices
--   trn_contra_voucher            — Contra voucher (Cash↔Bank transfer)
--   trn_bank_reconciliation       — Bank reconciliation header
--   trn_bank_reconciliation_item  — Bank reconciliation line items
--   trn_tax_ledger                — GST input/output tax ledger
--   trn_ar_outstanding            — Accounts receivable outstanding tracker
--   trn_ap_outstanding            — Accounts payable outstanding tracker
--   trn_purchase_order            — Purchase order header
--   trn_purchase_order_item       — Purchase order line items
--   trn_goods_receipt             — Goods receipt note (GRN) header
--   trn_goods_receipt_item        — GRN line items
--   trn_tds_ledger                — TDS deduction tracking
--
-- EXISTING TABLES (already in schema, NOT recreated):
--   trn_sales_invoice + trn_sales_invoice_item
--   trn_receipt + trn_receipt_allocation
--   trn_payment + trn_payment_allocation
--   trn_ledger, trn_account_ledger, trn_advance_ledger
--   mst_account_head, mst_bank_account, mst_voucher_type
--   mst_financial_year, mst_payment_term
-- ============================================================================
