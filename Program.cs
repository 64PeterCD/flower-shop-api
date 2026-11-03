using System.Text.Json.Serialization;
using FlowerShop.Api.Data;
using FlowerShop.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

// 4. Xác thực bằng JWT (Authorization: Bearer <token>) và phân quyền theo vai trò.
//    Cấu hình JWT đọc lười (lúc có request đầu tiên) để các lệnh `dotnet ef` vẫn chạy khi chưa đặt Jwt:Key.
builder.Services.AddScoped<TokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IConfiguration>((options, config) =>
    {
        var jwt = JwtSettings.From(config);
        options.MapInboundClaims = false; // giữ nguyên tên claim ngắn: "sub", "email", "role"
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwt.SigningKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = TokenService.RoleClaim,
            NameClaimType = "email",
        };
    });
builder.Services.AddAuthorization();

// 5. CORS: cho phép front-end (React/Vite) ở địa chỉ khác gọi API từ trình duyệt.
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
app.UseAuthentication(); // đọc token -> biết "ai đang gọi"; phải đứng trước UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();
