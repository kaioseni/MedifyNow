using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

[Route("api/[controller]")]
[ApiController]

public class AdministradorController : ControllerBase
{
    private readonly DataContext context;

    public AdministradorController(DataContext _context)
    {
        context = _context;
    }

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

    [HttpPut]
    public async Task<ActionResult> Put([FromBody] Administrador model)
    {
        try
        {
            var admin = await context.Administrador.FirstOrDefaultAsync();
            if (admin == null)
                return NotFound("Administrador não encontrado.");

            admin.Nome = model.Nome;
            admin.Email = model.Email;
            admin.Senha = model.Senha;

            await context.SaveChangesAsync();
            return Ok("Administrador salvo com sucesso");
        }
        catch
        {
            return BadRequest("Erro ao atualizar o administrador");
        }
    }

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