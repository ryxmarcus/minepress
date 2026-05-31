-- ═══════════════════════════════════════════════════════════
-- txn_user_activity — Logs all user activities in the system
-- ═══════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS press_db.txn_user_activity (
    activity_id         bigserial       PRIMARY KEY,
    activity_type       varchar(50)     NOT NULL,
    description         text,
    reference_no        varchar(50),
    created_by          varchar(100)    NOT NULL DEFAULT 'system',
    created_at          timestamp       NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_ua_activity_type ON press_db.txn_user_activity (activity_type);
CREATE INDEX IF NOT EXISTS idx_ua_reference_no  ON press_db.txn_user_activity (reference_no);
CREATE INDEX IF NOT EXISTS idx_ua_created_at    ON press_db.txn_user_activity (created_at DESC);

COMMENT ON TABLE press_db.txn_user_activity IS 'Logs all user activities — estimation sends, WhatsApp shares, prints, logins, etc.';

-- ═══════════════════════════════════════════════════════════
-- txn_notification — Internal notifications for roles/users
-- ═══════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS press_db.txn_notification (
    notification_id     bigserial       PRIMARY KEY,
    notification_type   varchar(50)     NOT NULL,
    title               varchar(200)    NOT NULL,
    message             text,
    target_role         varchar(50),
    target_user_id      bigint,
    reference_no        varchar(50),
    is_read             boolean         NOT NULL DEFAULT false,
    read_at             timestamp,
    created_at          timestamp       NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_notif_type       ON press_db.txn_notification (notification_type);
CREATE INDEX IF NOT EXISTS idx_notif_role       ON press_db.txn_notification (target_role);
CREATE INDEX IF NOT EXISTS idx_notif_user       ON press_db.txn_notification (target_user_id);
CREATE INDEX IF NOT EXISTS idx_notif_ref        ON press_db.txn_notification (reference_no);
CREATE INDEX IF NOT EXISTS idx_notif_unread     ON press_db.txn_notification (is_read) WHERE is_read = false;
CREATE INDEX IF NOT EXISTS idx_notif_created    ON press_db.txn_notification (created_at DESC);

COMMENT ON TABLE press_db.txn_notification IS 'Internal notifications sent to roles (SALES, MANAGEMENT, ADMIN) or specific users. Used for estimation alerts, job updates, etc.';
