using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BooksController : Controller
    {
        private readonly LibraryContext _libraryContext;

        public BooksController(LibraryContext libraryContext)
        {
            _libraryContext = libraryContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> Get()
        {
            var books = await this._libraryContext.Books.ToListAsync();
            return this.Ok(books);
        }

        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            var book = _libraryContext.Books
                .Where(b => b.InventoryNumber == id)
                .Select(b => new 
                {
                    Title = b.Title,
                    Status = _libraryContext.Borrows
                        .Where(br => br.InventoryNumber == id)
                        .Any() ? "borrowed" : "in",
                    BorrowerName = _libraryContext.Borrows
                        .Where(br => br.InventoryNumber == id)
                        .Join(_libraryContext.Users,
                            br => br.ReaderNumber,
                            u => u.ReaderNumber,
                            (br,u) => u.Name)
                        .FirstOrDefault(),
                    ReturnDate = _libraryContext.Borrows
                        .Where(br => br.InventoryNumber == id)
                        .Select(br => br.ReturnDate)
                        .FirstOrDefault(),
                })
                .FirstOrDefault();

            if (book is null) 
            {
                return this.NotFound();
            }

            return this.Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Book book)
        {
            this._libraryContext.Books.Add(book);
            await this._libraryContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Book book)
        {
            if (id != book.InventoryNumber)
            {
                return this.BadRequest();
            }

            var existingBook = await this._libraryContext.Books.FindAsync(id);

            if (existingBook is null)
            {
                return this.NotFound();
            }

            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.Publisher = book.Publisher;
            existingBook.PublicationYear = book.PublicationYear;

            await this._libraryContext.SaveChangesAsync();

            return this.NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingBook = await this._libraryContext.Books.FindAsync(id);

            if (existingBook is null)
            {
                return this.NotFound();
            }

            this._libraryContext.Books.Remove(existingBook);
            await this._libraryContext.SaveChangesAsync();

            return this.NoContent();
        }
    }
}
