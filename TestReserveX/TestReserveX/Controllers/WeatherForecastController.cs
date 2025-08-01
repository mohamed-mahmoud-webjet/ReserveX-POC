using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ReservexService _ReservexService;

    public SearchController(ReservexService ReservexService)
    {
        _ReservexService = ReservexService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest("Search term is required.");
        }

        try
        {
            var results = await _ReservexService.SearchAsync(term);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
