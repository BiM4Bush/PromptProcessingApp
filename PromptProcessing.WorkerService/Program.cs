using MassTransit;
using PromptProcessing.Core.Data;
using PromptProcessing.WorkerService.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMassTransit(x =>
{
	x.AddConsumer<PromptSubmittedConsumer>();

	x.UsingRabbitMq((context, cfg) =>
	{
		var connectionString = builder.Configuration.GetConnectionString("messaging");
		cfg.Host(connectionString);

		cfg.ConfigureEndpoints(context);
	});
});

builder.AddNpgsqlDbContext<PromptProcessingAppDbContext>("PromptDb");
var host = builder.Build();
host.Run();