using EventAz.Domain.Entities;
using EventAz.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventAz.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<TicketTier> TicketTiers => Set<TicketTier>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Identity cədvəlləri üçün vacibdir

        builder.Entity<Event>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(150).IsRequired();
            e.Property(x => x.Location).HasMaxLength(200).IsRequired();
            e.HasOne(x => x.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TicketTier>(e =>
        {
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Event)
                .WithMany(ev => ev.Tiers)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Ticket>(e =>
        {
            e.Property(x => x.PricePaid).HasColumnType("decimal(18,2)");
            e.HasIndex(x => x.QRCode).IsUnique();
            e.HasOne(x => x.TicketTier)
                .WithMany(t => t.Tickets)
                .HasForeignKey(x => x.TicketTierId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PromoCode)
                .WithMany()
                .HasForeignKey(x => x.PromoCodeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<PromoCode>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.DiscountPercent).HasColumnType("decimal(5,2)");
            e.HasIndex(x => x.Code).IsUnique();
            e.HasOne(x => x.Event)
                .WithMany()
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Review>(e =>
        {
            e.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            e.HasIndex(x => new { x.UserId, x.EventId }).IsUnique();
            e.HasOne(x => x.Event)
                .WithMany(ev => ev.Reviews)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Favorite>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.EventId }).IsUnique(); // eyni event 2 dəfə favorit olunmasın
            e.HasOne(x => x.Event)
                .WithMany(ev => ev.FavoritedBy)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
