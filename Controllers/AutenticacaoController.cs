using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedifyNow.Controllers;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;

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

    public class TrocaSenhaDto
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha temporária é obrigatória.")]
        public string SenhaTemporaria { get; set; }

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "A senha deve conter no mínimo 8 caracteres, incluindo uma letra maiúscula, uma letra minúscula, um número e um caractere especial.")]
        public string NovaSenha { get; set; }
    }

    public class ConfirmarTrocaSenhaDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string CodVerificacao { get; set; }

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$",
            ErrorMessage = "A senha deve conter no mínimo 8 caracteres, incluindo uma letra maiúscula, uma letra minúscula, um número e um caractere especial.")]
        public string NovaSenha { get; set; }
    }
    public class TrocaEmailDto
    {
        [Required(ErrorMessage = "O novo e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string NovoEmail { get; set; }
    }

    public class ConfirmarTrocaEmailDto
    {
        [Required(ErrorMessage = "O código de verificação é obrigatório.")]
        [StringLength(6, ErrorMessage = "O código de verificação deve ter 6 caracteres.")]
        public string Codigo { get; set; }
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


        [HttpPost("Login")]
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
                    if (!usuario.Login)
                        return Ok(new
                        {
                            Tipo = "Secretaria",
                            Nome = usuario.Nome,
                            Mensagem = "Primeiro acesso, é necessário trocar a senha."
                        });
                }

                usuario.Login = true;

                context.Usuarios.Update(usuario);
                await context.SaveChangesAsync();

                return Ok(new LoginResponse
                {
                    Tipo = "Secretaria",
                    Nome = usuario.Nome
                });
            }

            return Unauthorized(new { Erro = "E-mail ou senha inválidos." });
        }

        [HttpPost("PrimeiroAcesso")]
        public async Task<IActionResult> PrimeiroAcesso([FromBody] TrocaSenhaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await context.Usuarios
                                        .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            var resultado = _hasherUsuario.VerifyHashedPassword(usuario, usuario.Senha, dto.SenhaTemporaria);

            if (resultado == PasswordVerificationResult.Failed)
                return BadRequest("Senha temporária inválida.");

            usuario.Senha = _hasherUsuario.HashPassword(usuario, dto.NovaSenha);
            usuario.Login = true;

            context.Usuarios.Update(usuario);
            await context.SaveChangesAsync();

            return Ok("Senha atualizada com sucesso.");
        }

        [HttpPost("EsqueciSenha/{id}")]
        public async Task<IActionResult> EsqueciSenha(int id)
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            await GerarCodAleatorio(usuario);

            return Ok("Código enviado para o e-mail cadastrado.");
        }

        private async Task GerarCodAleatorio(Usuario item)
        {
            item.CodVerificacao = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            await context.SaveChangesAsync();
            try
            {
                var mensagem = new MailMessage();
                mensagem.From = new MailAddress("noreply.medifynow@gmail.com", "MedifyNow");
                mensagem.To.Add(item.Email);
                mensagem.Subject = "Solicitação de alteração de senha";
                mensagem.Body = $@"Olá {item.Nome},

        Seu código para redefinir sua senha é: {item.CodVerificacao}

        Atenciosamente,
        Equipe MedifyNow";

                using var smtp = new SmtpClient("smtp.gmail.com")

                {
                    Port = 587,
                    Credentials = new NetworkCredential("noreply.medifynow@gmail.com", "chrv tmqg rnwq nvnk"),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(mensagem);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Erro ao enviar e-mail: " + ex.Message);
                throw;
            }
        }

        [HttpPost("RedefinirSenha")]
        public async Task<IActionResult> RedefinirSenha([FromBody] ConfirmarTrocaSenhaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == dto.Id);

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            if (usuario.CodVerificacao != dto.CodVerificacao)
                return BadRequest("Código de verificação inválido.");

            var resultado = _hasherUsuario.VerifyHashedPassword(usuario, usuario.Senha, dto.NovaSenha);

            if (resultado == PasswordVerificationResult.Success)
            {
                return BadRequest("A nova senha não pode ser igual à senha atual.");
            }

            usuario.Senha = _hasherUsuario.HashPassword(usuario, dto.NovaSenha);
            usuario.CodVerificacao = null;

            context.Usuarios.Update(usuario);
            await context.SaveChangesAsync();

            return Ok("Senha atualizada com sucesso.");
        }

        [HttpPost("TrocaEmail/{id}")]
        public async Task<IActionResult> TrocaEmail(int id, [FromBody] TrocaEmailDto dto)
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            usuario.CodVerificacao = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            usuario.EmailTemporario = dto.NovoEmail;

            await context.SaveChangesAsync();

            try
            {
                var mensagem = new MailMessage();
                mensagem.From = new MailAddress("noreply.medifynow@gmail.com", "MedifyNow");
                mensagem.To.Add(dto.NovoEmail);
                mensagem.Subject = "Confirmação de troca de e-mail";
                mensagem.Body = $@"Olá {usuario.Nome},

                    Seu código para confirmar a troca de e-mail é: {usuario.CodVerificacao}

                    Atenciosamente,
                    Equipe MedifyNow";

                using var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("noreply.medifynow@gmail.com", "chrv tmqg rnwq nvnk"),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(mensagem);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao enviar e-mail: " + ex.Message);
                throw;
            }

            return Ok("Código de verificação enviado ao novo e-mail.");
        }

        [HttpPost("ConfirmarTrocaEmail/{id}")]
        public async Task<IActionResult> ConfirmarTrocaEmail(int id, [FromBody] ConfirmarTrocaEmailDto  dto)
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            if (usuario.CodVerificacao != dto.Codigo)
                return BadRequest("Código de verificação inválido.");

            if (string.IsNullOrEmpty(usuario.EmailTemporario))
                return BadRequest("Não há solicitação de troca de e-mail pendente.");

            usuario.Email = usuario.EmailTemporario;
            usuario.EmailTemporario = null;
            usuario.CodVerificacao = null;

            await context.SaveChangesAsync();

            return Ok("E-mail atualizado com sucesso.");
        }

    }
}

