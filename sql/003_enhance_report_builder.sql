-- ═══════════════════════════════════════════════════════════════════════════════
--  003 — Enhance Report Builder: multi-table joins, detail/summary, totals
-- ═══════════════════════════════════════════════════════════════════════════════

-- Add new columns to rpt_saved_reports
ALTER TABLE press_db.rpt_saved_reports
    ADD COLUMN IF NOT EXISTS report_type       VARCHAR(20)  NOT NULL DEFAULT 'detail',   -- 'detail' | 'summary'
    ADD COLUMN IF NOT EXISTS show_totals        BOOLEAN      NOT NULL DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS show_grand_total   BOOLEAN      NOT NULL DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS joined_tables      TEXT         NULL;                         -- JSON: [{table, alias, joinType, fkColumn, pkColumn}]

COMMENT ON COLUMN press_db.rpt_saved_reports.report_type      IS 'detail = row-level, summary = grouped aggregates';
COMMENT ON COLUMN press_db.rpt_saved_reports.show_totals       IS 'Show column totals for numeric columns';
COMMENT ON COLUMN press_db.rpt_saved_reports.show_grand_total  IS 'Show grand total row at the bottom';
COMMENT ON COLUMN press_db.rpt_saved_reports.joined_tables     IS 'JSON array of joined tables with FK/PK mapping';
