namespace LibraryAutomation.Models
{
    public class Loan
    {
        public int Id { get; set; }

        public int MemberId { get; set; }
        public Member Member { get; set; }

        public int BookCopyId { get; set; }
        public BookCopy BookCopy { get; set; }

        public DateTime BorrowDate { get; set; } = DateTime.Now;
        public DateTime? ReturnDate { get; set; }

    }
}
