// #region HEADER
// 
// // Copyright Grail Team 2025
// 
// #endregion

namespace PromptProcessing.Common;

public class PromptModel
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string Content { get; set; } = string.Empty;
	public string? Result { get; set; }
	public PromptStatus Status { get; set; } = PromptStatus.Pending;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? ModifiedAt { get; set; }
	
}