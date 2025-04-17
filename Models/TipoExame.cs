using System.ComponentModel.DataAnnotations;

public class TipoExame
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string Nome { get; set; }

    [Required]
    public string Descricao { get; set; }

    [Required]
    public double DuracaoPadrao { get; set; }

    [Required]
    public string InstrucoesPreparo { get; set; }

    public bool Ativo { get; set; }
}