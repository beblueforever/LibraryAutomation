using System;
using System.ComponentModel.DataAnnotations;
namespace LibraryAutomation.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "İsim zorunludur.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Soyisim zorunludur.")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        public string Password { get; set; }
        [Required]
        public UserRole Role { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public List<Loan>Loans{ get; set; } = new List<Loan>();       // Oduncler (Navigasyon)
    }
}
