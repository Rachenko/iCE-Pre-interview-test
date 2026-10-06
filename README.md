# Project Monitoring API

Backend API สำหรับจัดการและวิเคราะห์ข้อมูลโครงการ (Project Monitoring) — .NET 10 Web API (Clean Architecture) + PostgreSQL, รันทั้งระบบด้วย Docker Compose คำสั่งเดียว

## Tech Stack

- **Backend:** .NET 10 Web API, Clean Architecture (Domain / Application / Infrastructure / Api), Dapper + Npgsql
- **Database:** PostgreSQL 17 (schema auto-initialize ผ่าน `docker-entrypoint-initdb.d`)
- **Infra:** Docker & Docker Compose
- **Docs:** Swagger UI + Postman Collection

## Quick Start (One-command setup)

```bash
docker compose up --build -d
```

- API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger
- Health check: http://localhost:8080/health
- PostgreSQL: `localhost:5432` (db `monitoring`, user/pass `postgres`/`postgres`)

ครั้งแรกที่รัน Postgres จะรัน `db/init.sql` สร้างตาราง + seed ข้อมูลอัตโนมัติ

## โครงสร้างโปรเจกต์

```
src/
  Domain/          # Entities (User, Project, Task, SystemLog) — ไม่มี dependency ภายนอก
  Application/     # DTOs + Repository interfaces (ports)
  Infrastructure/  # Dapper/Npgsql repositories (adapters) + DI wiring
  Api/             # Controllers, Program.cs (composition root)
db/init.sql        # Schema + indexes + seed data (init อัตโนมัติ)
scripts/           # backup / healthcheck / resource-stats / log-rotate (.sh)
postman/           # Postman collection สำหรับยิง API ทุก endpoint
Dockerfile         # Multi-stage build (sdk → aspnet runtime)
docker-compose.yml # api + db + volumes + healthchecks
```

## Database Design

| Table | จุดประสงค์ | ความสัมพันธ์ |
|---|---|---|
| `users` | ผู้ใช้ + แผนก (department) | owner ของ project, assignee ของ task |
| `projects` | โครงการ | `owner_id → users.id` (ON DELETE RESTRICT) |
| `tasks` | งานย่อยในโครงการ | `project_id → projects.id` (CASCADE), `assignee_id → users.id` (SET NULL) |
| `system_logs` | log ของระบบ | ไม่มี FK (append-only, JSONB metadata) |

Indexes: `projects(owner_id,status)`, `tasks(project_id,assignee_id,status)`, `system_logs(level, created_at)` — เลือกตาม query ที่ API ใช้จริง

## API Endpoints

| Method | Path | คำอธิบาย |
|---|---|---|
| GET/POST/PUT/DELETE | `/api/users` `/api/users/{id}` | CRUD ผู้ใช้ |
| GET/POST/PUT/DELETE | `/api/projects` `/api/projects/{id}` | CRUD โครงการ |
| GET/POST/PUT/DELETE | `/api/tasks` `/api/tasks/{id}` | CRUD งาน (filter: `?projectId=&status=`) |
| GET | `/api/reports/department-progress` | % ความคืบหน้าแยกตามแผนก (JOIN + aggregate) |
| GET | `/api/reports/project-summary` | สรุปทุกโครงการ + วันครบกำหนดที่ใกล้สุด |
| GET | `/api/reports/*.csv` | export รายงานเดียวกันเป็น CSV |
| GET | `/health` | health check (เช็ค DB ด้วย) |

ตัวอย่างรายงาน department-progress ใช้ Advanced SQL:

```sql
SELECT u.department,
       COUNT(t.id)                                              AS total_tasks,
       COUNT(t.id) FILTER (WHERE t.status = 'done')             AS done_tasks,
       ROUND(100.0 * COUNT(t.id) FILTER (WHERE t.status='done')
             / NULLIF(COUNT(t.id),0), 2)                        AS progress_percent
FROM users u
JOIN tasks t ON t.assignee_id = u.id
GROUP BY u.department;
```

ทดลองเร็ว ๆ:

```bash
curl localhost:8080/api/reports/department-progress
curl -OJ localhost:8080/api/reports/project-summary.csv
curl -X POST localhost:8080/api/tasks -H 'Content-Type: application/json' \
     -d '{"projectId":1,"title":"New task","priority":"high","assigneeId":1}'
```

## Operations Scripts (Day 3)

| Script | สิ่งที่ทำ |
|---|---|
| `scripts/backup.sh` | `pg_dump` ผ่าน `docker exec`, บีบอัด `.sql.gz` ลง `./backups`, เก็บย้อนหลัง 14 ไฟล์ |
| `scripts/healthcheck.sh` | แสดงสถานะ container + ยิง `/health` ของ API |
| `scripts/resource-stats.sh` | `docker stats` สรุป CPU/Mem/NetIO ของ stack |
| `scripts/log-rotate.sh` | rotate ไฟล์ `.log` > 1MB เป็น `.log.gz` และลบ archive เก่ากว่า 7 วัน |

```bash
bash scripts/backup.sh
bash scripts/healthcheck.sh
bash scripts/resource-stats.sh
bash scripts/log-rotate.sh
```

ตั้ง cron ให้ backup อัตโนมัติ เช่น `0 2 * * * /path/to/scripts/backup.sh`

## คำอธิบายสำหรับสัมภาษณ์ (จุดที่ควรพูดได้)

- **Clean Architecture:** Domain ไม่รู้จักใคร, Application นิยาม interface, Infrastructure implement ด้วย Dapper, Api เป็น composition root — เปลี่ยน ORM ได้โดยไม่แก้ business logic
- **Auto schema init:** mount `db/init.sql` เข้า `/docker-entrypoint-initdb.d/` ของ postgres — รันเฉพาะครั้งแรกที่ volume ว่าง, script เป็น idempotent (`IF NOT EXISTS`, `ON CONFLICT DO NOTHING`)
- **Health dependency:** `depends_on: service_healthy` ทำให้ API รอ DB พร้อมจริง ไม่ใช่แค่ container ติด
- **Multi-stage Dockerfile:** build ด้วย `sdk:10.0` แล้ว copy เฉพาะ publish output ไป `aspnet:10.0` — image เล็ก ไม่มี SDK หลงเหลือ
- **Indexing:** index ตาม query pattern จริง (FK lookups + status filter + log by level/time) ไม่ใส่ index มั่วเพราะทำให้ write ช้า
- **CSV export:** สร้าง string แล้วส่ง `File()` กับ content type `text/csv` — เบา ไม่ต้อง library เพิ่ม
