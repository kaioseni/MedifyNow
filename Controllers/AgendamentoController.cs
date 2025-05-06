using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Route("api/[controller]")]
[ApiController]

public class AgendamentoController : ControllerBase
{
    private readonly DataContext context;

    public AgendamentoController(DataContext _context)
    {
        context = _context;
    }

    [HttpGet]
     public async Task<ActionResult<IEnumerable<Agendamento>>> Get()
    {
        try
        {
            return Ok(await context.Agendamentos.ToListAsync());
            //return Ok(await context.Agendamentos.Include(p => p.TipoExame).ToListAsync()); --> Para preencher o objeto TipoExame com seus respectivos dados
        }
        catch
        {
            return BadRequest("Erro ao listar os agendamentos");
        }
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody]Agendamento item)
    {
        try
        {

            await context.Agendamentos.AddAsync(item);
            await context.SaveChangesAsync();
            return Ok("Agendamento salvo com sucesso");
        }
        catch 
        {
            return BadRequest("Erro ao salvar o agendamento");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Agendamento>> Get([FromRoute] int id)
    {
        try
        {
            if(await context.Agendamentos.AnyAsync(p => p.Id == id))
                return Ok(await context.Agendamentos.FindAsync(id));
            else    
                return NotFound("O agendamento informado não foi encontrado");
        }
        catch
        {
            return BadRequest("Erro ao realizar a busca do agendamento");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Put([FromRoute] int id, [FromBody] Agendamento model)
    {
        if (id != model.Id)
            return BadRequest("Agendamento inválido");
    try
    {   
        if(!await context.Agendamentos.AnyAsync(p => p.Id == id))
            return NotFound("Agendamento inválido");
            
        context.Agendamentos.Update(model);
        await context.SaveChangesAsync();
        return Ok("Agendamento salvo com sucesso");
    }
     catch
     {
        return BadRequest("Erro ao salvar o agendamento informado");
     }   
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        try
        {
            Agendamento model = await context.Agendamentos.FindAsync(id);

            if(model == null)
                return NotFound("Agendamento inválido");

            context.Agendamentos.Remove(model);
            await context.SaveChangesAsync();
            return Ok("Agendamento removido com sucesso");
        }
        catch
        {
            return BadRequest("Falha ao remover o agendamento");
        }
    }

    [HttpGet("pesquisaCPF/{CPF}")]
    public async Task<ActionResult<IEnumerable<Agendamento>>> Get([FromRoute] string CPF)
    {
        try
        {
            List<Agendamento> resultado = await context.Agendamentos.Where(p => p.CPF.Contains(CPF)).ToListAsync();
            return Ok(resultado);
        }
        catch
        {
            return BadRequest("Falha ao buscar um agendamento");
        }
    }
}
