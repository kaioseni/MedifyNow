using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

[Route("api/[controller]")]
[ApiController]

public class UsuarioController : ControllerBase
{
    private readonly DataContext context;

    public UsuarioController(DataContext _context)
    {
        context = _context;
    }

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

    [HttpPost]
    public async Task<ActionResult> Post([FromBody]Usuario item)
    {
        try
        {
            var hasher = new PasswordHasher<Usuario>();

            item.Senha = hasher.HashPassword(item, item.Senha);

            item.Ativo = true;
            await context.Usuarios.AddAsync(item);
            await context.SaveChangesAsync();
            return Ok("Usuário salvo com sucesso");
        }
        catch 
        {
            return BadRequest("Erro ao salvar o usuário");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Usuario>> Get([FromRoute] int id)
    {
        try
        {
            if(await context.Usuarios.AnyAsync(p => p.Id == id))
                return Ok(await context.Usuarios.FindAsync(id));
            else    
                return NotFound("Usuário informado não foi encontrado");
        }
        catch
        {
            return BadRequest("Erro ao realizar a busca do usuário");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Put([FromRoute] int id, [FromBody] Usuario model)
    {
        if (id != model.Id)
            return BadRequest("Usuário inválido");
    try
    {   
        if(!await context.Usuarios.AnyAsync(p => p.Id == id))
            return NotFound("Usuário inválido");

        var hasher = new PasswordHasher<Usuario>();

        model.Senha = hasher.HashPassword(model, model.Senha);

        model.Ativo = true;
        context.Usuarios.Update(model);
        await context.SaveChangesAsync();
        return Ok("Usuário salvo com sucesso");
    }
     catch
     {
        return BadRequest("Erro ao salvar o usuário informado");
     }   
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        try
        {
            Usuario model = await context.Usuarios.FindAsync(id);

            if(model == null)
                return NotFound("Tipo de usuário inválido");

            context.Usuarios.Remove(model);
            await context.SaveChangesAsync();
            return Ok("Usuário removido com sucesso");
        }
        catch
        {
            return BadRequest("Falha ao remover o usuário");
        }
    }
}