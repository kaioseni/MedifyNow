using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]

public class SystemController : ControllerBase
{
    private readonly DataContext context;

    public SystemController(DataContext _context)
    {
        context = _context;
    }

    //RF01 - Primeira execução - Valida se já existe usuario do tipo Administrador
    [HttpGet]
    public async Task<ActionResult<bool>> Get()
    {
        var Admin = await context.Administrador.AnyAsync();
        return Ok(Admin);
    }
}
