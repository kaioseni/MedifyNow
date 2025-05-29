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

    public class AtendimentoRealizadoDto
    {
        public string NomePaciente { get; set; }
        public DateTime DataHoraAgendamento { get; set; }
        public DateTime? DataHoraInicioAtendimento { get; set; }
        public DateTime? DataHoraFinalizacao { get; set; }
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

    //RF09 - Retorna todos agendamentos para o dia atual
    [HttpGet("Hoje")]
    public IActionResult GetPainelHoje()
    {
        var hoje = DateTime.Today;
        var agora = DateTime.Now;

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
    [HttpPost("ConfirmacaoDados/{id}")]
   
    public IActionResult ConfirmacaoDados(int id, [FromQuery] string? observacoes)
    {
        var agendamento = context.Agendamentos.FirstOrDefault(a => a.Id == id);

        if (agendamento == null)
            return NotFound(new { mensagem = "Agendamento não encontrado." });

        if (agendamento.ConfirmacaoChamada == true)
            return BadRequest(new { mensagem = "Paciente já foi chamado." });

        agendamento.ConfirmacaoChamada = true;
        agendamento.ConfirmacaoComparecimento = DateTime.Now;

        if (!string.IsNullOrEmpty(observacoes))
        {
            agendamento.Observacoes = observacoes;
        }

        context.SaveChanges();

        return Ok(new { mensagem = $"Paciente {agendamento.Nome} chamado com sucesso." });
    }

    //RF12 - Finalizacao de Exame
    [HttpPost("FinalizarExame/{id}")]
   
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

    //RF13 - Informar Desistencia do paicente
    [HttpPost("DesistirAtendimento/{id}")]
   
    public IActionResult MarcarDesistencia(int id, [FromBody] string motivo)
    {
        var agendamento = context.Agendamentos.FirstOrDefault(a => a.Id == id);

        if (agendamento == null)
            return NotFound("Agendamento não encontrado.");

        if (agendamento.DataHoraDesistencia.HasValue)
            return BadRequest("Paciente já desistiu.");

        agendamento.DataHoraDesistencia = DateTime.Now;
        agendamento.MotivoDesistencia = motivo;

        context.SaveChanges();

        return Ok(new { message = "Desistência registrada com sucesso." });
    }

    //RF03 - Porcentagem de exames realizados
    [HttpGet("ExamesRealizados")]
    public async Task<IActionResult> ExamesRealizados()
    {
        try
        {
            var totalAgendamentos = await context.Agendamentos.CountAsync();

            if (totalAgendamentos == 0)
            {
                return Ok(new { porcentagem = 0, mensagem = "Nenhum agendamento encontrado." });
            }

            var totalRealizados = await context.Agendamentos
                .Where(a => a.DataHoraFinalizacao != null && (a.Cancelado == false || a.Cancelado == null))
                .CountAsync();

            var porcentagem = (double)totalRealizados / totalAgendamentos * 100;

            return Ok(new
            {
                totalAgendamentos,
                totalRealizados,
                porcentagem = Math.Round(porcentagem, 2)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro ao calcular a porcentagem.", detalhes = ex.Message });
        }
    }

    //RF03 - Tempo Medio de espera
    [HttpGet("TempoMedio")]
    public async Task<IActionResult> TempoMedioEspera()
    {
        try
        {
            var agendamentos = await context.Agendamentos
                .Where(a => a.DataHoraFinalizacao != null
                            && (a.Cancelado == false || a.Cancelado == null)
                            && a.DataHoraFinalizacao >= a.DataHoraExame)
                .ToListAsync();

            if (agendamentos.Count == 0)
            {
                return Ok(new { TempoMedioEspera = 0, mensagem = "Nenhum exame finalizado encontrado ou com dados consistentes." });
            }

            var temposEspera = agendamentos
                .Select(a => (a.DataHoraFinalizacao.Value - a.DataHoraExame).TotalMinutes)
                .ToList();

            var tempoMedio = temposEspera.Average();

            return Ok(new
            {
                totalExames = agendamentos.Count,
                tempoMedioMinutos = Math.Round(tempoMedio, 2)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro ao calcular o tempo médio de espera.", detalhes = ex.Message });
        }
    }

    //RF09 - Get para todos exames realizados filtrados pelo id do tipo de Exame {id}
    [HttpGet("AtendimentosRealizados/{id}")]
    public async Task<IActionResult> GetAtendimentosRealizados(int id)
    {
        var tipoExame = await context.TipoExames.FindAsync(id);

        if (tipoExame == null)
        {
            return NotFound(new { mensagem = "Tipo de exame não encontrado." });
        }

        var atendimentos = await context.Agendamentos
            .Where(a => a.TipoExameId == id && a.DataHoraFinalizacao != null)
            .Select(a => new AtendimentoRealizadoDto
            {
                NomePaciente = a.Nome,
                DataHoraAgendamento = a.DataHoraExame,
                DataHoraInicioAtendimento = a.ConfirmacaoComparecimento,
                DataHoraFinalizacao = a.DataHoraFinalizacao
            })
            .ToListAsync();

        return Ok(atendimentos);
    }

    //RF11 - Lista os agendamentos confirmados do dia, ordenados para chamada
    [HttpGet("ListaChamada")]
   
    public async Task<IActionResult> ListaChamada()
    {
        try
        {
            var hoje = DateTime.Today;

            var listaChamada = await context.Agendamentos
                .Where(a => a.DataHoraExame.Date == hoje &&
                            a.Comparecimento == true &&
                            a.ConfirmacaoComparecimento != null &&
                            a.Cancelado != true &&
                            a.DataHoraDesistencia == null)
                .OrderBy(a => a.DataHoraExame)
                .Select(a => new
                {
                    a.Id,
                    a.Nome,
                    a.CPF,
                    a.DataHoraExame,
                    a.TipoExameId,
                    a.ConfirmacaoChamada
                })
                .ToListAsync();

            return Ok(listaChamada);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro ao buscar a lista de chamada.", detalhes = ex.Message });
        }
    }

    //RF11 - Chamar paciente para realizar exame
    [HttpPost("ChamarPaciente/{id}")]
   
    public async Task<IActionResult> ChamarPaciente(int id)
    {
        try
        {
            var agendamento = await context.Agendamentos.FindAsync(id);

            if (agendamento == null)
            {
                return NotFound(new { mensagem = "Agendamento não encontrado." });
            }

            if (agendamento.Comparecimento != true || agendamento.ConfirmacaoComparecimento == null)
            {
                return BadRequest(new { mensagem = "Paciente não está apto para ser chamado." });
            }

            agendamento.ConfirmacaoChamada = true;
            agendamento.DataHoraInicial = DateTime.Now;

            context.Agendamentos.Update(agendamento);
            await context.SaveChangesAsync();

            return Ok(new { mensagem = "Paciente chamado com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro ao chamar paciente.", detalhes = ex.Message });
        }
    }

    //RF13 - Desistencia Paciente
    [HttpPost("DesistirPaciente/{id}")]
   
    public async Task<IActionResult> DesistirPaciente(int id, [FromBody] string motivoDesistencia)
    {
        try
        {
            var agendamento = await context.Agendamentos.FindAsync(id);

            if (agendamento == null)
            {
                return NotFound(new { mensagem = "Agendamento não encontrado." });
            }

            if (agendamento.Comparecimento != true || agendamento.ConfirmacaoComparecimento == null)
            {
                return BadRequest(new { mensagem = "Paciente não está apto para desistência." });
            }

            agendamento.DataHoraDesistencia = DateTime.Now;
            agendamento.MotivoDesistencia = motivoDesistencia;

            context.Agendamentos.Update(agendamento);
            await context.SaveChangesAsync();

            return Ok(new { mensagem = "Desistência registrada com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro ao registrar desistência.", detalhes = ex.Message });
        }
    }


    //RF14 - Informar Ausencia do Paicente
    [HttpPost("InformarAusencia/{id}")]
   
    public async Task<IActionResult> InformarAusencia(int id)
    {
        try
        {
            var agendamento = await context.Agendamentos.FindAsync(id);

            if (agendamento == null)
            {
                return NotFound(new { mensagem = "Agendamento não encontrado." });
            }

            if (agendamento.Comparecimento == true || agendamento.ConfirmacaoComparecimento != null)
            {
                return BadRequest(new { mensagem = "Paciente confirmou presença, não pode ser marcado como ausente." });
            }

            var tempoDeAtraso = DateTime.Now - agendamento.DataHoraExame;

            var Administrador = context.Administrador.FirstOrDefault();
            var tempoAtrasoPadrao = TimeSpan.FromMinutes(Administrador!.TempoMaximoAtraso);
            var tempoMaximo = tempoAtrasoPadrao * 2;

            if (tempoDeAtraso < tempoMaximo)
            {
                return BadRequest(new { mensagem = $"Ainda não ultrapassou o tempo de atraso máximo de {tempoMaximo.TotalMinutes} minutos." });
            }

            agendamento.Cancelado = true;
            agendamento.MotivoDesistencia = "Ausência";
            agendamento.DataHoraDesistencia = DateTime.Now;

            context.Agendamentos.Update(agendamento);
            await context.SaveChangesAsync();

            return Ok(new { mensagem = "Ausência registrada com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro ao registrar ausência.", detalhes = ex.Message });
        }
    }

    //RF15 - Resumo Atividade Diaria
    [HttpGet("ResumoAtividade")]
   
    public async Task<IActionResult> ResumoAtividade(
    [FromQuery] DateTime? data,
    [FromQuery] string? cpf,
    [FromQuery] int? tipoExameId,
    [FromQuery] string? situacao)
    {
        try
        {
            var query = context.Agendamentos.AsQueryable();

            if (data.HasValue)
            {
                query = query.Where(a => a.DataHoraExame.Date == data.Value.Date);
            }

            if (!string.IsNullOrEmpty(cpf))
            {
                query = query.Where(a => a.CPF == cpf);
            }

            if (tipoExameId.HasValue)
            {
                query = query.Where(a => a.TipoExameId == tipoExameId.Value);
            }

            if (!string.IsNullOrEmpty(situacao))
            {
                switch (situacao.ToLower())
                {
                    case "confirmado":
                        query = query.Where(a => a.Comparecimento == true);
                        break;
                    case "cancelado":
                        query = query.Where(a => a.Cancelado == true && a.MotivoDesistencia != "Ausência");
                        break;
                    case "ausente":
                        query = query.Where(a => a.MotivoDesistencia == "Ausência");
                        break;
                    case "pendente":
                        query = query.Where(a => a.Comparecimento == null && a.Cancelado != true);
                        break;
                    default:
                        break;
                }
            }

            var agendamentos = await query
                .Select(a => new
                {
                    a.Id,
                    a.Nome,
                    a.CPF,
                    a.Email,
                    a.DataHoraExame,
                    TipoExame = a.TipoExame != null ? a.TipoExame.Nome : null,
                    a.Comparecimento,
                    a.Cancelado,
                    a.MotivoDesistencia,
                    a.ConfirmacaoComparecimento,
                    a.DataHoraDesistencia,
                    a.DataHoraInicial,
                    a.DataHoraFinalizacao,
                    a.Observacoes
                })
                .ToListAsync();

            return Ok(agendamentos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro ao obter resumo de atividades.", detalhes = ex.Message });
        }
    }
}
