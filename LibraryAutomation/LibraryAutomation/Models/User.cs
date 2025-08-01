using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace LibraryAutomation.Models



{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        public string Password { get; set; }
      
        [Required(ErrorMessage = "Rol seçimi zorunludur.")]
        public UserRole Role { get; set; }  // Enum olarak kullandım. kayıt aşamasında kutucuk yaptım. 

       


    }

 }
