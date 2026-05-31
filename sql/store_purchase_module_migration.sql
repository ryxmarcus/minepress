-- ============================================================================
-- MinePress ERP — Store & Purchase Module — Database Migration Script
-- Schema: press_db
-- Database: PostgreSQL (minepress_db)
-- ============================================================================

-- ────────────────────────────────────────────────────────────────────────────
-- 1. ALTER existing master tables: add inventory columns
-- ────────────────────────────────────────────────────────────────────────────

-- mst_chemical
ALTER TABLE press_db.mst_chemical
    ADD COLUMN IF NOT EXISTS reorder_level        NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS current_stock         NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS uom                   VARCHAR(20)     DEFAULT 'Ltr',
    ADD COLUMN IF NOT EXISTS min_order_qty         NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS lead_time_days        INT             DEFAULT 0,
    ADD COLUMN IF NOT EXISTS last_purchase_rate    NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS last_purchase_date    DATE;

-- mst_ink
ALTER TABLE press_db.mst_ink
    ADD COLUMN IF NOT EXISTS reorder_level        NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS current_stock         NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS uom                   VARCHAR(20)     DEFAULT 'Kg',
    ADD COLUMN IF NOT EXISTS min_order_qty         NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS lead_time_days        INT             DEFAULT 0,
    ADD COLUMN IF NOT EXISTS last_purchase_rate    NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS last_purchase_date    DATE;

-- mst_paper
ALTER TABLE press_db.mst_paper
    ADD COLUMN IF NOT EXISTS reorder_level        NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS current_stock         NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS uom                   VARCHAR(20)     DEFAULT 'Sheets',
    ADD COLUMN IF NOT EXISTS min_order_qty         NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS last_purchase_rate    NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS last_purchase_date    DATE;

-- mst_plate
ALTER TABLE press_db.mst_plate
    ADD COLUMN IF NOT EXISTS reorder_level        NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS current_stock         NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS uom                   VARCHAR(20)     DEFAULT 'Pcs',
    ADD COLUMN IF NOT EXISTS min_order_qty         NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS last_purchase_rate    NUMERIC(14,2)   DEFAULT 0,
    ADD COLUMN IF NOT EXISTS last_purchase_date    DATE;


-- ────────────────────────────────────────────────────────────────────────────
-- 2. mst_other_items — Master table for other printing press consumables
-- ────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS press_db.mst_other_items (
    item_id             BIGSERIAL       PRIMARY KEY,
    item_code           VARCHAR(30)     NOT NULL UNIQUE,
    item_name           VARCHAR(200)    NOT NULL,
    item_category       VARCHAR(50),            -- CONSUMABLE, PACKING, FINISHING, BINDING, LAMINATION, MISC
    item_type           VARCHAR(50),
    description         TEXT,
    uom                 VARCHAR(20)     DEFAULT 'Pcs',
    rate_per_unit       NUMERIC(14,2)   DEFAULT 0,
    reorder_level       NUMERIC(14,2)   DEFAULT 0,
    current_stock       NUMERIC(14,2)   DEFAULT 0,
    min_order_qty       NUMERIC(14,2)   DEFAULT 0,
    lead_time_days      INT             DEFAULT 0,
    last_purchase_rate  NUMERIC(14,2)   DEFAULT 0,
    last_purchase_date  DATE,
    supplier_name       VARCHAR(200),
    brand               VARCHAR(100),
    hsn_code            VARCHAR(20),
    gst_rate            NUMERIC(5,2)    DEFAULT 18.00,
    is_active           BOOLEAN         DEFAULT TRUE,
    remarks             TEXT,
    created_by          BIGINT,
    created_on          TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    modified_by         VARCHAR(50),
    modified_on         TIMESTAMP WITHOUT TIME ZONE
);

CREATE INDEX IF NOT EXISTS idx_mst_other_items_category ON press_db.mst_other_items(item_category);
CREATE INDEX IF NOT EXISTS idx_mst_other_items_active   ON press_db.mst_other_items(is_active);

