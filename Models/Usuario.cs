using System.ComponentModel.DataAnnotations;

public class Usuario
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string Nome { get; set ;}

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string? FotoUrl { get; set; }

    [Required]
    public DateTime DataNascimento { get; set; }

    
    public bool Ativo { get; set; }
}