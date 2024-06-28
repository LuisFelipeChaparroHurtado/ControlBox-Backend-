using ControlBoxPruebaTecnica.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using ControlBoxPruebaTecnica.Models;
using ControlBoxPruebaTecnica.Helpers;
using System.Security.Cryptography;

namespace ControlBoxPruebaTecnica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _authContext;
        private readonly ILogger<UserController> _logger;

        public UserController(AppDbContext appDbContext, ILogger<UserController> logger)
        {
            _authContext = appDbContext;
            _logger = logger;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            try
            {
                if (userObj == null)
                    return BadRequest();

                var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == userObj.Email);
                if (user == null)
                    return NotFound(new { Message = "Usuario no encontrado." });

                if (!PasswordHasher.VerifyPassword(userObj.Password, user.Password))
                {
                    return BadRequest(new { Message = "La contraseña es incorrecta." });
                }

                user.Token = await CreateJwtToken(user);

                return Ok(new
                {
                    Token = user.Token,
                    Message = "¡Inicio de sesión exitoso!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la autenticación.");
                return StatusCode(500, new { Message = "Error interno del servidor." });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest(new { Message = "Invalid user data." });

            // Validar si el correo electrónico ya está registrado
            var existingUser = await _authContext.Users.FirstOrDefaultAsync(u => u.Email == userObj.Email);
            if (existingUser != null)
                return BadRequest(new { Message = "Email is already registered." });

            try
            {
                // Verificar si el nombre de usuario ya existe
                if (await CheckUserNameExistAsync(userObj.Username))
                {
                    return BadRequest(new
                    {
                        Message = "Username already exists!"
                    });
                }

                // Verificar si el correo electrónico ya existe
                if (await CheckEmailExistAsync(userObj.Email))
                {
                    return BadRequest(new
                    {
                        Message = "Email already exists!"
                    });
                }

                // Verificar la fortaleza de la contraseña
                var pass = CheckPasswordStrength(userObj.Password);
                if (!string.IsNullOrEmpty(pass))
                    return BadRequest(new
                    {
                        Message = pass
                    });

                userObj.Password = PasswordHasher.HashPassword(userObj.Password);

                // Obtener el rol por defecto "user" desde la base de datos
                var userRole = await _authContext.Roles.FirstOrDefaultAsync(r => r.NameRole == "user");
                if (userRole == null)
                    return BadRequest(new { Message = "Default role 'user' does not exist in the database." });

                userObj.Role = userRole;
                userObj.Token = "";

                await _authContext.Users.AddAsync(userObj);
                await _authContext.SaveChangesAsync();

                return Ok(new { Message = "User registered successfully!" });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while registering user.");
                return StatusCode(500, new { Message = "Database error.", Details = dbEx.InnerException?.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while registering user.");
                return StatusCode(500, new { Message = "Internal server error.", Details = ex.Message });
            }
        }



        // READ - Obtener todos los usuarios
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return await _authContext.Users.ToListAsync();
        }

        // READ - Obtener usuario por ID
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _authContext.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { Message = "Usuario no encontrado." });
            }



            return Ok(user);
        }

        // UPDATE - Actualizar usuario
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User userObj)
        {
            if (id != userObj.Id)
            {
                return BadRequest(new { Message = "El ID del usuario no coincide con el objeto proporcionado." });
            }

            var existingUser = await _authContext.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { Message = "Usuario no encontrado." });
            }

            try
            {
                existingUser.FirstName = userObj.FirstName;
                existingUser.LastName = userObj.LastName;
                existingUser.Username = userObj.Username;
                existingUser.Email = userObj.Email;
                


                _authContext.Entry(existingUser).State = EntityState.Modified;
                await _authContext.SaveChangesAsync();

                return Ok(new { Message = "¡Usuario actualizado exitosamente!" });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de base de datos al actualizar el usuario.");
                return StatusCode(500, new { Message = "Error en la base de datos.", Details = dbEx.InnerException?.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al actualizar el usuario.");
                return StatusCode(500, new { Message = "Error interno del servidor.", Details = ex.Message });
            }
        }

        // DELETE - Eliminar usuario
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _authContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "Usuario no encontrado." });
            }

            try
            {
                _authContext.Users.Remove(user);
                await _authContext.SaveChangesAsync();

                return Ok(new { Message = "¡Usuario eliminado exitosamente!" });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de base de datos al eliminar el usuario.");
                return StatusCode(500, new { Message = "Error en la base de datos.", Details = dbEx.InnerException?.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al eliminar el usuario.");
                return StatusCode(500, new { Message = "Error interno del servidor.", Details = ex.Message });
            }
        }

        private async Task<bool> CheckUserNameExistAsync(string userName)
        {
            return await _authContext.Users.AnyAsync(x => x.Username == userName);
        }

        private async Task<bool> CheckEmailExistAsync(string email)
        {
            return await _authContext.Users.AnyAsync(x => x.Email == email);
        }

        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if (password.Length < 8)
                sb.AppendLine("La contraseña debe tener al menos 8 caracteres.");
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]")))
                sb.AppendLine("La contraseña debe ser alfanumérica.");
            if (!Regex.IsMatch(password, "[<,>,@,!,#,$,%,&,*,(,),_,\\[,\\],{,},?,:,;,|,',\\,.,/,-,=]"))
                sb.AppendLine("La contraseña debe contener caracteres especiales.");

            return sb.ToString();
        }

        


            private async Task<string> CreateJwtToken(User user)
        {
            // Cargar el objeto Role si no está cargado
            if (user.Role == null && user.RoleId.HasValue)
            {
                user.Role = await _authContext.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);
            }

            string roleId = user?.Role?.Id.ToString() ?? "DefaultRole";
            var fullName = $"{user?.FirstName ?? "Usuario"} {user?.LastName ?? "Desconocido"}";

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysceretandsecurekeythatneedstobelonger12345");

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Role, roleId),
        new Claim(ClaimTypes.Name, fullName)
    };

            var claimsIdentity = new ClaimsIdentity(claims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

    }
}
