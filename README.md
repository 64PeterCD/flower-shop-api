# Sắc Hoa — API back-end

Bài thực hành 2 môn Lập trình web (INT1334): **ASP.NET Core Web API + EF Core + PostgreSQL**, phục vụ front-end React ở project `flower-shop-manager`.

- .NET 10, controllers (`Controllers/`), EF Core 10 với provider Npgsql (PostgreSQL).
- Dữ liệu: danh mục, sản phẩm, đơn hàng (kèm dòng hàng).

## Chạy API

Cần .NET SDK 10 và PostgreSQL (mặc định `localhost:5432`).

1. Đặt chuỗi kết nối có mật khẩu thật bằng **user-secrets** (lưu ngoài thư mục project, không bị commit):

   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=flower_shop;Username=postgres;Password=MAT_KHAU_CUA_BAN"
   ```

2. Cài lại công cụ `dotnet-ef` của project (lần đầu trên máy mới) rồi tạo database + bảng + dữ liệu mẫu từ migration:

   ```bash
   dotnet tool restore
   dotnet ef database update
   ```

3. Chạy:

   ```bash
   dotnet run
   ```

   API ở `http://localhost:5080`, tài liệu thử API (Swagger UI) ở `http://localhost:5080/swagger`. File `FlowerShop.Api.http` có sẵn các request mẫu.

## Các endpoint

| Method | URL | Chức năng | Mã trả về |
|---|---|---|---|
| GET | `/api/categories` | Danh sách danh mục | 200 |
| GET | `/api/products?category=&search=&featured=` | Danh sách sản phẩm, có lọc | 200 |
| GET | `/api/products/{id}` | Chi tiết sản phẩm | 200, 404 |
| POST | `/api/products` | Thêm sản phẩm | 201, 400 |
| PUT | `/api/products/{id}` | Sửa sản phẩm (gửi đủ trường) | 200, 400, 404 |
| DELETE | `/api/products/{id}` | Xóa sản phẩm chưa từng được đặt | 204, 404, 409 |
| GET | `/api/orders?status=` | Danh sách đơn, mới nhất trước | 200 |
| GET | `/api/orders/{id}` | Chi tiết đơn | 200, 404 |
| POST | `/api/orders` | Khách đặt hoa, trừ tồn kho trong transaction | 201, 400, 409 |
| PUT | `/api/orders/{id}` | Đổi trạng thái, địa chỉ, lịch giao (hủy đơn thì trả hàng về kho) | 200, 400, 404, 409 |
| DELETE | `/api/orders/{id}` | Xóa đơn đã hủy | 204, 404, 409 |

Quy tắc đáng chú ý:
- Lỗi trả theo chuẩn **ProblemDetails** (`title`, `detail`); lỗi nhập liệu trả 400 kèm `errors`.
- Đơn ở trạng thái `Completed` / `Cancelled` không sửa được nữa (409); chỉ xóa được đơn `Cancelled`.
- Không xóa được sản phẩm đã nằm trong đơn (409). Muốn ngừng bán thì đặt tồn kho về 0.
- Trừ kho bằng một câu `UPDATE ... WHERE Stock >= số lượng` nên hai khách đặt cùng lúc không làm kho bị âm.

## Cấu hình database và EF Core (ghi chú học)

**1. Cài gói** (đã làm; `dotnet add package` ghi vào `FlowerShop.Api.csproj`):

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL   # provider PostgreSQL (kéo theo EF Core)
dotnet add package Microsoft.EntityFrameworkCore.Design    # để dotnet ef sinh migration
dotnet new tool-manifest
dotnet tool install dotnet-ef                               # công cụ dòng lệnh, cài theo project
```

**2. Chuỗi kết nối**: mục `ConnectionStrings` trong `appsettings.json`. File này được commit nên chỉ để mật khẩu giả `CHANGE_ME`; giá trị thật nằm trong user-secrets (môi trường Development tự đọc) hoặc biến môi trường `ConnectionStrings__DefaultConnection` khi triển khai.

**3. DbContext**: `Data/AppDbContext.cs` khai báo các `DbSet` và cấu hình bảng (độ dài cột, khóa ngoại, chỉ mục, dữ liệu mẫu ở `Data/SeedData.cs`). `Program.cs` đăng ký bằng `AddDbContext<AppDbContext>(o => o.UseNpgsql(...))`.

**4. Migration** — mỗi lần đổi model (thêm/sửa lớp trong `Models/` hoặc cấu hình trong `AppDbContext`):

```bash
dotnet ef migrations add TenThayDoi   # sinh file trong Migrations/
dotnet ef database update             # áp dụng vào database
dotnet ef migrations remove           # hoàn tác migration vừa tạo (nếu chưa update)
dotnet ef migrations script -o init.sql   # xuất ra SQL thuần để xem hoặc chạy tay
```

**Đổi sang SQL Server** (như slide của môn): thay gói `Npgsql.EntityFrameworkCore.PostgreSQL` bằng `Microsoft.EntityFrameworkCore.SqlServer`, đổi `UseNpgsql` thành `UseSqlServer` trong `Program.cs`, đổi chuỗi kết nối, xóa thư mục `Migrations/` rồi tạo lại bằng `dotnet ef migrations add InitialCreate`. (Migration sinh ra gắn với provider nên không dùng chéo được.)

## Cấu trúc

```
Controllers/   CategoriesController, ProductsController, OrdersController
Models/        Category, Product, Order (+ OrderItem, OrderStatus), ShopRules
Dtos/          dữ liệu vào/ra của API (kèm kiểm tra bằng DataAnnotations)
Data/          AppDbContext, SeedData
Migrations/    sinh bởi dotnet ef
Program.cs     đăng ký DbContext, controllers, CORS, Swagger
```

## Chưa có

Đăng nhập/phân quyền (mọi endpoint đang mở), phân trang, kiểm thử tự động.
