using Microsoft.EntityFrameworkCore;
using GenericProject.Infrastructure.Data;
using GenericProject.Domain.Interfaces.UnitofWork;
using GenericProject.Infrastructure.Data.Repositories; // GenericRepository vs. i�in
using GenericProject.Domain.Interfaces.Repositories;
using GenericProject.Application.Interfaces.Services;
using GenericProject.Application.Services;
using GenericProject.Application.Mapping; // AutoMapperProfile
using FluentValidation; // AddFluentValidation extension
using GenericProject.Application.Validators; // Validator s�n�flar�
using GenericProject.API.Middlewares; // ExceptionMiddleware
using Asp.Versioning; // API Versioning
using Microsoft.OpenApi.Models; // Swagger
using AspNetCoreRateLimit; // Rate Limiting
//using Serilog; // Logging (Opsiyonel ama �nerilir)
using GenericProject.Infrastructure.ExternalServices; // Placeholder servisler
using Asp.Versioning.ApiExplorer; // IApiVersionDescriptionProvider i�in
using GenericProject.API.Configuration; // Yeni ekledi�imiz s�n�f�n namespace'i


var builder = WebApplication.CreateBuilder(args);

// 1. Logging (Serilog �rne�i - �ste�e ba�l�)
// NuGet: Serilog.AspNetCore, Serilog.Sinks.Console, Serilog.Sinks.File
// builder.Host.UseSerilog((context, configuration) =>
//     configuration.ReadFrom.Configuration(context.Configuration));
// Temel .NET Logging'i kullanmak i�in bu k�sm� kald�rabilirsiniz.


// 2. Configuration (Rate Limiting i�in appsettings.json'dan okuma)
builder.Services.AddOptions(); // Rate Limiting i�in gerekli
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));


// 3. Services (DI Container Konfig�rasyonu)

// Infrastructure Katman� Servisleri
builder.Services.AddDbContext<AppDbContext>(options => 
    //options.UseInMemoryDatabase("ECommerceAppDb")); // �rnek i�in InMemory DB
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); // UnitOfWork (Scoped �nerilir)
// Repository'ler UnitOfWork �zerinden eri�ildi�i i�in genellikle direkt register edilmez.
// Ancak isterseniz register edebilirsiniz:
// builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
// builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Placeholder External Services
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Application Katman� Servisleri
builder.Services.AddScoped<IProductService, ProductService>(); // Servisler

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile)); // Profilin bulundu�u Assembly'yi tarar

// FluentValidation
//builder.Services.AddFluentValidationAutoValidation(); // Otomatik validasyon middleware'i ekler (�nerilir)
// builder.Services.AddFluentValidationClientsideAdapters(); // �stemci taraf� adapt�rler (MVC/Razor Pages i�in daha relevant)
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>(); // Validator'lar�n bulundu�u Assembly'yi tarar


// Caching
builder.Services.AddMemoryCache(); // InMemory Cache

// Rate Limiting
builder.Services.AddInMemoryRateLimiting(); // Cache ile birlikte �al���r
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>(); // Rate Limit konfig�rasyonunu y�kler

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // Varsay�lan versiyon v1.0
    options.AssumeDefaultVersionWhenUnspecified = true; // Versiyon belirtilmezse varsay�lan� kullan
    options.ReportApiVersions = true; // Yan�tlarda desteklenen versiyonlar� bildir (api-supported-versions header)
    options.ApiVersionReader = ApiVersionReader.Combine( // Versiyonun nas�l okunaca��n� belirle
        new UrlSegmentApiVersionReader(), // /api/v1/...
        new HeaderApiVersionReader("x-api-version"), // x-api-version header
        new MediaTypeApiVersionReader("ver")); // accept header (�rn: application/json;ver=1.0)
}).AddApiExplorer(options => // Swagger ile entegrasyon i�in
{
    options.GroupNameFormat = "'v'VVV"; // Swagger UI'da grup ad� format�: v1, v2 etc.
    options.SubstituteApiVersionInUrl = true; // URL'deki {version} parametresini de�i�tir
});



// CORS Ayarlar�
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", // Politika ad�
        policy =>
        {
            policy.WithOrigins("http://localhost:7057", "http://localhost:5229", "https://myfrontend.com") // �zin verilen frontend adresleri
                  .AllowAnyHeader() // T�m header'lara izin ver
                  .AllowAnyMethod(); // T�m HTTP metotlar�na izin ver (GET, POST, PUT, DELETE vb.)
                                     // .AllowCredentials(); // Cookie/Authorization header g�ndermek i�in (WithOrigins("*") ile kullan�lamaz)
        });
    options.AddPolicy("AllowAll", // Daha esnek ama daha az g�venli bir politika
       policy =>
       {
           policy.AllowAnyOrigin()
                 .AllowAnyHeader()
                 .AllowAnyMethod();
       });
});



// Controller'lar ve API Davran��lar�
builder.Services.AddControllers()
    .AddJsonOptions(options => // JSON serile�tirme ayarlar�
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles; // D�ng�sel referanslar� yok say
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull; // Null de�erleri JSON'a yazma
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // camelCase kullan
    });

// Swagger / OpenAPI Entegrasyonu
builder.Services.AddEndpointsApiExplorer(); // API Explorer'� etkinle�tirir
builder.Services.AddSwaggerGen();

// ConfigureSwaggerOptions'� kaydet. Bu, IApiVersionDescriptionProvider'� inject edecek.
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>(); 

var app = builder.Build();


// 4. Middleware Pipeline Konfig�rasyonu (S�ralama �nemlidir!)

// Geli�tirme ortam� ayarlar�
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Detayl� hata sayfas�
    app.UseSwagger(); // Swagger middleware'i
    app.UseSwaggerUI(options => // Swagger UI'� yap�land�r
    {
        var provider = app.Services.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
        // Her versiyon i�in bir endpoint olu�tur
        foreach (var description in provider.ApiVersionDescriptions.Reverse()) // En son versiyon ba�ta g�r�ns�n
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
        // options.RoutePrefix = string.Empty; // Swagger UI'� k�k dizinde sunmak i�in
    });
}
else // �retim ortam� ayarlar�
{
    // app.UseExceptionHandler("/Error"); // �zel bir hata sayfas�na y�nlendirme (MVC/Razor)
    app.UseHsts(); // HTTP Strict Transport Security
}

// �zel Exception Handling Middleware'imiz
app.UseMiddleware<ExceptionMiddleware>();


app.UseHttpsRedirection();

app.UseRouting(); // Routing middleware'i (Authentication/Authorization'dan �NCE, CORS'tan SONRA)

// CORS Middleware'i (UseRouting'den sonra, UseAuthorization/UseEndpoints'ten �nce)
app.UseCors("AllowSpecificOrigin"); // Tan�mlad���m�z politikay� kullan
// app.UseCors("AllowAll"); // Veya daha esnek politika

// Rate Limiting Middleware'i (Genellikle Authentication/Authorization'dan �nce)
app.UseIpRateLimiting();

// Authentication & Authorization (Varsa buraya eklenir)
// app.UseAuthentication();
 app.UseAuthorization();

app.MapControllers(); // Controller endpoint'lerini e�le�tir

// Uygulama ba�lad���nda veritaban�n� olu�tur ve seed et (InMemory i�in)
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    dbContext.Database.EnsureCreated(); // Veritaban� yoksa olu�turur (Migration kullanm�yorsak)
//}

app.Run();
