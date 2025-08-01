using LibraryAutomation.Data;
using LibraryAutomation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAutomation.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        private readonly LibraryDbContext _context;

        public AccountController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {

            if (!ModelState.IsValid)
                return View(model);

            if (model.Role == UserRole.Admin)
            {
                var admin = new User
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    UserName = model.UserName,
                    Password = model.Password,
                    Role = UserRole.Admin
                };

                _context.Users.Add(admin);
            }
            else if (model.Role == UserRole.Member)
            {
                var member = new Member
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    UserName = model.UserName,
                    Password = model.Password
                };

                _context.Members.Add(member);
            }

            _context.SaveChanges();
            return RedirectToAction("Login", "Account");
        }



        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Role == UserRole.Admin)
            {
                var admin = _context.Users
                    .FirstOrDefault(u => u.UserName == model.UserName && u.Password == model.Password);

                if (admin != null)
                {
                    // Giriş başarılı
                    HttpContext.Session.SetString("username", admin.UserName);// HttpContext o andaki http isteğiyle tüm bilgilere ulaşmamı sağlar. 
                    HttpContext.Session.SetString("role", "Admin"); // .Session ise trayıcıya ait oturum verilerini saklamamı sağlar.
                    return RedirectToAction("Dashboard", "Admin"); // RedirectToAction() metodu sayfa yönlendirmeli için kullanılır. 
                    //SetString("username", admin.UserName)'de oturum içerisinde username adında bir key oluşturur ve 
                    // admin.username ibaresini içinde saklar. 
                    // yani kullanıcı giriş yaptığında kullanıcı adını oturumda saklamış oluyorum. 

                    //HttpContext.Session.SetString("role", "Admin"); 
                    // burada ise role oluşturup admin tanımlıyorum ve oturumda saklıyorum. böylece admin rolüne sahip mi diye kontrol ederken
                    //direkt role üzerinden kontrol edebilirim. 

                    // böylece kullanıcı login olduğunda bilgilerini tekrar sorgulamadan sessionda tutarak okuyabilirim. yani ayrı bir katman ediyor. 
                }


            }
            else if (model.Role == UserRole.Member)
            {
                var member = _context.Members
                    .FirstOrDefault(m => m.UserName == model.UserName && m.Password == model.Password);

                if (member != null)
                {
                    HttpContext.Session.SetString("username", member.UserName);
                    HttpContext.Session.SetString("role", "Member");
                    HttpContext.Session.SetInt32("userId", member.Id); // Burası eksikti!
                    return RedirectToAction("Dashboard", "Member");
                }
            }

            ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
            return View(model);
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

