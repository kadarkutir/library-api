using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BorrowsController : Controller
    {
        private readonly LibraryContext _libraryContext;

        public BorrowsController(LibraryContext libraryContext)
        {
            _libraryContext = libraryContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Borrow>>> Get()
        {
            var loans = from borrow in this._libraryContext.Borrows
                        join people in this._libraryContext.Users on borrow.ReaderNumber equals people.ReaderNumber
                        join book in this._libraryContext.Books on borrow.InventoryNumber equals book.InventoryNumber
                        select new
                        {
                            BorrowNumber = borrow.BorrowNumber,
                            Name = people.Name,
                            BookTitle = book.Title,
                            BorrowDate = borrow.BorrowDate,
                            ReturnDate = borrow.ReturnDate,
                        };
            return this.Ok(loans);
        }

        [HttpGet("/{name}/borrows")]
        public async Task<ActionResult<IEnumerable<Dictionary<string,DateTime>>>> Get(string name)
        {
            var user = await _libraryContext.Users.FirstOrDefaultAsync(u => u.Name == name);

            if (user == null)
            {
                return NotFound();
            }

            var result = _libraryContext.Users
                .Where(u => u.Name == name)
                .Join(
                    _libraryContext.Borrows,
                    user => user.ReaderNumber,
                    borrow => borrow.ReaderNumber,
                    (user, borrow) => new { User = user, Borrow = borrow }
                )
                .Join(
                    _libraryContext.Books,
                    b => b.Borrow.InventoryNumber,
                    book => book.InventoryNumber,
                    (b, book) => new { Book = book, Borrow = b.Borrow }
                )
                .Select(b => new Dictionary<string, object>
                {
                    { "Title", b.Book.Title },
                    { "ReturnDate", b.Borrow.ReturnDate },
                }).ToList();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Borrow borrow)
        {
            if (this._libraryContext.Borrows.Any(x => x.InventoryNumber == borrow.InventoryNumber))
            {
                return this.Conflict();
            }

            this._libraryContext.Borrows.Add(borrow);
            await this._libraryContext.SaveChangesAsync();

            return this.Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Borrow borrow)
        {
            if (id != borrow.BorrowNumber)
            {
                return this.BadRequest();
            }

            var existingBorrow = await this._libraryContext.Borrows.FindAsync(id);

            if (existingBorrow is null)
            {
                return this.NotFound();
            }

            existingBorrow.BorrowDate = borrow.BorrowDate;
            existingBorrow.ReturnDate = borrow.ReturnDate;
            existingBorrow.InventoryNumber = borrow.InventoryNumber;
            existingBorrow.ReaderNumber = borrow.ReaderNumber;

            await this._libraryContext.SaveChangesAsync();

            return this.NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingBorrow = await this._libraryContext.Borrows.FindAsync(id);

            if (existingBorrow is null)
            {
                return this.NotFound();
            }

            this._libraryContext.Borrows.Remove(existingBorrow);
            await this._libraryContext.SaveChangesAsync();

            return this.NoContent();
        }
    }
}