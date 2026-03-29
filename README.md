# Construction Payment Request Management System (CPMS)

Web app nội bộ quản lý hồ sơ thanh toán và duyệt hóa đơn cho công ty xây dựng, kiến trúc monorepo:
- Backend: .NET 8 Web API + Entity Framework Core Code First + SQLite (đổi SQL Server/MySQL/TiDB dễ dàng)
- Frontend: React + Vite + TypeScript + Ant Design

## 1. Cấu trúc thư mục
```text
.
├─ backend/
│  ├─ ConstructionPayment.sln
│  └─ src/
│     ├─ ConstructionPayment.Domain/
│     ├─ ConstructionPayment.Application/
│     ├─ ConstructionPayment.Infrastructure/
│     └─ ConstructionPayment.Api/
├─ frontend/
│  ├─ src/
│  │  ├─ pages/
│  │  ├─ components/
│  │  ├─ services/
│  │  ├─ hooks/
│  │  ├─ layouts/
│  │  └─ types/
│  └─ .env.example
└─ .vscode/
   ├─ tasks.json
   └─ launch.json
```

## 2. Yêu cầu môi trường
- .NET SDK 8.0+
- Node.js 20+ và npm
- VS Code (khuyên dùng extensions):
  - C# / C# Dev Kit
  - ES7+ React/TypeScript snippets

Link cài đặt:
- .NET 8 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- Node.js LTS: https://nodejs.org/

### 2.1 Cài .NET local theo project (không cần admin)
Nếu máy chưa có `dotnet` global, dùng wrapper để tự cài vào `backend/.tools/dotnet`:

PowerShell (Windows):
```powershell
cd backend
./scripts/dotnetw.ps1 --info
```

Bash (Linux/macOS):
```bash
cd backend
./scripts/dotnetw.sh --info
```

## 3. Cài đặt nhanh
### 3.1 Backend
```bash
cd backend
./scripts/dotnetw.sh restore ConstructionPayment.sln
./scripts/dotnetw.sh build ConstructionPayment.sln
```

Trước khi chạy với TiDB, cập nhật mật khẩu thật trong:
`backend/src/ConstructionPayment.Api/appsettings.Development.json`
`ConnectionStrings:MySqlConnection` (thay `<PASSWORD>`).

### 3.2 Frontend
```bash
cd frontend
npm install
```

## 4. Chạy dự án
### Cách 1: Chạy toàn hệ thống bằng `Ctrl + Shift + B` trong VS Code
1. Mở root workspace này trong VS Code.
2. Nhấn `Ctrl + Shift + B`.
3. Task mặc định `dev: fullstack` sẽ chạy song song:
   - Backend: chạy với `ASPNETCORE_ENVIRONMENT=Development`, `dotnet restore` rồi `dotnet watch --project src/ConstructionPayment.Api/ConstructionPayment.Api.csproj run`
   - Frontend: `npm install` rồi `npm run dev`
   - Tự mở trình duyệt mặc định tại `http://localhost:5173`
   - Tự kiểm tra nhanh `http://localhost:5000/health/db` để xác nhận kết nối DB

### Cách 2: Chạy thủ công
Terminal 1:
```bash
cd backend
dotnet watch --project src/ConstructionPayment.Api/ConstructionPayment.Api.csproj run
```

Terminal 2:
```bash
cd frontend
npm run dev
```

## 5. URL mặc định
- Frontend: `http://localhost:5173`
- Backend API: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
- Health check: `http://localhost:5000/health`
- Health DB: `http://localhost:5000/health/db`

## 6. Database, Migration, Seed
Mặc định môi trường Development lấy cấu hình từ `appsettings.Development.json` (SQLite local hoặc MySQL/TiDB tùy bạn đặt `DatabaseProvider`).

### 6.1 Tự động migrate + seed khi chạy Development
Khi backend khởi động ở môi trường `Development`, app sẽ:
1. Tự apply migration theo Code First cho SQLite / SQL Server / MySQL(TiDB)
2. Nếu migrate MySQL/TiDB lỗi trong Development, app sẽ thử fallback `EnsureCreated()`
3. Kiểm tra kết nối DB và kiểm tra truy cập bảng `Users` trước khi nhận request
4. Tự seed dữ liệu mẫu
5. Nếu seed lỗi do DB SQLite dev cũ/sai schema, app sẽ thử reset DB dev và seed lại 1 lần
6. Nếu cấu hình `MySql` nhưng máy thiếu provider/runtime MySQL, app sẽ tự fallback sang SQLite (Development) để vẫn đăng nhập/test được

