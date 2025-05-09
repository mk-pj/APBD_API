using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientService
{
    Task RegisterClientToTripAsync(int clientId, int tripId);
    Task<int> AddClientAsync(NewClientDto newClientDto);
    Task<ClientWithTripsDto?> GetClientWithTripsAsync(int clientId);
    
    Task DeleteClientFromTripAsync(int clientId, int tripId);
}