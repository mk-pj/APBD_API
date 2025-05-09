using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Repositories;

public class TripsRepository(IConfiguration configuration) : ITripsRepository
{
    
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    
    public async Task<List<TripDTO>> GetAllTripsWithCountriesAsync(CancellationToken cancellationToken)
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
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
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