using Microsoft.EntityFrameworkCore;
using XmasWishes.Models.account;
using XmasWishes.Models.persons;
using XmasWishes.Models.wishes;

namespace XmasWishes;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<PersonDbContext>(options =>
            options.UseMySQL(builder.Configuration.GetConnectionString("MYSQL"))
        );
        builder.Services.AddDbContext<AccountDbContext>(options =>
            options.UseMySQL(builder.Configuration.GetConnectionString("MYSQL"))
        );
        builder.Services.AddDbContext<WishesDbContext>(options =>
            options.UseMySQL(builder.Configuration.GetConnectionString("MYSQL"))
        );

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddControllers();
        builder.Services.AddHttpClient();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        app.MapStaticAssets();
        
        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            ).WithStaticAssets();
        
        app.MapControllerRoute(
            name: "make-a-wish",
            pattern: "gui/{action=MakeAWish}",
            defaults: new { controller = "Gui", action = "MakeAWish" });
        
        
        app.Run();
    }
}