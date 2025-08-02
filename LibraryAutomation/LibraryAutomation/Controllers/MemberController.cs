using LibraryAutomation.Data;
using LibraryAutomation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
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

        // Üye ana sayfası: kitapları ve her birinin stok durumunu listeler
        public IActionResult Dashboard()
        {
            var username = HttpContext.Session.GetString("username");
            var role = HttpContext.Session.GetString("role");

            if (string.IsNullOrEmpty(username) || role != "Member")
                return RedirectToAction("Login", "Account");

            ViewBag.Username = username;

            var books = _context.Books
                .Include(b => b.Copies)
                .ToList()
                .Select(book =>
                {
                    var availableCount = book.Copies.Count(c => !c.IsBorrowed);
                    book.Description = $"Stok: {availableCount}"; // Description alanı geçici olarak stok bilgisi için kullanılıyor
                    return book;
                })
                .ToList();

            return View(books);
        }

        // Kitap ödünç alma işlemi
        [HttpPost]
       
        public async Task<IActionResult> BorrowBook(int bookId, int copyId)
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

            // Üyenin bu kitapla ilgili aktif bir ödünç kaydı var mı? (henüz iade edilmemiş)
            bool alreadyBorrowed = await _context.Loans
                .Include(l => l.BookCopy)
                .AnyAsync(l => l.MemberId == member.Id &&
                               l.BookCopy.BookId == bookId &&
                               l.ReturnDate == null);

            if (alreadyBorrowed)
            {
                TempData["Error"] = "Bu kitabı zaten ödünç aldınız. İade etmeden tekrar alamazsınız.";
                return RedirectToAction("Dashboard");
            }

            // copyId ile seçilen kopyayı al
            var selectedCopy = await _context.BookCopies
                .FirstOrDefaultAsync(c => c.Id == copyId && c.BookId == bookId);

            if (selectedCopy == null)
            {
                TempData["Error"] = "Seçilen kitap kopyası bulunamadı.";
                return RedirectToAction("Dashboard");
            }

            if (selectedCopy.IsBorrowed)
            {
                TempData["Error"] = "Seçilen kitap kopyası zaten ödünç alınmış.";
                return RedirectToAction("Dashboard");
            }

            // Kitap kopyasını ödünç al
            selectedCopy.IsBorrowed = true;

            var loan = new Loan
            {
                MemberId = member.Id,
                BookCopyId = selectedCopy.Id,
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
                TempData["Error"] = "İade edilemedi veya zaten iade edilmiş.";
                return RedirectToAction("MyLoans");
            }

            loan.ReturnDate = DateTime.Now;
            loan.BookCopy.IsBorrowed = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Kitap başarıyla iade edildi.";
            return RedirectToAction("MyLoans");
        }

        // Üyenin ödünç geçmişi
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
