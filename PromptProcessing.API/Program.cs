using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PromptProcessing.Core.Data;
using PromptProcessing.Core.Interfaces;
using PromptProcessing.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
	.AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
builder.Services.AddScoped<IPromptHandleService, PromptHandleService>();
builder.AddNpgsqlDbContext<PromptProcessingAppDbContext>("PromptDb");
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy => { policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
});

builder.Services.AddMassTransit(x =>
{
	x.UsingRabbitMq((context, cfg) =>
	{
		var connectionString = builder.Configuration.GetConnectionString("messaging");
		cfg.Host(connectionString);
		cfg.ConfigureEndpoints(context);
	});
});

builder.AddServiceDefaults();

var app = builder.Build();

app.UseCors();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<PromptProcessingAppDbContext>();
	await db.Database.EnsureCreatedAsync();
}

app.Run();