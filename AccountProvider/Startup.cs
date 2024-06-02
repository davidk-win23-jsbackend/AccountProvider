using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using AccountProvider.Models;
using Data.Contexts;
using Data.Entities;

[assembly: FunctionsStartup(typeof(Startup))]

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging();
        builder.Services.AddHttpContextAccessor();

        // Retrieve configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        builder.Services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("AccountDatabase")));

        builder.Services.AddDefaultIdentity<UserAccount>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
        })
        .AddEntityFrameworkStores<DataContext>();

        builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
    }
}
