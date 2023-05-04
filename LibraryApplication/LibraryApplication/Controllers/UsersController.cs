using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly LibraryContext _libraryContext;

        public UsersController(LibraryContext libraryContext)
        {
            this._libraryContext = libraryContext;
        }

        /// <summary>
        ///     All the users with their datas.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            var users = await this._libraryContext.Users.ToListAsync();
            return this.Ok(users);
        }

        /// <summary>
        ///     Gets a single users data.
        /// </summary>
        /// <param name="id">Users id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var user = await this._libraryContext.Users.FindAsync(id);

            if (user is null)
            {
                return this.NotFound();
            }

            var borrowedBooks = await this._libraryContext.Users
                .Where(u => u.ReaderNumber == id)
                .Join(
                    this._libraryContext.Borrows,
                    user => user.ReaderNumber,
                    borrow => borrow.ReaderNumber,
                    (user, borrow) => new { User = user, Borrow = borrow })
                .Join(
                    this._libraryContext.Books,
                    b => b.Borrow.InventoryNumber,
                    book => book.InventoryNumber,
                    (b, book) => new { Book = book, Borrow = b.Borrow })
                .Select(b => new Dictionary<string, object>
                {
                    { "Title", b.Book.Title },
                    { "ReturnDate", b.Borrow.ReturnDate },
                }).ToListAsync();

            var result = new Dictionary<string, object>
            {
                { "Id", user.ReaderNumber },
                { "Name", user.Name },
                { "Address", user.Address },
                { "BirthDate", user.BirthDate },
                { "BorrowedBooks", borrowedBooks },
            };

            return this.Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User user)
        {
            this._libraryContext.Users.Add(user);
            await this._libraryContext.SaveChangesAsync();

            return this.Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] User user)
        {
            if (id != user.ReaderNumber)
            {
                return this.BadRequest();
            }

            var existingUser = await this._libraryContext.Users.FindAsync(id);

            if (existingUser is null)
            {
                return this.NotFound();
            }

            existingUser.Name = user.Name;
            existingUser.Address = user.Address;
            existingUser.BirthDate = user.BirthDate;

            await this._libraryContext.SaveChangesAsync();

            return this.NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingUser = await this._libraryContext.Users.FindAsync(id);

            if (existingUser is null)
            {
                return this.NotFound();
            }

            this._libraryContext.Users.Remove(existingUser);
            await this._libraryContext.SaveChangesAsync();

            return this.NoContent();
        }
    }
}
