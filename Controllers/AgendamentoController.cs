using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;


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

    //RF07 - Agendamento de exames - realiza GET em todos os agendamentos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Agendamento>>> Get()
    {
        try
        {
            return Ok(await context.Agendamentos.ToListAsync());
        }
        catch
        {
            return BadRequest("Erro ao listar os agendamentos");
        }
    }

    //RF07 - Agendamento de exames - Cadastra um novo agendamento e envia um e-mail(paciente) ao e-mail informado no momento do agendamento
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

    //RF07 - Agendamento de exames - Realiza GET em um agendamento informado o ID
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

    //RF07 - Agendamento de exames - Realiza a alteração dos dados de um agendamento
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

    //RF07 - Agendamento de exames && RF08 - Informar comparecimento de paciente - Realiza a pesquisa com base no CPF do paciente e atribui comparecimento do paciente com base no RF08 - Informar comparecimento de paciente
    [HttpGet("PesquisaCPF/{CPF}")]
    public async Task<ActionResult> Get([FromRoute] string CPF)
    {
        try
        {
            var hoje = DateTime.Today;

            var agendamentos = await context.Agendamentos
                .Include(a => a.TipoExame)
                .Where(a => a.CPF == CPF && a.DataHoraExame.Date == hoje)
                .ToListAsync();

            if (agendamentos == null || !agendamentos.Any())
                return NotFound("Nenhum agendamento encontrado para hoje.");

            foreach (var agendamento in agendamentos)
            {
                if (!agendamento.Comparecimento.GetValueOrDefault())
                {
                    agendamento.Comparecimento = true;
                }
            }

            await context.SaveChangesAsync();

            var nomePaciente = agendamentos.First().Nome;

            var exames = agendamentos.Select(a => new
            {
                Exame = a.TipoExame?.Nome ?? "Não especificado",
                Horario = a.DataHoraExame.ToString("HH:mm"),
                Instrucoes = a.TipoExame?.InstrucoesPreparo ?? "Nenhuma instrução fornecida."
            });

            var resposta = new
            {
                Mensagem = $"Bem-vindo(a), {nomePaciente}!",
                Exames = exames
            };

            return Ok(resposta);
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro: {ex.Message}");
        }
    }

    //RF07 - Agendamento de exames - Realiza cancelamento em um agendamento
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

    //RF09 - 
    [HttpGet("Hoje")]
    public IActionResult GetPainelHoje()
    {
        var hoje = DateTime.Today;
        var agora = DateTime.Now;

        // Buscar todos os agendamentos confirmados de hoje
        var agendamentosHoje = context.Agendamentos
            .Where(a => a.DataHoraExame.Date == hoje
                        && a.Comparecimento == true
                        && (a.Cancelado == null || a.Cancelado == false))
            .Include(a => a.TipoExame)
            .ToList();

        var agrupadoPorTipo = agendamentosHoje
            .GroupBy(a => a.TipoExameId)
            .Select(g =>
            {
                var tipoExame = g.First().TipoExame;
                var duracaoPadrao = tipoExame.DuracaoPadrao;

                var tempoMedio = duracaoPadrao;

                var fila = g.OrderBy(a => a.DataHoraExame)
                            .Select((a, index) => new
                            {
                                a.Nome,
                                a.DataHoraExame,
                                TempoEstimadoAtendimento = TimeSpan.FromMinutes((index + 1) * tempoMedio.TotalMinutes).ToString(@"hh\:mm\:ss")
                            }).ToList();

                return new
                {
                    tipoExameId = tipoExame.Id,
                    nome = tipoExame.Nome,
                    duracaoPadrao = duracaoPadrao.ToString(@"hh\:mm\:ss"),
                    tempoMedioAtendimento = tempoMedio.ToString(@"hh\:mm\:ss"),
                    fila = fila
                };
            })
            .ToList();

        var result = new
        {
            dataAtual = hoje.ToString("yyyy-MM-dd"),
            horaAtual = agora.ToString("HH:mm:ss"),
            tiposExame = agrupadoPorTipo
        };

        return Ok(result);
    }

    //RF10 - Ordenação dos pacientes atrasados e pacientes dentro do prazo
    [HttpGet("ChamadaSecretaria")]
    //[Authorize(Roles = "secretaria")]
    public IActionResult GetListaChamada()
    {
        var hoje = DateTime.Today;
        var Administrador = context.Administrador.FirstOrDefault();
        var tempoMaximoEspera = TimeSpan.FromMinutes(Administrador!.TempoMaximoAtraso);

        var agendamentos = context.Agendamentos
            .Where(a => a.DataHoraExame.Date == hoje
                        && a.Comparecimento == true
                        && (a.Cancelado == null || a.Cancelado == false)
                        && (a.ConfirmacaoChamada == null || a.ConfirmacaoChamada == false || a.ConfirmacaoChamada == true))
            .Include(a => a.TipoExame)
            .ToList();

        var naoAtrasados = new List<dynamic>();
        var atrasados = new List<dynamic>();

        foreach (var agendamento in agendamentos)
        {
            var limiteEspera = agendamento.DataHoraExame + tempoMaximoEspera;
            var dataConfirmacao = agendamento.ConfirmacaoComparecimento;

            if (dataConfirmacao.HasValue && dataConfirmacao <= limiteEspera)
            {
                naoAtrasados.Add(new
                {
                    agendamento.Id,
                    agendamento.Nome,
                    agendamento.DataHoraExame,
                    agendamento.ConfirmacaoComparecimento,
                    TipoExameNome = agendamento.TipoExame?.Nome ?? "Não informado",
                    Status = "No prazo"
                });
            }
            else
            {
                atrasados.Add(new
                {
                    agendamento.Id,
                    agendamento.Nome,
                    agendamento.DataHoraExame,
                    agendamento.ConfirmacaoComparecimento,
                    TipoExameNome = agendamento.TipoExame?.Nome ?? "Não informado",
                    Status = "Atrasado"
                });
            }
        }

        var listaFinal = naoAtrasados
            .OrderBy(a => a.DataHoraExame)
            .Concat(atrasados.OrderBy(a => a.DataHoraExame))
            .ToList();

        return Ok(listaFinal);
    }

    //RF10 - Chamada da Secretaria para confirmação de dados do paciente
    [HttpPost("ChamarPaciente/{id}/chamar")]
    //[Authorize(Roles = "Secretaria")]
    public IActionResult ChamarPaciente(int id)
    {
        var agendamento = context.Agendamentos.FirstOrDefault(a => a.Id == id);

        if (agendamento == null)
            return NotFound("Agendamento não encontrado.");

        if (agendamento.ConfirmacaoChamada == true)
            return BadRequest("Paciente já foi chamado.");

        agendamento.ConfirmacaoChamada = true;
        agendamento.ConfirmacaoComparecimento = DateTime.Now;
        context.SaveChanges();

        return Ok(new { message = $"Paciente {agendamento.Nome} chamado com sucesso." });
    }

    //RF12 - Finalizacao de Exame
    [HttpPost("FinalizarExame/{id}")]
    //[Authorize(Roles = "secretaria")]
    public IActionResult FinalizarExame(int id)
    {
        var agendamento = context.Agendamentos.FirstOrDefault(a => a.Id == id);

        if (agendamento == null)
            return NotFound("Agendamento não encontrado.");

        if (agendamento.DataHoraFinalizacao.HasValue)
            return BadRequest("Atendimento já foi finalizado.");

        agendamento.DataHoraFinalizacao = DateTime.Now;

        context.SaveChanges();

        return Ok(new { message = "Atendimento finalizado com sucesso." });
    }


}
