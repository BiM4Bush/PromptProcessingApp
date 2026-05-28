// #region HEADER
// 
// // Copyright Grail Team 2025
// 
// #endregion

using PromptProcessing.Common;

namespace PromptProcessing.Core.Interfaces;

public interface IPromptHandleService
{
	Task<PromptDTO.PromptResponse> CreatePromptAsync(PromptDTO.CreatePromptRequest request);
	Task<IEnumerable<PromptDTO.PromptResponse>> GetAllPromptsAsync();
	Task<PromptDTO.PromptResponse?> GetPromptByIdAsync(Guid id);
}