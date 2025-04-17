using Microsoft.EntityFrameworkCore;
using GenericProject.Infrastructure.Data;
using GenericProject.Domain.Interfaces.UnitofWork;
using GenericProject.Infrastructure.Data.Repositories; // GenericRepository vs. için
using GenericProject.Domain.Interfaces.Repositories;
using GenericProject.Application.Interfaces.Services;
using GenericProject.Application.Services;
using GenericProject.Application.Mapping; // AutoMapperProfile
using FluentValidation; // AddFluentValidation extension
using GenericProject.Application.Validators; // Validator sýnýflarý
using GenericProject.API.Middlewares; // ExceptionMiddleware
using Asp.Versioning; // API Versioning
using Microsoft.OpenApi.Models; // Swagger
using AspNetCoreRateLimit; // Rate Limiting
//using Serilog; // Logging (Opsiyonel ama önerilir)
using GenericProject.Infrastructure.ExternalServices; // Placeholder servisler
using Asp.Versioning.ApiExplorer; // IApiVersionDescriptionProvider için
using GenericProject.API.Configuration; // Yeni eklediðimiz sýnýfýn namespace'i


var builder = WebApplication.CreateBuilder(args);

// 1. Logging (Serilog Örneði - Ýsteðe baðlý)
// NuGet: Serilog.AspNetCore, Serilog.Sinks.Console, Serilog.Sinks.File
// builder.Host.UseSerilog((context, configuration) =>
//     configuration.ReadFrom.Configuration(context.Configuration));
// Temel .NET Logging'i kullanmak için bu kýsmý kaldýrabilirsiniz.


// 2. Configuration (Rate Limiting için appsettings.json'dan okuma)
builder.Services.AddOptions(); // Rate Limiting için gerekli
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));


// 3. Services (DI Container Konfigürasyonu)

// Infrastructure Katmaný Servisleri
builder.Services.AddDbContext<AppDbContext>(options => 
    //options.UseInMemoryDatabase("ECommerceAppDb")); // Örnek için InMemory DB
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); // UnitOfWork (Scoped önerilir)
// Repository'ler UnitOfWork üzerinden eriþildiði için genellikle direkt register edilmez.
// Ancak isterseniz register edebilirsiniz:
// builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
// builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Placeholder External Services
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Application Katmaný Servisleri
builder.Services.AddScoped<IProductService, ProductService>(); // Servisler

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile)); // Profilin bulunduðu Assembly'yi tarar

// FluentValidation
//builder.Services.AddFluentValidationAutoValidation(); // Otomatik validasyon middleware'i ekler (önerilir)
// builder.Services.AddFluentValidationClientsideAdapters(); // Ýstemci tarafý adaptörler (MVC/Razor Pages için daha relevant)
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>(); // Validator'larýn bulunduðu Assembly'yi tarar


// Caching
builder.Services.AddMemoryCache(); // InMemory Cache

// Rate Limiting
builder.Services.AddInMemoryRateLimiting(); // Cache ile birlikte çalýþýr
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>(); // Rate Limit konfigürasyonunu yükler

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // Varsayýlan versiyon v1.0
    options.AssumeDefaultVersionWhenUnspecified = true; // Versiyon belirtilmezse varsayýlaný kullan
    options.ReportApiVersions = true; // Yanýtlarda desteklenen versiyonlarý bildir (api-supported-versions header)
    options.ApiVersionReader = ApiVersionReader.Combine( // Versiyonun nasýl okunacaðýný belirle
        new UrlSegmentApiVersionReader(), // /api/v1/...
        new HeaderApiVersionReader("x-api-version"), // x-api-version header
        new MediaTypeApiVersionReader("ver")); // accept header (örn: application/json;ver=1.0)
}).AddApiExplorer(options => // Swagger ile entegrasyon için
{
    options.GroupNameFormat = "'v'VVV"; // Swagger UI'da grup adý formatý: v1, v2 etc.
    options.SubstituteApiVersionInUrl = true; // URL'deki {version} parametresini deðiþtir
});



// CORS Ayarlarý
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", // Politika adý
        policy =>
        {
            policy.WithOrigins("http://localhost:7057", "http://localhost:5229", "https://myfrontend.com") // Ýzin verilen frontend adresleri
                  .AllowAnyHeader() // Tüm header'lara izin ver
                  .AllowAnyMethod(); // Tüm HTTP metotlarýna izin ver (GET, POST, PUT, DELETE vb.)
                                     // .AllowCredentials(); // Cookie/Authorization header göndermek için (WithOrigins("*") ile kullanýlamaz)
        });
    options.AddPolicy("AllowAll", // Daha esnek ama daha az güvenli bir politika
       policy =>
       {
           policy.AllowAnyOrigin()
                 .AllowAnyHeader()
                 .AllowAnyMethod();
       });
});



// Controller'lar ve API Davranýþlarý
builder.Services.AddControllers()
    .AddJsonOptions(options => // JSON serileþtirme ayarlarý
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles; // Döngüsel referanslarý yok say
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull; // Null deðerleri JSON'a yazma
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // camelCase kullan
    });

// Swagger / OpenAPI Entegrasyonu
builder.Services.AddEndpointsApiExplorer(); // API Explorer'ý etkinleþtirir
builder.Services.AddSwaggerGen();

// ConfigureSwaggerOptions'ý kaydet. Bu, IApiVersionDescriptionProvider'ý inject edecek.
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>(); 

var app = builder.Build();


// 4. Middleware Pipeline Konfigürasyonu (Sýralama önemlidir!)

// Geliþtirme ortamý ayarlarý
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Detaylý hata sayfasý
    app.UseSwagger(); // Swagger middleware'i
    app.UseSwaggerUI(options => // Swagger UI'ý yapýlandýr
    {
        var provider = app.Services.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
        // Her versiyon için bir endpoint oluþtur
        foreach (var description in provider.ApiVersionDescriptions.Reverse()) // En son versiyon baþta görünsün
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
        // options.RoutePrefix = string.Empty; // Swagger UI'ý kök dizinde sunmak için
    });
}
else // Üretim ortamý ayarlarý
{
    // app.UseExceptionHandler("/Error"); // Özel bir hata sayfasýna yönlendirme (MVC/Razor)
    app.UseHsts(); // HTTP Strict Transport Security
}

// Özel Exception Handling Middleware'imiz
app.UseMiddleware<ExceptionMiddleware>();


app.UseHttpsRedirection();

app.UseRouting(); // Routing middleware'i (Authentication/Authorization'dan ÖNCE, CORS'tan SONRA)

// CORS Middleware'i (UseRouting'den sonra, UseAuthorization/UseEndpoints'ten önce)
app.UseCors("AllowSpecificOrigin"); // Tanýmladýðýmýz politikayý kullan
// app.UseCors("AllowAll"); // Veya daha esnek politika

// Rate Limiting Middleware'i (Genellikle Authentication/Authorization'dan önce)
app.UseIpRateLimiting();

// Authentication & Authorization (Varsa buraya eklenir)
// app.UseAuthentication();
 app.UseAuthorization();

app.MapControllers(); // Controller endpoint'lerini eþleþtir

// Uygulama baþladýðýnda veritabanýný oluþtur ve seed et (InMemory için)
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    dbContext.Database.EnsureCreated(); // Veritabaný yoksa oluþturur (Migration kullanmýyorsak)
//}

app.Run();
