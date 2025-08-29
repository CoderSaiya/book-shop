using System.Security.Claims;
using System.Text;
using BookShop.Domain.Specifications;
using BookShop.Infrastructure.Configuration;
using BookShop.Infrastructure.Hubs;
using BookShop.Infrastructure.ML;
using BookShop.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MediSchedule", Version = "v1" }));

builder.Services.AddSwaggerGen(c =>
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    }));
builder.Services.AddSwaggerGen(c =>
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    }));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = false;
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var authHeader = ctx.Request.Headers["Authorization"].FirstOrDefault();
                logger.LogDebug("JWT Header: {Auth}", authHeader);
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(ctx.Exception, "JWT failed");
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    })
    .AddCookie("External")
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Auth:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Auth:Google:ClientSecret"]!;
        options.CallbackPath = "/signin-google"; // đăng trong Google Console
        options.SignInScheme = "External";
        options.SaveTokens = true;
    })
    .AddGitHub("GitHub", options =>
    {
        options.ClientId = builder.Configuration["Auth:GitHub:ClientId"]!;
        options.ClientSecret = builder.Configuration["Auth:GitHub:ClientSecret"]!;
        options.CallbackPath = "/signin-github"; // đăng trong GitHub OAuth app
        options.SignInScheme = "External";
        options.SaveTokens = true;
        options.Scope.Add("read:user");
        options.Scope.Add("user:email");
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200", 
                "http://localhost:4201")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMqSettings"));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<MlIntentOptions>(builder.Configuration.GetSection("Ml"));
builder.Services.Configure<MomoSettings>(builder.Configuration.GetSection("Payment:MoMo"));
builder.Services.Configure<VnPaySettings>(builder.Configuration.GetSection("Payment:VnPay"));

builder.Services.AddApplication();
builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});
builder.Services.AddInfrastructure(builder.Configuration);

var contentRoot = builder.Environment.ContentRootPath;
var modelDir = Path.Combine(contentRoot, "..", "BookShop.Infrastructure","ML", "models", "intent_llm");

Console.WriteLine(modelDir);

// Tạo đường dẫn tuyệt đối tới các tệp
var onnxPath   = Path.Combine(modelDir, "onnx", "model.onnx");
var labelsPath = Path.Combine(modelDir, "labels.json");
var tokJsonPath= Path.Combine(modelDir, "hf_model", "tokenizer.json");

builder.Services.AddSingleton<BookShop.Application.Interface.AI.IIntentClassifier>(_ =>
    new BookShop.Infrastructure.ML.IntentClassifier(
        onnxPath: onnxPath,
        tokJsonPath: tokJsonPath,
        labelsPath: labelsPath,
        maxLen: 128
    )
);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();