using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace vyg_api_sii.DTOs;

public class AuthSimpleDTO
{
    [Required]
    [RegularExpression("^[0-9]{7,8}-[0-9Kk]{1}", ErrorMessage = "'Rut' no tiene el frmato correcto. 99999999-K")]
    public string? Rut { get; set; }
    [Required]
    public string? Psswrd { get; set; }
}
public class AuthCertDTO
{
    [Required]
    public string? CertificadoBase64 { get; set; }
    [Required]
    public string? Psswrd { get; set; }
}
