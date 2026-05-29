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

		if (!string.IsNullOrEmpty(connectionString))
		{
			cfg.Host(connectionString);
		}

		cfg.ConfigureEndpoints(context);
	});
});

builder.AddNpgsqlDbContext<PromptProcessingAppDbContext>("PromptDb");

var rawConnectionString = builder.Configuration.GetConnectionString("ollama-phi3")
                          ?? builder.Configuration.GetConnectionString("phi3")
                          ?? builder.Configuration.GetConnectionString("ollama")
                          ?? "http://localhost:11434";

string endpointUrl = rawConnectionString;

if (rawConnectionString.Contains("Endpoint=", StringComparison.OrdinalIgnoreCase))
{
	var parts = rawConnectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
	var endpointPart = parts.FirstOrDefault(p => p.StartsWith("Endpoint=", StringComparison.OrdinalIgnoreCase));

	if (endpointPart != null)
	{
		endpointUrl = endpointPart.Substring("Endpoint=".Length);
	}
}

endpointUrl = endpointUrl.TrimEnd('/');
var aiHttpClient = new HttpClient
{
	Timeout = TimeSpan.FromMinutes(2)
};

builder.Services.AddKernel()
	.AddOpenAIChatCompletion(
		modelId: "phi3",
		apiKey: "not-required",
		endpoint: new Uri($"{endpointUrl}/v1"),
		httpClient: aiHttpClient
	);

builder.AddServiceDefaults();

var host = builder.Build();
host.Run();