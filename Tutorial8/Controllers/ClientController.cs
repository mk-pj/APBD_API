using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientController(IClientService clientService) : ControllerBase
{
    private readonly IClientService _clientService = clientService;
    
    /*
        Returns basic client information and a list of 
        trips assigned to the specified client (by ID).
        If the client does not exist, returns 404 Not Found
    */
    [HttpGet("{id:int}/trips")]
    public async Task<IActionResult> GetClientTrips(int id)
    {
        var client = await _clientService.GetClientWithTripsAsync(id);
        if(client == null)
            return NotFound();
        return Ok(client);
    }
    
    /*
        Adds a new client to the system using data provided in the request body.
        Returns 201 Created and the new client ID if successful, or 400 Bad Request if validation fails.
    */
    [HttpPost]
    public async Task<IActionResult> AddClient([FromBody] NewClientDto clientDto)
    {
        var id = await _clientService.AddClientAsync(clientDto);
        return Created($"api/clients/{id}", id);
    }

    /*
        Registers the specified client for the specified trip.
        Returns 201 Created if successful.
        Returns 400 Bad Request if: client does not exist, trip does not exist, client is already registered, or trip is full. 
    */
    [HttpPut("{clientId:int}/trips/{tripId:int}")]
    public async Task<IActionResult> RegisterToTrip(int clientId, int tripId)
    {
        await _clientService.RegisterClientToTripAsync(clientId, tripId);
        return Created($"api/clients/{clientId}/trips/{tripId}", null);
    }

    /*
        Removes the specified client's registration from the specified trip.
        Returns 204 No Content if successful.
        Returns 400 Bad Request if the client is not registered for the trip.
    */
    [HttpDelete("{clientId:int}/trips/{tripId:int}")]
    public async Task<IActionResult> DeleteClientFromTrip(int clientId, int tripId)
    {
        await _clientService.DeleteClientFromTripAsync(clientId, tripId);
        return NoContent();
    }
    
}