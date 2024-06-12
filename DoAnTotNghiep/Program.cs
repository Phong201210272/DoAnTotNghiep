var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
/*builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));*/
builder.Services.AddSession(); // Thêm cấu hình session
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Đảm bảo UseSession trước UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "exportSchedule",
    pattern: "schedule/export",
    defaults: new { controller = "Schedule", action = "ExportScheduleToExcel" });

app.Run();
