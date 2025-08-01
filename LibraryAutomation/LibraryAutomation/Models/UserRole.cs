using System.ComponentModel.DataAnnotations;

namespace LibraryAutomation.Models
{
    public enum UserRole
    {
        [Display(Name = "Admin")]
        Admin,
        [Display(Name = "Üye")]
        Member
    }
}
