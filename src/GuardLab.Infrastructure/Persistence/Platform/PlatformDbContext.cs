using GuardLab.Core.Labs;
using GuardLab.Core.AdminAuth;
using Microsoft.EntityFrameworkCore;

namespace GuardLab.Infrastructure.Persistence.Platform;

public sealed class PlatformDbContext(DbContextOptions<PlatformDbContext> options) : DbContext(options)
{
    public DbSet<Lab> Labs => Set<Lab>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var lab = modelBuilder.Entity<Lab>();
        lab.ToTable("labs", "platform");
        lab.HasKey(x => x.Id);
        lab.Property(x => x.Id).HasColumnName("id");
        lab.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
        lab.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        lab.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(1000).IsRequired();
        lab.Property(x => x.Theory).HasColumnName("theory").IsRequired();
        lab.Property(x => x.Status).HasColumnName("status").HasColumnType("smallint").IsRequired();
        lab.HasIndex(x => x.Slug).IsUnique();
        lab.HasIndex(x => x.Status);

        var admin = modelBuilder.Entity<AdminUser>();
        admin.ToTable("admin_users", "platform");
        admin.HasKey(x => x.Id);
        admin.Property(x => x.Id).HasColumnName("id");
        admin.Property(x => x.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        admin.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
        admin.Property(x => x.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
        admin.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        admin.HasIndex(x => x.Email).IsUnique();
    }
}
