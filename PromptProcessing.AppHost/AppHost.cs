var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging");
var postgres = builder.AddPostgres("postgres");
var postgresDb = postgres.AddDatabase("PromptDb");
var ollama = builder.AddOllama("ollama")
	.AddModel("phi3");

var api = builder.AddProject<Projects.PromptProcessing_API>("api")
	.WithReference(messaging)
	.WithReference(postgresDb)
	.WaitFor(messaging)
	.WaitFor(postgresDb);

var worker = builder.AddProject<Projects.PromptProcessing_WorkerService>("worker")
	.WithReference(messaging)
	.WithReference(postgresDb)
	.WithReference(ollama)
	.WaitFor(messaging)
	.WaitFor(postgresDb);

builder.Build().Run();