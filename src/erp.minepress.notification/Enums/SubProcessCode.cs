namespace erp.minepress.notification.Enums;

/// <summary>
/// Sub-process code constants matching mst_sub_process.subprocesscode.
/// Used as string constants since there are 80+ codes across all processes.
/// </summary>
public static class SubProcessCode
{
    // ── Process 1: Enquiry & Job Order (ENQ_JOB) ──
    public const string ReceiveEnq = "RECEIVE_ENQ";
    public const string IdentifyCustomer = "IDENTIFY_CUSTOMER";
    public const string CrmProcess = "CRM_PROCESS";
    public const string CollectSample = "COLLECT_SAMPLE";
    public const string CollectContent = "COLLECT_CONTENT";
    public const string DefineReq = "DEFINE_REQ";
    public const string JobEstimation = "JOB_ESTIMATION";
    public const string SendQuote = "SEND_QUOTE";
    public const string RateNegotiation = "RATE_NEGOTIATION";
    public const string RateFinalize = "RATE_FINALIZE";
    public const string OrderReceived = "ORDER_RECEIVED";
    public const string GenerateJob = "GENERATE_JOB";

    // ── Process 2: Pre-Design (PRE_DES) ──
    public const string CollectArtwork = "COLLECT_ARTWORK";
    public const string VerifyContent = "VERIFY_CONTENT";
    public const string TextProof = "TEXT_PROOF";
    public const string ImageCorr = "IMAGE_CORR";
    public const string ScanHard = "SCAN_HARD";
    public const string OcrText = "OCR_TEXT";
    public const string ContentFlow = "CONTENT_FLOW";

    // ── Process 3: Design / DTP (DES_DTP) ──
    public const string DesignPages = "DESIGN_PAGES";
    public const string LayoutCreate = "LAYOUT_CREATE";
    public const string Imposition = "IMPOSITION";
    public const string SetMargins = "SET_MARGINS";
    public const string TextFormat = "TEXT_FORMAT";
    public const string ImgEdit = "IMG_EDIT";
    public const string ColorProfile = "COLOR_PROFILE";
    public const string Pagination = "PAGINATION";
    public const string Spine = "SPINE";
    public const string ExportProof = "EXPORT_PROOF";

    // ── Process 4: Proofing (PROOF) ──
    public const string SendProof = "SEND_PROOF";
    public const string GetApproval = "GET_APPROVAL";
    public const string CorrectionCycle = "CORRECTION_CYCLE";
    public const string FinalOk = "FINAL_OK";

    // ── Process 5: Pre-Press (PRE_PRESS) ──
    public const string Preflight = "PREFLIGHT";
    public const string ColorSep = "COLOR_SEP";
    public const string ImpositionSig = "IMPOSITION_SIG";
    public const string CtpPlate = "CTP_PLATE";
    public const string PlateInspect = "PLATE_INSPECT";
    public const string PlateStore = "PLATE_STORE";
    public const string RipProcess = "RIP_PROCESS";

    // ── Process 6: Procurement (PROC) ──
    public const string PaperProc = "PAPER_PROC";
    public const string InkProc = "INK_PROC";
    public const string BindMat = "BIND_MAT";
    public const string LamFilm = "LAM_FILM";
    public const string UvChem = "UV_CHEM";
    public const string Board = "BOARD";
    public const string PackMat = "PACK_MAT";

    // ── Process 7: Cutting (CUT) ──
    public const string ReelCut = "REEL_CUT";
    public const string ReamMark = "REAM_MARK";
    public const string SizeVerify = "SIZE_VERIFY";
    public const string CountBundle = "COUNT_BUNDLE";
    public const string CutQc = "CUT_QC";

    // ── Process 8: Printing (PRINT) ──
    public const string PlateMount = "PLATE_MOUNT";
    public const string InkSetup = "INK_SETUP";
    public const string WaterBal = "WATER_BAL";
    public const string RegAlign = "REG_ALIGN";
    public const string FirstProof = "FIRST_PROOF";
    public const string FullRun = "FULL_RUN";
    public const string SideChange = "SIDE_CHANGE";
    public const string Cleanup = "CLEANUP";
    public const string PrintCount = "PRINT_COUNT";

    // ── Process 9: Drying (DRY) ──
    public const string NatDry = "NAT_DRY";
    public const string IrDry = "IR_DRY";
    public const string UvCure = "UV_CURE";
    public const string AntiSet = "ANTI_SET";

