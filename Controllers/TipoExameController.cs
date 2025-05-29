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

    //RF04 - Manutenção de tipos de exames - Realiza o get em todos os tipos de exames cadastrados
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

    //RF04 - Manutenção de tipos de exames - Realiza cadastro de um novo tipo de exame
    [HttpPost]
    public async Task<ActionResult> Post([FromBody] TipoExame item)
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

    //RF04 - Manutenção de tipos de exames - Altera os dados de um tipo de exame com base eu seu ID
    [HttpGet("{id}")]
    public async Task<ActionResult<TipoExame>> Get([FromRoute] int id)
    {
        try
        {
            if (await context.TipoExames.AnyAsync(p => p.Id == id))
                return Ok(await context.TipoExames.FindAsync(id));
            else
                return NotFound("O tipo de exame informado não foi encontrado");
        }
        catch
        {
            return BadRequest("Erro ao realizar a busca do tipo de Exame");
        }
    }

    //RF04 - Manutenção de tipos de exames - Altera os dados de um Tipo de exame
    [HttpPut("{id}")]
    public async Task<ActionResult> Put([FromRoute] int id, [FromBody] TipoExame model)
    {
        if (id != model.Id)
            return BadRequest("Tipo de exame inválido");
        try
        {
            if (!await context.TipoExames.AnyAsync(p => p.Id == id))
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

    //RF04 - Manutenção de tipos de exames - Alterar pois o mesmo nao pode ser excluido caso não haja nenhum agendamento finalizado ou que ainda será atendido
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTipoExame(int id)
    {
        var tipoExame = await context.TipoExames.FindAsync(id);

        if (tipoExame == null)
        {
            return NotFound(new { mensagem = "Tipo de exame não encontrado." });
        }

        // Verifica se existe algum agendamento para este TipoExame
        var existeAgendamento = await context.Agendamentos
            .AnyAsync(a => a.TipoExameId == id);

        if (existeAgendamento)
        {
            return BadRequest(new { mensagem = "Não é possível excluir: existem agendamentos associados a este tipo de exame." });
        }

        context.TipoExames.Remove(tipoExame);
        await context.SaveChangesAsync();

        return Ok(new { mensagem = "Tipo de exame excluído com sucesso." });
    }

}