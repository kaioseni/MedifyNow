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

    [HttpGet]
    public async Task<ActionResult<bool>> Get()
    {
        var Admin = await context.Administrador.AnyAsync();
        return Ok(Admin);
    }
}