    // ── Process 10: Post-Press (POST_PRESS) ──
    public const string Lamination = "LAMINATION";
    public const string SpotUv = "SPOT_UV";
    public const string Foiling = "FOILING";
    public const string Emboss = "EMBOSS";
    public const string Varnish = "VARNISH";
    public const string DieCut = "DIE_CUT";
    public const string Crease = "CREASE";
    public const string Punch = "PUNCH";

    // ── Process 11: Folding (FOLD) ──
    public const string Folding = "FOLDING";
    public const string Gather = "GATHER";
    public const string Collate = "COLLATE";
    public const string SeqVerify = "SEQ_VERIFY";

    // ── Process 12: Binding (BIND) ──
    public const string EdgeTrim = "EDGE_TRIM";
    public const string Glue = "GLUE";
    public const string CoverPaste = "COVER_PASTE";
    public const string SpineFormation = "SPINE";
    public const string BindQc = "BIND_QC";

    // ── Process 13: Trimming (TRIM) ──
    public const string Trim3K = "TRIM_3K";
    public const string SingleTrim = "SINGLE_TRIM";

    // ── Process 14: Quality Check (QC) ──
    public const string ColorQc = "COLOR_QC";
    public const string PrintQc = "PRINT_QC";
    public const string BindStrength = "BIND_STRENGTH";
    public const string SizeQc = "SIZE_QC";
    public const string FinalSample = "FINAL_SAMPLE";

    // ── Process 15: Packing (PACK) ──
    public const string Counting = "COUNTING";
    public const string Shrink = "SHRINK";
    public const string Boxing = "BOXING";
    public const string Label = "LABEL";

    // ── Process 16: Challan (CHALLAN) ──
    public const string Challan = "CHALLAN";
    public const string Invoice = "INVOICE";
    public const string PackList = "PACK_LIST";
    public const string GatePass = "GATE_PASS";

    // ── Process 17: Dispatch (DISPATCH) ──
    public const string VehAlloc = "VEH_ALLOC";
    public const string Loading = "LOADING";
    public const string Dispatch = "DISPATCH";
    public const string Pod = "POD";

    // ── Process 18: Billing (BILL) ──
    public const string BillPrep = "BILL_PREP";
    public const string AdvAdj = "ADV_ADJ";
    public const string FinalPay = "FINAL_PAY";
    public const string Receipt = "RECEIPT";

    // ── Process 19: Job Closure (JOB_CLOSE) ──
    public const string Archive = "ARCHIVE";
    public const string PlateClose = "PLATE_CLOSE";
    public const string Feedback = "FEEDBACK";
    public const string Costing = "COSTING";
    public const string JobClose = "JOB_CLOSE";

    // ── Accounts Receivable (ACC_RECV) ──
    public const string CreateSalesInvoice = "CREATE_SALES_INV";
    public const string PostSalesInvoice = "POST_SALES_INV";
    public const string SendSalesInvoice = "SEND_SALES_INV";
    public const string ReceivePayment = "RECEIVE_PAYMENT";
    public const string IssueCreditNote = "ISSUE_CREDIT_NOTE";
    public const string CreateProforma = "CREATE_PROFORMA";
    public const string SendProforma = "SEND_PROFORMA";
    public const string ArFollowUp = "AR_FOLLOW_UP";

    // ── Accounts Payable (ACC_PAY) ──
    public const string CreatePurchaseInvoice = "CREATE_PURCHASE_INV";
    public const string MakePayment = "MAKE_PAYMENT";
    public const string IssueDebitNote = "ISSUE_DEBIT_NOTE";
    public const string RecordExpense = "RECORD_EXPENSE";
    public const string ApproveExpense = "APPROVE_EXPENSE";
    public const string ApFollowUp = "AP_FOLLOW_UP";

    // ── Banking (BANKING) ──
    public const string BankReceipt = "BANK_RECEIPT";
    public const string BankPayment = "BANK_PAYMENT";
    public const string ContraVoucher = "CONTRA_VOUCHER";
    public const string BankReconciliation = "BANK_RECON";

    // ── General Ledger (GL_JOURNAL) ──
    public const string JournalEntry = "JOURNAL_ENTRY";
    public const string PostToGL = "POST_TO_GL";
    public const string ReversingEntry = "REVERSING_ENTRY";

    // ── Purchase (PURCHASE) ──
    public const string CreatePO = "CREATE_PO";
    public const string ApprovePO = "APPROVE_PO";
    public const string SendPO = "SEND_PO";
    public const string CreateGRN = "CREATE_GRN";
}
