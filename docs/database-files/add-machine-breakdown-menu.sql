-- ═══════════════════════════════════════════════════════════════
-- Add Machine Breakdown page to navigation menu (mst_module)
-- Run this script against the press_db schema after deployment.
-- ═══════════════════════════════════════════════════════════════

-- First, find the parent module_id for the Production group.
-- Adjust the WHERE clause if your Production parent uses a different module_code.
DO $$
DECLARE
    v_parent_id INT;
    v_max_order INT;
BEGIN
    -- Get the Production parent module
    SELECT module_id INTO v_parent_id
    FROM press_db.mst_module
    WHERE module_code = 'PRODUCTION' AND module_level = 1
    LIMIT 1;

    IF v_parent_id IS NULL THEN
        RAISE NOTICE 'Production parent module not found. Please check module_code.';
        RETURN;
    END IF;

    -- Get the max display_order under Production to append at the end
    SELECT COALESCE(MAX(display_order), 0) INTO v_max_order
    FROM press_db.mst_module
    WHERE parent_module_id = v_parent_id;

    -- Insert Machine Breakdown menu item (skip if already exists)
    INSERT INTO press_db.mst_module (
        module_code, module_name, parent_module_id, route_url, icon,
        display_order, is_mobile, is_web, is_active, module_level,
        is_section_header, section_name, badge_text, badge_class,
        has_divider_before, icon_svg, created_by, created_on
    )
    SELECT
        'MACHINE_BREAKDOWN',
        'Machine Breakdown',
        v_parent_id,
        '/Production/MachineBreakdown',
        'tool',
        v_max_order + 1,
        false,
        true,
        true,
        2,
        false,
        NULL,
        'New',
        'badge badge-sm bg-red-lt',
        false,
        NULL,
        'system',
        CURRENT_TIMESTAMP
    WHERE NOT EXISTS (
        SELECT 1 FROM press_db.mst_module WHERE module_code = 'MACHINE_BREAKDOWN'
    );

    RAISE NOTICE 'Machine Breakdown menu entry added successfully.';
END $$;
