using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Sahro.DocumentGen.Api.HostedServices;

public class StartupService : IHostedService
{
    private readonly IConfiguration _configuration;

    public StartupService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("ServiceBus");
        var serviceBusAdministrationClient = new ServiceBusAdministrationClient(connectionString);

        if (!await serviceBusAdministrationClient.QueueExistsAsync("DocumentGenerated", cancellationToken))
        {
            await serviceBusAdministrationClient.CreateQueueAsync("DocumentGenerated", cancellationToken);
        }
        
        if (!await serviceBusAdministrationClient.QueueExistsAsync("DocumentCreated", cancellationToken))
        {
            await serviceBusAdministrationClient.CreateQueueAsync("DocumentCreated", cancellationToken);
        }


        await using var serviceBusClient = new ServiceBusClient(connectionString);

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
