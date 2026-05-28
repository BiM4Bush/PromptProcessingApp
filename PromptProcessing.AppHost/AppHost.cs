var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging");
var postgres = builder.AddPostgres("postgres");
var postgresDb = postgres.AddDatabase("PromptDb");

var api = builder.AddProject<Projects.PromptProcessing_API>("api")
	.WithReference(messaging)
	.WithReference(postgresDb);

var worker = builder.AddProject<Projects.PromptProcessing_WorkerService>("worker")
	.WithReference(messaging)
	.WithReference(postgresDb);

builder.Build().Run();