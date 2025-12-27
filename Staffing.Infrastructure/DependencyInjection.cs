using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Staffing.Infrastructure.Excel;
using Staffing.Infrastructure.Persistence;

namespace Staffing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<StaffingDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("Db")));
        services.AddScoped<ExcelImportProcessor>();
        return services;
    }
}
