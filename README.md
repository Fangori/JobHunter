# JobHunter

Nền tảng tuyển dụng full-stack: 3 vai trò Ứng viên / Nhà tuyển dụng / Quản trị viên, lọc & gợi ý ứng viên theo CV, quy trình đăng - duyệt tin tuyển dụng, và hệ thống gói dịch vụ trả phí.

## Tech stack

- **Frontend**: React 19 + Vite
- **Backend**: ASP.NET Core 10 Web API (EF Core, Database-First)
- **CSDL**: SQL Server 2022 (chạy trong Docker)
- **Lưu file** (CV, ảnh đại diện, logo): Cloudinary
- **Xác thực**: JWT

## Yêu cầu hệ thống

- [Docker](https://www.docker.com/) (khuyến nghị — dùng cho SQL Server, hoặc chạy toàn bộ dự án)
- [.NET 10 SDK](https://dotnet.microsoft.com/) (nếu chạy backend không qua Docker)
- [Node.js](https://nodejs.org/) 18+ (nếu chạy frontend không qua Docker)
- Tài khoản [Cloudinary](https://cloudinary.com/) (miễn phí) để lấy `Cloud name` / `API Key` / `API Secret`

## Cài đặt

1. Sao chép `.env.example` thành `.env` (cùng thư mục gốc) và điền giá trị thật:

   ```
   DB_SA_PASSWORD=...       # mật khẩu sa cho SQL Server (>=8 ký tự, có hoa/thường/số/ký tự đặc biệt)
   JWT_KEY=...              # chuỗi ngẫu nhiên bất kỳ, >= 32 ký tự
   CLOUDINARY_CLOUD_NAME=...
   CLOUDINARY_API_KEY=...
   CLOUDINARY_API_SECRET=...
   ```

2. Tạo schema + seed dữ liệu mẫu. **Bắt buộc** dùng cờ `-f i:65001` (UTF-8) để không hỏng dữ liệu tiếng Việt có dấu:

   ```bash
   sqlcmd -S localhost -U sa -P "<DB_SA_PASSWORD>" -C -f i:65001 -i database/JobHunter_CreateTables.sql
   sqlcmd -S localhost -U sa -P "<DB_SA_PASSWORD>" -C -f i:65001 -i database/JobHunter_SeedData.sql
   ```

   (Muốn có thêm dữ liệu demo phong phú hơn — nhiều công ty/tin tuyển dụng theo từng ngành nghề, lịch sử vài tháng cho biểu đồ báo cáo — chạy thêm `database/seed-demo-history.sql` và `database/seed-demo-industries.sql` theo cùng cú pháp trên.)

### Cách 1 — Docker Compose (chạy cả 3 dịch vụ cùng lúc)

```bash
docker compose up -d
```

- Frontend: http://localhost:5173
- Backend API: http://localhost:5147
- SQL Server: `localhost,1433`

### Cách 2 — Chạy riêng từng phần (dev mode)

```bash
# SQL Server (nếu chưa có sẵn instance nào khác)
docker compose up -d sqlserver

# Backend
cd backend/JobHunter.API
dotnet run --launch-profile http   # http://localhost:5147

# Frontend
cd frontend
npm install
npm run dev                         # http://localhost:5173
```

## Chạy test

```bash
# Backend (unit + integration — cần SQL Server đang chạy)
cd backend/JobHunter.Tests
dotnet test

# Frontend (component test)
cd frontend
npm test
```

## Deploy (không cần domain riêng)

Không bắt buộc phải có tên miền — các nền tảng dưới đây tự cấp subdomain HTTPS miễn phí:

| Phần | Gợi ý nền tảng | Ghi chú |
|---|---|---|
| Frontend | Vercel / Netlify | Free, tự deploy khi push GitHub |
| Backend API | Azure App Service (free tier) | Chạy .NET native, không cần Docker |
| Database | Azure SQL Database (free tier) | Duy nhất trong các lựa chọn free hỗ trợ đúng SQL Server |

Có email trường (.edu) thì dùng [Azure for Students](https://azure.microsoft.com/free/students/) — $100 credit, không cần thẻ tín dụng, đủ host cả 3 phần trên cùng 1 tài khoản.

**2 biến môi trường cần set khi deploy** (không đặt thì mặc định chạy như dev local):

- Frontend (Vercel/Netlify → Environment Variables): `VITE_API_BASE_URL=https://ten-api-cua-ban.azurewebsites.net/api`
- Backend (Azure App Service → Configuration): `CORS_ALLOWED_ORIGINS=https://ten-frontend-cua-ban.vercel.app` (nhiều origin cách nhau bởi dấu phẩy)

Muốn gửi email thật (xác thực đăng ký, quên mật khẩu) khi deploy thì set thêm ở Backend: `Frontend__BaseUrl=https://ten-frontend-cua-ban.vercel.app` và `Smtp__Host`/`Smtp__Port`/`Smtp__Username`/`Smtp__Password`/`Smtp__FromEmail`/`Smtp__FromName` (xem `.env.example`) — không đặt thì tự động fallback về log console như cũ, không lỗi.

## Cấu trúc thư mục

```
JobHunter/
  database/          # Script tạo bảng, seed dữ liệu
  backend/
    JobHunter.API/    # Controllers, Services, Repositories, Models, DTOs
    JobHunter.Tests/  # xUnit
  frontend/
    src/
      pages/          # guest / auth / candidate / employer / admin
      components/
      api/
  docker-compose.yml
```
