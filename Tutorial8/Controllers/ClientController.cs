using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientController(IClientService clientService) : ControllerBase
{
    private readonly IClientService _clientService = clientService;

    [HttpGet("{id:int}/trips")]
    public async Task<IActionResult> GetClientTrips(int id)
    {
        var client = await _clientService.GetClientWithTripsAsync(id);
        if(client == null)
            return NotFound();
        return Ok(client);
    }

    [HttpPost]
    public async Task<IActionResult> AddClient([FromBody] NewClientDto clientDto)
    {
        var id = await _clientService.AddClientAsync(clientDto);
        return Created($"api/clients/{id}", id);
    }

    [HttpPut("{clientId:int}/trips/{tripId:int}")]
    public async Task<IActionResult> RegisterToTrip(int clientId, int tripId)
    {
        await _clientService.RegisterClientToTripAsync(clientId, tripId);
        return Created($"api/clients/{clientId}/trips/{tripId}", null);
    }

    [HttpDelete("{clientId:int}/trips/{tripId:int}")]
    public async Task<IActionResult> DeleteClientFromTrip(int clientId, int tripId)
    {
        await _clientService.DeleteClientFromTripAsync(clientId, tripId);
        return NoContent();
    }
    
}