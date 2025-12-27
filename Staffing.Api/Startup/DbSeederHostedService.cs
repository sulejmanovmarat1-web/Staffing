using Microsoft.EntityFrameworkCore;
using Staffing.Domain.Entities;
using Staffing.Infrastructure.Persistence;

namespace Staffing.Api.Startup;

public sealed class DbSeederHostedService : IHostedService
{
    private readonly IServiceProvider _sp;

    public DbSeederHostedService(IServiceProvider sp) => _sp = sp;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffingDbContext>();

        // Убедимся, что БД актуальна
        await db.Database.MigrateAsync(cancellationToken);

        // Seed Positions (если пусто)
        if (!await db.Positions.AnyAsync(cancellationToken))
        {
            db.Positions.AddRange(new[]
            {
                new Position { Name = "Проводники пассажирских вагонов", ReportTemplateKey = "provodniki" },
                new Position { Name = "Начальники пассажирских поездов", ReportTemplateKey = "nachalniki_poezdov" },
                new Position { Name = "Поездные электромеханики", ReportTemplateKey = "poezdnye_elektromekhaniki" },
                new Position { Name = "Слесари по ремонту подвижного состава", ReportTemplateKey = "slesari_rps" },
                new Position { Name = "Осмотрщики-ремонтники вагонов", ReportTemplateKey = "osmotrshchiki_remontniki" }
            });

            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.Divisions.AnyAsync(cancellationToken))
        {
            var root = new Division { NameOriginal = "ГОРЬК", IsTotalLevel = true };
            db.Divisions.Add(root);

            db.Divisions.AddRange(
                new Division { NameOriginal = "ЛВЧД-1", Parent = root },
                new Division { NameOriginal = "ЛВЧД-2", Parent = root }
            );

            await db.SaveChangesAsync(cancellationToken);
        }

    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
