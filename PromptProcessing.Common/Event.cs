// #region HEADER
// 
// // Copyright Sebastian Krzynówek 2025
// 
// #endregion

namespace PromptProcessing.Common;

public class Event
{
	public record PromptSubmittedEvent(Guid PromptId);
}