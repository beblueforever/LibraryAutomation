using Microsoft.AspNetCore.Mvc;

namespace LibraryAutomation.Models
{
    public class Catalog
    {
        public int Id { get; set; }

        public string Code { get; set; } // Örnek: KTG-2025-001

        public string Name { get; set; } // Örnek: Bilim Kurgu, Roman

        // İlişki: Bir katalog birden fazla kitap içerebilir
        public ICollection<Book> Books { get; set; }
    }
}
