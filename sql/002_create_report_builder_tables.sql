-- ══════════════════════════════════════════════════════════════════════════
-- MinePress ERP — Report Builder Tables
-- Schema: press_db
-- ══════════════════════════════════════════════════════════════════════════

-- ── Saved Reports ──
CREATE TABLE IF NOT EXISTS press_db.rpt_saved_reports (
    report_id           BIGSERIAL       PRIMARY KEY,
    report_code         VARCHAR(50)     NOT NULL,
    report_name         VARCHAR(200)    NOT NULL,
    description         TEXT,
    source_table        VARCHAR(200)    NOT NULL,
    is_shared           BOOLEAN         NOT NULL DEFAULT FALSE,
    is_default          BOOLEAN         NOT NULL DEFAULT FALSE,
    group_by_columns    TEXT,           -- JSON array of column names
    order_by_columns    TEXT,           -- JSON array e.g. [{"column":"name","dir":"asc"}]
    page_size           INT             NOT NULL DEFAULT 25,
    chart_type          VARCHAR(50),    -- 'bar','line','pie','donut','area',null
    chart_config        TEXT,           -- JSON config for chart
    ai_summary_prompt   TEXT,           -- optional custom AI prompt
    created_by          VARCHAR(100)    NOT NULL,
    created_on          TIMESTAMP       NOT NULL DEFAULT NOW(),
    modified_by         VARCHAR(100),
    modified_on         TIMESTAMP,
    is_active           BOOLEAN         NOT NULL DEFAULT TRUE
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_rpt_saved_reports_code
    ON press_db.rpt_saved_reports (report_code) WHERE is_active;

CREATE INDEX IF NOT EXISTS idx_rpt_saved_reports_user
    ON press_db.rpt_saved_reports (created_by, is_active);

COMMENT ON TABLE press_db.rpt_saved_reports IS 'User-saved report definitions for the self-service report builder';

-- ── Saved Report Columns ──
CREATE TABLE IF NOT EXISTS press_db.rpt_saved_report_columns (
    report_column_id    BIGSERIAL       PRIMARY KEY,
    report_id           BIGINT          NOT NULL REFERENCES press_db.rpt_saved_reports(report_id) ON DELETE CASCADE,
    column_name         VARCHAR(200)    NOT NULL,
    display_name        VARCHAR(200),
    column_order        INT             NOT NULL DEFAULT 0,
    is_visible          BOOLEAN         NOT NULL DEFAULT TRUE,
    aggregate_function  VARCHAR(20),    -- 'SUM','AVG','COUNT','MIN','MAX',null
    format_string       VARCHAR(100),   -- e.g. 'N2','dd-MMM-yyyy','P1'
    column_width        INT,
    is_active           BOOLEAN         NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_rpt_saved_report_columns_report
    ON press_db.rpt_saved_report_columns (report_id);

COMMENT ON TABLE press_db.rpt_saved_report_columns IS 'Column selection and display config for saved reports';

-- ── Saved Report Filters ──
CREATE TABLE IF NOT EXISTS press_db.rpt_saved_report_filters (
    report_filter_id    BIGSERIAL       PRIMARY KEY,
    report_id           BIGINT          NOT NULL REFERENCES press_db.rpt_saved_reports(report_id) ON DELETE CASCADE,
    column_name         VARCHAR(200)    NOT NULL,
    operator            VARCHAR(30)     NOT NULL DEFAULT 'eq', -- eq,neq,gt,gte,lt,lte,contains,startswith,endswith,in,between,isnull,isnotnull
    filter_value        TEXT,
    filter_value2       TEXT,           -- for 'between' operator
    filter_order        INT             NOT NULL DEFAULT 0,
    logic_operator      VARCHAR(5)      NOT NULL DEFAULT 'AND', -- AND, OR
    is_active           BOOLEAN         NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_rpt_saved_report_filters_report
    ON press_db.rpt_saved_report_filters (report_id);

COMMENT ON TABLE press_db.rpt_saved_report_filters IS 'Filter conditions for saved reports';
