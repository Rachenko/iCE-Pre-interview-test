# Project Monitoring API

API สำหรับจัดการข้อมูลโครงการพร้อมรายงานสรุปผล เขียนด้วย .NET 10 + PostgreSQL รันทั้งระบบด้วย Docker คำสั่งเดียว

## สิ่งที่ต้องมีก่อนรัน

- ติดตั้ง [Docker Desktop](https://www.docker.com/products/docker-desktop/) (รวม Docker Compose มาให้แล้ว)
- แค่นี้พอ ไม่ต้องลง .NET หรือ PostgreSQL เอง เพราะทุกอย่างรันใน container

## Quick Start (รันทั้งระบบด้วยคำสั่งเดียว)

```bash
git clone https://github.com/Rachenko/iCE-Pre-interview-test.git
cd iCE-Pre-interview-test
docker compose up --build -d
```

`docker compose up --build -d` คำสั่งเดียวรันทั้งหมด: build API, start PostgreSQL, สร้างตารางและ seed ข้อมูลตัวอย่าง — ไม่ต้องสั่งอะไรเพิ่ม รอบแรกอาจใช้เวลาสัก 1–2 นาที

รันเสร็จแล้วเปิดดูได้เลย:

- Swagger UI (หน้าทดลองยิง API): http://localhost:8080/swagger
- Health check: http://localhost:8080/health
- PostgreSQL: `localhost:5432` — ชื่อ db `monitoring`, user `postgres`, รหัส `postgres`

ถ้าไม่ถนัดยิง API ผ่าน command line ในโฟลเดอร์ `postman/` มี Postman collection ครบทุก endpoint ให้ import ไปใช้ได้เลย

## คำสั่งที่ใช้บ่อย

```bash
docker compose logs -f api      # ดู log ของ API
docker compose ps               # เช็คว่า container ทำงานอยู่ไหม
docker compose down             # หยุดทั้งระบบ
docker compose down -v          # หยุดและลบข้อมูลใน DB ทิ้ง (รีเซ็ตใหม่หมด)
docker compose up --build -d    # แก้โค้ดแล้วอยากรันใหม่
```

## Scripts เสริม

ในโฟลเดอร์ `scripts/` มี script สำหรับดูแลระบบ มี 2 เวอร์ชัน: `.sh` สำหรับ Mac/Linux และ `.ps1` สำหรับ PowerShell บน Windows:

```bash
bash scripts/backup.sh          # สำรอง DB เป็นไฟล์ .sql.gz ลงโฟลเดอร์ backups/ (เก็บ 14 ไฟล์ล่าสุด)
bash scripts/healthcheck.sh     # เช็คสถานะ container + ยิง /health
bash scripts/resource-stats.sh  # ดู CPU/Memory ที่ stack ใช้
bash scripts/log-rotate.sh      # บีบอัดไฟล์ log ที่ใหญ่กว่า 1MB และลบของเก่ากว่า 7 วัน
```

บน Windows ใช้ชื่อเดียวกันนามสกุล `.ps1` เช่น `./scripts/healthcheck.ps1`
ถ้า PowerShell บล็อกไม่ให้รัน script ให้เรียกผ่าน:

```powershell
powershell -ExecutionPolicy Bypass -File ./scripts/healthcheck.ps1
```

ถ้าอยากให้ backup อัตโนมัติทุกวัน ตั้ง cron ได้ เช่น `0 2 * * * /path/to/scripts/backup.sh`
