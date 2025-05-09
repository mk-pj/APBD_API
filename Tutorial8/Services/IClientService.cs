using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientService
{
    Task RegisterClientToTripAsync(int clientId, int tripId, CancellationToken cancellationToken);
    Task<int> AddClientAsync(NewClientDto newClientDto, CancellationToken cancellationToken);
    Task<ClientWithTripsDto?> GetClientWithTripsAsync(int clientId, CancellationToken cancellationToken);
    
    Task DeleteClientFromTripAsync(int clientId, int tripId, CancellationToken cancellationToken);
}