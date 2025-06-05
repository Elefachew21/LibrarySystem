using AutoMapper;
using LibraryWebAPI.Data;
using LibraryWebAPI.DTOs;
using LibraryWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryWebAPI.Services
{
    public interface IBookService
    {
        Task<IEnumerable<BookDTO>> GetBooksAsync();
        Task<BookDTO> GetBookByIdAsync(int id);
        Task<BookDTO> AddBookAsync(BookCreateDTO bookDto);
        Task UpdateBookAsync(int id, BookUpdateDTO bookDto);
        Task DeleteBookAsync(int id);
    }

    public class BookService : IBookService
    {
        private readonly LibraryDbContext _context;
        private readonly IMapper _mapper;

        public BookService(LibraryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BookDTO>> GetBooksAsync()
        {
            var books = await _context.Books.ToListAsync();
            return _mapper.Map<IEnumerable<BookDTO>>(books);
        }

        public async Task<BookDTO> GetBookByIdAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            return _mapper.Map<BookDTO>(book);
        }

        public async Task<BookDTO> AddBookAsync(BookCreateDTO bookDto)
        {
            var book = _mapper.Map<Book>(bookDto);
            book.AvailableCopies = book.TotalCopies;

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return _mapper.Map<BookDTO>(book);
        }

        public async Task UpdateBookAsync(int id, BookUpdateDTO bookDto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                throw new KeyNotFoundException("Book not found");
            }

            var originalAvailableCopies = book.AvailableCopies;
            var originalTotalCopies = book.TotalCopies;

            _mapper.Map(bookDto, book);

            // Adjust available copies if total copies changed
            if (bookDto.TotalCopies != originalTotalCopies)
            {
                book.AvailableCopies = originalAvailableCopies + (bookDto.TotalCopies - originalTotalCopies);
            }

            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                throw new KeyNotFoundException("Book not found");
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

    }
}