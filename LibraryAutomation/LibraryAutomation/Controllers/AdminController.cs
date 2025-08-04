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
            if (!ModelState.IsValid || model.PhysicalLocations == null || model.PhysicalLocations.Count != model.Stock)
            {
                if (model.PhysicalLocations == null || model.PhysicalLocations.Count != model.Stock)
                {
                    ModelState.AddModelError("", "Lütfen her kitap kopyası için bir fiziksel konum girin.");
                }

                
    ViewBag.Catalogs = new SelectList(_context.Catalogs, "Id", "Name");
                return View(model);
            }

            var existingBook = await _context.Books
                .Include(b => b.Copies)
                .FirstOrDefaultAsync(b =>
                    b.Title == model.Title &&
                    b.Author == model.Author &&
                    b.Publisher == model.Publisher &&
                    b.CatalogId == model.CatalogId);

            Book book;

            if (existingBook != null)
            {
                book = existingBook;
            }
            else
            {
                book = new Book
                {
                    Title = model.Title,
                    Author = model.Author,
                    Publisher = model.Publisher,
                    Description = model.Description,
                    CatalogId = model.CatalogId,
                    Copies = new List<BookCopy>()
                };

                _context.Books.Add(book);
                await _context.SaveChangesAsync(); // BookId oluşturmak için
            }

            for (int i = 0; i < model.Stock; i++)
            {
                var copy = new BookCopy
                {
                    BookId = book.Id,
                    PhysicalLocation = model.PhysicalLocations[i],
                    IsBorrowed = false
                };

                _context.BookCopies.Add(copy);
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

            var model = new EditBookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Publisher = book.Publisher,
                Description = book.Description,
                CatalogId = book.CatalogId,
                Copies = book.Copies.Select(c => new BookCopyEditViewModel
                {
                    Id = c.Id,
                    PhysicalLocation = c.PhysicalLocation
                }).ToList()
            };

            ViewBag.Catalogs = new SelectList(_context.Catalogs, "Id", "Name", book.CatalogId);
            return View(model);
        }

        // Kitap düzenleme POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditBook(EditBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Catalogs = new SelectList(_context.Catalogs, "Id", "Name", model.CatalogId);
                return View(model);
            }

            var book = _context.Books
                .Include(b => b.Copies)
                .FirstOrDefault(b => b.Id == model.Id);

            if (book == null) return NotFound();

            book.Title = model.Title;
            book.Author = model.Author;
            book.Publisher = model.Publisher;
            book.Description = model.Description;
            book.CatalogId = model.CatalogId;

            // Her bir kopyanın lokasyonunu güncelle
            foreach (var copyModel in model.Copies)
            {
                var copy = book.Copies.FirstOrDefault(c => c.Id == copyModel.Id);
                if (copy != null)
                {
                    copy.PhysicalLocation = copyModel.PhysicalLocation;
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Dashboard");
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

        [HttpGet]
        public IActionResult DeleteCopy(int id)
        {
            var copy = _context.BookCopies.FirstOrDefault(c => c.Id == id);
            if (copy == null)
            {
                return NotFound();
            }

            int bookId = copy.BookId; // Kitabın id'sini al, sonra yönlendirmek için

            _context.BookCopies.Remove(copy);
            _context.SaveChanges();

            return RedirectToAction("EditBook", new { id = bookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCopyConfirmed(int id)
        {
            var copy = _context.BookCopies.FirstOrDefault(c => c.Id == id);
            if (copy == null)
                return NotFound();

            int bookId = copy.BookId; // Önce sakla çünkü birazdan sileceğiz

            _context.BookCopies.Remove(copy);
            _context.SaveChanges();

            return RedirectToAction("EditBook", new { id = bookId });
        }

        // Kitap stok bilgisi listeleme
        public IActionResult BookStock(string searchString)
        {
            var booksQuery = _context.Books
                .Include(b => b.Copies)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(searchString) ||
                    b.Author.Contains(searchString) ||
                    b.Publisher.Contains(searchString));
            }

            var books = booksQuery
                .ToList()  // Veritabanından çek
                .Select(book => new BookStockViewModel
                {
                    Title = book.Title,
                    Author = book.Author,
                    Publisher = book.Publisher,
                    TotalStock = book.Copies.Count,
                    Available = book.Copies.Count(c => !c.IsBorrowed),
                    PhysicalLocations = book.Copies
                        .Where(c => !c.IsBorrowed)
                        .Select(c => c.PhysicalLocation)
                        .Distinct()
                        .ToList()
                })
                .ToList();

            return View(books);
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
