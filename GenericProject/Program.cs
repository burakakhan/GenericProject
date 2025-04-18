

using GenericProject.API.Middlewares; // ExceptionMiddleware
using AspNetCoreRateLimit; // Rate Limiting
using GenericProject.API.Extensions; // Yeni ekledi�imiz s�n�f�n namespace'i


var builder = WebApplication.CreateBuilder(args);

// 1. Logging (Serilog �rne�i - �ste�e ba�l�)
// NuGet: Serilog.AspNetCore, Serilog.Sinks.Console, Serilog.Sinks.File



// 2. Services (DI Container Konfig�rasyonu)
builder.Services.AddServices(builder.Configuration); 




// Controller'lar ve API Davran��lar�
builder.Services.AddControllers()
    .AddJsonOptions(options => // JSON serile�tirme ayarlar�
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles; // D�ng�sel referanslar� yok say
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull; // Null de�erleri JSON'a yazma
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // camelCase kullan
    });




var app = builder.Build();


// 3. Middleware Pipeline Konfig�rasyonu (S�ralama �nemlidir!)

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