### 6.2 Kiểm tra đã kết nối DB hay chưa
Sau khi chạy backend, gọi:
```bash
curl http://localhost:5000/health/db
```

Kết quả mong đợi:
- `status = "ok"`: kết nối DB thành công và schema dùng được
- `configuredProvider`: provider bạn cấu hình trong appsettings
- `provider`: provider thực tế EF Core đang dùng (nếu fallback SQLite sẽ thấy khác `configuredProvider`)
- `userCount >= 5` sau khi seed dữ liệu mẫu
### 6.3 Migration thủ công (nếu cần)
```bash
cd backend
./scripts/dotnetw.sh tool restore
./scripts/dotnetw.sh ef migrations add <MigrationName> \
  --project src/ConstructionPayment.Infrastructure \
  --startup-project src/ConstructionPayment.Api

./scripts/dotnetw.sh ef database update \
  --project src/ConstructionPayment.Infrastructure \
  --startup-project src/ConstructionPayment.Api
```

### 6.4 Bootstrap Code First + Seed (1 lệnh)
Chế độ bootstrap sẽ chạy `Migrate + SeedDemoData` rồi thoát, không cần mở web server.

PowerShell (Windows):
```powershell
cd backend
./scripts/bootstrap-tidb.ps1 "mysql://<USERNAME>:<PASSWORD>@<HOST>:4000/<DB>"
```

Bash (Linux/macOS):
```bash
cd backend
./scripts/bootstrap-tidb.sh "mysql://<USERNAME>:<PASSWORD>@<HOST>:4000/<DB>"
```

Nếu connection string đã nằm sẵn trong `appsettings.Development.json`, bạn có thể chạy:
```bash
cd backend
./scripts/dotnetw.sh run --project src/ConstructionPayment.Api/ConstructionPayment.Api.csproj -- --bootstrap-db
```

### 6.5 Chạy Development với TiDB (1 lệnh, chống lỗi env ghi đè)
Script này sẽ:
1. Ép đúng `ConnectionStrings__MySqlConnection`
2. Chạy bootstrap `Migrate + Seed`
3. Chạy `dotnet watch`

PowerShell (Windows):
```powershell
cd backend
./scripts/run-dev-tidb.ps1
```

Bash (Linux/macOS):
```bash
cd backend
./scripts/run-dev-tidb.sh
```

## 7. Tài khoản mẫu (seed)
- `admin` / `admin123`
- `employee` / `employee123`
- `manager` / `manager123`
- `director` / `director123`
- `accountant` / `accountant123`
- (thêm) `viewer` / `viewer123`

Bạn có thể đổi mật khẩu seed trong file:
`backend/src/ConstructionPayment.Infrastructure/Seed/DbSeeder.cs`

## 8. Cấu hình frontend env
File mẫu:
`frontend/.env.example`

Nội dung:
```env
VITE_API_BASE_URL=/api
VITE_BACKEND_ORIGIN=http://localhost:5000
```

Ghi chú:
- Development khuyến nghị dùng `VITE_API_BASE_URL=/api` để đi qua Vite proxy (tránh lỗi CORS/network chéo port).
- `VITE_BACKEND_ORIGIN` là địa chỉ backend thật mà proxy sẽ chuyển tiếp tới.
- Production có thể đặt `VITE_API_BASE_URL` thành URL API tuyệt đối.

## 9. Đổi SQLite sang SQL Server / MySQL / TiDB
Chọn provider qua `DatabaseProvider` theo môi trường bạn muốn chạy.

Sửa trong:
`backend/src/ConstructionPayment.Api/appsettings.json` hoặc `appsettings.Development.json`

