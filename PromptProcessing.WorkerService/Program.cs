using MassTransit;
using Microsoft.SemanticKernel;
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

var ollamaEndpoint = builder.Configuration.GetConnectionString("phi3") ?? "http://localhost:11434";

builder.Services.AddKernel()
	.AddOpenAIChatCompletion(
		modelId: "phi3",
		apiKey: "not-required",
		endpoint: new Uri($"{ollamaEndpoint}/v1")
	);
builder.AddServiceDefaults();
var host = builder.Build();
host.Run();