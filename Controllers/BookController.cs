using ControlBoxPruebaTecnica.Context;
using ControlBoxPruebaTecnica.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlBoxPruebaTecnica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BookController> _logger;

        public BookController(AppDbContext context, ILogger<BookController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Book
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            try
            {
                var books = await _context.Books.ToListAsync();
                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los libros");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        // GET: api/Book/{id}/Reviews
        [HttpGet("{id}/Reviews")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsForBook(int id)
        {
            try
            {
                var book = await _context.Books
                    .Include(b => b.Reviews)
                    .ThenInclude(r => r.User) // Incluye la información del usuario si es necesario
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (book == null)
                {
                    return NotFound(new { Message = "Libro no encontrado" });
                }

                var reviews = book.Reviews.OrderByDescending(r => r.FechaReseña).ToList();

                // Añade un registro de depuración para ver las reseñas en la consola
                _logger.LogInformation("Reseñas del libro: {@Reviews}", reviews);

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las reseñas del libro");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }


        // GET: api/Book/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            try
            {
                var book = await _context.Books
                    .Include(b => b.Reviews)
                    .ThenInclude(r => r.User) // Incluye la información del usuario si es necesario
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (book == null)
                {
                    return NotFound(new { Message = "Libro no encontrado" });
                }

                var reviews = book.Reviews.OrderByDescending(r => r.FechaReseña).ToList();
                book.Reviews = reviews;

                // Añade un registro de depuración para ver las reseñas en la consola
                _logger.LogInformation("Reseñas del libro: {@Reviews}", reviews);

                return Ok(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el libro");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }



        // POST: api/Book
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] Book book)
        {
            await _context.AddAsync(book); 
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Book created successfully!" });

        }

        // PUT: api/Book/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            if (id != book.Id)
            {
                return BadRequest(new { Message = "ID de libro no coincide." });
            }

            try
            {
                _context.Entry(book).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException dbEx)
            {
                if (!BookExists(id))
                {
                    return NotFound(new { Message = "Libro no encontrado" });
                }
                else
                {
                    _logger.LogError(dbEx, "Error de concurrencia al actualizar el libro");
                    return StatusCode(500, new { Message = "Error de concurrencia", Details = dbEx.InnerException?.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el libro");
                return StatusCode(500, new { Message = "Error interno del servidor", Details = ex.Message });
            }
        }

        // DELETE: api/Book/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    return NotFound(new { Message = "Libro no encontrado" });
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el libro");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
