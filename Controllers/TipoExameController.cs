using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class TipoExameController : ControllerBase
{
    private readonly DataContext context;

    public TipoExameController(DataContext _context)
    {
        context = _context;
    }
    
    [HttpGet]
    public ActionResult<IEnumerable<TipoExame>> Get()
    {
        return new List<TipoExame>();
    }

    [HttpPost]
    public ActionResult Post(TipoExame item)
    {
        return Ok("Apenas Validando");
    }
}