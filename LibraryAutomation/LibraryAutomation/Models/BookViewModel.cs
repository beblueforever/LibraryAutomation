﻿using System.ComponentModel.DataAnnotations;

namespace LibraryAutomation.Models
{
    public class BookViewModel  // ekleme yaparken kullanıyoruz genellikle.
    {
        [Required(ErrorMessage = "Başlık gereklidir")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Yazar gereklidir")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Yayınevi gereklidir")]
        public string Publisher { get; set; }

        [Required(ErrorMessage = "Açıklama gereklidir")]
        public string Description { get; set; }

        

        [Required(ErrorMessage = "Katalog seçmeniz gereklidir")]
        public int CatalogId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Stok en az 1 olmalı")]
        public int Stock { get; set; } // Eklenmek istenen kopya sayısı
                                       // Her bir kopyanın fiziksel lokasyonu için

        [Required(ErrorMessage = "Fiziksel Lokasyon gereklidir")]
        public List<string>PhysicalLocations { get; set; } = new List<string>();
    }
}
