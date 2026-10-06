# Project Monitoring API

API สำหรับจัดการข้อมูลโครงการพร้อมรายงานสรุปผล เขียนด้วย .NET 10 + PostgreSQL รันทั้งระบบด้วย Docker คำสั่งเดียว

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

ในโฟลเดอร์ `scripts/` มี script สำหรับดูแลระบบ ใช้ผ่าน bash:

```bash
bash scripts/backup.sh          # สำรอง DB เป็นไฟล์ .sql.gz ลงโฟลเดอร์ backups/ (เก็บ 14 ไฟล์ล่าสุด)
bash scripts/healthcheck.sh     # เช็คสถานะ container + ยิง /health
bash scripts/resource-stats.sh  # ดู CPU/Memory ที่ stack ใช้
bash scripts/log-rotate.sh      # บีบอัดไฟล์ log ที่ใหญ่กว่า 1MB และลบของเก่ากว่า 7 วัน
```

ถ้าอยากให้ backup อัตโนมัติทุกวัน ตั้ง cron ได้ เช่น `0 2 * * * /path/to/scripts/backup.sh`
