-- =====================================================================
-- MinePress ERP — Store/Purchase Module: Master Table Column Additions
-- Adds hsn_code and gst_rate columns to 4 master tables
-- for unified material selection in Store Issue, Receive & Purchase GRN
-- =====================================================================
-- Run against: PostgreSQL (press_db schema)
-- Date: 2025
-- =====================================================================

-- ── 1. mst_chemical ──
ALTER TABLE press_db.mst_chemical
    ADD COLUMN IF NOT EXISTS hsn_code VARCHAR(20) DEFAULT NULL,
    ADD COLUMN IF NOT EXISTS gst_rate NUMERIC(5,2) DEFAULT NULL;

COMMENT ON COLUMN press_db.mst_chemical.hsn_code IS 'HSN/SAC code for GST classification';
COMMENT ON COLUMN press_db.mst_chemical.gst_rate IS 'GST rate percentage (e.g., 18.00 for 18%)';

-- ── 2. mst_ink ──
ALTER TABLE press_db.mst_ink
    ADD COLUMN IF NOT EXISTS hsn_code VARCHAR(20) DEFAULT NULL,
    ADD COLUMN IF NOT EXISTS gst_rate NUMERIC(5,2) DEFAULT NULL;

COMMENT ON COLUMN press_db.mst_ink.hsn_code IS 'HSN/SAC code for GST classification';
COMMENT ON COLUMN press_db.mst_ink.gst_rate IS 'GST rate percentage (e.g., 18.00 for 18%)';

-- ── 3. mst_paper ──
ALTER TABLE press_db.mst_paper
    ADD COLUMN IF NOT EXISTS hsn_code VARCHAR(20) DEFAULT NULL,
    ADD COLUMN IF NOT EXISTS gst_rate NUMERIC(5,2) DEFAULT NULL;

COMMENT ON COLUMN press_db.mst_paper.hsn_code IS 'HSN/SAC code for GST classification';
COMMENT ON COLUMN press_db.mst_paper.gst_rate IS 'GST rate percentage (e.g., 18.00 for 18%)';

-- ── 4. mst_plate ──
ALTER TABLE press_db.mst_plate
    ADD COLUMN IF NOT EXISTS hsn_code VARCHAR(20) DEFAULT NULL,
    ADD COLUMN IF NOT EXISTS gst_rate NUMERIC(5,2) DEFAULT NULL;

COMMENT ON COLUMN press_db.mst_plate.hsn_code IS 'HSN/SAC code for GST classification';
COMMENT ON COLUMN press_db.mst_plate.gst_rate IS 'GST rate percentage (e.g., 18.00 for 18%)';

-- ── Verification ──
-- Run these queries to confirm columns were added:
-- SELECT column_name, data_type FROM information_schema.columns WHERE table_schema = 'press_db' AND table_name = 'mst_chemical' AND column_name IN ('hsn_code', 'gst_rate');
-- SELECT column_name, data_type FROM information_schema.columns WHERE table_schema = 'press_db' AND table_name = 'mst_ink' AND column_name IN ('hsn_code', 'gst_rate');
-- SELECT column_name, data_type FROM information_schema.columns WHERE table_schema = 'press_db' AND table_name = 'mst_paper' AND column_name IN ('hsn_code', 'gst_rate');
-- SELECT column_name, data_type FROM information_schema.columns WHERE table_schema = 'press_db' AND table_name = 'mst_plate' AND column_name IN ('hsn_code', 'gst_rate');

-- NOTE: mst_other_items already has hsn_code and gst_rate columns — no changes needed for that table.
