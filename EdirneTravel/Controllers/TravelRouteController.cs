using EdirneTravel.Application.Services;
using EdirneTravel.Application.Services.Base;
using EdirneTravel.Controllers.Base;
using EdirneTravel.Models.Dtos;
using EdirneTravel.Models.Dtos.TravelRoute;
using EdirneTravel.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EdirneTravel.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class TravelRouteController : BaseController<TravelRoute,TravelRouteDto>
    {
        private readonly ITravelRouteService _travelRouteService;
        public TravelRouteController(ITravelRouteService service) : base(service)
        {
            _travelRouteService = service;

        }

        [HttpPost("SaveTravelRouteWithPlaces")]
        public IActionResult SaveTravelRouteWithPlaces([FromBody] CreateTravelRouteWithPlacesDto travelRouteDto)
        {
            var result = _travelRouteService.SaveTravelRouteWithPlaces(travelRouteDto);

            if (result.Success)
                return Ok(result.Data);

            return BadRequest(result.Message);
        }

        [HttpGet("GetTravelRouteWithPlaces/{id}")]
        public IActionResult GetTravelRouteWithPlaces(int id)
        {
            var result = _travelRouteService.GetTravelRouteWithPlacesById(id);

            if (result.Success)
                return Ok(result.Data);

            return BadRequest(result.Message);
        }
    }
}
