using GuardLab.Core.Sandbox;
using Microsoft.EntityFrameworkCore;

namespace GuardLab.Infrastructure.Persistence.Sandbox;

public sealed class SandboxDbContext(DbContextOptions<SandboxDbContext> options) : DbContext(options)
{
    public DbSet<SandboxAccount> Accounts => Set<SandboxAccount>();
    public DbSet<SandboxApiKey> ApiKeys => Set<SandboxApiKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var account = modelBuilder.Entity<SandboxAccount>();
        account.ToTable("accounts", "sandbox");
        account.HasKey(x => x.Id);
        account.Property(x => x.Id).HasColumnName("id");
        account.Property(x => x.Alias).HasColumnName("alias").HasMaxLength(100).IsRequired();
        account.HasIndex(x => x.Alias).IsUnique();

        var apiKey = modelBuilder.Entity<SandboxApiKey>();
        apiKey.ToTable("api_keys", "sandbox");
        apiKey.HasKey(x => x.Id);
        apiKey.Property(x => x.Id).HasColumnName("id");
        apiKey.Property(x => x.AccountId).HasColumnName("account_id").IsRequired();
        apiKey.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(100).IsRequired();
        apiKey.Property(x => x.MaskedValue).HasColumnName("masked_value").HasMaxLength(64).IsRequired();
        apiKey.HasIndex(x => x.AccountId);
        apiKey.HasOne<SandboxAccount>()
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
