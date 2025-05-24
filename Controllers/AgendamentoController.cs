using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;


[Route("api/[controller]")]
[ApiController]


public class AgendamentoController : ControllerBase
{
    public class CriarAgendamentoDto
    {
        public int Id { get; set; }
        public string CPF { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public DateTime DataNascimento { get; set; }
        public int TipoExameId { get; set; }
        public TipoExame? TipoExame { get; set; }
        public DateTime DataHoraExame { get; set; }
        public bool Cancelado { get; set; }
        public bool Comparecimento { get; set; }

        public CriarAgendamentoDto() { }
    }
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
    public async Task<ActionResult> Post([FromBody] CriarAgendamentoDto dto)
    {
        var agendamento = new Agendamento
        {
            CPF = dto.CPF,
            Nome = dto.Nome,
            Email = dto.Email,
            DataNascimento = dto.DataNascimento,
            TipoExameId = dto.TipoExameId,
            DataHoraExame = dto.DataHoraExame,
            Cancelado = false,
            Comparecimento = null
        };

        try
        {
            await context.Agendamentos.AddAsync(agendamento);
            await context.SaveChangesAsync();

            // Buscar o TipoExame para obter os dados
            var tipoExame = await context.TipoExames
                .FirstOrDefaultAsync(t => t.Id == dto.TipoExameId);

            var mensagem = new MailMessage();
            mensagem.From = new MailAddress("noreply.medifynow@gmail.com", "MedifyNow");
            mensagem.To.Add(dto.Email);
            mensagem.Subject = "Novo Agendamento de Exame";
            mensagem.Body = $@"Olá {dto.Nome},

                Seu exame foi agendado com sucesso!

                Dados do agendamento:
                - Data: {dto.DataHoraExame:dd/MM/yyyy HH:mm}
                - Exame: {tipoExame?.Nome ?? "Não especificado"}

                Instruções de preparo:
                {tipoExame?.InstrucoesPreparo ?? "Nenhuma instrução fornecida."}

                Atenciosamente,
                Equipe MedifyNow";

            using var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("noreply.medifynow@gmail.com", "chrv tmqg rnwq nvnk"),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mensagem);

            return Ok("Agendamento salvo com sucesso.");
        }
        catch
        {
            return BadRequest("Erro ao salvar o agendamento:");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Agendamento>> Get([FromRoute] int id)
    {
        try
        {
            if (await context.Agendamentos.AnyAsync(p => p.Id == id))
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
            var agendamentoExistente = await context.Agendamentos
                .Include(a => a.TipoExame)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (agendamentoExistente == null)
                return NotFound("Agendamento inválido");

            agendamentoExistente.CPF = model.CPF;
            agendamentoExistente.Nome = model.Nome;
            agendamentoExistente.Email = model.Email;
            agendamentoExistente.DataNascimento = model.DataNascimento;
            agendamentoExistente.TipoExameId = model.TipoExameId;
            agendamentoExistente.DataHoraExame = model.DataHoraExame;
            agendamentoExistente.Cancelado = model.Cancelado;
            agendamentoExistente.Comparecimento = model.Comparecimento;

            await context.SaveChangesAsync();

            try
            {
                var tipoExame = await context.TipoExames.FindAsync(model.TipoExameId);

                var mensagem = new MailMessage();
                mensagem.From = new MailAddress("noreply.medifynow@gmail.com", "MedifyNow");
                mensagem.To.Add(model.Email);
                mensagem.Subject = "Alteração no seu agendamento";

                mensagem.Body = $@"Olá {model.Nome},

                Seu agendamento foi alterado com sucesso!

                Dados atualizados do agendamento:
                - Data: {model.DataHoraExame:dd/MM/yyyy HH:mm}
                - Exame: {tipoExame?.Nome ?? "Não especificado"}

                Instruções de preparo:
                {tipoExame?.InstrucoesPreparo ?? "Nenhuma instrução fornecida."}

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
            catch
            {
                return BadRequest("Erro ao enviar e-mail: ");
            }

            return Ok("Agendamento atualizado com sucesso e e-mail enviado.");
        }
        catch
        {
            return BadRequest("Erro ao salvar o agendamento informado");
        }
    }

    [HttpGet("pesquisaCPF/{CPF}")]
    public async Task<ActionResult<IEnumerable<Agendamento>>> Get([FromRoute] string CPF)
    {
        try
        {
            var agendamento = await context.Agendamentos.FirstOrDefaultAsync(p => p.CPF == CPF);

            if (agendamento == null)
                return NotFound("Agendamento não encontrado.");

            if (!agendamento.Comparecimento.GetValueOrDefault())
            {
                agendamento.Comparecimento = true;
                context.Agendamentos.Update(agendamento);
                await context.SaveChangesAsync();
            }

            return Ok("Comparecimento confirmado com sucesso.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro: {ex.Message}");
        }
    }

    [HttpPost("CancelarAgendamento/{id}")]
    public async Task<IActionResult> CancelarAgendamento(int id)
    {
        var agendamento = await context.Agendamentos
            .Include(a => a.TipoExame)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agendamento == null)
            return NotFound("Agendamento não encontrado.");

        if ((bool)agendamento.Cancelado)
            return BadRequest("Agendamento já está cancelado.");

        if (agendamento.DataHoraExame < DateTime.Now)
            return BadRequest("Não é possível cancelar agendamentos com datas anteriores a data atual.");

        agendamento.Cancelado = true;

        await context.SaveChangesAsync();

        try
        {
            var mensagem = new MailMessage();
            mensagem.From = new MailAddress("noreply.medifynow@gmail.com", "MedifyNow");
            mensagem.To.Add(agendamento.Email);
            mensagem.Subject = "Cancelamento de Agendamento";

            mensagem.Body = $@"Olá {agendamento.Nome},

            Seu agendamento foi cancelado com sucesso.

            Dados do agendamento cancelado:
            - Data: {agendamento.DataHoraExame:dd/MM/yyyy HH:mm}
            - Exame: {agendamento.TipoExame?.Nome ?? "Não especificado"}

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
        }

        return Ok("Agendamento cancelado e e-mail enviado com sucesso.");
    }
}
