# Sắc Hoa — API back-end

Bài thực hành 2 và 3 môn Lập trình web (INT1334): **ASP.NET Core Web API + EF Core + PostgreSQL**, có đăng ký / đăng nhập bằng JWT, phục vụ front-end React ở project `flower-shop-manager`.

- .NET 10, controllers (`Controllers/`), EF Core 10 với provider Npgsql (PostgreSQL).
- Dữ liệu: danh mục, sản phẩm, đơn hàng (kèm dòng hàng), người dùng.

## Chạy API

Cần .NET SDK 10 và PostgreSQL (mặc định `localhost:5432`).

1. Đặt chuỗi kết nối có mật khẩu thật bằng **user-secrets** (lưu ngoài thư mục project, không bị commit):

   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=flower_shop;Username=postgres;Password=MAT_KHAU_CUA_BAN"
   ```

2. Đặt khóa ký JWT (chuỗi ngẫu nhiên, ít nhất 32 ký tự) và email của tài khoản quản trị:

   ```bash
   dotnet user-secrets set "Jwt:Key" "CHUOI_NGAU_NHIEN_DAI_IT_NHAT_32_KY_TU"
   dotnet user-secrets set "Auth:AdminEmails:0" "email-cua-ban@example.com"
   ```

   Ai đăng ký (hoặc đăng nhập) bằng đúng email này sẽ có vai trò `Admin`; các email khác là `Customer`.

3. Cài lại công cụ `dotnet-ef` của project (lần đầu trên máy mới) rồi tạo database + bảng + dữ liệu mẫu từ migration:

   ```bash
   dotnet tool restore
   dotnet ef database update
   ```

4. Chạy (nhớ dùng launch profile; chạy ở môi trường khác sẽ không đọc user-secrets):

   ```bash
   dotnet run --launch-profile http
   ```

   API ở `http://localhost:5080`, tài liệu thử API (Swagger UI) ở `http://localhost:5080/swagger`. File `FlowerShop.Api.http` có sẵn các request mẫu, kể cả đăng ký / đăng nhập và gọi endpoint cần token.

## Các endpoint

| Method | URL | Chức năng | Quyền | Mã trả về |
|---|---|---|---|---|
| POST | `/api/auth/register` | Đăng ký: kiểm tra dữ liệu, lưu user vào DB, trả JWT | Mở | 201, 400, 409 |
| POST | `/api/auth/login` | Đăng nhập: truy vấn user theo email, kiểm tra mật khẩu, trả JWT | Mở | 200, 400, 401 |
| GET | `/api/auth/me` | Thông tin người đang đăng nhập (dùng để kiểm tra token còn hợp lệ) | Đã đăng nhập | 200, 401 |
| GET | `/api/categories` | Danh sách danh mục | Mở | 200 |
| GET | `/api/products?category=&search=&featured=` | Danh sách sản phẩm, có lọc | Mở | 200 |
| GET | `/api/products/{id}` | Chi tiết sản phẩm | Mở | 200, 404 |
| POST | `/api/products` | Thêm sản phẩm | Admin | 201, 400, 401, 403 |
| PUT | `/api/products/{id}` | Sửa sản phẩm (gửi đủ trường) | Admin | 200, 400, 401, 403, 404 |
| DELETE | `/api/products/{id}` | Xóa sản phẩm chưa từng được đặt | Admin | 204, 401, 403, 404, 409 |
| POST | `/api/orders` | Khách đặt hoa (không cần đăng nhập), trừ tồn kho trong transaction | Mở | 201, 400, 409 |
| GET | `/api/orders?status=` | Danh sách đơn, mới nhất trước | Admin | 200, 401, 403 |
| GET | `/api/orders/{id}` | Chi tiết đơn | Admin | 200, 401, 403, 404 |
| PUT | `/api/orders/{id}` | Đổi trạng thái, địa chỉ, lịch giao (hủy đơn thì trả hàng về kho) | Admin | 200, 400, 401, 403, 404, 409 |
| DELETE | `/api/orders/{id}` | Xóa đơn đã hủy | Admin | 204, 401, 403, 404, 409 |

Quy tắc đáng chú ý:
- Lỗi trả theo chuẩn **ProblemDetails** (`title`, `detail`); lỗi nhập liệu trả 400 kèm `errors` theo từng trường.
- `401` = chưa đăng nhập hoặc token sai / hết hạn; `403` = đã đăng nhập nhưng không phải Admin.
- Đơn ở trạng thái `Completed` / `Cancelled` không sửa được nữa (409); chỉ xóa được đơn `Cancelled`.
- Không xóa được sản phẩm đã nằm trong đơn (409). Muốn ngừng bán thì đặt tồn kho về 0.
- Trừ kho bằng một câu `UPDATE ... WHERE Stock >= số lượng` nên hai khách đặt cùng lúc không làm kho bị âm.

