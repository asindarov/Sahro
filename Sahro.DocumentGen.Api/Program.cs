using Microsoft.EntityFrameworkCore;
using Sahro.DocumentGen.Api.Extensions;
using Sahro.DocumentGen.Api.HostedServices;
using Shared;
using Shared.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("SahroDb");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.RegisterBlobStorage<TemporaryBlobStorageConfiguration>(nameof(TemporaryBlobStorageConfiguration), "temporary");

builder.Services.AddHostedService<StartupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();