using Microsoft.EntityFrameworkCore;
using Sahro.DocumentGen.Worker;
using Sahro.DocumentGen.Worker.Extensions;
using Shared;
using Shared.Configurations;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("SahroDb");
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddHostedService<Worker>();
    })
    .RegisterBlobStorage<TemporaryBlobStorageConfiguration>(nameof(TemporaryBlobStorageConfiguration), "temporary")
    .Build();

host.Run();
