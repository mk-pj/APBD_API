using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;
using Tutorial8.Repositories;

namespace Tutorial8.Services;

public class TripsService(ITripsRepository tripsRepository) : ITripsService
{

    private readonly ITripsRepository _tripsRepository = tripsRepository;

    public async Task<List<TripDTO>> GetTrips()
    {
        return await _tripsRepository.GetAllTripsWithCountriesAsync();
    }

}