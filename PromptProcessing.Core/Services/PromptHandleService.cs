// #region HEADER
// 
// // Copyright Sebastian Krzynówek 2025
// 
// #endregion

using MassTransit;
using Microsoft.EntityFrameworkCore;
using PromptProcessing.Common;
using PromptProcessing.Core.Data;
using PromptProcessing.Core.Interfaces;
using Event = PromptProcessing.Common.Event;

namespace PromptProcessing.Core.Services;

public class PromptHandleService : IPromptHandleService
{
	private readonly PromptProcessingAppDbContext dbContext;
	private readonly IPublishEndpoint publishEndpoint;

	public PromptHandleService(PromptProcessingAppDbContext dbContext, IPublishEndpoint publishEndpoint)
	{
		this.dbContext = dbContext;
		this.publishEndpoint = publishEndpoint;
	}
	
	public async Task<PromptDTO.PromptResponse> CreatePromptAsync(PromptDTO.CreatePromptRequest request)
	{
		var task = new PromptModel
		{
			Content = request.PromptContent,
			Status = PromptStatus.Pending,
			CreatedAt = DateTime.UtcNow,
		};

		dbContext.PromptModel.Add(task);
		await dbContext.SaveChangesAsync();

		await publishEndpoint.Publish(new Event.PromptSubmittedEvent(task.Id));

		return new PromptDTO.PromptResponse(
			task.Id,
			task.Content,
			task.Result,
			task.Status.ToString(),
			task.CreatedAt
		);
	}

	public async Task<IEnumerable<PromptDTO.PromptResponse>> GetAllPromptsAsync()
	{
		return await dbContext.PromptModel.OrderByDescending(x => x.CreatedAt).Select(x =>
			new PromptDTO.PromptResponse(x.Id, x.Content, x.Result, x.Status.ToString(), x.CreatedAt)).ToListAsync();
	}

	public async Task<PromptDTO.PromptResponse?> GetPromptByIdAsync(Guid id)
	{
		var task = await dbContext.PromptModel.FindAsync(id);
		
		if (task == null)
		{
			return null;
		}

		return new PromptDTO.PromptResponse(
			task.Id,
			task.Content,
			task.Result,
			task.Status.ToString(),
			task.CreatedAt
		);
	}
}