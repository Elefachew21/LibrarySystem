using AutoMapper;
using LibraryWebAPI.Data;
using LibraryWebAPI.DTOs;
using LibraryWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryWebAPI.Services
{
    public class BorrowerService : IBorrowerService
    {
        private readonly LibraryDbContext _context;
        private readonly IMapper _mapper;

        public BorrowerService(LibraryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BorrowerDTO>> GetBorrowersAsync()
        {
            var borrowers = await _context.Borrowers.ToListAsync();
            return _mapper.Map<IEnumerable<BorrowerDTO>>(borrowers);
        }

        public async Task<BorrowerDTO> GetBorrowerByIdAsync(int id)
        {
            var borrower = await _context.Borrowers.FindAsync(id);
            return _mapper.Map<BorrowerDTO>(borrower);
        }

        public async Task<BorrowerDTO> AddBorrowerAsync(BorrowerCreateDTO borrowerDto)
        {
            var borrower = _mapper.Map<Borrower>(borrowerDto);

            _context.Borrowers.Add(borrower);
            await _context.SaveChangesAsync();

            return _mapper.Map<BorrowerDTO>(borrower);
        }

        public async Task UpdateBorrowerAsync(int id, BorrowerUpdateDTO borrowerDto)
        {
            var borrower = await _context.Borrowers.FindAsync(id);
            if (borrower == null)
            {
                throw new KeyNotFoundException("Borrower not found");
            }

            _mapper.Map(borrowerDto, borrower);
            _context.Borrowers.Update(borrower);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBorrowerAsync(int id)
        {
            var borrower = await _context.Borrowers.FindAsync(id);
            if (borrower == null)
            {
                throw new KeyNotFoundException("Borrower not found");
            }

            // Check if borrower has any active loans
            var hasLoans = await _context.Loans.AnyAsync(l => l.BorrowerId == id && !l.ReturnDate.HasValue);
            if (hasLoans)
            {
                throw new InvalidOperationException("Cannot delete borrower with active loans");
            }

            _context.Borrowers.Remove(borrower);
            await _context.SaveChangesAsync();
        }
    }
}