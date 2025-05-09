using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController(ITripsService tripsService) : ControllerBase
    {
        /*
            Returns a list of all trips including their name, description, dates, max participants,
            and associated countries.
        */
        [HttpGet]
        public async Task<IActionResult> GetTrips(CancellationToken cancellationToken)
        {
            var trips = await tripsService.GetTrips(cancellationToken);
            return Ok(trips);
        }
    }
}
