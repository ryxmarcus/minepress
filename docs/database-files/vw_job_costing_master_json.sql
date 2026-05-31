CREATE OR REPLACE VIEW press_db.vw_job_costing_master_json AS
SELECT jsonb_build_object(

    -- 1. JOB TYPES
    'job_types_master',
    (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', j.jobtypeid,
                'table_name', 'mst_job_type',
                'job_type_code', j.jobtypecode,
                'job_type_name', j.jobtypename,
                'description', j.description,
                'is_design_required', j.isdesignrequired,
                'is_dtp_required', j.isdtprequired,
                'is_ctp_required', j.isctprequired,
                'is_printing_required', j.isprintingrequired,
                'is_binding_required', j.isbindingrequired,
                'is_finishing_required', j.isfinishingrequired,
                'printing_mode', j.printingmode,
                'is_single_process', j.issingleprocess,
                'is_full_process', j.isfullprocess,
                'is_customer_material', j.iscustomermaterial,
                'is_inhouse_material', j.isinhousematerial,
                'is_outsource_job', j.isoutsourcejob,
                'allow_advance_payment', j.allowadvancepayment,
                'require_costing_approval', j.requirecostingapproval,
                'default_start_process_code', j.defaultstartprocesscode,
                'default_end_process_code', j.defaultendprocesscode
            )
            ORDER BY j.jobtypecode
        ), '[]'::jsonb)
        FROM press_db.mst_job_type j
    ),

    -- 2. PRODUCT TYPES
    'print_product_types',
    (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', p.printproducttypeid,
                'table_name', 'mst_print_product_type',
                'product_code', p.productcode,
                'product_name', p.productname,
                'category', p.category,
                'description', p.description,
                'is_custom_size', p.iscustomsize,
                'is_binding_required', p.isbindingrequired,
                'is_printing_required', p.isprintingrequired,
                'is_finishing_required', p.isfinishingrequired
            )
            ORDER BY p.productcode
        ), '[]'::jsonb)
        FROM press_db.mst_print_product_type p
    ),

    -- 3. PRODUCT PARTS
    'product_parts',
    (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', pp.productpartid,
                'table_name', 'mst_product_part',
                'part_code', pp.partcode,
                'part_name', pp.partname,
                'description', pp.description,
                'print_product_type_id', pp.printproducttypeid,
                'is_page_based', pp.ispagebased,
                'is_multiple', pp.ismultiple,
                'default_pages', pp.defaultpages,
                'requires_paper', pp.requirespaper,
                'requires_plate', pp.requiresplate,
                'requires_printing', pp.requiresprinting,
                'requires_binding', pp.requiresbinding,
                'requires_finishing', pp.requiresfinishing,
                'display_order', pp.displayorder
            )
            ORDER BY pp.displayorder
        ), '[]'::jsonb)
        FROM press_db.mst_product_part pp
    ),

    -- 4. PRODUCT SIZES
    'print_product_sizes',
    (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', s.productsizeid,
                'table_name', 'mst_print_product_size',
                'size_code', s.sizecode,
                'size_name', s.sizename,
                'width_mm', s.widthmm,
                'height_mm', s.heightmm,
                'width_inch', s.widthinch,
                'height_inch', s.heightinch,
                'category', s.category,
                'is_standard', s.isstandard,
                'is_active', s.isactive,
                'remarks', s.remarks
            )
            ORDER BY s.sizecode
        ), '[]'::jsonb)
        FROM press_db.mst_print_product_size s
        WHERE s.isactive = true
    ),

    -- 5. DESIGNING
    'designing',
    (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', d.designing_id,
                'table_name', 'mst_designing',
                'design_code', d.design_code,
                'design_name', d.design_name,
                'category', d.design_category,
                'design_type', d.design_type,
                'job_types', d.job_types_supported,
                'design_by_party', d.is_design_by_party,
                'plate_by_party', d.is_plate_by_party,
                'software', d.software_used,
                'file_format', d.file_format,
                'color_mode', d.color_mode,
                'revision_allowed', d.revision_allowed,
                'rework_charge', d.rework_charge_per_revision,
                'base_cost', d.base_cost,
                'cost_unit', d.cost_unit,
                'avg_time_hours', d.avg_time_hours,
                'manpower_required', d.manpower_required,
                'cost_applicable', d.is_cost_applicable
            )
        ), '[]'::jsonb)
        FROM press_db.mst_designing d
        WHERE d.is_active = true
    ),

    -- 6. PLATES
    'plates',
    (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', pl.plate_id,
                'table_name', 'mst_plate',
                'plate_code', pl.plate_code,
                'plate_type', pl.plate_type,
                'plate_size_mm', jsonb_build_object(
                    'length', pl.plate_length_mm,
                    'width', pl.plate_width_mm
                ),
                'rate', pl.plate_cost + pl.processing_cost,
                'job_types_supported', pl.supported_job_types
            )
        ), '[]'::jsonb)
        FROM press_db.mst_plate pl
        WHERE pl.is_active = true
    ),

    -- 7. PAPERS
    'papers',
    (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', p.paper_id,
                'table_name', 'mst_paper',
                'paper_code', p.paper_code,
                'paper_name', p.paper_name,
                'category', p.paper_category,
                'type', p.paper_type,
                'gsm', p.gsm,
                'grain', p.grain_direction,
                'size_mm', jsonb_build_object(
                    'length', p.sheet_length_mm,
                    'width', p.sheet_width_mm
                ),
                'rate_per_kg', p.cost_per_kg,
                'rate_per_sheet', p.cost_per_sheet,
                'job_types', p.supported_job_types
            )
        ), '[]'::jsonb)
        FROM press_db.mst_paper p
        WHERE p.is_active = true
    ),

    -- 8. INKS
    'inks',
    (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', t.sl_no,
                'table_name', 'mst_ink',
                'ink_code', t.ink_code,
                'ink_name', t.ink_name,
                'color', t.color_name,
                'drying', t.drying_type,
                'rate_per_kg', t.cost_per_kg,
                'consumption_gsm', t.consumption_gsm,
                'job_types', t.supported_job_types
            )
        ), '[]'::jsonb)
        FROM (
            SELECT row_number() OVER (ORDER BY i.ink_code) AS sl_no, i.*
            FROM press_db.mst_ink i
            WHERE i.is_active = true
        ) t
    ),

    -- 9. MACHINES
    'machines',
    (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', m.machine_id,
                'table_name', 'mst_machine',
                'machine_code', m.machine_code,
                'machine_name', m.machine_name,
                'department', m.department_code,
                'category', m.machine_category,
                'type', m.machine_type,
                'manufacturer', m.manufacturer,
                'model', m.model_no,
                'max_colors', m.max_colors,
                'printing_side', m.printing_side,
                'speed', jsonb_build_object(
                    'unit', m.speed_unit,
                    'max_per_hour', m.max_speed_per_hour
                ),
                'costing', jsonb_build_object(
                    'hourly', m.hourly_running_cost,
                    'setup', m.setup_cost
                ),
                'priority', m.auto_select_priority
            )
        ), '[]'::jsonb)
        FROM press_db.mst_machine m
        WHERE m.is_active = true
    ),

    -- 10. CHEMICALS
    'chemicals',
    (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', t.sl_no,
                'table_name', 'mst_chemical',
                'chemical_code', t.chemical_code,
                'chemical_name', t.chemical_name,
                'rate', t.rate_per_unit
            )
        ), '[]'::jsonb)
        FROM (
            SELECT row_number() OVER (ORDER BY c.chemical_code) AS sl_no, c.*
            FROM press_db.mst_chemical c
            WHERE c.is_active = true
        ) t
    ),

    -- 11. PRESS MATERIALS
    'press_materials',
    (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', t.sl_no,
                'table_name', 'mst_material',
                'material_code', t.material_code,
                'material_name', t.material_name,
                'rate', t.rate_per_unit
            )
        ), '[]'::jsonb)
        FROM (
            SELECT row_number() OVER (ORDER BY pm.material_code) AS sl_no, pm.*
            FROM press_db.mst_material pm
            WHERE pm.is_active = true
        ) t
    ),

    -- 12. FINISHING
    'finishing',
    (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', f.finishing_id,
                'table_name', 'mst_finishing',
                'finish_code', f.finishing_code,
                'finish_type', f.finishing_type,
                'rate', f.cost_per_sheet
            )
        ), '[]'::jsonb)
        FROM press_db.mst_finishing f
        WHERE f.is_active = true
    ),

    -- 13. BINDING
    'bindings',
    (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', b.binding_id,
                'table_name', 'mst_binding',
                'binding_code', b.binding_code,
                'binding_type', b.binding_type,
                'cost_per_unit', b.cost_per_book
            )
        ), '[]'::jsonb)
        FROM press_db.mst_binding b
        WHERE b.is_active = true
    ),

    -- 14. JOB TYPE RULES (single block only)
    'job_type_rules', jsonb_build_object(
        'FULL_OFFSET', jsonb_build_object('category','FULL','printing_mode','OFFSET'),
        'FULL_DIGITAL', jsonb_build_object('category','FULL','printing_mode','DIGITAL'),
        'FULL_SCREEN', jsonb_build_object('category','FULL','printing_mode','SCREEN'),
        'FULL_FLEX', jsonb_build_object('category','FULL','printing_mode','FLEX'),
        'FULL_UV', jsonb_build_object('category','FULL','printing_mode','UV'),
        'DESIGN_ONLY', jsonb_build_object('category','SERVICE'),
        'DTP_ONLY', jsonb_build_object('category','SERVICE'),
        'CTP_ONLY', jsonb_build_object('category','PREPRESS'),
        'PROOF_ONLY', jsonb_build_object('category','PREPRESS'),
        'PRINT_OFFSET', jsonb_build_object('category','PRINT_ONLY','printing_mode','OFFSET'),
        'PRINT_DIGITAL', jsonb_build_object('category','PRINT_ONLY','printing_mode','DIGITAL'),
        'PRINT_SCREEN', jsonb_build_object('category','PRINT_ONLY','printing_mode','SCREEN'),
        'PRINT_FLEX', jsonb_build_object('category','PRINT_ONLY','printing_mode','FLEX'),
        'PRINT_UV', jsonb_build_object('category','PRINT_ONLY','printing_mode','UV'),
        'BINDING_ONLY', jsonb_build_object('category','POST'),
        'FINISH_ONLY', jsonb_build_object('category','POST'),
        'LAMINATION', jsonb_build_object('category','POST'),
        'CUTTING', jsonb_build_object('category','POST'),
        'FOLDING', jsonb_build_object('category','POST'),
        'PACKAGING', jsonb_build_object('category','POST'),
        'OUT_PRINT', jsonb_build_object('category','OUTSOURCE'),
        'OUT_BIND', jsonb_build_object('category','OUTSOURCE'),
        'OUT_FINISH', jsonb_build_object('category','OUTSOURCE'),
        'JOB_WORK', jsonb_build_object('category','LABOUR')
    ),

    -- 15. JOB TYPE DYNAMIC FIELDS
    'job_type_dynamic_fields', jsonb_build_object(

        'FULL_OFFSET', jsonb_build_object(
            'category','FULL',
            'printing_mode','OFFSET',
            'workflow', jsonb_build_array('DESIGN','DTP','CTP','PRINTING','BINDING','FINISHING','DISPATCH'),
            'modules', jsonb_build_object('design',true,'dtp',true,'ctp',true,'printing',true,'binding',true,'finishing',true),
            'fields', jsonb_build_object(
                'required', jsonb_build_array('customer_id','product_type_id','product_part_id','quantity','paper_id','machine_id','color_count','printing_side','plate_count'),
                'optional', jsonb_build_array('lamination_type','binding_type','finishing_type'),
                'conditional', jsonb_build_array(
                    jsonb_build_object('field','binding_type','required_if','is_binding_required=true'),
                    jsonb_build_object('field','lamination_type','required_if','is_finishing_required=true')
                )
            ),
            'costing', jsonb_build_object('plate',true,'ink',true,'machine',true,'binding',true,'finishing',true),
            'costing_dependencies', jsonb_build_array('paper','ink','plate','machine','binding','finishing'),
            'rules', jsonb_build_object('wastage_percent',3,'min_qty',500)
        ),

        'FULL_DIGITAL', jsonb_build_object(
            'category','FULL',
            'printing_mode','DIGITAL',
            'workflow', jsonb_build_array('DESIGN','DTP','PRINTING','BINDING','FINISHING','DISPATCH'),
            'fields', jsonb_build_object(
                'required', jsonb_build_array('customer_id','product_type_id','quantity','machine_id','color_mode'),
                'optional', jsonb_build_array('paper_id','finishing_type')
            ),
            'costing', jsonb_build_object('click',true),
            'costing_dependencies', jsonb_build_array('machine','click'),
            'rules', jsonb_build_object('wastage_percent',1)
        ),

        'FULL_SCREEN', jsonb_build_object(
            'category','FULL',
            'printing_mode','SCREEN',
            'workflow', jsonb_build_array('DESIGN','DTP','PRINTING','DRYING','FINISHING'),
            'fields', jsonb_build_object(
                'required', jsonb_build_array('customer_id','quantity','screen_count','color_count')
            ),
            'costing_dependencies', jsonb_build_array('screen','ink','labour'),
            'rules', jsonb_build_object('wastage_percent',5)
        ),

        'FULL_FLEX', jsonb_build_object(
            'category','FULL',
            'printing_mode','FLEX',
            'workflow', jsonb_build_array('DESIGN','PRINTING','FINISHING'),
            'fields', jsonb_build_object(
                'required', jsonb_build_array('customer_id','width','height','quantity')
            ),
            'costing_dependencies', jsonb_build_array('area','material'),
            'rules', jsonb_build_object('area_based',true)
        ),

        'FULL_UV', jsonb_build_object(
            'category','FULL',
            'printing_mode','UV',
            'workflow', jsonb_build_array('DESIGN','PRINTING','CURING','FINISHING'),
            'fields', jsonb_build_object(
                'required', jsonb_build_array('customer_id','material_type','quantity')
            ),
            'costing_dependencies', jsonb_build_array('uv_ink','machine'),
            'rules', jsonb_build_object('uv_multiplier',1.5)
        ),

        'DESIGN_ONLY', jsonb_build_object(
            'category','SERVICE',
            'workflow', jsonb_build_array('DESIGN'),
            'fields', jsonb_build_object(
                'required', jsonb_build_array('customer_id','design_type','pages'),
                'optional', jsonb_build_array('software','reference_file')
            ),
            'costing_dependencies', jsonb_build_array('design')
        ),

        'DTP_ONLY', jsonb_build_object(
            'category','SERVICE',
            'workflow', jsonb_build_array('DTP'),
            'fields', jsonb_build_object(
                'required', jsonb_build_array('customer_id','pages','page_size')
            ),
            'costing_dependencies', jsonb_build_array('dtp')
        ),

        'CTP_ONLY', jsonb_build_object(
            'category','PREPRESS',
            'workflow', jsonb_build_array('CTP'),
            'fields', jsonb_build_object(
                'required', jsonb_build_array('plate_size','plate_count')
            ),
            'costing_dependencies', jsonb_build_array('plate')
        ),

        'PROOF_ONLY', jsonb_build_object(
            'category','PREPRESS',
            'workflow', jsonb_build_array('PRINTING'),
            'fields', jsonb_build_object(
                'required', jsonb_build_array('proof_type','quantity')
            ),
            'costing_dependencies', jsonb_build_array('proof')
        ),

        'PRINT_OFFSET', jsonb_build_object(
            'category','PRINT_ONLY',
            'printing_mode','OFFSET',
            'fields', jsonb_build_object(
                'required', jsonb_build_array('machine_id','paper_id','quantity','color_count')
            ),
            'costing_dependencies', jsonb_build_array('machine','ink')
        ),

        'PRINT_DIGITAL', jsonb_build_object(
            'category','PRINT_ONLY',
            'printing_mode','DIGITAL',
            'fields', jsonb_build_object(
                'required', jsonb_build_array('machine_id','quantity','color_mode')
            ),
            'costing_dependencies', jsonb_build_array('click')
        ),

        'PRINT_SCREEN', jsonb_build_object(
            'category','PRINT_ONLY',
            'printing_mode','SCREEN',
            'fields', jsonb_build_object(
                'required', jsonb_build_array('screen_count','quantity')
            ),
            'costing_dependencies', jsonb_build_array('screen')
        ),

        'PRINT_FLEX', jsonb_build_object(
            'category','PRINT_ONLY',
            'printing_mode','FLEX',
            'fields', jsonb_build_object(
                'required', jsonb_build_array('width','height','quantity')
            ),
            'costing_dependencies', jsonb_build_array('area')
        ),

        'PRINT_UV', jsonb_build_object(
            'category','PRINT_ONLY',
            'printing_mode','UV',
            'fields', jsonb_build_object(
                'required', jsonb_build_array('material_type','quantity')
            ),
            'costing_dependencies', jsonb_build_array('uv')
        ),

        'BINDING_ONLY', jsonb_build_object(
            'category','POST',
            'workflow', jsonb_build_array('BINDING'),
            'fields', jsonb_build_object(
                'required', jsonb_build_array('binding_type','quantity')
            ),
            'costing_dependencies', jsonb_build_array('binding')
        ),

        'FINISH_ONLY', jsonb_build_object(
            'category','POST',
            'workflow', jsonb_build_array('FINISHING'),
            'fields', jsonb_build_object(
                'required', jsonb_build_array('finishing_type','quantity')
            ),
            'costing_dependencies', jsonb_build_array('finishing')
        ),

        'LAMINATION', jsonb_build_object(
            'category','POST',
            'fields', jsonb_build_object(
                'required', jsonb_build_array('lamination_type','paper_id','quantity')
            ),
            'costing_dependencies', jsonb_build_array('lamination')
        ),

        'CUTTING', jsonb_build_object(
            'category','POST',
            'fields', jsonb_build_object(
                'required', jsonb_build_array('quantity','cut_size')
            ),
            'costing_dependencies', jsonb_build_array('cutting')
        ),

        'FOLDING', jsonb_build_object(
            'category','POST',
            'fields', jsonb_build_object(
                'required', jsonb_build_array('quantity','fold_type')
            ),
            'costing_dependencies', jsonb_build_array('folding')
        ),

        'PACKAGING', jsonb_build_object(
            'category','POST',
            'fields', jsonb_build_object(
                'required', jsonb_build_array('quantity','package_type')
            ),
            'costing_dependencies', jsonb_build_array('packaging')
        ),

        'OUT_PRINT', jsonb_build_object(
            'category','OUTSOURCE',
            'fields', jsonb_build_object(
                'required', jsonb_build_array('vendor_id','job_description','cost')
            ),
            'costing_dependencies', jsonb_build_array('vendor')
        ),

        'OUT_BIND', jsonb_build_object(
            'category','OUTSOURCE',
            'fields', jsonb_build_object(
                'required', jsonb_build_array('vendor_id','binding_type','cost')
            ),
            'costing_dependencies', jsonb_build_array('vendor')
        ),

        'OUT_FINISH', jsonb_build_object(
            'category','OUTSOURCE',
            'fields', jsonb_build_object(
                'required', jsonb_build_array('vendor_id','finishing_type','cost')
            ),
            'costing_dependencies', jsonb_build_array('vendor')
        ),

        'JOB_WORK', jsonb_build_object(
            'category','LABOUR',
            'fields', jsonb_build_object(
                'required', jsonb_build_array('labour_type','hours','rate')
            ),
            'costing_dependencies', jsonb_build_array('labour')
        )

    ),

    -- 16. COSTING RULES
    'costing_rules', jsonb_build_object(
        'base_rules', jsonb_build_object(
            'currency', 'INR',
            'rounding_strategy', 'nearest_10',
            'decimal_precision', 2,
            'minimum_job_value', 500
        ),
        'production_rules', jsonb_build_object(
            'machine_efficiency_percent', 85,
            'downtime_percent', 10,
            'plate_change_time_minutes', 15,
            'shift_hours', 8
        ),
        'paper_rules', jsonb_build_object(
            'cutting_wastage_percent', 2,
            'handling_wastage_percent', 1,
            'storage_loss_percent', 0.5,
            'minimum_order_qty_sheets', 500
        ),
        'ink_rules', jsonb_build_object(
            'wastage_percent', 5,
            'startup_ink_grams', 200
        ),
        'plate_rules', jsonb_build_object(
            'plate_wastage_percent', 3,
            'replate_percent', 2,
            'minimum_plate_count', 1
        ),
        'printing_rules', jsonb_build_object(
            'double_side_factor', 1.8,
            'perfecting_factor', 1.6,
            'color_registration_wastage_percent', 2
        ),
        'postpress_rules', jsonb_build_object(
            'binding_wastage_percent', 2,
            'finishing_wastage_percent', 2,
            'packing_cost_per_unit', 2
        ),
        'pricing_rules', jsonb_build_object(
            'gst_percent', 18,
            'round_off_to', 10
        )
    )

) AS job_costing_master_json;