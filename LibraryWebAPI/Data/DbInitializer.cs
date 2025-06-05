using LibraryWebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryWebAPI.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(LibraryDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await context.Database.MigrateAsync();

            // Seed roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("Librarian"))
            {
                await roleManager.CreateAsync(new IdentityRole("Librarian"));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Seed admin user
            if (await userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@library.com",
                    FullName = "Administrator"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(adminUser, new[] { "Admin", "Librarian", "User" });
                }
            }

            // Seed books if empty
            if (!context.Books.Any())
            {
                var books = new List<Book>
                {
                    new Book { Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", ISBN = "9780743273565", PublishedYear = 1925, TotalCopies = 5, AvailableCopies = 5 },
                    new Book { Title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "9780061120084", PublishedYear = 1960, TotalCopies = 3, AvailableCopies = 3 },
                    new Book { Title = "1984", Author = "George Orwell", ISBN = "9780451524935", PublishedYear = 1949, TotalCopies = 4, AvailableCopies = 4 },
                    new Book { Title = "Pride and Prejudice", Author = "Jane Austen", ISBN = "9780486284736", PublishedYear = 1813, TotalCopies = 2, AvailableCopies = 2 },
                    new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien", ISBN = "9780547928227", PublishedYear = 1937, TotalCopies = 6, AvailableCopies = 6 }
                };

                await context.Books.AddRangeAsync(books);
            }

            // Seed borrowers if empty
            if (!context.Borrowers.Any())
            {
                var borrowers = new List<Borrower>
                {
                    new Borrower { Name = "John Smith", Email = "john.smith@example.com", Phone = "555-0101" },
                    new Borrower { Name = "Emily Johnson", Email = "emily.johnson@example.com", Phone = "555-0102" },
                    new Borrower { Name = "Michael Williams", Email = "michael.williams@example.com", Phone = "555-0103" },
                    new Borrower { Name = "Sarah Brown", Email = "sarah.brown@example.com", Phone = "555-0104" },
                    new Borrower { Name = "David Jones", Email = "david.jones@example.com", Phone = "555-0105" }
                };

                await context.Borrowers.AddRangeAsync(borrowers);
            }

            await context.SaveChangesAsync();
        }
    }
}