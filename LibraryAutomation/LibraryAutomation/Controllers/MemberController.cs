using LibraryAutomation.Data;
using LibraryAutomation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAutomation.Controllers
{
    public class MemberController : Controller
    {
        private readonly LibraryDbContext _context;

        public MemberController(LibraryDbContext context)
        {
            _context = context;
        }

        // Üye ana sayfası: kitapları stok durumu ile listeler
        public IActionResult Dashboard()
        {
            var username = HttpContext.Session.GetString("username");
            var role = HttpContext.Session.GetString("role");

            if (string.IsNullOrEmpty(username) || role != "Member")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Username = username;

            var books = _context.Books
                .Select(book => new
                {
                    Book = book,
                    AvailableCopies = _context.BookCopies.Count(c => c.BookId == book.Id && !c.IsBorrowed)
                })
                .AsEnumerable()
                .Select(x =>
                {
                    // Geçici stok bilgisi için ekstra property yoksa view model veya ViewBag ile gönderilebilir
                    // Burada ViewBag veya ViewModel daha temiz olur ama senin yapında Book içinde yok
                    x.Book.Description = $"Stok: {x.AvailableCopies}";
                    return x.Book;
                })
                .ToList();

            return View(books);
        }

        // Kitap kiralama işlemi
        [HttpPost]
        public async Task<IActionResult> BorrowBook(int bookId)
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "Member" || userId == null)
                return RedirectToAction("Login", "Account");

            var member = await _context.Members.FindAsync(userId);
            if (member == null)
            {
                TempData["Error"] = "Üye bulunamadı.";
                return RedirectToAction("Dashboard");
            }

            // Ödünç alınabilir ilk kitap kopyasını bul
            var availableCopy = await _context.BookCopies
                .FirstOrDefaultAsync(c => c.BookId == bookId && !c.IsBorrowed);

            if (availableCopy == null)
            {
                TempData["Error"] = "Kitap stokta yok.";
                return RedirectToAction("Dashboard");
            }

            availableCopy.IsBorrowed = true;

            var loan = new Loan
            {
                MemberId = member.Id,
                BookCopyId = availableCopy.Id,
                BorrowDate = DateTime.Now,
                ReturnDate = null
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kitap başarıyla ödünç alındı.";
            return RedirectToAction("Dashboard");
        }

        // Kitap iade işlemi
        [HttpPost]
        public async Task<IActionResult> ReturnBook(int loanId)
        {
            var loan = await _context.Loans
                                     .Include(l => l.BookCopy)
                                     .FirstOrDefaultAsync(l => l.Id == loanId);

            if (loan == null || loan.ReturnDate != null)
            {
                TempData["Error"] = "İade edilemedi.";
                return RedirectToAction("MyLoans");
            }

            loan.ReturnDate = DateTime.Now;
            loan.BookCopy.IsBorrowed = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Kitap iade edildi.";
            return RedirectToAction("MyLoans");
        }

        // Üyenin ödünç aldığı kitapların geçmişi
        public IActionResult MyLoans()
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var member = _context.Members.FirstOrDefault(m => m.Id == userId);
            if (member == null)
            {
                TempData["Error"] = "Üye bulunamadı.";
                return RedirectToAction("Dashboard");
            }

            var loans = _context.Loans
                                .Include(l => l.BookCopy)
                                .ThenInclude(bc => bc.Book)
                                .Where(l => l.MemberId == member.Id)
                                .OrderByDescending(l => l.BorrowDate)
                                .ToList();

            return View(loans);
        }
    }
}