-- INSERT: Common printing press consumables
INSERT INTO press_db.mst_other_items (item_code, item_name, item_category, item_type, uom, rate_per_unit, reorder_level, hsn_code, gst_rate, remarks) VALUES
    ('OTH-001', 'Gum Arabic Solution',          'CONSUMABLE',  'Dampening',      'Ltr',    350.00,  10, '3809',    18.00, 'Used in offset dampening system'),
    ('OTH-002', 'Blanket Wash / Roller Wash',    'CONSUMABLE',  'Cleaning',       'Ltr',    280.00,  20, '3814',    18.00, 'For cleaning blankets & rollers'),
    ('OTH-003', 'Fountain Solution Concentrate',  'CONSUMABLE',  'Dampening',      'Ltr',    450.00,  10, '3824',    18.00, 'Dampening fountain additive'),
    ('OTH-004', 'Spray Powder (Anti-set-off)',    'CONSUMABLE',  'Drying Aid',     'Kg',     220.00,  15, '3824',    18.00, 'Prevents ink set-off between sheets'),
    ('OTH-005', 'Isopropyl Alcohol (IPA)',        'CONSUMABLE',  'Dampening',      'Ltr',    180.00,  25, '2905',    18.00, 'Dampening system additive'),
    ('OTH-006', 'UV Varnish',                     'FINISHING',   'Coating',        'Kg',     520.00,   5, '3208',    18.00, 'UV coating for finishing'),
    ('OTH-007', 'Aqueous Varnish',                'FINISHING',   'Coating',        'Kg',     380.00,   5, '3209',    18.00, 'Water-based varnish coating'),
    ('OTH-008', 'Lamination Film — Gloss (BOPP)', 'LAMINATION',  'Thermal Film',   'Mtr',      3.50, 500, '3920',    18.00, 'Gloss BOPP thermal lamination film'),
    ('OTH-009', 'Lamination Film — Matt (BOPP)',   'LAMINATION',  'Thermal Film',   'Mtr',      4.00, 500, '3920',    18.00, 'Matt BOPP thermal lamination film'),
    ('OTH-010', 'Lamination Film — Metallic',      'LAMINATION',  'Thermal Film',   'Mtr',      6.50, 200, '3920',    18.00, 'Metallic thermal lamination film'),
    ('OTH-011', 'Hot Melt Glue Sticks (EVA)',      'BINDING',     'Adhesive',       'Kg',     280.00,  10, '3506',    18.00, 'For perfect binding machines'),
    ('OTH-012', 'PUR Glue',                        'BINDING',     'Adhesive',       'Kg',     650.00,   5, '3506',    18.00, 'Polyurethane reactive adhesive for binding'),
    ('OTH-013', 'Binding Thread (Sewing)',          'BINDING',     'Thread',         'Mtr',      1.20, 1000,'5401',   12.00, 'Thread for section-sewn binding'),
    ('OTH-014', 'Binding Wire / Staple Pin',        'BINDING',     'Wire',           'Kg',     150.00,  20, '7317',    18.00, 'Wire for saddle-stitch binding'),
    ('OTH-015', 'Spiral Coil (Plastic)',            'BINDING',     'Coil',           'Pcs',      5.00, 200, '3926',    18.00, 'Plastic spiral binding coils'),
    ('OTH-016', 'Double Loop Wire-O',               'BINDING',     'Wire',           'Pcs',      6.50, 200, '7317',    18.00, 'Metal wire-O binding loops'),
    ('OTH-017', 'Corrugated Box (Inner)',            'PACKING',     'Box',            'Pcs',     35.00, 100, '4819',    18.00, 'Inner corrugated packing box'),
    ('OTH-018', 'Corrugated Box (Master)',           'PACKING',     'Box',            'Pcs',     65.00,  50, '4819',    18.00, 'Master corrugated carton'),
    ('OTH-019', 'Shrink Wrap Film',                  'PACKING',     'Film',           'Kg',     180.00,  20, '3920',    18.00, 'Shrink wrapping film'),
    ('OTH-020', 'Stretch Wrap Film',                 'PACKING',     'Film',           'Kg',     160.00,  20, '3920',    18.00, 'Pallet stretch wrap'),
    ('OTH-021', 'Brown Tape / BOPP Tape',            'PACKING',     'Tape',           'Pcs',     45.00,  50, '4811',    18.00, 'Packing tape rolls'),
    ('OTH-022', 'Embossing Die',                     'FINISHING',   'Die',            'Pcs',   2500.00,   0, '8442',    18.00, 'Custom embossing/debossing die'),
    ('OTH-023', 'Foil Stamping Roll — Gold',         'FINISHING',   'Foil',           'Mtr',     12.00, 100, '3212',    18.00, 'Gold hot-stamping foil'),
    ('OTH-024', 'Foil Stamping Roll — Silver',       'FINISHING',   'Foil',           'Mtr',     11.00, 100, '3212',    18.00, 'Silver hot-stamping foil'),
    ('OTH-025', 'Die-Cut Forme (Steel Rule)',        'FINISHING',   'Die',            'Pcs',   3500.00,   0, '8208',    18.00, 'Custom die-cutting forme'),
    ('OTH-026', 'Numbering Ink',                     'CONSUMABLE',  'Ink',            'Ltr',    320.00,   5, '3215',    18.00, 'Ink for numbering machines'),
    ('OTH-027', 'Padding Compound / Glue',           'BINDING',     'Adhesive',       'Kg',     200.00,  10, '3506',    18.00, 'Flexible adhesive for padding'),
    ('OTH-028', 'Perforating Rule',                  'FINISHING',   'Rule',           'Mtr',     80.00,  10, '8208',    18.00, 'Perforating steel rule'),
    ('OTH-029', 'Creasing Matrix',                   'FINISHING',   'Matrix',         'Mtr',     55.00,  20, '8442',    18.00, 'Creasing channel matrix'),
    ('OTH-030', 'CTP Developer Solution',            'CONSUMABLE',  'Pre-Press',      'Ltr',    420.00,  10, '3707',    18.00, 'Developer for CTP plate processing'),
    ('OTH-031', 'CTP Finisher / Gum',                'CONSUMABLE',  'Pre-Press',      'Ltr',    380.00,  10, '3707',    18.00, 'Post-exposure plate finisher'),
    ('OTH-032', 'Plate Cleaner',                     'CONSUMABLE',  'Pre-Press',      'Ltr',    260.00,  10, '3402',    18.00, 'Offset plate cleaning solution'),
    ('OTH-033', 'Anti-Skinning Agent',               'CONSUMABLE',  'Ink Additive',   'Ltr',    340.00,   5, '3824',    18.00, 'Prevents ink skinning in fountain'),
    ('OTH-034', 'Ink Drier (Cobalt)',                 'CONSUMABLE',  'Ink Additive',   'Ltr',    400.00,   5, '3824',    18.00, 'Accelerates ink drying'),
    ('OTH-035', 'Tack Reducer / Ink Reducer',         'CONSUMABLE',  'Ink Additive',   'Kg',     300.00,   5, '3824',    18.00, 'Reduces ink tack/viscosity');