### 9.1 SQL Server
```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost;Database=ConstructionPaymentDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Sau đó chạy lại migrate/update database.

### 9.2 TiDB Cloud (MySQL protocol)
App đã hỗ trợ `DatabaseProvider = "MySql"` với Pomelo EF Core.

Bạn có thể dùng trực tiếp URL dạng `mysql://...` như sau:
```json
{
  "DatabaseProvider": "MySql",
  "Database": {
    "SeedDemoData": true
  },
  "ConnectionStrings": {
    "MySqlConnection": "mysql://2MGq6vKV1Xvchpy.root:<PASSWORD>@gateway01.ap-southeast-1.prod.aws.tidbcloud.com:4000/test"
  }
}
```

Lưu ý:
- Với MySQL/TiDB, app ưu tiên `Migrate()` theo Code First, sau đó seed dữ liệu mẫu.
- Chỉ khi `Migrate()` lỗi trong môi trường Development, app mới fallback `EnsureCreated()` để hỗ trợ local test nhanh.
- Không nên hard-code mật khẩu trong source. Nên dùng env var:
  - `ConnectionStrings__MySqlConnection`
  - `DatabaseProvider=MySql`

Ví dụ PowerShell:
```powershell
$env:DatabaseProvider="MySql"
$env:ConnectionStrings__MySqlConnection="mysql://2MGq6vKV1Xvchpy.root:<PASSWORD>@gateway01.ap-southeast-1.prod.aws.tidbcloud.com:4000/test"
```

## 10. Tính năng chính đã có
- Authentication/Authorization bằng JWT + phân quyền role
- CRUD: Nhà cung cấp, Dự án, Hợp đồng, Hồ sơ thanh toán
- Cấu hình ma trận duyệt (Approval Matrix) theo phòng ban/dự án/ngưỡng tiền
- Workflow duyệt nhiều cấp: Submit / Approve / Reject / ReturnForEdit
- Kế toán xác nhận thanh toán
- Upload/download attachment (local storage) + metadata DB
- Dashboard tổng quan
- Audit log truy vết thao tác quan trọng
- Seed dữ liệu mẫu đầy đủ (users, suppliers, projects, contracts, payment requests, approval matrix)

## 11. Danh sách màn hình frontend
- Login
- Dashboard
- Danh sách + form Nhà cung cấp
- Danh sách + form Dự án
- Danh sách + form Hợp đồng
- Danh sách + form Hồ sơ thanh toán
- Chi tiết Hồ sơ thanh toán
- Duyệt hồ sơ
- Kế toán xác nhận thanh toán
- Quản trị người dùng và phân quyền
- Cấu hình Approval Matrix
- Lịch sử Audit Log

## 12. Ghi chú triển khai
- Backend CORS đã mở cho frontend local (`http://localhost:5173`).
- Frontend Development mặc định dùng `VITE_API_BASE_URL=/api` qua Vite proxy để giảm lỗi CORS/network.
- File upload demo lưu theo cấu hình `Storage:UploadPath` (mặc định `uploads`, tính theo thư mục chạy backend), thiết kế service để dễ thay bằng cloud storage.

## 13. Troubleshooting nhanh
- Lỗi `'vite' is not recognized`:
  - Chạy `cd frontend && npm install` rồi chạy lại `Ctrl + Shift + B`.
- Lỗi seed/migration khi startup backend:
  - App đã có cơ chế tự thử reset DB SQLite dev.
  - Nếu vẫn lỗi, xóa file DB local rồi chạy lại:
    - `backend/app.dev.db` hoặc `backend/app.db`
- Lỗi login `[DEV] SqliteException: no such table: Users`:
  - Backend chưa migrate vào đúng file DB runtime.
  - Đã fix code để tự migrate + seed cho SQLite local kể cả khi env lệch.
  - Nếu gặp trạng thái lệch migration history (ví dụ có `__EFMigrationsHistory` nhưng thiếu bảng `Users`), app Development sẽ tự reset schema và migrate lại.
  - Nếu vẫn gặp, dừng app và xóa DB local rồi chạy lại `Ctrl + Shift + B`.
- Dùng TiDB/MySQL nhưng chưa có bảng:
  - Đặt `DatabaseProvider = "MySql"`.
  - Kiểm tra `ConnectionStrings:MySqlConnection` đúng host/user/password/db.
  - Khởi động lại backend để app tự `Migrate + SeedDemoData`.
  - Gọi `http://localhost:5000/health/db` để xác nhận `status = ok`.
