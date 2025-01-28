using YouTubeDownloaderWebApp.Utility;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IYoutubeDownloader, YoutubeExplodeDownloader>();
builder.Services.AddSingleton<CleanUp>();

var app = builder.Build();
CleanUp();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


// Perform cleanup at application startup
void CleanUp()
{
    using (var scope = app.Services.CreateScope())
    {
        var cleanUp = scope.ServiceProvider.GetRequiredService<CleanUp>();
        cleanUp.ClearTempDownloads();
    }
}