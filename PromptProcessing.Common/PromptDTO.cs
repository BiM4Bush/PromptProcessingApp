// #region HEADER
// 
// // Copyright Grail Team 2025
// 
// #endregion

namespace PromptProcessing.Common;

public class PromptDTO
{
	public record CreatePromptRequest(string PromptContent);

	public record PromptResponse(
		Guid PromptId,
		string PromptContent,
		string? Result,
		string Status,
		DateTime CreatedAt);
}