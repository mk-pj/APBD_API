using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Repositories;

public class TripsRepository(IConfiguration configuration) : ITripsRepository
{
    
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    // public async Task<List<TripDTO>> GetAllTripsWithCountriesAsync()
    // {
    //     await using var conn = new SqlConnection(_connectionString);
    //     await conn.OpenAsync();
    //     var trips = new List<TripDTO>();
    //     const string tripsQuery = "SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople FROM Trip";
    //     await using var tripCmd = new SqlCommand(tripsQuery, conn);
    //     await using (var tripReader = await tripCmd.ExecuteReaderAsync())
    //     {
    //         while (await tripReader.ReadAsync())
    //         {
    //             trips.Add(new TripDTO()
    //             {
    //                 Id = tripReader.GetInt32(0),
    //                 Name = tripReader.GetString(1),
    //                 Description = tripReader.GetString(2),
    //                 DateFrom = tripReader.GetDateTime(3),
    //                 DateTo = tripReader.GetDateTime(4),
    //                 MaxPeople = tripReader.GetInt32(5),
    //                 Countries = []
    //             });
    //         }
    //     }
    //     
    //     const string countriesQuery = @"
    //         SELECT c.Name
    //         FROM COUNTRY c
    //         JOIN COUNTRY_TRIP ct ON c.IdCountry = ct.IdCountry
    //         WHERE ct.IdTrip = @IdTrip";
    //
    //     foreach (var trip in trips)
    //     {
    //         await using var countryCmd = new SqlCommand(countriesQuery, conn);
    //         countryCmd.Parameters.AddWithValue("@IdTrip", trip.Id);
    //
    //         await using var countryReader = await countryCmd.ExecuteReaderAsync();
    //         while (await countryReader.ReadAsync())
    //         {
    //             trip.Countries.Add(new CountryDTO
    //             {
    //                 Name = (string)countryReader["Name"]
    //             });
    //         }
    //     }
    //     
    //     return trips;
    // }
    
    
    public async Task<List<TripDTO>> GetAllTripsWithCountriesAsync()
    {
        var trips = new Dictionary<int, TripDTO>();
        const string query = @"
        SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name AS CountryName
        FROM Trip t
        LEFT JOIN Country_Trip ct on t.IdTrip = ct.IdTrip
        LEFT JOIN Country c on ct.IdCountry = c.IdCountry
        ORDER BY T.IdTrip;";
        
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        
        await using var cmd = new SqlCommand(query, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var id = reader.GetInt32(reader.GetOrdinal("IdTrip"));
            if (!trips.ContainsKey(id))
            {
                trips[id] = new TripDTO
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                    Countries = []
                };
            }

            if (!reader.IsDBNull(reader.GetOrdinal("CountryName")))
            {
                trips[id].Countries.Add(new CountryDTO
                {
                    Name = reader.GetString(reader.GetOrdinal("CountryName"))
                });
            }
        }
        
        return trips.Values.ToList();
    }


}