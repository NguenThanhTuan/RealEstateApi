using RealEstateApi.Data;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Models;
using Npgsql;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Serilog;
using System.Diagnostics;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var enableLogging = builder.Configuration.GetValue<bool>("ApiLogging:Enable");
// Cấu hình Serilog để log ra file
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(enableLogging ? Serilog.Events.LogEventLevel.Information : Serilog.Events.LogEventLevel.Fatal)
    .WriteTo.File("logs/api-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
// Đọc cấu hình ApiLogging
builder.Services.Configure<ApiLoggingSettings>(
    builder.Configuration.GetSection("ApiLogging")
);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

//try
//{
//    using var conn = new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
//    conn.Open();
//    Console.WriteLine("Đã kết nối thành công đến PostgreSQL!");

//    using (var command = new NpgsqlCommand("SELECT * FROM public.'Users'", conn))
//    {

//        var reader = command.ExecuteReader();
//        while (reader.Read())
//        {
//            Console.WriteLine(
//                string.Format(
//                    "Reading from table=({0}, {1}, {2})",
//                    reader.GetInt32(0).ToString(),
//                    reader.GetString(1),
//                    reader.GetInt32(2).ToString()
//                    )
//                );
//        }
//        reader.Close();
//    }
//}
//catch (Exception ex)
//{
//    Console.WriteLine("❌ Không thể kết nối đến PostgreSQL: " + ex.Message);
//}

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
            )
        };

        options.Events = new JwtBearerEvents
        {
            // Khi không có token hoặc token hết hạn
            OnChallenge = async context =>
            {
                context.HandleResponse(); // ngăn không cho response mặc định

                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";

                    var result = new
                    {
                        success = false,
                        message = "Bạn chưa đăng nhập",
                        code = 401
                    };
                    await context.Response.WriteAsJsonAsync(result);
                }
                return;
            },

            // Khi token đúng nhưng không có quyền
            OnForbidden = async context =>
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";
                    var result = new
                    {
                        success = false,
                        message = "Bạn không có quyền truy cập chức năng này",
                        code = 403
                    };

                    await context.Response.WriteAsJsonAsync(result);
                }
                return;
            },

            // Khi token hợp lệ, kiểm tra device hoặc user login khác
            OnTokenValidated = async context =>
            {
                var endpoint = context.HttpContext.GetEndpoint();

                var allowAnonymous = endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null;
                if (allowAnonymous)
                {
                    // Đây là endpoint AllowAnonymous (như Login) => bỏ qua check device
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

                var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var deviceIdFromToken = claimsIdentity?.FindFirst("DeviceId")?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(deviceIdFromToken))
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        var result = new
                        {
                            success = false,
                            message = "Không xác thực được thiết bị",
                            code = 403
                        };

                        await context.Response.WriteAsJsonAsync(result);
                    }
                        
                    context.Fail("Không xác thực được thiết bị");
                    return;
                }

                var user = await db.Users.FindAsync(int.Parse(userId));

                if (user == null || user.deviceId?.Trim() != deviceIdFromToken?.Trim())
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        var result = new
                        {
                            success = false,
                            message = "Tài khoản đã được đăng nhập từ thiết bị khác",
                            code = 403
                        };
                        await context.Response.WriteAsJsonAsync(result);
                    }
                        //context.Fail("Thiết bị không hợp lệ hoặc đã bị đăng xuất.");
                    context.Fail("Tài khoản đã được đăng nhập từ thiết bị khác");
                    return;
                }
            }
        };
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()     // Hoặc .WithOrigins("https://your-frontend.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// Đăng ký FirebaseService
builder.Services.AddSingleton<FirebaseService>();

// Swagger, CORS, DB, v.v.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 👉 THÊM cấu hình Bearer Token
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập token theo định dạng: Bearer {your token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add NotificationService
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.MapPost("/register", async (User user, AppDbContext db) =>
//{
//    db.Users.Add(user);
//    await db.SaveChangesAsync();
//    return Results.Ok(user);
//});

//app.MapPost("/login", async (User loginUser, AppDbContext db) =>
//{
//    var user = await db.Users
//        .FirstOrDefaultAsync(u => u.PhoneNumber == loginUser.PhoneNumber && u.Password == loginUser.Password);

//    if (user is null)
//        return Results.Unauthorized();

//    return Results.Ok(user);
//});
app.UseCors();
app.UseRouting();

app.UseStaticFiles(); // Cho phép truy cập wwwroot
//app.Use(async (context, next) =>
//{
//    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
//    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
//    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
//    await next();
//});

app.UseAuthentication();

app.Use(async (context, next) =>
{
    var settings = context.RequestServices.GetRequiredService<IOptions<ApiLoggingSettings>>().Value;

    if (!settings.Enable)
    {
        await next.Invoke();
        return;
    }

    var sw = Stopwatch.StartNew();
    var ip = context.Connection.RemoteIpAddress?.ToString();

    await next.Invoke();

    sw.Stop();

    var statusCode = context.Response.StatusCode;
    // Get the elapsed time in milliseconds
    //var elapsedMs = sw.ElapsedMilliseconds;

    //if (elapsedMs > settings.SlowApiThresholdMs)
    //{
    //    var log = $"🐢 Slow API detected: {context.Request.Method} {context.Request.Path} from {ip} => Status {statusCode} took {elapsedMs} ms";
    //    Log.Warning(log);
    //}
    //else
    //{
    //    var log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context.Request.Method} {context.Request.Path} from {ip} => Status {statusCode} took {elapsedMs} ms";
    //    Log.Information(log);
    //}

    var log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context.Request.Method} {context.Request.Path} from {ip} => Status {statusCode} took {sw.ElapsedMilliseconds} ms";
    Log.Information(log);
});

// Check if the device ID is valid
//app.UseMiddleware<DeviceIdValidatorMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.Run();

//var summaries = new[]
//{
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast = Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast")
//.WithOpenApi();

//app.Run();

//internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}
