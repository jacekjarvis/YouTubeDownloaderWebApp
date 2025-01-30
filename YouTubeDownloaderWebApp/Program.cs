using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using YouTubeDownloaderWebApp.Utility;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IYoutubeDownloader, YoutubeExplodeDownloader>();
builder.Services.AddSingleton<CleanUp>();

// Configure data protection to persist keys to a specific directory
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")))
    .SetApplicationName("JarvoYouTubeDownloader")
    .AddKeyManagementOptions(options => options.NewKeyLifetime = TimeSpan.FromDays(90))
    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });

//Configure to either obtain user consent before setting cookies or to mark session cookies as essential
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false; // or true, based on your requirement
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Add session services with a custom timeout (e.g., 30 minutes).
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(General.SessionAndFileExpiryTime);
    options.Cookie.Name = ".JarvoYouTubeDownloader.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
//CleanUp();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Enable session middleware.
app.UseMiddleware<SessionCleanupMiddleware>();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Perform cleanup at application startup
//void CleanUp()
//{
//    using (var scope = app.Services.CreateScope())
//    {
//        var cleanUp = scope.ServiceProvider.GetRequiredService<CleanUp>();
//        cleanUp.ClearTempDownloads();
//    }
//}