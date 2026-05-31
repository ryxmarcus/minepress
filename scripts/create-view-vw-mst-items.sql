-- ============================================================
-- VIEW: press_db.vw_mst_items
-- Unifies 5 master material tables into a single queryable view
--   1. press_db.mst_chemical
--   2. press_db.mst_ink
--   3. press_db.mst_paper
--   4. press_db.mst_plate
--   5. press_db.mst_other_items
-- ============================================================

CREATE OR REPLACE VIEW press_db.vw_mst_items AS

-- ── Chemical ──
SELECT
    ROW_NUMBER() OVER (ORDER BY src, item_code) AS item_id,
    item_code,
    item_name,
    item_description,
    item_category,
    uom,
    purchase_rate,
    reorder_level,
    current_stock,
    hsn_code,
    gst_rate,
    last_purchase_rate,
    last_purchase_date,
    is_active,
    remarks,
    source_table
FROM (

    SELECT
        c.chemical_code                         AS item_code,
        c.chemical_name                         AS item_name,
        NULL::text                              AS item_description,
        COALESCE(c.chemical_category, 'Chemical') AS item_category,
        c.uom,
        c.rate_per_unit                         AS purchase_rate,
        c.reorder_level,
        c.current_stock,
        c.hsn_code,
        c.gst_rate,
        c.last_purchase_rate,
        c.last_purchase_date,
        c.is_active,
        c.remarks,
        'CHEMICAL'                              AS source_table,
        'CHEMICAL'                              AS src
    FROM press_db.mst_chemical c

    UNION ALL

    -- ── Ink ──
    SELECT
        i.ink_code                              AS item_code,
        i.ink_name                              AS item_name,
        NULL::text                              AS item_description,
        COALESCE(i.ink_category, 'Ink')         AS item_category,
        i.uom,
        i.cost_per_kg                           AS purchase_rate,
        i.reorder_level,
        i.current_stock,
        i.hsn_code,
        i.gst_rate,
        i.last_purchase_rate,
        i.last_purchase_date,
        i.is_active,
        i.remarks,
        'INK'                                   AS source_table,
        'INK'                                   AS src
    FROM press_db.mst_ink i

    UNION ALL

    -- ── Paper ──
    SELECT
        p.paper_code                            AS item_code,
        p.paper_name                            AS item_name,
        NULL::text                              AS item_description,
        COALESCE(p.paper_category, 'Paper')     AS item_category,
        p.uom,
        COALESCE(p.cost_per_sheet, p.cost_per_kg) AS purchase_rate,
        p.reorder_level,
        p.current_stock,
        p.hsn_code,
        p.gst_rate,
        p.last_purchase_rate,
        p.last_purchase_date,
        p.is_active,
        p.remarks,
        'PAPER'                                 AS source_table,
        'PAPER'                                 AS src
    FROM press_db.mst_paper p

    UNION ALL

    -- ── Plate ──
    SELECT
        pl.plate_code                           AS item_code,
        pl.plate_name                           AS item_name,
        NULL::text                              AS item_description,
        COALESCE(pl.plate_type, 'Plate')        AS item_category,
        pl.uom,
        pl.plate_cost                           AS purchase_rate,
        pl.reorder_level,
        pl.current_stock,
        pl.hsn_code,
        pl.gst_rate,
        pl.last_purchase_rate,
        pl.last_purchase_date,
        pl.is_active,
        pl.remarks,
        'PLATE'                                 AS source_table,
        'PLATE'                                 AS src
    FROM press_db.mst_plate pl

    UNION ALL

    -- ── Other Items ──
    SELECT
        o.item_code,
        o.item_name,
        o.description                           AS item_description,
        COALESCE(o.item_category, 'Other')      AS item_category,
        o.uom,
        o.rate_per_unit                         AS purchase_rate,
        o.reorder_level,
        o.current_stock,
        o.hsn_code,
        o.gst_rate,
        o.last_purchase_rate,
        o.last_purchase_date,
        o.is_active,
        o.remarks,
        'OTHER'                                 AS source_table,
        'OTHER'                                 AS src
    FROM press_db.mst_other_items o

) AS unified
ORDER BY source_table, item_code;
