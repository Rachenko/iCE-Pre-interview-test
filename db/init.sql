-- Auto-initialized by the postgres container on first `docker compose up`
-- (mounted into /docker-entrypoint-initdb.d/)

CREATE TABLE IF NOT EXISTS users (
    id          SERIAL PRIMARY KEY,
    username    VARCHAR(50)  NOT NULL UNIQUE,
    email       VARCHAR(255) NOT NULL UNIQUE,
    full_name   VARCHAR(150) NOT NULL,
    department  VARCHAR(100) NOT NULL,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS projects (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(200) NOT NULL,
    description TEXT,
    status      VARCHAR(20)  NOT NULL DEFAULT 'active'
                CHECK (status IN ('active','on_hold','completed','cancelled')),
    owner_id    INTEGER      NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    start_date  DATE         NOT NULL,
    end_date    DATE,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS tasks (
    id           SERIAL PRIMARY KEY,
    project_id   INTEGER      NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    title        VARCHAR(200) NOT NULL,
    description  TEXT,
    status       VARCHAR(20)  NOT NULL DEFAULT 'todo'
                 CHECK (status IN ('todo','in_progress','done','blocked')),
    priority     VARCHAR(10)  NOT NULL DEFAULT 'medium'
                 CHECK (priority IN ('low','medium','high','critical')),
    assignee_id  INTEGER      REFERENCES users(id) ON DELETE SET NULL,
    due_date     DATE,
    completed_at TIMESTAMPTZ,
    created_at   TIMESTAMPTZ  NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS system_logs (
    id          BIGSERIAL PRIMARY KEY,
    level       VARCHAR(10)  NOT NULL DEFAULT 'info'
                CHECK (level IN ('debug','info','warn','error')),
    source      VARCHAR(100) NOT NULL,
    message     TEXT         NOT NULL,
    metadata    JSONB,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT now()
);

-- Indexes for the queries the API actually runs
CREATE INDEX IF NOT EXISTS idx_projects_owner     ON projects(owner_id);
CREATE INDEX IF NOT EXISTS idx_projects_status    ON projects(status);
CREATE INDEX IF NOT EXISTS idx_tasks_project      ON tasks(project_id);
CREATE INDEX IF NOT EXISTS idx_tasks_assignee     ON tasks(assignee_id);
CREATE INDEX IF NOT EXISTS idx_tasks_status       ON tasks(status);
CREATE INDEX IF NOT EXISTS idx_logs_level_time    ON system_logs(level, created_at DESC);

-- ---- Seed data (idempotent) ----
INSERT INTO users (username, email, full_name, department) VALUES
    ('anong',  'anong@example.com',  'Anong Srisuk',   'Engineering'),
    ('bosco',  'bosco@example.com',  'Bosco Chen',     'Engineering'),
    ('chana',  'chana@example.com',  'Chana Ploy',     'Data'),
    ('dao',    'dao@example.com',    'Dao Meekun',     'Data'),
    ('ek',     'ek@example.com',     'Ek Wattana',     'Operations')
ON CONFLICT (username) DO NOTHING;

INSERT INTO projects (name, description, status, owner_id, start_date, end_date)
SELECT v.name, v.description, v.status, u.id, v.start_date, v.end_date
FROM (VALUES
    ('Customer Portal Revamp',  'Rebuild the customer-facing portal',       'active',    'anong', '2026-01-10'::date, '2026-12-31'::date),
    ('Data Warehouse Phase 2',  'Extend DWH to cover sales data',           'active',    'chana', '2026-02-01'::date, NULL),
    ('Legacy Billing Sunset',   'Decommission legacy billing system',       'on_hold',   'ek',    '2025-11-01'::date, '2026-06-30'::date),
    ('Mobile App v3',           'New mobile application release',           'completed', 'bosco', '2025-06-01'::date, '2026-01-15'::date)
) AS v(name, description, status, owner, start_date, end_date)
JOIN users u ON u.username = v.owner
WHERE NOT EXISTS (SELECT 1 FROM projects p WHERE p.name = v.name);

INSERT INTO tasks (project_id, title, status, priority, assignee_id, due_date, completed_at)
SELECT p.id, v.title, v.status, v.priority, u.id, v.due, CASE WHEN v.status='done' THEN now() - interval '2 days' ELSE NULL END
FROM (VALUES
    ('Customer Portal Revamp', 'Design new landing page',     'done',        'high',     'anong', '2026-03-01'::date),
    ('Customer Portal Revamp', 'Implement SSO login',         'in_progress', 'critical', 'bosco', '2026-10-15'::date),
    ('Customer Portal Revamp', 'Payment gateway integration', 'todo',        'high',     'anong', '2026-11-01'::date),
    ('Customer Portal Revamp', 'Load testing',                'todo',        'medium',   NULL,    '2026-12-01'::date),
    ('Data Warehouse Phase 2', 'Sales schema design',         'done',        'high',     'chana', '2026-04-01'::date),
    ('Data Warehouse Phase 2', 'ETL pipeline for orders',     'in_progress', 'high',     'dao',   '2026-10-20'::date),
    ('Data Warehouse Phase 2', 'Data quality checks',         'blocked',     'medium',   'dao',   '2026-11-10'::date),
    ('Legacy Billing Sunset',  'Export historical invoices',  'done',        'medium',   'ek',    '2026-01-31'::date),
    ('Legacy Billing Sunset',  'Migrate customers to new DB', 'in_progress', 'high',     'ek',    '2026-05-01'::date),
    ('Mobile App v3',          'Publish v3.0 to stores',      'done',        'high',     'bosco', '2026-01-10'::date)
) AS v(project, title, status, priority, assignee, due)
JOIN projects p ON p.name = v.project
LEFT JOIN users u ON u.username = v.assignee
WHERE NOT EXISTS (SELECT 1 FROM tasks t WHERE t.project_id = p.id AND t.title = v.title);

INSERT INTO system_logs (level, source, message, metadata)
SELECT 'info', 'init', 'Database seeded', jsonb_build_object('at', now())
WHERE NOT EXISTS (SELECT 1 FROM system_logs);
