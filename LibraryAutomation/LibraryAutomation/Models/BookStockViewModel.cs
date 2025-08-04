namespace LibraryAutomation.Models
{
    public class BookStockViewModel
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public int TotalStock { get; set; }
        public int Available { get; set; }
        public int Borrowed { get; set; }

        public List<string> PhysicalLocations { get; set; } = new List<string>();

    }
}
