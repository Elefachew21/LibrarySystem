using LibraryWebAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryWebAPI.Data
{
    public class LibraryDbContext : IdentityDbContext<AppUser>
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Borrower> Borrowers { get; set; }
        public DbSet<Loan> Loans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Book)
                .WithMany(b => b.Loans)
                .HasForeignKey(l => l.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Borrower)
                .WithMany(b => b.Loans)
                .HasForeignKey(l => l.BorrowerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            modelBuilder.Entity<Book>().HasData(
                new Book { BookId = 1, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", ISBN = "9780743273565", PublishedYear = 1925, TotalCopies = 5, AvailableCopies = 5 },
                new Book { BookId = 2, Title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "9780061120084", PublishedYear = 1960, TotalCopies = 3, AvailableCopies = 3 },
                new Book { BookId = 3, Title = "1984", Author = "George Orwell", ISBN = "9780451524935", PublishedYear = 1949, TotalCopies = 4, AvailableCopies = 4 }
            );

            modelBuilder.Entity<Borrower>().HasData(
                new Borrower { BorrowerId = 1, Name = "John Smith", Email = "john.smith@example.com", Phone = "555-0101" },
                new Borrower { BorrowerId = 2, Name = "Emily Johnson", Email = "emily.johnson@example.com", Phone = "555-0102" }
            );
        }
    }
}