// #region HEADER
// 
// // Copyright Sebastian Krzynówek 2025
// 
// #endregion

using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using PromptProcessing.Core.Data;
using PromptProcessing.WorkerService.Consumers;
using Event = PromptProcessing.Common.Event;

namespace PromptProcessing.Tests;

public class PromptSubmittedConsumerTests
{
	[Fact]
	public async Task Consumer_Should_Receive_Message_And_Trigger_AI_Processing()
	{
		// 1. ARRANGE
		var dbOptions = new DbContextOptionsBuilder<PromptProcessingAppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		var kernel = Kernel.CreateBuilder().Build();
		
		await using var provider = new ServiceCollection()
			.AddSingleton(new PromptProcessingAppDbContext(dbOptions))
			.AddSingleton(kernel)
			.AddMassTransitTestHarness(x => { x.AddConsumer<PromptSubmittedConsumer>(); })
			.BuildServiceProvider(true);

		var harness = provider.GetRequiredService<ITestHarness>();
		await harness.Start();

		try
		{
			var testEvent = new Event.PromptSubmittedEvent(Guid.NewGuid());

			// 2. ACT
			await harness.Bus.Publish(testEvent);

			// 3. ASSERT
			Assert.True(await harness.Published.Any<Event.PromptSubmittedEvent>());
			var consumerHarness = harness.GetConsumerHarness<PromptSubmittedConsumer>();
			Assert.True(await consumerHarness.Consumed.Any<Event.PromptSubmittedEvent>());
		}
		finally
		{
			await harness.Stop();
		}
	}
}