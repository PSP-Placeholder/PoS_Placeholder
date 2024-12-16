using System.Text;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PoS_Placeholder.Server.Data;
using PoS_Placeholder.Server.Logging;
using PoS_Placeholder.Server.Models;
using PoS_Placeholder.Server.Repositories;
using PoS_Placeholder.Server.Services;
using PoS_Placeholder.Server.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING"));
});

builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<ProductVariationRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<DiscountRepository>();
builder.Services.AddScoped<BusinessRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserWorkTimeRepository>();
builder.Services.AddScoped<GiftcardRepository>();

builder.Logging.AddLogger(configuration =>
{
    builder.Configuration.GetSection("FileLogger").Bind(configuration);
});

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new TimeOnlyConverter());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });


builder.Services.AddSingleton(u =>
    new BlobServiceClient(builder.Configuration.GetConnectionString("IMG_STORAGE_CONTAINER_CONNECTIONSTRING")));

builder.Services.AddSingleton<IImageService>(provider =>
{
    var blobServiceClient = provider.GetRequiredService<BlobServiceClient>();
    var containerName = builder.Configuration.GetSection("IMG_STORAGE_CONTAINER")["ContainerName"];
    if (string.IsNullOrEmpty(containerName))
    {
        throw new InvalidOperationException("Container name is not configured.");
    }
    return new ImageService(containerName, blobServiceClient);
});

builder.Services.AddSingleton<ITaxService>(provider =>
{
    var filePath = Path.Combine(AppContext.BaseDirectory, "locale.json");
    return new TaxService(filePath);
});

builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

var key = builder.Configuration.GetValue<string>("JwtSettings:Secret");
builder.Services.AddAuthentication(u =>
{
    u.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    u.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(u =>
{
    u.RequireHttpsMetadata = false;
    u.SaveToken = true;
    u.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                      "Enter 'Bearer' [space] and then your token in the text input below. \r\n\r\n" +
                      "Example: Bearer 12345abcdefgh",
        BearerFormat = "JWT",
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});


var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
