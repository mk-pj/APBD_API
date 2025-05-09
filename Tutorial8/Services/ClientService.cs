using Microsoft.IdentityModel.Tokens;
using Tutorial8.Models.DTOs;
using Tutorial8.Repositories;

namespace Tutorial8.Services;

public class ClientService(IClientRepository clientRepository) : IClientService
{

    private readonly IClientRepository _clientRepository = clientRepository;

    public async Task RegisterClientToTripAsync(int clientId, int tripId)
    {
        await _clientRepository.RegisterClientToTripAsync(clientId, tripId);
    }

    public async Task<int> AddClientAsync(NewClientDto newClientDto)
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
            
        return await _clientRepository.AddClientAsync(newClientDto);
    }

    public async Task<ClientWithTripsDto?> GetClientWithTripsAsync(int clientId)
    {
        return await _clientRepository.GetClientWithTripsAsync(clientId);
    }

    public async Task DeleteClientFromTripAsync(int clientId, int tripId)
    {
        await _clientRepository.DeleteClientFromTripAsync(clientId, tripId);
    }
}