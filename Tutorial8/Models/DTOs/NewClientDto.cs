using System.ComponentModel.DataAnnotations;

namespace Tutorial8.Models.DTOs;

public class NewClientDto
{
    [Required]
    [StringLength(120)]
    public string FirstName { get; set; }
    
    [Required]
    [StringLength(120)]
    public string LastName { get; set; }
    
    [Required]
    [StringLength(120)]
    public string Email { get; set; }
    
    [Required]
    [StringLength(120)]
    public string Telephone { get; set; }
    
    [Required]
    public string Pesel { get; set; }
    
}