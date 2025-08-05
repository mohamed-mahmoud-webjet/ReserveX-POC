using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Tours.Services;
using ToursApi.Models;


[ApiController]
[Route("api/[controller]")]
public class ToursController : ControllerBase
{
    private readonly IToursProviderService _toursProviderService;
    private readonly IMapper _mapper;
    public ToursController(IToursProviderService toursProviderService, IMapper mapper)
    {
        _toursProviderService = toursProviderService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> AutoComplete([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest("Search term is required.");
        }

        var dtos = await _toursProviderService.AutoCompleteAsync(term);
        var results = _mapper.Map<IEnumerable<AutoCompleteResult>>(dtos);
        return Ok(results);
    }
}