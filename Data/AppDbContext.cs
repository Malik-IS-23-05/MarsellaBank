using MarsellaBank.Models;
using Microsoft.EntityFrameworkCore;

namespace MarsellaBank.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Client -> Account (1 ко многим)
            modelBuilder.Entity<Client>()
                .HasMany(c => c.Accounts)
                .WithOne(a => a.Client)
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Account -> Card (1 ко многим)
            modelBuilder.Entity<Account>()
                .HasMany(a => a.Cards)
                .WithOne(c => c.Account)
                .HasForeignKey(c => c.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Account -> Transaction (1 ко многим)
            modelBuilder.Entity<Account>()
                .HasMany(a => a.Transactions)
                .WithOne(t => t.Account)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Transaction -> Card (From)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.FromCard)
                .WithMany()
                .HasForeignKey(t => t.FromCardId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction -> Card (To)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ToCard)
                .WithMany()
                .HasForeignKey(t => t.ToCardId)
                .OnDelete(DeleteBehavior.Restrict);

            // Уникальный email
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Формат номера карты
            modelBuilder.Entity<Card>()
                .Property(c => c.Number)
                .HasMaxLength(19); // 0000 0000 0000 0000

            // Настройка точности для суммы
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");
        }
    }
}