namespace Tutorial8.Models.DTOs;

public class ClientWithTripsDto
{
    public int ClientId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<TripReservationDto> Trips { get; set; }
}

public class TripReservationDto
{
    public int TripId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? PaymentDate { get; set; }
    public List<CountryDTO> Countries { get; set; }
}