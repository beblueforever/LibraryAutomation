using Microsoft.AspNetCore.Mvc;

namespace LibraryAutomation.Models
{
    public class BookCopy
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }

        public string PhysicalLocation { get; set; }
        public bool IsBorrowed { get; set; } = false;
    }
}
