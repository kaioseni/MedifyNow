using System.ComponentModel.DataAnnotations;

public class Agendamento
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve conter 11 dígitos.")]
    public string CPF { get; set; }

    [Required]
    public string Nome { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DataNascimento { get; set; }

    [Required]
    public int TipoExameId { get; set; }

    public TipoExame? TipoExame { get; set; }

    [Required]
    public DateTime DataHoraExame { get; set; }

    public bool? Cancelado { get; set; }

    public bool? Comparecimento { get; set; }

    public DateTime? ConfirmacaoComparecimento { get; set; }
    public bool? ConfirmacaoChamada { get; set; }

    public DateTime? DataHoraInicial { get; set; }
    public DateTime? DataHoraFinalizacao { get; set; }

    public DateTime? DataHoraDesistencia { get; set; }

    public string? MotivoDesistencia { get; set; }

    public string? Observacoes { get; set; }
}