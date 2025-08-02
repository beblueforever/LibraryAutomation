public class EditBookViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Publisher { get; set; }
    public string Description { get; set; }
    public int CatalogId { get; set; }

    public List<BookCopyEditViewModel> Copies { get; set; }
}

public class BookCopyEditViewModel
{
    public int Id { get; set; }
    public string PhysicalLocation { get; set; }
}
