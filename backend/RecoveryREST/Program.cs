using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecoveryREST.Data;
using RecoveryREST.Interfaces;
using RecoveryREST.Models.Classes;
using RecoveryREST.Services;
using Newtonsoft.Json;
using RecoveryREST.Repos;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => {
    options.ListenLocalhost(5129, listenOptions => {
        listenOptions.UseHttps("certs/server.pfx", "123qwe");
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(option => {
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Recovery API", Version = "v1" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    option.IncludeXmlComments(xmlPath);
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement {{
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddControllers().AddNewtonsoftJson(options => {
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
    
builder.Services.AddScoped<IInjuryRepo, InjuryRepo>();
builder.Services.AddScoped<IRecoveryExerciseRepo, RecoveryExerciseRepo>();
builder.Services.AddScoped<IRecoveryPlanRepo, RecoveryPlanRepo>();
builder.Services.AddScoped<IUserInjuryRepo, UserInjuryRepo>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IDoctorTokenService, DoctorTokenService>();
builder.Services.AddScoped<IDoctorRepo, DoctorRepo>();

builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder => {
    builder
        .SetIsOriginAllowed(_ => true)
        .AllowCredentials()
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    }
));

builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultForbidScheme =
                options.DefaultScheme =
                    options.DefaultSignInScheme =
                        options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    var serviceProvider = scope.ServiceProvider;

    var context = serviceProvider.GetService<ApplicationDbContext>();
    context?.Database.EnsureCreated();

    var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    await SeedUsersAsync(userManager, roleManager, configuration);
}

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("MyPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedUsersAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration) {
    var usersToSeed = new[] {
        new { Role = "Admin", Email = configuration["AdminUser:Email"], Username = configuration["AdminUser:UserName"], Password = configuration["AdminUser:Password"] },
        new { Role = "Doctor", Email = configuration["DoctorUser:Email"], Username = configuration["DoctorUser:UserName"], Password = configuration["DoctorUser:Password"] },
        new { Role = "User", Email = configuration["RegularUser:Email"], Username = configuration["RegularUser:UserName"], Password = configuration["RegularUser:Password"] }
    };

    foreach (var userInfo in usersToSeed) {
        await EnsureRoleExistsAsync(roleManager, userInfo.Role);
         var existingUser = await userManager.FindByEmailAsync(userInfo.Email);
        if (existingUser == null) {
            await EnsureUserExistsAsync(userManager, userInfo.Role, userInfo.Email, userInfo.Username, userInfo.Password);
        }
    }
}

static async Task EnsureRoleExistsAsync(RoleManager<IdentityRole> roleManager, string roleName){
    if (!await roleManager.RoleExistsAsync(roleName)) {
        await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}

static async Task EnsureUserExistsAsync(UserManager<AppUser> userManager, string roleName, string email, string username, string password) {
    var user = await userManager.FindByEmailAsync(email);
    if (user == null) {
        user = new AppUser {
            UserName = username,
            Email = email
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded) {
            throw new Exception($"Failed to create {roleName} user: " +
                                string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, roleName);
    }
}
