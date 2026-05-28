// #region HEADER
// 
// // Copyright Grail Team 2025
// 
// #endregion

using MassTransit;
using Microsoft.SemanticKernel;
using PromptProcessing.Common;
using PromptProcessing.Core.Data;
using Event = PromptProcessing.Common.Event;

namespace PromptProcessing.WorkerService.Consumers;

public class PromptSubmittedConsumer : IConsumer<Event.PromptSubmittedEvent>
{
	private readonly PromptProcessingAppDbContext dbContext;
	private readonly Kernel kernel;
	private readonly ILogger<PromptSubmittedConsumer> logger;

	public PromptSubmittedConsumer(PromptProcessingAppDbContext dbContext, Kernel kernel,
		ILogger<PromptSubmittedConsumer> logger)
	{
		this.dbContext = dbContext;
		this.kernel = kernel;
		this.logger = logger;
	}

	public async Task Consume(ConsumeContext<Event.PromptSubmittedEvent> context)
	{
		var promptId = context.Message.PromptId;
		logger.LogInformation("Worker received event for Prompt ID: {PromptId}", promptId);

		var promptTask = await dbContext.PromptModel.FindAsync(promptId);

		if (promptTask == null)
		{
			logger.LogWarning("No task with ID found: {PromptId}", promptId);
			return;
		}

		promptTask.Status = PromptStatus.Processing;
		promptTask.ModifiedAt = DateTime.UtcNow;
		await dbContext.SaveChangesAsync();

		try
		{
			logger.LogInformation("Send the LLM model with prompt: {Prompt}", promptTask.Content);

			var result = await kernel.InvokePromptAsync(promptTask.Content);

			promptTask.Result = result.GetValue<string>();
			promptTask.Status = PromptStatus.Completed;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error communicating with LLM for Prompt ID: {PromptId}", promptId);

			promptTask.Status = PromptStatus.Failed;
			promptTask.Result = $"Model error: {ex.Message}";
		}
		finally
		{
			promptTask.ModifiedAt = DateTime.UtcNow;
			await dbContext.SaveChangesAsync();
			logger.LogInformation("Prompt ID processing completed: {PromptId} with status: {Status}", promptId,
				promptTask.Status);
		}
	}
}