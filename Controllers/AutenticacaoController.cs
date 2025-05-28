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

        //RF02 - Página inicial - Recebe e-mail e senha para realizar login
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

        //RF05 - Manutenção de usuários para Usuarios - Usuário do tipo secratária inputam a senha temporária enviada ao e-mail e realizam a troca de senha
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

        //RF06 - Redefinição de senha - Usuario do tipo secretária e administrador solicitam a troca de senha, código aleatorio é enviado ao e-mail cadastrado para redefinir senha
        [HttpPost("EsqueciSenha/{tipo}/{id}")]
        public async Task<IActionResult> EsqueciSenha(string tipo, int id)
        {

            if (tipo.ToLower() == "usuario")
            {
                var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
                if (usuario == null)
                    return NotFound("Usuário não encontrado.");

                await GerarCodAleatorio(usuario.Email, usuario.Nome, usuario); //chama função GerarCodAleatorio
            }
            else if (tipo.ToLower() == "admin")
            {
                var admin = await context.Administrador.FirstOrDefaultAsync(a => a.Id == id);
                if (admin == null)
                    return NotFound("Administrador não encontrado.");

                await GerarCodAleatorio(admin.Email, admin.Nome, admin); //chama função GerarCodAleatorio
            }
            else
            {
                return BadRequest("Tipo inválido. Use 'usuario' ou 'admin'.");
            }

            return Ok("Código enviado para o e-mail cadastrado.");
        }

        //RF06 - Redefinição de senha - Gera codigo aleatorio
        private async Task GerarCodAleatorio(string email, string nome, dynamic item)
        {
            item.CodVerificacao = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            await context.SaveChangesAsync();

            try
            {
                var mensagem = new MailMessage
                {
                    From = new MailAddress("noreply.medifynow@gmail.com", "MedifyNow"),
                    Subject = "Solicitação de alteração de senha",
                    Body = $@"Olá {nome},

Seu código para redefinir sua senha é: {item.CodVerificacao}

Atenciosamente,
Equipe MedifyNow"
                };

                mensagem.To.Add(email);

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

        //RF06 - Redefinição de senha - Com base no código aleatorio recebido o endpoint verifica se o codigo é o mesmo enviado ao e-mail e realiza a troca de senha para a nova senha informada pelo usuario
        [HttpPost("RedefinirSenha/{tipo}")] 
        public async Task<IActionResult> RedefinirSenha(string tipo, [FromBody] ConfirmarTrocaSenhaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (tipo.ToLower() == "usuario")
            {
                var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == dto.Id);

                if (usuario == null)
                    return NotFound("Usuário não encontrado.");

                if (usuario.CodVerificacao != dto.CodVerificacao)
                    return BadRequest("Código de verificação inválido.");

                var resultado = _hasherUsuario.VerifyHashedPassword(usuario, usuario.Senha, dto.NovaSenha);

                if (resultado == PasswordVerificationResult.Success)
                    return BadRequest("A nova senha não pode ser igual à senha atual.");

                usuario.Senha = _hasherUsuario.HashPassword(usuario, dto.NovaSenha);
                usuario.CodVerificacao = null;

                context.Usuarios.Update(usuario);
            }
            else if (tipo.ToLower() == "admin")
            {
                var admin = await context.Administrador.FirstOrDefaultAsync(a => a.Id == dto.Id);

                if (admin == null)
                    return NotFound("Administrador não encontrado.");

                if (admin.CodVerificacao != dto.CodVerificacao)
                    return BadRequest("Código de verificação inválido.");

                var resultado = _hasherAdmin.VerifyHashedPassword(admin, admin.Senha, dto.NovaSenha);

                if (resultado == PasswordVerificationResult.Success)
                    return BadRequest("A nova senha não pode ser igual à senha atual.");

                admin.Senha = _hasherAdmin.HashPassword(admin, dto.NovaSenha);
                admin.CodVerificacao = null;

                context.Administrador.Update(admin);
            }
            else
            {
                return BadRequest("Tipo inválido. Use 'usuario' ou 'admin'.");
            }

            await context.SaveChangesAsync();

            return Ok("Senha atualizada com sucesso.");
        }

        //RF05 - Manutenção de usuários - Solicitação de troca de email - Gera um código aleatorio e envia ao novo e-mail informado
        [HttpPost("TrocaEmail/{tipo}/{id}")]
        public async Task<IActionResult> TrocaEmail(string tipo, int id, [FromBody] TrocaEmailDto dto)
        {
            dynamic item;

            if (tipo.ToLower() == "usuario")
            {
                item = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            }
            else if (tipo.ToLower() == "admin")
            {
                item = await context.Administrador.FirstOrDefaultAsync(a => a.Id == id);
            }
            else
            {
                return BadRequest("Tipo inválido. Use 'usuario' ou 'admin'.");
            }

            if (item == null)
                return NotFound($"{tipo} não encontrado.");

            item.CodVerificacao = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            item.EmailTemporario = dto.NovoEmail;

            await context.SaveChangesAsync();

            try
            {
                var mensagem = new MailMessage();
                mensagem.From = new MailAddress("noreply.medifynow@gmail.com", "MedifyNow");
                mensagem.To.Add(dto.NovoEmail);
                mensagem.Subject = "Confirmação de troca de e-mail";
                mensagem.Body = $@"Olá {item.Nome},

Seu código para confirmar a troca de e-mail é: {item.CodVerificacao}

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

        //RF05 - Manutenção de usuários - Realiza a troca de e-maail com a validação do código enviado ao novo e-mail
        [HttpPost("ConfirmarTrocaEmail/{tipo}/{id}")]
        public async Task<IActionResult> ConfirmarTrocaEmail(string tipo, int id, [FromBody] ConfirmarTrocaEmailDto dto)
        {
            dynamic item;

            if (tipo.ToLower() == "usuario")
            {
                item = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            }
            else if (tipo.ToLower() == "admin")
            {
                item = await context.Administrador.FirstOrDefaultAsync(a => a.Id == id);
            }
            else
            {
                return BadRequest("Tipo inválido. Use 'usuario' ou 'admin'.");
            }

            if (item == null)
                return NotFound($"{tipo} não encontrado.");

            if (item.CodVerificacao != dto.Codigo)
                return BadRequest("Código de verificação inválido.");

            if (string.IsNullOrEmpty(item.EmailTemporario))
                return BadRequest("Não há solicitação de troca de e-mail pendente.");

            item.Email = item.EmailTemporario;
            item.EmailTemporario = null;
            item.CodVerificacao = null;

            await context.SaveChangesAsync();

            return Ok("E-mail atualizado com sucesso.");
        }

    }
}

