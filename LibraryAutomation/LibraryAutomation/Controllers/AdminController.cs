using LibraryAutomation.Models;
using LibraryAutomation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LibraryAutomation.Controllers
{
    public class AdminController : Controller
    {
        private readonly LibraryDbContext _context;

        public AdminController(LibraryDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("role");
            if (role != "Admin")
                return RedirectToAction("Login", "Account");

            ViewBag.BookCount = _context.Books.Count();
            ViewBag.MemberCount = _context.Members.Count();
            ViewBag.ActiveLoans = _context.Loans.Count(l => l.ReturnDate == null);

            return View();
        }

        // Kitapları listeleme (kitap + kopyaları)
        public async Task<IActionResult> BookList()
        {
            var books = await _context.Books.Include(b => b.Copies).ToListAsync();
            return View(books);
        }

        // Kitap ekleme GET
        public IActionResult AddBook()
        {
            ViewBag.Catalogs = new SelectList(_context.Catalogs, "Id", "Name");
            return View();
        }

        // Kitap ekleme POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBook(BookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Catalogs = new SelectList(_context.Catalogs, "Id", "Name");
                return View(model);
            }

            var book = new Book
            {
                Title = model.Title,
                Author = model.Author,
                Publisher = model.Publisher,
                CatalogId = model.CatalogId,
                Copies = new List<BookCopy>()
            };

            // Kitap eklemeden önce SaveChangesAsync yapmıyoruz, EF otomatik id atar.
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            for (int i = 0; i < model.Stock; i++)
            {
                book.Copies.Add(new BookCopy
                {
                    BookId = book.Id,
                    PhysicalLocation = model.PhysicalLocation,
                    IsBorrowed = false
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        // Kitap düzenleme GET
        public IActionResult EditBook(int id)
        {
            var book = _context.Books
        .Include(b => b.Copies)
        .FirstOrDefault(b => b.Id == id);

            if (book == null) return NotFound();

            ViewBag.Catalogs = new SelectList(_context.Catalogs, "Id", "Name", book.CatalogId);
            return View(book);
        }

        // Kitap düzenleme POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditBook(Book book)
        {
            if (!ModelState.IsValid)
                return View(book);

            _context.Books.Update(book);
            _context.SaveChanges();
            return RedirectToAction("BookList");
        }

        // Kitap silme
        public IActionResult DeleteBook(int id)
        {
            var book = _context.Books
                .Include(b => b.Copies)
                .FirstOrDefault(b => b.Id == id);

            if (book != null)
            {
                _context.Books.Remove(book);
                _context.SaveChanges();
            }

            return RedirectToAction("BookList");
        }

        // Kitap stok bilgisi listeleme
        public IActionResult BookStock()
        {
            var stockList = _context.Books
                .Include(b => b.Copies)
                .Select(b => new BookStockViewModel
                {
                    Title = b.Title,
                    Author = b.Author,
                    Publisher = b.Publisher,
                    TotalStock = b.Copies.Count,
                    Available = b.Copies.Count(c => !c.IsBorrowed),
                    Borrowed = b.Copies.Count(c => c.IsBorrowed)
                })
                .ToList();

            return View(stockList);
        }

        // Üyelerin ödünç aldığı kitaplar listesi
        public IActionResult MemberBookList()
        {
            var borrowedBooks = _context.Loans
                .Include(l => l.Member)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderByDescending(l => l.BorrowDate)
                .ToList();

            return View("~/Views/Admin/MemberBookList.cshtml", borrowedBooks);
        }
    }
}
