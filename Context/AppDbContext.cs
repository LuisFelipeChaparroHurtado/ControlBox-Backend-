using ControlBoxPruebaTecnica.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlBoxPruebaTecnica.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet y configuraciones de entidades aquí
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de relaciones
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Reviews)
                .WithOne(r => r.Book)
                .HasForeignKey(r => r.BookId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict); // Opcional: asegura que no se elimine en cascada

            // Configuración adicional según necesidades

            // Configuración para evitar ciclos en la serialización JSON
            modelBuilder.Entity<Review>()
                .Navigation(r => r.User)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            modelBuilder.Entity<Review>()
                .Navigation(r => r.Book)
                .UsePropertyAccessMode(PropertyAccessMode.Property);
        }
    }
}
