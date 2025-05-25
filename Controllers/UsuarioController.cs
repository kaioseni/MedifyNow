using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Net.Mail;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
public class UsuarioCreateRequest
{
    [Required]
    public string Nome { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string? FotoUrl { get; set; }

    [Required]
    public DateTime DataNascimento { get; set; }
}

[Route("api/[controller]")]
[ApiController]

public class UsuarioController : ControllerBase
{
    public class UsuarioDto
    {
        [Required]
        public string Nome { get; set; }

        public string FotoUrl { get; set; }
    }


    private readonly DataContext context;

    public UsuarioController(DataContext _context)
    {
        context = _context;
    }

    //RF05 - Manutenção de usuários - Get de todos os usarios do tipo secretária 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Usuario>>> Get()
    {
        try
        {
            return Ok(await context.Usuarios.ToListAsync());
        }
        catch
        {
            return BadRequest("Erro ao listar os usuarios");
        }
    }

    //RF05 - Manutenção de usuários - Cadastra novo usuario do tipo secretaria  e envia senha aleatoria ao e-mail informado
    [HttpPost]
    public async Task<ActionResult> Post([FromBody] Usuario item)
    {
        try
        {
            var senhaTemporaria = GerarSenhaAleatoria();

            var hasher = new PasswordHasher<Usuario>();
            item.Senha = hasher.HashPassword(item, senhaTemporaria);
            item.Ativo = true;
            item.Login = false;
            await context.Usuarios.AddAsync(item);
            await context.SaveChangesAsync();

            await EnviarEmailBoasVindas(senhaTemporaria, item);

            return Ok("Usuário salvo com sucesso e e-mail enviado.");
        }
        catch
        {
            return BadRequest("Erro ao salvar o usuário");
        }
    }

    //RF05 - Manutenção de usuários - Realiza envio de senha aleatoria gerada
    private string GerarSenhaAleatoria(int comprimento = 12)
    {
        const string caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890@$!%*?&";
        var random = new Random();
        return new string(Enumerable.Repeat(caracteres, comprimento)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    //RF05 - Manutenção de usuários - Realiza envio de e-mail ao final do cadastro do usuario do tipo secretaria
    private async Task EnviarEmailBoasVindas(string senhaGerada, Usuario item)
    {
        try
        {
            var mensagem = new MailMessage();
            mensagem.From = new MailAddress("noreply.medifynow@gmail.com", "MedifyNow");
            mensagem.To.Add(item.Email);
            mensagem.Subject = "Bem-vindo ao MedifyNow!";
            mensagem.Body = $@"Olá {item.Nome},

        Bem-vindo(a) ao sistema MedifyNow!

        Sua senha temporária para acesso é: {senhaGerada}

        Recomendamos alterar a senha após o primeiro login.

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

    //RF05 - Manutenção de usuários - Busca usuario do tipo secretaria pelo ID
    [HttpGet("{id}")]
    public async Task<ActionResult<Usuario>> Get([FromRoute] int id)
    {
        try
        {
            if (await context.Usuarios.AnyAsync(p => p.Id == id))
                return Ok(await context.Usuarios.FindAsync(id));
            else
                return NotFound("Usuário informado não foi encontrado");
        }
        catch
        {
            return BadRequest("Erro ao realizar a busca do usuário");
        }
    }

    //RF05 - Manutenção de usuários - Altera nome e Foto dos usuarios do tipo secretaria
    [HttpPatch("{id}")]
    public async Task<ActionResult> Patch([FromRoute] int id, [FromBody] UsuarioDto dto)
    {
        try
        {
            var usuario = await context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            usuario.Nome = dto.Nome;
            usuario.FotoUrl = dto.FotoUrl;

            await context.SaveChangesAsync();

            return Ok("Dados do usuario atualizado com sucesso.");
        }
        catch
        {
            return BadRequest("Erro ao atualizar o nome do usuário.");
        }
    }

    //RF05 - Manutenção de usuários - Exlui um usuario do tipo secretaria caso o mesmo nunca tenha realizado login no sistema
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        try
        {
            Usuario model = await context.Usuarios.FindAsync(id);

            if (model == null)
                return NotFound("Tipo de usuário inválido");

            if (model.Login)
                return BadRequest("Usuário não pode ser excluído, pois já realizou login.");
            context.Usuarios.Remove(model);
            await context.SaveChangesAsync();
            return Ok("Usuário removido com sucesso");

        }
        catch
        {
            return BadRequest("Falha ao remover o usuário");
        }
    }

    //RF03 - Dashboard de usuário - Pega o dia atual
    [HttpGet("DataAtual")]
    public async Task<ActionResult> DataAtual()
    {
        var hoje = DateTime.Now;

        try
        {
            return Ok($"Data de hoje: {hoje}");
        }
        catch
        {
            return BadRequest("Falha ao obter a data de hoje");
        }
    }

    //RF03 - Dashboard de usuário - Retorna o total d eexames agendados para o dia atual
    [HttpGet("TotalExames")]
    public async Task<ActionResult> TotalExames()
    {
        var hoje = DateTime.Today;
        var examesDoDia = await context.Agendamentos.Where(e => e.DataHoraExame.Date == hoje).ToListAsync();
        var totalExamesDia = examesDoDia.Count;

        try
        {
            return Ok($"Total de exames para hoje: {totalExamesDia}");
        }
        catch
        {
            return BadRequest("Falha ao obter o total de exames para o dia de hoje");
        }
    }
}