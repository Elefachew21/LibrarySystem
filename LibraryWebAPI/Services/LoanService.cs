using AutoMapper;
using LibraryWebAPI.Data;
using LibraryWebAPI.DTOs;
using LibraryWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryWebAPI.Services
{
    public interface ILoanService
    {
        Task<LoanDTO> IssueBookAsync(LoanCreateDTO loanDto);
        Task<LoanDTO> ReturnBookAsync(ReturnDTO returnDto);
        Task<IEnumerable<LoanDTO>> GetOverdueLoansAsync();
    }

    public class LoanService : ILoanService
    {
        private readonly LibraryDbContext _context;
        private readonly IMapper _mapper;

        public LoanService(LibraryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LoanDTO> IssueBookAsync(LoanCreateDTO loanDto)
        {
            var book = await _context.Books.FindAsync(loanDto.BookId);
            if (book == null)
            {
                throw new KeyNotFoundException("Book not found");
            }

            var borrower = await _context.Borrowers.FindAsync(loanDto.BorrowerId);
            if (borrower == null)
            {
                throw new KeyNotFoundException("Borrower not found");
            }

            if (book.AvailableCopies <= 0)
            {
                throw new InvalidOperationException("No available copies of this book");
            }

            var loan = new Loan
            {
                BookId = loanDto.BookId,
                BorrowerId = loanDto.BorrowerId,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14), // 2 weeks loan period
                ReturnDate = null
            };

            book.AvailableCopies--;

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<LoanDTO>(loan);
            result.BookTitle = book.Title;
            result.BorrowerName = borrower.Name;

            return result;
        }

        public async Task<LoanDTO> ReturnBookAsync(ReturnDTO returnDto)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Borrower)
                .FirstOrDefaultAsync(l => l.LoanId == returnDto.LoanId);

            if (loan == null)
            {
                throw new KeyNotFoundException("Loan not found");
            }

            if (loan.ReturnDate.HasValue)
            {
                throw new InvalidOperationException("Book already returned");
            }

            loan.ReturnDate = DateTime.UtcNow;
            loan.Book.AvailableCopies++;

            await _context.SaveChangesAsync();

            var result = _mapper.Map<LoanDTO>(loan);
            result.BookTitle = loan.Book.Title;
            result.BorrowerName = loan.Borrower.Name;

            return result;
        }

        public async Task<IEnumerable<LoanDTO>> GetOverdueLoansAsync()
        {
            var overdueLoans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Borrower)
                .Where(l => !l.ReturnDate.HasValue && l.DueDate < DateTime.UtcNow)
                .ToListAsync();

            return overdueLoans.Select(loan => new LoanDTO
            {
                LoanId = loan.LoanId,
                BookId = loan.BookId,
                BookTitle = loan.Book.Title,
                BorrowerId = loan.BorrowerId,
                BorrowerName = loan.Borrower.Name,
                LoanDate = loan.LoanDate,
                DueDate = loan.DueDate,
                ReturnDate = loan.ReturnDate
            });
        }
    }
}