- Lỗi `Option 'connectionstrings__mysqlconnection' not supported` khi deploy:
  - Bạn đang nhập sai **Value** của biến môi trường.
  - Trên Render:
    - `Key`: `ConnectionStrings__MySqlConnection`
    - `Value`: chỉ dán chuỗi kết nối thật (không dán `ConnectionStrings__MySqlConnection=...`, không dán `connectionstrings__mysqlconnection`).
  - App cũng hỗ trợ đọc từ `DATABASE_URL` / `MYSQL_URL` / `TIDB_URL` nếu bạn muốn dùng alias env.
- Lỗi `[DEV] FileNotFoundException: Pomelo.EntityFrameworkCore.MySql...`:
  - Đã thêm cơ chế fallback SQLite trong Development để không chặn đăng nhập.
  - Chạy lại `Ctrl + Shift + B` (task đã thêm bước `backend: build` trước `watch`).
  - Nếu cần chạy đúng TiDB, chạy `dotnet restore` lại solution để tải đủ package NuGet.
- Backend báo lỗi `MySQL connection string still contains <PASSWORD>`:
  - Mở `backend/src/ConstructionPayment.Api/appsettings.Development.json`
  - Thay `<PASSWORD>` bằng mật khẩu thật của TiDB Cloud.
- Login báo `Network Error`:
  - Kiểm tra backend có chạy: `http://localhost:5000/health`.
  - Kiểm tra DB: `http://localhost:5000/health/db`.
  - Đảm bảo frontend dùng env mới (`VITE_API_BASE_URL=/api`, `VITE_BACKEND_ORIGIN=http://localhost:5000`) rồi khởi động lại `npm run dev`.
- Không đăng nhập được bằng tài khoản seed:
  - Dừng app, xóa `backend/app.dev.db` (hoặc `backend/app.db`) rồi chạy lại `Ctrl + Shift + B`.
  - Seed user đã được thiết kế dạng upsert trong Development để luôn có:
    - `admin/admin123`, `employee/employee123`, `manager/manager123`, `director/director123`, `accountant/accountant123`.

## 14. Deploy lên Render (1 link duy nhất + TiDB)
Repo đã có sẵn:
- `backend/Dockerfile`: build frontend React và nhúng vào `wwwroot` của API .NET
- `render.yaml`: Blueprint chỉ còn 1 service `cpms-api`

### 14.1 Chuẩn bị TiDB connection string (ADO.NET format)
Không dùng `mysql://...` khi nhập vào Render secret, dùng dạng:
```text
Server=<host>;Port=4000;Database=<db>;User Id=<user>;Password=<password>;SslMode=Required;AllowPublicKeyRetrieval=True;ConnectionTimeout=30;DefaultCommandTimeout=60;
```

### 14.2 Tạo Blueprint trên Render
1. Push code mới nhất lên GitHub/GitLab.
2. Trong Render, chọn `New +` -> `Blueprint`.
3. Chọn repo chứa dự án này, Render sẽ đọc `render.yaml`.
4. Deploy service duy nhất `cpms-api` (Docker web service).

### 14.3 Set biến môi trường bắt buộc
Trong service `cpms-api`, set:
- `ConnectionStrings__MySqlConnection` = connection string TiDB thật (secret)
  - hoặc dùng alias: `DATABASE_URL` / `MYSQL_URL` / `TIDB_URL`

Các biến còn lại đã được khai báo sẵn trong `render.yaml`.

Lưu ý quan trọng:
- Ở Render, `Key` là `ConnectionStrings__MySqlConnection`.
- `Value` chỉ dán phần connection string, **không** dán theo dạng `ConnectionStrings__MySqlConnection=...`.

### 14.4 Nếu bạn đổi tên service
Nếu không dùng tên `cpms-api`, cập nhật env `Cors__AllowedOrigins__0` cho khớp domain thực tế.

### 14.5 Verify sau deploy
Sau khi deploy `Live`, kiểm tra:
- App (frontend + backend chung 1 domain): `https://<api-service>.onrender.com/login`
- API health: `https://<api-service>.onrender.com/health`
- API DB health: `https://<api-service>.onrender.com/health/db`

Tài khoản seed mặc định:
- `admin / admin123`
- `employee / employee123`
- `manager / manager123`
- `director / director123`
- `accountant / accountant123`
- `viewer / viewer123`
