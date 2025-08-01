using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryAutomation.Models
{
    public class Book
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public int CatalogId { get; set; }
        public Catalog Catalog { get; set; }
        public string? Description { get; set; }  // null olabilir ekledim ki nul olduğunda okurken hata almayayım. 

        public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
    }
}
