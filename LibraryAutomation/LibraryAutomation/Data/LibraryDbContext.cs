using Microsoft.EntityFrameworkCore;
using System;
using LibraryAutomation.Models;

namespace LibraryAutomation.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Catalog> Catalogs { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }

    }
}
