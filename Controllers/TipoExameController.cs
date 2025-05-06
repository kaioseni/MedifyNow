using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class TipoExameController : ControllerBase
{
    private readonly DataContext context;

    public TipoExameController(DataContext _context)
    {
        context = _context;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TipoExame>>> Get()
    {
        try
        {
            return Ok(await context.TipoExames.ToListAsync());
        }
        catch
        {
            return BadRequest("Erro ao listar os tipos de exame");
        }
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody]TipoExame item)
    {
        try
        {
            item.Ativo = true;
            await context.TipoExames.AddAsync(item);
            await context.SaveChangesAsync();
            return Ok("Tipo de exame salvo com sucesso");
        }
        catch
        {
            return BadRequest("Erro ao salvar o tipo de exame");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TipoExame>> Get([FromRoute] int id)
    {
        try
        {
            if(await context.TipoExames.AnyAsync(p => p.Id == id))
                return Ok(await context.TipoExames.FindAsync(id));
            else    
                return NotFound("O tipo de exame informado não foi encontrado");
        }
        catch
        {
            return BadRequest("Erro ao realizar a busca do tipo de Exame");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Put([FromRoute] int id, [FromBody] TipoExame model)
    {
        if (id != model.Id)
            return BadRequest("Tipo de exame inválido");
    try
    {   
        if(!await context.TipoExames.AnyAsync(p => p.Id == id))
            return NotFound("Tipo de exame inválido");
        context.TipoExames.Update(model);
        await context.SaveChangesAsync();
        return Ok("Tipo de exame salvo com sucesso");
    }
     catch
     {
        return BadRequest("Erro ao salvar o tipo de exame informado");
     }   
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        try
        {
            TipoExame model = await context.TipoExames.FindAsync(id);

            if(model == null)
                return NotFound("Tipo de exame inválido");

            context.TipoExames.Remove(model);
            await context.SaveChangesAsync();
            return Ok("Tipo de exame removido com sucesso");
        }
        catch
        {
            return BadRequest("Falha ao remover o tipo de exame");
        }
    }
    
}