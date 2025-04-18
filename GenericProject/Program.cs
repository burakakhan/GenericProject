

using GenericProject.API.Middlewares; // ExceptionMiddleware
using AspNetCoreRateLimit; // Rate Limiting
using GenericProject.API.Extensions; // Yeni eklediðimiz sýnýfýn namespace'i


var builder = WebApplication.CreateBuilder(args);

// 1. Logging (Serilog Örneði - Ýsteðe baðlý)
// NuGet: Serilog.AspNetCore, Serilog.Sinks.Console, Serilog.Sinks.File



// 2. Services (DI Container Konfigürasyonu)
builder.Services.AddServices(builder.Configuration); 




// Controller'lar ve API Davranýþlarý
builder.Services.AddControllers()
    .AddJsonOptions(options => // JSON serileþtirme ayarlarý
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles; // Döngüsel referanslarý yok say
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull; // Null deðerleri JSON'a yazma
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // camelCase kullan
    });




var app = builder.Build();


// 3. Middleware Pipeline Konfigürasyonu (Sýralama önemlidir!)

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
