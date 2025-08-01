using Microsoft.AspNetCore.Mvc;

namespace LibraryAutomation.Models
{
    public class RegisterViewModel
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; } // enum
    }
}
