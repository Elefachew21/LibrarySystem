using System;

namespace LibraryShared.DTOs
{
    public class LoanDTO
    {
        public int LoanId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public int BorrowerId { get; set; }
        public string BorrowerName { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }

    public class LoanCreateDTO
    {
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public int BookId { get; set; }
        public int BorrowerId { get; set; }
    }

   





    public class LoanReturnDTO
    {
        public int LoanId { get; set; }
        public DateTime ReturnDate { get; set; } // Make sure this property exists
    }

    // Also, double check that these exist and have their properties (Username, Password, Token, etc.)
  
 
}