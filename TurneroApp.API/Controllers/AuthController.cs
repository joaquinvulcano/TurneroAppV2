using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TurneroApp.API.Context;
using TurneroApp.API.DTOs;
using TurneroApp.API.Models;


namespace TurneroApp.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _accessToken;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _accessToken = configuration["AccessToken"];
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, [FromHeader] string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken) || accessToken != _accessToken)
            {
                return Unauthorized("Token de acceso nulo o inválido.");
            }

            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest("Las contraseñas no coinciden.");
            }

            var usuario = new Usuario
            {
                Nombre = request.Name,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                Rol = request.Role
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok("Registro exitoso.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromHeader] string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken) || accessToken != _accessToken)
            {
                return Unauthorized("Token de acceso nulo o inválido.");
            }

            var usuario = await _context.Usuarios.SingleOrDefaultAsync(u => u.Email == request.Email);
            if (usuario == null || !VerifyPassword(request.Password, usuario.PasswordHash))
            {
                return Unauthorized("Credenciales inválidas.");
            }

            var token = GenerateJwtToken(usuario);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}