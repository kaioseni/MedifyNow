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
}