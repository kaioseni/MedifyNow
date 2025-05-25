using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

[Route("api/[controller]")]
[ApiController]

public class AdministradorController : ControllerBase
{
    public class AdministradorDto
    {
        [Required]
        public string Nome { get; set; }
    }
    private readonly DataContext context;

    public AdministradorController(DataContext _context)
    {
        context = _context;
    }


    //RF01 - Primeira execução - Realiza get no usuario do tipo administrador
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Administrador>>> Get()
    {
        try
        {
            return Ok(await context.Administrador.ToListAsync());
        }
        catch
        {
            return BadRequest("Erro ao buscar Administrador");
        }
    }

    //RF01 - Primeira execução - Realiza o cadastro no usuario administrador
    [HttpPost]
    public async Task<ActionResult> Post([FromBody] Administrador item)
    {
        if (await context.Administrador.AnyAsync())
            return BadRequest("Administrador já foi cadastrado.");

        try
        {
            var hasher = new PasswordHasher<Administrador>();

            item.Senha = hasher.HashPassword(item, item.Senha);

            item.TempoMaximoAtraso = 0;
            await context.Administrador.AddAsync(item);
            await context.SaveChangesAsync();
            return Ok("Administrador salvo com sucesso");
        }
        catch
        {
            return BadRequest("Erro ao salvar o administrador");
        }
    }

    //RF01 - Primeira execução - Realiza alteração no nome do usuario administrador
    [HttpPatch]
    public async Task<ActionResult> Patch([FromBody] AdministradorDto dto)
    {
        try
        {
            var admin = await context.Administrador.FirstOrDefaultAsync();
            if (admin == null)
                return NotFound("Administrador não encontrado.");

            admin.Nome = dto.Nome;

            await context.SaveChangesAsync();
            return Ok("Nome do administrador atualizado com sucesso.");
        }
        catch
        {
            return BadRequest("Erro ao atualizar o administrador.");
        }
    }

    //RF01 - Primeira execução - Usuario administrador escolhe o tempo maximo de atraso de um paciente
    [HttpPatch("TempoAtraso")]
    public async Task<ActionResult> AtualizarTempoAtraso([FromBody] int novoTempo)
    {
        var admin = await context.Administrador.FirstOrDefaultAsync();
        if (admin == null)
            return NotFound("Administrador não encontrado.");

        admin.TempoMaximoAtraso = novoTempo;

        try
        {
            admin.TempoMaximoAtraso = novoTempo;
            await context.SaveChangesAsync();
            return Ok("Tempo máximo de atraso atualizado com sucesso.");
        }
        catch
        {
            return BadRequest("Erro ao atualizar o tempo de atraso.");
        }
    }

    //RF03 - Dashboard de usuário - Retorna o total de usuarios do tipo secretaria
    [HttpGet("TotalUsuarios")]
    public async Task<ActionResult> TotalUsuarios()
    {
        try
        {
            var totalUsuarios = await context.Usuarios.CountAsync();

            return Ok($"Toal de Usuarios: {totalUsuarios}");
        }
        catch
        {
            return StatusCode(500, "Erro ao recuperar os dados do dashboard.");
        }
    }

    //RF03 - Dashboard de usuário - Retorna o total do tipo de exames cadastrados no sistema
    [HttpGet("TotalExames")]
    public async Task<ActionResult> TotalExames()
    {
        try
        {
            var TotalExames = await context.TipoExames.CountAsync();

            return Ok($"Toal de Exames: {TotalExames}");
        }
        catch
        {
            return StatusCode(500, "Erro ao recuperar os dados do dashboard.");
        }
    }

}