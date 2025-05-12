using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedifyNow.Controllers;
using System.ComponentModel.DataAnnotations;

namespace MedifyNow.Controllers
{
    // DTOs
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Senha { get; set; }
    }

    public class LoginResponse
    {
        public string Tipo { get; set; }    
        public string Nome { get; set; }
    }

    
    [Route("api/[controller]")]
    [ApiController]
    public class AutenticacaoController : ControllerBase
    {
        private readonly DataContext context;
        private readonly IPasswordHasher<Administrador> _hasherAdmin;
        private readonly IPasswordHasher<Usuario> _hasherUsuario;

        public AutenticacaoController(
            DataContext db,
            IPasswordHasher<Administrador> hasherAdmin,
            IPasswordHasher<Usuario> hasherUsuario)
        {
            context = db;
            _hasherAdmin = hasherAdmin;
            _hasherUsuario = hasherUsuario;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LoginRequest item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var admin = await context.Set<Administrador>()
                                 .FirstOrDefaultAsync(p => p.Email == item.Email);
            if (admin != null)
            {
                var result = _hasherAdmin.VerifyHashedPassword(admin, admin.Senha, item.Senha);
                if (result == PasswordVerificationResult.Success)
                {
                    return Ok(new LoginResponse
                    {
                        Tipo = "Administrador",
                        Nome = admin.Nome
                    });
                }
            }
            var usuario = await context.Set<Usuario>()
                               .FirstOrDefaultAsync(u => u.Email == item.Email && u.Ativo);
            if (usuario != null)
            {
                var result = _hasherUsuario.VerifyHashedPassword(usuario, usuario.Senha, item.Senha);
                if (result == PasswordVerificationResult.Success)
                {
                    return Ok(new LoginResponse
                    {
                        Tipo = "Secretaria",
                        Nome = usuario.Nome
                    });
                }
            }
            return Unauthorized(new { Erro = "E-mail ou senha inválidos." });
        }
    }
}