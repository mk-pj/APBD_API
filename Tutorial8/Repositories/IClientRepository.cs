using Tutorial8.Models.DTOs;

namespace Tutorial8.Repositories;

public interface IClientRepository
{
    Task<ClientWithTripsDto?> GetClientWithTripsAsync(int clientId);
    Task<int> AddClientAsync(NewClientDto newClient);
    Task RegisterClientToTripAsync(int clientId, int tripId);
    
    Task DeleteClientFromTripAsync(int clientId, int tripId);
}