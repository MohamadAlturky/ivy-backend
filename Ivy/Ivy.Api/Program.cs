using System.Security.Claims;
using System.Text;
using Ivy.Api.Services;
using Ivy.Contracts.Services;
using Ivy.Core.DataContext;
using Ivy.Core.Jwt;
using Ivy.Core.Services;
using IvyBackend.Services;
using Mazad.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<JwtService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null)
{
    throw new InvalidOperationException("JwtSettings configuration is missing or invalid.");
}
var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ivy API Documentation", Version = "v1" });

    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        }
    );

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );

    c.OperationFilter<AcceptLanguageHeaderFilter>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("patient", policy => policy.RequireClaim(ClaimTypes.Role, "patient"));

    options.AddPolicy("admin", policy => policy.RequireClaim(ClaimTypes.Role, "admin"));

    options.AddPolicy("doctor", policy => policy.RequireClaim(ClaimTypes.Role, "doctor"));
});

// Database Context
builder.Services.AddDbContext<IvyContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Register application services
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IGovernorateService, GovernorateService>();
builder.Services.AddScoped<IMedicalSpecialityService, MedicalSpecialityService>();
builder.Services.AddScoped<IPatientAuthService, PatientAuthService>();
builder.Services.AddScoped<IDoctorAuthService, DoctorAuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminSeederService, AdminSeederService>();
builder.Services.AddScoped<IAdminRoleService, AdminRoleService>();
builder.Services.AddScoped<IAdminUserRoleService, AdminUserRoleService>();
builder.Services.AddScoped<IAdminRolePermissionService, AdminRolePermissionService>();
builder.Services.AddScoped<IAdminPermissionService, AdminPermissionService>();
builder.Services.AddScoped<IClinicService, ClinicService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IDoctorClinicService, DoctorClinicService>();
builder.Services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
builder.Services.AddScoped<
    IClientDataCollectionForBookingService,
    ClientDataCollectionForBookingService
>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<ICustomerAppointmentService, CustomerAppointmentService>();
builder.Services.AddScoped<IClinicAppointmentService, ClinicAppointmentService>();
builder.Services.AddScoped<IMedicalHistoryService, MedicalHistoryService>();

// Register OTP service as singleton
builder.Services.AddSingleton<IOtpService>(provider => OtpService.Instance);

// Register message store and response representer
builder.Services.AddSingleton<IMessageStore, MessageStore>();
builder.Services.AddScoped<IApiResponseRepresenter, ApiResponseRepresenter>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Seed default admin if none exists
using (var scope = app.Services.CreateScope())
{
    try
    {
        var adminSeederService = scope.ServiceProvider.GetRequiredService<IAdminSeederService>();
        var seedResult = await adminSeederService.SeedDefaultAdminAsync();
        if (seedResult.Success && seedResult.Data)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Default admin seeding completed successfully");
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to seed default admin during application startup");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseSwagger();
app.UseSwaggerUI();

// Add CORS middleware
app.UseCors();

// Don't force HTTPS redirect in production hosting environments
// app.UseHttpsRedirection();

// Configure static files to serve from wwwroot
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
