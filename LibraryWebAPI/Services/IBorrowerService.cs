using LibraryWebAPI.DTOs;

namespace LibraryWebAPI.Services
{
    public interface IBorrowerService
    {
        Task<IEnumerable<BorrowerDTO>> GetBorrowersAsync();
        Task<BorrowerDTO> GetBorrowerByIdAsync(int id);
        Task<BorrowerDTO> AddBorrowerAsync(BorrowerCreateDTO borrowerDto);
        Task UpdateBorrowerAsync(int id, BorrowerUpdateDTO borrowerDto);
        Task DeleteBorrowerAsync(int id);
    }
}