using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Locations;
using Selu383.SP26.Api.Features.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")));

// Identity Configuration
builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<DataContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401; // No redirect for API
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migrate and Seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<DataContext>();
    db.Database.Migrate();

    var roleManager = services.GetRequiredService<RoleManager<Role>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    if (!roleManager.Roles.Any())
    {
        await roleManager.CreateAsync(new Role { Name = "Admin" });
        await roleManager.CreateAsync(new Role { Name = "User" });
    }

    if (!userManager.Users.Any())
    {
        const string pass = "Password123!";
        var galkadi = new User { UserName = "galkadi" };
        await userManager.CreateAsync(galkadi, pass);
        await userManager.AddToRoleAsync(galkadi, "Admin");

        var bob = new User { UserName = "bob" };
        await userManager.CreateAsync(bob, pass);
        await userManager.AddToRoleAsync(bob, "User");

        var sue = new User { UserName = "sue" };
        await userManager.CreateAsync(sue, pass);
        await userManager.AddToRoleAsync(sue, "User");
    }

    if (!db.Locations.Any())
    {
        db.Locations.AddRange(
            new Location { Name = "Location 1", Address = "123 Main St", TableCount = 10 },
            new Location { Name = "Location 2", Address = "456 Oak Ave", TableCount = 20 },
            new Location { Name = "Location 3", Address = "789 Pine Ln", TableCount = 15 }
        );
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // Ensure Authentication comes before Authorization
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }