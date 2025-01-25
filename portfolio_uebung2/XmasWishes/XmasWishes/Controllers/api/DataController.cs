using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using XmasWishes.Models;
using XmasWishes.Models.wishes;

namespace XmasWishes.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly WishesDbContext _dbContext;

        public DataController(WishesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [HttpGet]
        public IActionResult GetAllWishes()
        {
            try
            {
                Console.WriteLine(_dbContext != null);
            
                var connStr = _dbContext.Database.GetConnectionString();
                Console.WriteLine($"Connection String: {connStr}");
            
            
                Console.WriteLine("(1) GetAllWishes");
                // if (string.IsNullOrEmpty(accessToken))
                //     return Ok("AccessToken is required.");

                Console.WriteLine("(2) GetAllWishes");
                var wishesListTask = _dbContext.Wishes.ToListAsync();
                wishesListTask.Wait();

                Console.WriteLine("(3) GetAllWishes");
                var wishes = wishesListTask.Result;
                return Ok(wishes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, $"An error occurred while processing your request: {e.Message}");
            }
        }

        [HttpPost]
        public IActionResult AddWish([FromHeader(Name = "AccessToken")] string accessToken, [FromBody] Wish wish)
        {
            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized("AccessToken is required.");

            if (string.IsNullOrEmpty(wish.Description))
                return BadRequest("Wish description cannot be empty.");

            wish.Id = 0; // Dummy value; AUTO_INCREMENT will assign the correct value
            _dbContext.Wishes.Add(wish);
            
            var saveTask = _dbContext.SaveChangesAsync();
            saveTask.Wait();
            
            return CreatedAtAction(nameof(GetAllWishes), new { id = wish.Id }, wish);
        }

        [HttpPatch("{id}")]
        public IActionResult UpdateWish([FromHeader(Name = "AccessToken")] string accessToken, Guid id, [FromBody] Wish wish)
        {
            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized("AccessToken is required.");
            
            var existingWishTask = _dbContext.Wishes.FindAsync(id);
            var awaiter = existingWishTask.GetAwaiter();
            var existingWish = awaiter.GetResult();
            
            if (existingWish == null)
                return NotFound("Wish not found.");

            existingWish.Description = wish.Description ?? existingWish.Description;
            existingWish.Status = wish.Status;

            _dbContext.Wishes.Update(existingWish);
            _dbContext.SaveChanges();

            return Ok(existingWish);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteWish([FromHeader(Name = "AccessToken")] string accessToken, Guid id)
        {
            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized("AccessToken is required.");

            var existingWish = _dbContext.Wishes.Find(id);
            if (existingWish == null)
                return NotFound("Wish not found.");

            _dbContext.Wishes.Remove(existingWish);
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}
