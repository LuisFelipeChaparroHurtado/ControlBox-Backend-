using ControlBoxPruebaTecnica.Context;
using ControlBoxPruebaTecnica.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlBoxPruebaTecnica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoleController> _logger;

        public RoleController(AppDbContext context, ILogger<RoleController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Role
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            try
            {
                return Ok(await _context.Roles.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los roles");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        // GET: api/Role/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);

                if (role == null)
                {
                    return NotFound(new { Message = "Rol no encontrado" });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el rol");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] Role role)
        {
            if (role == null)
                return BadRequest(new { Message = "Invalid role data." });

            try
            {
                await _context.Roles.AddAsync(role);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Role created successfully!" });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while creating role.");
                return StatusCode(500, new { Message = "Database error occurred.", Details = dbEx.InnerException?.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating role.");
                return StatusCode(500, new { Message = "Internal server error occurred.", Details = ex.Message });
            }
        }

        // PUT: api/Role/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role role)
        {
            if (id != role.Id)
            {
                return BadRequest(new { Message = "ID de rol no coincide" });
            }

            try
            {
                _context.Entry(role).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException dbEx)
            {
                if (!RoleExists(id))
                {
                    return NotFound(new { Message = "Rol no encontrado" });
                }
                else
                {
                    _logger.LogError(dbEx, "Error de concurrencia al actualizar el rol");
                    return StatusCode(500, new { Message = "Error de concurrencia", Details = dbEx.InnerException?.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el rol");
                return StatusCode(500, new { Message = "Error interno del servidor", Details = ex.Message });
            }
        }

        // DELETE: api/Role/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null)
                {
                    return NotFound(new { Message = "Rol no encontrado" });
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el rol");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }
    }
}
