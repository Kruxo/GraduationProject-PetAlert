using GraduationProject.Models;
using GraduationProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetAlert.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<LocationService>();
builder.Services.AddHttpClient<ChatbotService>();

builder.Services.AddScoped<LocationService>(); // Ensure LocationService is properly registered
builder.Services.AddScoped<ChatbotService>();  // Ensure ChatbotService is properly registered
builder.Services.AddLogging(); // Logging should be added

// Register ApplicationDbContext properly
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AzureDb")));


builder.Services.AddDefaultIdentity<IdentityUser>()
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI();

builder.Services.AddTransient<DatabaseSetupService>();
builder.Services.AddTransient<UserService>();



builder.Services.AddHttpClient<LocationService>();
builder.Services.AddScoped<LocationService>();

var app = builder.Build();


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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var databaseSetupService = scope.ServiceProvider.GetRequiredService<DatabaseSetupService>();
    await databaseSetupService.InitializeAsync();
}
app.Run();
