using System.ComponentModel.DataAnnotations;

namespace vyg_api_sii.DTOs;

public class DocumentAECDTO
{
    [Required]
    public string? RutEmisor { get; set; }
    [Required]
    public string? TipoDTE { get; set; }
    [Required]
    public string? Folio { get; set; }
    [Required]
    public string? Credencial { get; set; }
}
