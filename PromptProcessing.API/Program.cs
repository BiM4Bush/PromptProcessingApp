using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PromptProcessing.Core.Data;
using PromptProcessing.Core.Interfaces;
using PromptProcessing.Core.Services;
using PromptProcessing.WorkerService.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers()
	.AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

builder.Services.AddScoped<IPromptHandleService, PromptHandleService>();

builder.AddNpgsqlDbContext<PromptProcessingAppDbContext>("PromptDb");

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.WithOrigins("http://localhost:5173")
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

builder.Services.AddMassTransit(x =>
{
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


var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<PromptProcessingAppDbContext>();
	await db.Database.EnsureCreatedAsync();
}

app.Run();