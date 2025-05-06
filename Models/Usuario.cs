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

    [Required(ErrorMessage = "A senha é obrigatória.")]

    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
        ErrorMessage = "A senha deve conter no mínimo 8 caracteres, incluindo uma letra maiúscula, uma letra minúscula, um número e um caractere especial.")]
    public string Senha { get; set; }

    public string? FotoUrl { get; set; }

    [Required]
    public DateTime DataNascimento { get; set; }

    
    public bool Ativo { get; set; }
}