using Microsoft.EntityFrameworkCore;
using Asp.Versioning; // API Versioning
using AspNetCoreRateLimit; // Rate Limiting
using FluentValidation; // Validation
using GenericProject.API.Configuration; // Swagger Configuration
using GenericProject.Application.Interfaces.Services; // Servis Interfaces
using GenericProject.Application.Mapping;   // AutoMapperProfile
using GenericProject.Application.Services;  // Servisler
using GenericProject.Application.Validators; // Validator sınıfları
using GenericProject.Domain.Interfaces.UnitofWork; // UnitOfWork Interface
using GenericProject.Infrastructure.Data; // Entity Framework DbContext
using GenericProject.Infrastructure.ExternalServices; // Placeholder servisler


namespace GenericProject.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {

            // Infrastructure Katmanı Servisleri
            services.AddDbContext<AppDbContext>(options =>
                //options.UseInMemoryDatabase("ECommerceAppDb")); // Örnek için InMemory DB
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Repository'ler UnitOfWork üzerinden erişildiği için genellikle direkt register edilmez.
            services.AddScoped<IUnitOfWork, UnitOfWork>(); // UnitOfWork (Scoped önerilir)


            // Placeholder External Services
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IPaymentService, PaymentService>();

            // Application Katmanı Servisleri
            services.AddScoped<IProductService, ProductService>(); // Servisler

            // AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile)); // Profilin bulunduğu Assembly'yi tarar

            // FluentValidation
            //builder.Services.AddFluentValidationAutoValidation(); // Otomatik validasyon middleware'i ekler (önerilir)
            //builder.Services.AddFluentValidationClientsideAdapters(); // İstemci tarafı adaptörler (MVC/Razor Pages için daha relevant)
            services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>(); // Validator'ların bulunduğu Assembly'yi tarar


            // Caching
            services.AddMemoryCache(); // InMemory Cache

            // Rate Limiting
            services.AddOptions(); // Rate Limiting için gerekli
            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

            services.AddInMemoryRateLimiting(); // Cache ile birlikte çalışır
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>(); // Rate Limit konfigürasyonunu yükler

            // API Versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0); // Varsayılan versiyon v1.0
                options.AssumeDefaultVersionWhenUnspecified = true; // Versiyon belirtilmezse varsayılanı kullan
                options.ReportApiVersions = true; // Yanıtlarda desteklenen versiyonları bildir (api-supported-versions header)
                options.ApiVersionReader = ApiVersionReader.Combine( // Versiyonun nasıl okunacağını belirle
                    new UrlSegmentApiVersionReader(), // /api/v1/...
                    new HeaderApiVersionReader("x-api-version"), // x-api-version header
                    new MediaTypeApiVersionReader("ver")); // accept header (örn: application/json;ver=1.0)
            }).AddApiExplorer(options => // Swagger ile entegrasyon için
            {
                options.GroupNameFormat = "'v'VVV"; // Swagger UI'da grup adı formatı: v1, v2 etc.
                options.SubstituteApiVersionInUrl = true; // URL'deki {version} parametresini değiştir
            });


            // CORS Ayarları
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", // Politika adı
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:7057", "http://localhost:5229", "https://myfrontend.com") // İzin verilen frontend adresleri
                              .AllowAnyHeader() // Tüm header'lara izin ver
                              .AllowAnyMethod(); // Tüm HTTP metotlarına izin ver (GET, POST, PUT, DELETE vb.)
                                                 // .AllowCredentials(); // Cookie/Authorization header göndermek için (WithOrigins("*") ile kullanılamaz)
                    });
                options.AddPolicy("AllowAll", // Daha esnek ama daha az güvenli bir politika
                   policy =>
                   {
                       policy.AllowAnyOrigin()
                             .AllowAnyHeader()
                             .AllowAnyMethod();
                   });
            });

            // Swagger / OpenAPI Entegrasyonu
            services.AddEndpointsApiExplorer(); // API Explorer'ı etkinleştirir
            services.AddSwaggerGen();



            // ConfigureSwaggerOptions'ı kaydet. Bu, IApiVersionDescriptionProvider'ı inject edecek.
            services.ConfigureOptions<ConfigureSwaggerOptions>();

            // Diğer servisler
        }
    }
}
