# Project Monitoring API

API สำหรับจัดการข้อมูลโครงการ พร้อมรายงานสรุปผล เขียนด้วย .NET 10 (Clean Architecture) ใช้ PostgreSQL เป็นฐานข้อมูล รันทั้งระบบด้วย Docker คำสั่งเดียว

## สิ่งที่ต้องมีก่อนรัน

- ติดตั้ง [Docker Desktop](https://www.docker.com/products/docker-desktop/) (รวม Docker Compose มาให้แล้ว)
- แค่นี้พอ ไม่ต้องลง .NET หรือ PostgreSQL เอง เพราะทุกอย่างรันใน container

## วิธีติดตั้งและรัน

```bash
# 1. clone โปรเจกต์
git clone https://github.com/Rachenko/iCE-Pre-interview-test.git
cd iCE-Pre-interview-test

# 2. รันคำสั่งเดียว จบ
docker compose up --build -d
```

คำสั่งนี้จะ build API และ start PostgreSQL ให้อัตโนมัติ รอบแรกอาจใช้เวลาสัก 1–2 นาที

รันเสร็จแล้วเปิดดูได้เลย:

- Swagger UI (หน้าทดลองยิง API): http://localhost:8080/swagger
- Health check: http://localhost:8080/health
- PostgreSQL: `localhost:5432` — ชื่อ db `monitoring`, user `postgres`, รหัส `postgres`

ครั้งแรกที่รัน ฐานข้อมูลจะสร้างตารางและใส่ข้อมูลตัวอย่างให้อัตโนมัติจากไฟล์ `db/init.sql` — ไม่ต้องทำอะไรเพิ่ม

## คำสั่งที่ใช้บ่อย

```bash
docker compose logs -f api      # ดู log ของ API
docker compose ps               # เช็คว่า container ทำงานอยู่ไหม
docker compose down             # หยุดทั้งระบบ
docker compose down -v          # หยุดและลบข้อมูลใน DB ทิ้ง (รีเซ็ตใหม่หมด)
docker compose up --build -d    # แก้โค้ดแล้วอยากรันใหม่
```

## ลองเรียก API

```bash
# ดึงรายชื่อผู้ใช้
curl localhost:8080/api/users

# รายงาน % ความคืบหน้าแยกตามแผนก
curl localhost:8080/api/reports/department-progress

# ดาวน์โหลดรายงานเป็น CSV
curl -OJ localhost:8080/api/reports/project-summary.csv

# สร้างงานใหม่
curl -X POST localhost:8080/api/tasks -H 'Content-Type: application/json' \
     -d '{"projectId":1,"title":"New task","priority":"high","assigneeId":1}'
```

ถ้าไม่ถนัด curl ในโฟลเดอร์ `postman/` มี Postman collection ครบทุก endpoint ให้ import ไปใช้ได้เลย

## API มีอะไรบ้าง

| Method | Path | คำอธิบาย |
|---|---|---|
| GET/POST/PUT/DELETE | `/api/users` `/api/users/{id}` | จัดการผู้ใช้ |
| GET/POST/PUT/DELETE | `/api/projects` `/api/projects/{id}` | จัดการโครงการ |
| GET/POST/PUT/DELETE | `/api/tasks` `/api/tasks/{id}` | จัดการงาน (filter ได้ด้วย `?projectId=&status=`) |
| GET | `/api/reports/department-progress` | % ความคืบหน้าแยกตามแผนก |
| GET | `/api/reports/project-summary` | สรุปทุกโครงการ + วันครบกำหนดที่ใกล้สุด |
| GET | `/api/reports/*.csv` | รายงานเดียวกันแบบ export CSV |
| GET | `/health` | เช็คสุขภาพ API + DB |

## โครงสร้างโปรเจกต์

```
src/
  Domain/          # Entities (User, Project, Task, SystemLog)
  Application/     # DTOs + interface ของ repository
  Infrastructure/  # implement repository ด้วย Dapper/Npgsql
  Api/             # Controllers + Program.cs
db/init.sql        # สร้างตาราง + ข้อมูลตัวอย่าง (รันครั้งแรกอัตโนมัติ)
scripts/           # script ช่วยงาน: backup, healthcheck, stats, log-rotate
postman/           # Postman collection
Dockerfile         # build API เป็น image
docker-compose.yml # รัน api + db พร้อมกัน
```

## ฐานข้อมูล

| Table | เก็บอะไร | เชื่อมกับใคร |
|---|---|---|
| `users` | ผู้ใช้ + แผนก | เจ้าของ project / คนรับงาน |
| `projects` | โครงการ | `owner_id → users.id` |
| `tasks` | งานย่อยในโครงการ | `project_id → projects.id`, `assignee_id → users.id` |
| `system_logs` | log ของระบบ | ไม่มี FK (เก็บแบบ append-only) |

## Scripts เสริม

ในโฟลเดอร์ `scripts/` มี script สำหรับดูแลระบบ ใช้ผ่าน bash:

```bash
bash scripts/backup.sh          # สำรอง DB เป็นไฟล์ .sql.gz ลงโฟลเดอร์ backups/ (เก็บ 14 ไฟล์ล่าสุด)
bash scripts/healthcheck.sh     # เช็คสถานะ container + ยิง /health
bash scripts/resource-stats.sh  # ดู CPU/Memory ที่ stack ใช้
bash scripts/log-rotate.sh      # บีบอัดไฟล์ log ที่ใหญ่กว่า 1MB และลบของเก่ากว่า 7 วัน
```

ถ้าอยากให้ backup อัตโนมัติทุกวัน ตั้ง cron ได้ เช่น `0 2 * * * /path/to/scripts/backup.sh`
