using System.Text.Json.Serialization;
using FlowerShop.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Kết nối database: chuỗi kết nối nằm ở appsettings.json (mục ConnectionStrings).
//    Mật khẩu thật đặt bằng user-secrets hoặc biến môi trường, không ghi vào file được commit.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection trong cấu hình.");

// 2. Đăng ký DbContext (EF Core + PostgreSQL) vào hệ thống dependency injection.
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// 3. Controllers; enum (OrderStatus) được gửi/nhận dạng chữ như "New", "Completed".
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

// 4. CORS: cho phép front-end (React/Vite) ở địa chỉ khác gọi API từ trình duyệt.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Flower Shop API v1"));
}

// Không dùng UseHttpsRedirection ở bản thực hành: front-end gọi http://localhost nên redirect sang https sẽ làm hỏng CORS.
app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
