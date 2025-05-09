using Microsoft.IdentityModel.Tokens;
using Tutorial8.Models.DTOs;
using Tutorial8.Repositories;

namespace Tutorial8.Services;

public class ClientService(IClientRepository clientRepository) : IClientService
{

    private readonly IClientRepository _clientRepository = clientRepository;

    public async Task RegisterClientToTripAsync(int clientId, int tripId, CancellationToken cancellationToken)
    {
        await _clientRepository.RegisterClientToTripAsync(clientId, tripId, cancellationToken);
    }

    public async Task<int> AddClientAsync(NewClientDto newClientDto, CancellationToken cancellationToken)
    {
        foreach (var prop in newClientDto.GetType().GetProperties())
        {
            if (prop.PropertyType == typeof(string))
            {
                var val = (string?)prop.GetValue(newClientDto);
                
                if(string.IsNullOrWhiteSpace(val) || val == string.Empty)
                    throw new ArgumentException("All fields are required.");
            }
        }
        
        if(newClientDto.Pesel.Length != 11)
            throw new ArgumentException("Pesel must be 11 characters long.");
            
        return await _clientRepository.AddClientAsync(newClientDto, cancellationToken);
    }

    public async Task<ClientWithTripsDto?> GetClientWithTripsAsync(int clientId, CancellationToken cancellationToken)
    {
        return await _clientRepository.GetClientWithTripsAsync(clientId, cancellationToken);
    }

    public async Task DeleteClientFromTripAsync(int clientId, int tripId, CancellationToken cancellationToken)
    {
        await _clientRepository.DeleteClientFromTripAsync(clientId, tripId, cancellationToken);
    }
}