// #region HEADER
// 
// // Copyright Sebastian Krzynówek 2025
// 
// #endregion

using Microsoft.EntityFrameworkCore;
using PromptProcessing.Common;

namespace PromptProcessing.Core.Data;

public class PromptProcessingAppDbContext : DbContext
{
	public PromptProcessingAppDbContext(DbContextOptions<PromptProcessingAppDbContext> options) : base(options)
	{
	}

	public DbSet<PromptModel> PromptModel => Set<PromptModel>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.Entity<PromptModel>().Property(x => x.Status).HasConversion<string>();
	}
}