## Đăng ký, đăng nhập và phân quyền (lab 3)

**Luồng:** `register` → kiểm tra dữ liệu (DataAnnotations) → băm mật khẩu → `INSERT` vào bảng `Users` → trả JWT. `login` → truy vấn `Users` theo email → so mật khẩu với bản băm → trả JWT. Front-end lưu JWT + thông tin user trong localStorage và gửi lại ở header `Authorization: Bearer <token>`.

- **Kiểm tra dữ liệu đăng ký** (`Dtos/AuthDtos.cs`): email đúng dạng và tối đa 254 ký tự; mật khẩu 8–100 ký tự, có ít nhất một chữ cái và một chữ số. Email được cắt khoảng trắng và đổi về chữ thường trước khi lưu, có chỉ mục duy nhất nên không thể trùng (kể cả khi hai người đăng ký cùng lúc → 409).
- **Lưu mật khẩu:** không lưu mật khẩu gốc. Dùng `PasswordHasher<User>` của ASP.NET Core (PBKDF2-HMAC-SHA512, muối ngẫu nhiên riêng cho từng mật khẩu, 84 ký tự trong cột `PasswordHash`).
- **Đăng nhập sai:** "sai email" và "sai mật khẩu" trả cùng một câu và tốn thời gian xử lý như nhau (có hash giả), để không lộ email nào đã đăng ký.
- **JWT** (`Services/TokenService.cs`): ký HS256 bằng `Jwt:Key`, chứa `sub` (id), `email`, `role`, hết hạn sau `Jwt:ExpiresMinutes` (mặc định 120 phút). `Program.cs` cấu hình `AddJwtBearer` kiểm tra chữ ký, issuer, audience và hạn dùng.
- **Phân quyền:** `[Authorize(Roles = "Admin")]` trên các endpoint quản trị. Vai trò lấy từ cấu hình `Auth:AdminEmails` lúc đăng ký / đăng nhập, nên đổi vai trò có hiệu lực ở lần đăng nhập sau.
- **Migration `AddUsers`** tạo bảng `Users` (`Id`, `Email` duy nhất, `PasswordHash`, `Role`, `CreatedAt`).

**Lưu ý bảo mật:** khi triển khai thật phải chạy qua HTTPS, đặt `Jwt:Key` và `Auth:AdminEmails` bằng biến môi trường / kho bí mật, và nên thêm giới hạn số lần đăng nhập sai. Token trong localStorage có thể bị đánh cắp nếu trang dính lỗi XSS; cách an toàn hơn là cookie `HttpOnly` (bài thực hành này dùng localStorage theo yêu cầu).

## Cấu hình database và EF Core (ghi chú học)

**1. Cài gói** (đã làm; `dotnet add package` ghi vào `FlowerShop.Api.csproj`):

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL   # provider PostgreSQL (kéo theo EF Core)
dotnet add package Microsoft.EntityFrameworkCore.Design    # để dotnet ef sinh migration
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer   # xác thực JWT
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

**Đổi sang SQL Server** (như slide của môn): thay gói `Npgsql.EntityFrameworkCore.PostgreSQL` bằng `Microsoft.EntityFrameworkCore.SqlServer`, đổi `UseNpgsql` thành `UseSqlServer` trong `Program.cs`, đổi chuỗi kết nối, xóa thư mục `Migrations/` rồi tạo lại bằng `dotnet ef migrations add InitialCreate`. (Migration sinh ra gắn với provider nên không dùng chéo được; `AuthController` cũng đang bắt lỗi trùng email theo mã lỗi của PostgreSQL nên cần đổi sang mã lỗi tương ứng.)

## Cấu trúc

```
Controllers/   AuthController, CategoriesController, ProductsController, OrdersController
Models/        Category, Product, Order (+ OrderItem, OrderStatus), User, ShopRules
Dtos/          dữ liệu vào/ra của API (kèm kiểm tra bằng DataAnnotations)
Services/      TokenService (tạo JWT), JwtSettings
Data/          AppDbContext, SeedData
Migrations/    sinh bởi dotnet ef
Program.cs     đăng ký DbContext, controllers, JWT, CORS, Swagger
```

## Chưa có

Giới hạn số lần đăng nhập sai, quên / đổi mật khẩu, xác minh email, danh sách đơn riêng của từng khách, phân trang, kiểm thử tự động.