-- ────────────────────────────────────────────────────────────────────────────
-- 3. trn_store_issue — Store material issue header
-- ────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS press_db.trn_store_issue (
    issue_id            BIGSERIAL       PRIMARY KEY,
    issue_no            VARCHAR(30)     NOT NULL UNIQUE,
    issue_date          DATE            NOT NULL DEFAULT CURRENT_DATE,
    issue_type          VARCHAR(30)     NOT NULL DEFAULT 'JOB',   -- JOB, TRANSFER, STOCK_ADJUSTMENT
    job_id              BIGINT,                                    -- FK to trn_job when issue_type = JOB
    job_no              VARCHAR(30),
    rate_calc_id        BIGINT,                                    -- FK to hyb_job_rate_calculator (BOM source)
    from_location_id    INT,
    to_location_id      INT,
    company_id          INT             NOT NULL DEFAULT 1,
    total_items         INT             DEFAULT 0,
    total_amount        NUMERIC(14,2)   DEFAULT 0,
    status              VARCHAR(20)     NOT NULL DEFAULT 'DRAFT',  -- DRAFT, ISSUED, PARTIALLY_ISSUED, CANCELLED
    remarks             TEXT,
    approved_by         BIGINT,
    approved_on         TIMESTAMP WITHOUT TIME ZONE,
    created_by          BIGINT          NOT NULL,
    created_on          TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    modified_by         VARCHAR(50),
    modified_on         TIMESTAMP WITHOUT TIME ZONE,
    is_active           BOOLEAN         DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_trn_store_issue_job      ON press_db.trn_store_issue(job_id);
CREATE INDEX IF NOT EXISTS idx_trn_store_issue_type     ON press_db.trn_store_issue(issue_type);
CREATE INDEX IF NOT EXISTS idx_trn_store_issue_status   ON press_db.trn_store_issue(status);
CREATE INDEX IF NOT EXISTS idx_trn_store_issue_date     ON press_db.trn_store_issue(issue_date);


-- ────────────────────────────────────────────────────────────────────────────
-- 4. trn_store_issue_item — Store issue line items
-- ────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS press_db.trn_store_issue_item (
    issue_item_id       BIGSERIAL       PRIMARY KEY,
    issue_id            BIGINT          NOT NULL REFERENCES press_db.trn_store_issue(issue_id) ON DELETE CASCADE,
    item_sequence       INT             NOT NULL DEFAULT 1,
    material_category   VARCHAR(30)     NOT NULL,   -- PAPER, INK, PLATE, CHEMICAL, OTHER
    material_id         BIGINT,                      -- PK of the respective master table
    material_code       VARCHAR(50),
    material_name       VARCHAR(200)    NOT NULL,
    specification       TEXT,
    bom_quantity        NUMERIC(14,3)   DEFAULT 0,   -- Required as per BOM
    issued_quantity     NUMERIC(14,3)   NOT NULL DEFAULT 0,
    uom                 VARCHAR(20)     DEFAULT 'Pcs',
    rate                NUMERIC(14,2)   DEFAULT 0,
    amount              NUMERIC(14,2)   DEFAULT 0,
    available_stock     NUMERIC(14,3)   DEFAULT 0,   -- Snapshot at time of issue
    for_part            VARCHAR(50),                  -- Which product part this is for
    remarks             TEXT,
    is_selected         BOOLEAN         DEFAULT TRUE,
    created_on          TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_trn_store_issue_item_issue ON press_db.trn_store_issue_item(issue_id);
CREATE INDEX IF NOT EXISTS idx_trn_store_issue_item_cat   ON press_db.trn_store_issue_item(material_category);


-- ────────────────────────────────────────────────────────────────────────────
-- 5. trn_store_receive — Store material receive header
-- ────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS press_db.trn_store_receive (
    receive_id          BIGSERIAL       PRIMARY KEY,
    receive_no          VARCHAR(30)     NOT NULL UNIQUE,
    receive_date        DATE            NOT NULL DEFAULT CURRENT_DATE,
    receive_type        VARCHAR(30)     NOT NULL DEFAULT 'PURCHASE', -- PURCHASE, RETURN, STOCK_ADJUSTMENT
    grn_id              BIGINT,                                       -- FK to trn_purchase_grn when receive_type = PURCHASE
    grn_no              VARCHAR(30),
    job_id              BIGINT,
    job_no              VARCHAR(30),
    supplier_id         INT,
    supplier_name       VARCHAR(200),
    location_id         INT,
    company_id          INT             NOT NULL DEFAULT 1,
    total_items         INT             DEFAULT 0,
    total_amount        NUMERIC(14,2)   DEFAULT 0,
    status              VARCHAR(20)     NOT NULL DEFAULT 'DRAFT',   -- DRAFT, RECEIVED, PARTIALLY_RECEIVED, CANCELLED
    remarks             TEXT,
    approved_by         BIGINT,
    approved_on         TIMESTAMP WITHOUT TIME ZONE,
    created_by          BIGINT          NOT NULL,
    created_on          TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    modified_by         VARCHAR(50),
    modified_on         TIMESTAMP WITHOUT TIME ZONE,
    is_active           BOOLEAN         DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_trn_store_receive_grn    ON press_db.trn_store_receive(grn_id);
CREATE INDEX IF NOT EXISTS idx_trn_store_receive_type   ON press_db.trn_store_receive(receive_type);
CREATE INDEX IF NOT EXISTS idx_trn_store_receive_status ON press_db.trn_store_receive(status);
CREATE INDEX IF NOT EXISTS idx_trn_store_receive_date   ON press_db.trn_store_receive(receive_date);


-- ────────────────────────────────────────────────────────────────────────────
-- 6. trn_store_receive_item — Store receive line items
-- ────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS press_db.trn_store_receive_item (
    receive_item_id     BIGSERIAL       PRIMARY KEY,
    receive_id          BIGINT          NOT NULL REFERENCES press_db.trn_store_receive(receive_id) ON DELETE CASCADE,
    item_sequence       INT             NOT NULL DEFAULT 1,
    material_category   VARCHAR(30)     NOT NULL,   -- PAPER, INK, PLATE, CHEMICAL, OTHER
    material_id         BIGINT,
    material_code       VARCHAR(50),
    material_name       VARCHAR(200)    NOT NULL,
    specification       TEXT,
    ordered_quantity    NUMERIC(14,3)   DEFAULT 0,
    received_quantity   NUMERIC(14,3)   NOT NULL DEFAULT 0,
    rejected_quantity   NUMERIC(14,3)   DEFAULT 0,
    accepted_quantity   NUMERIC(14,3)   DEFAULT 0,
    uom                 VARCHAR(20)     DEFAULT 'Pcs',
    rate                NUMERIC(14,2)   DEFAULT 0,
    amount              NUMERIC(14,2)   DEFAULT 0,
    batch_no            VARCHAR(50),
    expiry_date         DATE,
    for_part            VARCHAR(50),
    remarks             TEXT,
    is_selected         BOOLEAN         DEFAULT TRUE,
    created_on          TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_trn_store_receive_item_rcv ON press_db.trn_store_receive_item(receive_id);
CREATE INDEX IF NOT EXISTS idx_trn_store_receive_item_cat ON press_db.trn_store_receive_item(material_category);


-- ────────────────────────────────────────────────────────────────────────────
-- 7. trn_purchase_grn — Purchase Goods Receipt Note header
-- ────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS press_db.trn_purchase_grn (
    grn_id              BIGSERIAL       PRIMARY KEY,
    grn_no              VARCHAR(30)     NOT NULL UNIQUE,
    grn_date            DATE            NOT NULL DEFAULT CURRENT_DATE,
    grn_type            VARCHAR(30)     NOT NULL DEFAULT 'JOB',   -- JOB, REQUISITION, STOCK_ADJUSTMENT
    job_id              BIGINT,
    job_no              VARCHAR(30),
    rate_calc_id        BIGINT,                                    -- FK to hyb_job_rate_calculator (BOM source)
    purchase_order_id   BIGINT,                                    -- FK to trn_purchase_order if applicable
    purchase_order_no   VARCHAR(30),
    supplier_id         INT,
    supplier_name       VARCHAR(200),
    invoice_no          VARCHAR(50),
    invoice_date        DATE,
    location_id         INT,
    company_id          INT             NOT NULL DEFAULT 1,
    total_items         INT             DEFAULT 0,
    total_amount        NUMERIC(14,2)   DEFAULT 0,
    tax_amount          NUMERIC(14,2)   DEFAULT 0,
    net_amount          NUMERIC(14,2)   DEFAULT 0,
    status              VARCHAR(20)     NOT NULL DEFAULT 'DRAFT',  -- DRAFT, RECEIVED, PARTIALLY_RECEIVED, INSPECTED, CANCELLED
    quality_status      VARCHAR(20)     DEFAULT 'PENDING',          -- PENDING, PASSED, FAILED, PARTIAL
    remarks             TEXT,
    approved_by         BIGINT,
    approved_on         TIMESTAMP WITHOUT TIME ZONE,
    created_by          BIGINT          NOT NULL,
    created_on          TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    modified_by         VARCHAR(50),
    modified_on         TIMESTAMP WITHOUT TIME ZONE,
    is_active           BOOLEAN         DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_trn_purchase_grn_job     ON press_db.trn_purchase_grn(job_id);
CREATE INDEX IF NOT EXISTS idx_trn_purchase_grn_type    ON press_db.trn_purchase_grn(grn_type);
CREATE INDEX IF NOT EXISTS idx_trn_purchase_grn_status  ON press_db.trn_purchase_grn(status);
CREATE INDEX IF NOT EXISTS idx_trn_purchase_grn_date    ON press_db.trn_purchase_grn(grn_date);
CREATE INDEX IF NOT EXISTS idx_trn_purchase_grn_supplier ON press_db.trn_purchase_grn(supplier_id);


-- ────────────────────────────────────────────────────────────────────────────
-- 8. trn_purchase_grn_item — GRN line items
-- ────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS press_db.trn_purchase_grn_item (
    grn_item_id         BIGSERIAL       PRIMARY KEY,
    grn_id              BIGINT          NOT NULL REFERENCES press_db.trn_purchase_grn(grn_id) ON DELETE CASCADE,
    item_sequence       INT             NOT NULL DEFAULT 1,
    material_category   VARCHAR(30)     NOT NULL,   -- PAPER, INK, PLATE, CHEMICAL, OTHER
    material_id         BIGINT,
    material_code       VARCHAR(50),
    material_name       VARCHAR(200)    NOT NULL,
    specification       TEXT,
    bom_quantity        NUMERIC(14,3)   DEFAULT 0,
    ordered_quantity    NUMERIC(14,3)   DEFAULT 0,
    received_quantity   NUMERIC(14,3)   NOT NULL DEFAULT 0,
    rejected_quantity   NUMERIC(14,3)   DEFAULT 0,
    accepted_quantity   NUMERIC(14,3)   DEFAULT 0,
    uom                 VARCHAR(20)     DEFAULT 'Pcs',
    rate                NUMERIC(14,2)   DEFAULT 0,
    amount              NUMERIC(14,2)   DEFAULT 0,
    tax_rate            NUMERIC(5,2)    DEFAULT 18.00,
    tax_amount          NUMERIC(14,2)   DEFAULT 0,
    net_amount          NUMERIC(14,2)   DEFAULT 0,
    batch_no            VARCHAR(50),
    expiry_date         DATE,
    available_stock     NUMERIC(14,3)   DEFAULT 0,
    for_part            VARCHAR(50),
    quality_status      VARCHAR(20)     DEFAULT 'PENDING',
    remarks             TEXT,
    is_selected         BOOLEAN         DEFAULT TRUE,
    created_on          TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_trn_purchase_grn_item_grn ON press_db.trn_purchase_grn_item(grn_id);
CREATE INDEX IF NOT EXISTS idx_trn_purchase_grn_item_cat ON press_db.trn_purchase_grn_item(material_category);


-- ────────────────────────────────────────────────────────────────────────────
-- 9. trn_stock_ledger — Unified stock movement ledger
-- ────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS press_db.trn_stock_ledger (
    ledger_id           BIGSERIAL       PRIMARY KEY,
    transaction_date    DATE            NOT NULL DEFAULT CURRENT_DATE,
    transaction_type    VARCHAR(30)     NOT NULL,    -- ISSUE, RECEIVE, GRN, ADJUSTMENT, TRANSFER, RETURN
    reference_type      VARCHAR(30),                 -- STORE_ISSUE, STORE_RECEIVE, PURCHASE_GRN
    reference_id        BIGINT,
    reference_no        VARCHAR(30),
    material_category   VARCHAR(30)     NOT NULL,    -- PAPER, INK, PLATE, CHEMICAL, OTHER
    material_id         BIGINT,
    material_code       VARCHAR(50),
    material_name       VARCHAR(200)    NOT NULL,
    uom                 VARCHAR(20)     DEFAULT 'Pcs',
    quantity_in         NUMERIC(14,3)   DEFAULT 0,
    quantity_out        NUMERIC(14,3)   DEFAULT 0,
    balance_quantity    NUMERIC(14,3)   DEFAULT 0,
    rate                NUMERIC(14,2)   DEFAULT 0,
    amount              NUMERIC(14,2)   DEFAULT 0,
    job_id              BIGINT,
    job_no              VARCHAR(30),
    location_id         INT,
    company_id          INT             NOT NULL DEFAULT 1,
    remarks             TEXT,
    created_by          BIGINT,
    created_on          TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_trn_stock_ledger_date    ON press_db.trn_stock_ledger(transaction_date);
CREATE INDEX IF NOT EXISTS idx_trn_stock_ledger_type    ON press_db.trn_stock_ledger(transaction_type);
CREATE INDEX IF NOT EXISTS idx_trn_stock_ledger_mat     ON press_db.trn_stock_ledger(material_category, material_id);
CREATE INDEX IF NOT EXISTS idx_trn_stock_ledger_ref     ON press_db.trn_stock_ledger(reference_type, reference_id);
CREATE INDEX IF NOT EXISTS idx_trn_stock_ledger_job     ON press_db.trn_stock_ledger(job_id);


-- ────────────────────────────────────────────────────────────────────────────
-- 10. trn_store_timeline — Timeline entries for store/purchase transactions
-- ────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS press_db.trn_store_timeline (
    timeline_id         BIGSERIAL       PRIMARY KEY,
    module              VARCHAR(30)     NOT NULL,    -- STORE_ISSUE, STORE_RECEIVE, PURCHASE_GRN
    reference_id        BIGINT          NOT NULL,
    event_type          VARCHAR(50)     NOT NULL,    -- CREATED, STATUS_CHANGED, ISSUED, RECEIVED, NOTIFICATION_SENT, etc.
    event_code          VARCHAR(50),
    event_title         VARCHAR(200)    NOT NULL,
    event_description   TEXT,
    old_status          VARCHAR(20),
    new_status          VARCHAR(20),
    remarks             TEXT,
    attachment_url      TEXT,
    created_by          BIGINT          DEFAULT 0,
    created_on          TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_active           BOOLEAN         DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_trn_store_timeline_module ON press_db.trn_store_timeline(module, reference_id);
CREATE INDEX IF NOT EXISTS idx_trn_store_timeline_type   ON press_db.trn_store_timeline(event_type);


-- ────────────────────────────────────────────────────────────────────────────
-- 11. Document Sequence entries for new processes
-- ────────────────────────────────────────────────────────────────────────────
INSERT INTO press_db.mst_document_sequence (process_code, prefix, suffix, current_number, padding_length, reset_on, is_active, created_at)
SELECT 'STORE_ISSUE', 'SI-', '', 0, 5, 'YEARLY', TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM press_db.mst_document_sequence WHERE process_code = 'STORE_ISSUE');

INSERT INTO press_db.mst_document_sequence (process_code, prefix, suffix, current_number, padding_length, reset_on, is_active, created_at)
SELECT 'STORE_RECEIVE', 'SR-', '', 0, 5, 'YEARLY', TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM press_db.mst_document_sequence WHERE process_code = 'STORE_RECEIVE');

INSERT INTO press_db.mst_document_sequence (process_code, prefix, suffix, current_number, padding_length, reset_on, is_active, created_at)
SELECT 'PURCHASE_GRN', 'GRN-', '', 0, 5, 'YEARLY', TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM press_db.mst_document_sequence WHERE process_code = 'PURCHASE_GRN');


-- ============================================================================
-- END OF MIGRATION
-- ============================================================================
