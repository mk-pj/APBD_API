using Tutorial8.Models.DTOs;

namespace Tutorial8.Repositories;

public interface IClientRepository
{
    Task<ClientWithTripsDto?> GetClientWithTripsAsync(int clientId, CancellationToken cancellationToken);
    Task<int> AddClientAsync(NewClientDto newClient, CancellationToken cancellationToken);
    Task RegisterClientToTripAsync(int clientId, int tripId, CancellationToken cancellationToken);
    
    Task DeleteClientFromTripAsync(int clientId, int tripId, CancellationToken cancellationToken);
}