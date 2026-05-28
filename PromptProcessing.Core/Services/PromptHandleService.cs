// #region HEADER
// 
// // Copyright Grail Team 2025
// 
// #endregion

using Microsoft.EntityFrameworkCore;
using PromptProcessing.Common;
using PromptProcessing.Core.Data;
using PromptProcessing.Core.Interfaces;

namespace PromptProcessing.Core.Services;

public class PromptHandleService : IPromptHandleService
{
	private readonly PromptProcessingAppDbContext dbContext;

	public PromptHandleService(PromptProcessingAppDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public async Task<PromptDTO.PromptResponse> CreatePromptAsync(PromptDTO.CreatePromptRequest request)
	{
		var task = new PromptModel
		{
			Content = request.PromptContent,
			Status = PromptStatus.Pending,
			CreatedAt = DateTime.Now,
		};

		dbContext.PromptModel.Add(task);
		await dbContext.SaveChangesAsync();

		// TODO: RabbitMQ publish event (w kolejnym kroku wstrzykniemy tu np. IPublishEndpoint)

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