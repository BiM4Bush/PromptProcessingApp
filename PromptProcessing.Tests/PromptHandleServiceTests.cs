// #region HEADER
// 
// // Copyright Grail Team 2025
// 
// #endregion

using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using PromptProcessing.Common;
using PromptProcessing.Core.Data;
using PromptProcessing.Core.Services;
using Event = PromptProcessing.Common.Event;

namespace PromptProcessing.Tests;

public class PromptHandleServiceTests
{
	[Fact]
	public async Task CreatePromptAsync_Should_SaveToDb_And_Publish_Event()
	{
		// 1. ARRANGE
		var dbOptions = new DbContextOptionsBuilder<PromptProcessingAppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		using var dbContext = new PromptProcessingAppDbContext(dbOptions);

		var mockPublishEndpoint = new Mock<IPublishEndpoint>();

		var service = new PromptHandleService(dbContext, mockPublishEndpoint.Object);

		var request = new PromptDTO.CreatePromptRequest("Tell me a joke about developers.");

		// 2. ACT
		var response = await service.CreatePromptAsync(request);

		// 3. ASSERT
		Assert.NotEqual(Guid.Empty, response.PromptId);
		Assert.Equal("Pending", response.Status);

		var savedTask = await dbContext.PromptModel.FindAsync(response.PromptId);
		Assert.NotNull(savedTask);
		Assert.Equal(request.PromptContent, savedTask.Content);

		mockPublishEndpoint.Verify(
			publish => publish.Publish(
				It.Is<Event.PromptSubmittedEvent>(e => e.PromptId == response.PromptId),
				It.IsAny<CancellationToken>()),
			Times.Once);
	}
}