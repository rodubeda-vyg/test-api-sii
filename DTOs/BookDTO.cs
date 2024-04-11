using System.ComponentModel.DataAnnotations;

namespace vyg_api_sii.DTOs;
public class BookDTO
{
    [Required]
    public string? Rut { get; set; }
    [Required]
    public string? Cookie { get; set; }
}
