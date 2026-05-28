// #region HEADER
// 
// // Copyright Grail Team 2025
// 
// #endregion

using Microsoft.AspNetCore.Mvc;
using PromptProcessing.Common;
using PromptProcessing.Core.Interfaces;

namespace PromptProcessing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromptsController : ControllerBase
{
	private readonly IPromptHandleService promptHandleService;

	public PromptsController(IPromptHandleService promptHandleService)
	{
		this.promptHandleService = promptHandleService;
	}

	[HttpPost]
	public async Task<IActionResult> CreatePrompt([FromBody] PromptDTO.CreatePromptRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.PromptContent))
		{
			return BadRequest("Prompt content cannot be empty");
		}

		var response = await promptHandleService.CreatePromptAsync(request);

		return CreatedAtAction(nameof(GetPromptById), new { id = response.PromptId }, response);
	}

	[HttpGet]
	public async Task<IActionResult> GetAllPrompts()
	{
		var responses = await promptHandleService.GetAllPromptsAsync();
		return Ok(responses);
	}

	[HttpGet("{id:guid}")]
	public async Task<IActionResult> GetPromptById(Guid id)
	{
		var response = await promptHandleService.GetPromptByIdAsync(id);

		if (response == null)
		{
			return NotFound();
		}

		return Ok(response);
	}
}