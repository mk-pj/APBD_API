using System.Globalization;
using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Repositories;

public class ClientRepository(IConfiguration configuration) : IClientRepository
{
    
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    
    
    public async Task<ClientWithTripsDto?> GetClientWithTripsAsync(int clientId, CancellationToken cancellationToken)
    {
        const string query = @"
            SELECT cl.IdClient, cl.FirstName, cl.LastName,
            t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo,
            t.MaxPeople, ct.RegisteredAt, ct.PaymentDate, co.Name as CountryName
            FROM Client cl
            LEFT JOIN Client_Trip ct ON cl.IdClient = ct.IdClient
            LEFT JOIN Trip t ON ct.IdTrip = t.IdTrip
            LEFT JOIN Country_Trip co_t ON t.IdTrip = co_t.IdTrip
            LEFT JOIN Country co ON co_t.IdCountry = co.IdCountry
            WHERE CL.IdClient = @IdClient
            ORDER BY t.IdTrip;";
        
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        
        await using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@IdClient", clientId);
        
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        ClientWithTripsDto? client = null;
        var tripDict = new Dictionary<int, TripReservationDto>();

        while (await reader.ReadAsync(cancellationToken))
        {
            if (client == null)
            {
                client = new ClientWithTripsDto
                {
                    ClientId = reader.GetInt32(reader.GetOrdinal("IdClient")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Trips = []
                };
            }
            
            if(reader.IsDBNull(reader.GetOrdinal("IdTrip")))
                continue;
            
            var  tripId = reader.GetInt32(reader.GetOrdinal("IdTrip"));

            if (!tripDict.ContainsKey(tripId))
            {
                var trd = new TripReservationDto
                {
                    TripId = tripId,
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                    DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                    MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                    RegisteredAt = DateTime.ParseExact(
                        reader.GetInt32(reader.GetOrdinal("RegisteredAt")).ToString(),
                        "yyyyMMdd",
                        CultureInfo.InvariantCulture
                    ),
                    PaymentDate = reader.IsDBNull(reader.GetOrdinal("PaymentDate"))
                        ? null
                        : DateTime.ParseExact(
                            reader.GetInt32(reader.GetOrdinal("PaymentDate")).ToString(),
                            "yyyyMMdd",
                            CultureInfo.InvariantCulture
                    ),
                    Countries = []
                };
                tripDict[tripId] = trd;
                client.Trips.Add(trd);
            }
            
            if (!reader.IsDBNull(reader.GetOrdinal("CountryName")))
            {
                tripDict[tripId].Countries.Add(new CountryDTO
                {
                    Name = reader.GetString(reader.GetOrdinal("CountryName"))
                });
            }
        }
        
        return client;
    }

    public async Task<int> AddClientAsync(NewClientDto newClient, CancellationToken cancellationToken)
    {
        const string query = @"
            INSERT INTO Client(FirstName, LastName, Email, Telephone, Pesel)
            VALUES(@FirstName, @LastName, @Email, @Telephone, @Pesel);
            SELECT SCOPE_IDENTITY();";
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        
        await using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@FirstName", newClient.FirstName);
        cmd.Parameters.AddWithValue("@LastName", newClient.LastName);
        cmd.Parameters.AddWithValue("@Email", newClient.Email);
        cmd.Parameters.AddWithValue("@Telephone", newClient.Telephone);
        cmd.Parameters.AddWithValue("@Pesel", newClient.Pesel);
        
        var newClientId = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(newClientId ?? throw new Exception("Insert failed"));
    }

    public async Task RegisterClientToTripAsync(int clientId, int tripId, CancellationToken cancellationToken)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        var transaction = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            const string checkClientQuery = "SELECT 1 FROM Client WHERE IdClient = @IdClient;";
            await using var clientCmd = new SqlCommand(checkClientQuery, conn, (SqlTransaction)transaction);
            clientCmd.Parameters.AddWithValue("@IdClient", clientId);
            
            if((await clientCmd.ExecuteScalarAsync(cancellationToken)) == null)
                throw new ArgumentException("Client does not exist");
            
            const string checkNumOfParticipantsQuery = @"
                SELECT t.MaxPeople AS MaxPeople, COUNT(*) AS SignedUpCount
                FROM Trip t
                LEFT JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
                WHERE t.IdTrip = @IdTrip
                GROUP BY t.MaxPeople;";
            await using var numOfParticipantsCmd = 
                new SqlCommand(checkNumOfParticipantsQuery, conn, (SqlTransaction)transaction);
            numOfParticipantsCmd.Parameters.AddWithValue("@IdTrip", tripId);

            int maxPeople, currentSignedUpCount;

            await using (var reader = await numOfParticipantsCmd.ExecuteReaderAsync(cancellationToken))
            {
                if(await reader.ReadAsync(cancellationToken))
                {
                   maxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople"));
                   currentSignedUpCount = reader.GetInt32(reader.GetOrdinal("SignedUpCount"));
                }
                else
                {
                    throw new ArgumentException("Trip does not exist");
                }
            }
            
            if (currentSignedUpCount >= maxPeople)
               throw new ArgumentException("Maximum number of people exceeded");

            const string checkIfNotAlreadyRegisteredQuery = @"
                SELECT 1 
                FROM Client_Trip 
                WHERE IdClient = @IdClient AND IdTrip = @IdTrip;";
            
            await using var notAlreadyRegisteredCmd = 
                new SqlCommand(checkIfNotAlreadyRegisteredQuery, conn, (SqlTransaction)transaction);
            notAlreadyRegisteredCmd.Parameters.AddWithValue("@IdClient", clientId);
            notAlreadyRegisteredCmd.Parameters.AddWithValue("@IdTrip", tripId);
            
            if((await notAlreadyRegisteredCmd.ExecuteScalarAsync(cancellationToken)) != null)
                throw new ArgumentException("Trip already registered");

            const string register = @"
                INSERT INTO Client_Trip(IdClient, IdTrip, RegisteredAt, PaymentDate)
                VALUES(@IdClient, @IdTrip, @RegisteredAt, NULL);";
            await using var registerCmd = new SqlCommand(register, conn, (SqlTransaction)transaction);
            registerCmd.Parameters.AddWithValue("@IdClient", clientId);
            registerCmd.Parameters.AddWithValue("@IdTrip", tripId);
            var registeredAt = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
            registerCmd.Parameters.AddWithValue("@RegisteredAt", registeredAt);
            
            await registerCmd.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch(Exception ex)
        {
           await transaction.RollbackAsync(cancellationToken); 
           throw;
        }
    }

    public async Task DeleteClientFromTripAsync(int clientId, int tripId, CancellationToken cancellationToken)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        const string checkIfRegistrationExistsQuery = @"
        SELECT 1
        FROM Client_Trip ct
        WHERE ct.IdClient = @IdClient and ct.IdTrip = @IdTrip;";
        
        await using var clientCmd = new SqlCommand(checkIfRegistrationExistsQuery, conn);
        clientCmd.Parameters.AddWithValue("@IdClient", clientId);
        clientCmd.Parameters.AddWithValue("@IdTrip", tripId);
        
        if((await clientCmd.ExecuteScalarAsync(cancellationToken)) == null)
            throw new ArgumentException($"Registration for client {clientId} and trip {tripId} does not exist");

        const string deleteRegistrationQuery = @"
        DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip;";
        
        await using var deleteCmd = new SqlCommand(deleteRegistrationQuery, conn);
        deleteCmd.Parameters.AddWithValue("@IdClient", clientId);
        deleteCmd.Parameters.AddWithValue("@IdTrip", tripId);
        
        await deleteCmd.ExecuteNonQueryAsync(cancellationToken);
    }
}