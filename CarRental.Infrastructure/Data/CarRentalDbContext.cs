using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CarRental.Infrastructure.Data
{
    public class CarRentalDbContext : IdentityDbContext<
        User,
        IdentityRole<Guid>,
        Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>
    {
        public CarRentalDbContext(DbContextOptions<CarRentalDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarDocument> CarDocuments { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingItem> BookingItems { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletEntry> WalletEntries { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Brand> Brands { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite keys first
            modelBuilder.Entity<IdentityUserLogin<Guid>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
            modelBuilder.Entity<IdentityUserRole<Guid>>().HasKey(r => new { r.UserId, r.RoleId });
            modelBuilder.Entity<IdentityUserToken<Guid>>().HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            // Then rename the tables
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            // Configure Car entity
            modelBuilder.Entity<Car>(entity =>
            {
                entity.HasIndex(e => e.LicensePlate)
                    .IsUnique();

                entity.Property(e => e.Status)
                    .HasDefaultValue("Available");

                entity.HasOne(c => c.Owner)
                    .WithMany(u => u.OwnedCars)
                    .HasForeignKey(c => c.OwnerID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add global query filter for soft delete
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure CarDocument entity
            modelBuilder.Entity<CarDocument>(entity =>
            {
                entity.HasOne(cd => cd.Car)
                    .WithMany(c => c.CarDocuments)
                    .HasForeignKey(cd => cd.CarID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Add global query filter for soft delete
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure Booking entity
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasIndex(e => e.BookingNo)
                    .IsUnique();

                entity.HasOne(b => b.Renter)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(b => b.RenterID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add global query filter for soft delete
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure BookingItem entity
            modelBuilder.Entity<BookingItem>(entity =>
            {
                entity.HasOne(bi => bi.Booking)
                    .WithMany(b => b.BookingItems)
                    .HasForeignKey(bi => bi.BookingID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bi => bi.Car)
                    .WithMany(c => c.BookingItems)
                    .HasForeignKey(bi => bi.CarID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add global query filter for soft delete
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure Wallet entity
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasIndex(e => e.UserId)
                    .IsUnique();

                entity.Property(e => e.Balance)
                    .HasDefaultValue(0);

                entity.HasOne(w => w.User)
                    .WithOne(u => u.Wallet)
                    .HasForeignKey<Wallet>(w => w.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Add global query filter for soft delete
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure WalletEntry entity
            modelBuilder.Entity<WalletEntry>(entity =>
            {
                entity.HasIndex(e => e.WalletId);

                entity.Property(e => e.Type)
                    .HasConversion<string>() // Convert enum <-> string
                    .HasColumnType("varchar(20)");

                entity.HasOne(wt => wt.Wallet)
                    .WithMany(w => w.Entries)
                    .HasForeignKey(wt => wt.WalletId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add global query filter for soft delete
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure Feedback entity
            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.HasOne(f => f.User)
                    .WithMany(u => u.Feedbacks)
                    .HasForeignKey(f => f.UserID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Car)
                    .WithMany(c => c.Feedbacks)
                    .HasForeignKey(f => f.CarID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Booking)
                    .WithMany(b => b.Feedbacks)
                    .HasForeignKey(f => f.BookingID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add global query filter for soft delete
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure Cart entity
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Carts)
                    .HasForeignKey(c => c.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Add global query filter for soft delete
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure CartItem entity
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasOne(ci => ci.Cart)
                    .WithMany(c => c.CartItems)
                    .HasForeignKey(ci => ci.CartID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Car)
                    .WithMany(c => c.CartItems)
                    .HasForeignKey(ci => ci.CarID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add global query filter for soft delete
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure BaseEntity properties for all entities that inherit from it
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity<Guid>).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseEntity<Guid>.CreatedAt))
                        .HasDefaultValueSql("GETDATE()");
                }
            }

            // Configure ApplicationUser CreatedAt property
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity<Guid>);

            foreach (var entry in entries)
            {
                var entity = (BaseEntity<Guid>)entry.Entity;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = DateTime.UtcNow;
                        entity.UpdatedAt = null;
                        entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entity.UpdatedAt = DateTime.UtcNow;
                        break;

                    case EntityState.Deleted:
                        // Implement soft delete
                        entry.State = EntityState.Modified;
                        entity.IsDeleted = true;
                        entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            var userEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is User && e.State == EntityState.Added);

            foreach (var entry in userEntries)
            {
                ((User)entry.Entity).CreatedAt = DateTime.UtcNow;
            }
        }
    }
}