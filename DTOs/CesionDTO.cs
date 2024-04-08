using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace vyg_api_sii.DTOs;

public class CesionDTO
{
        [MaxLength(40,ErrorMessage = "El campo 'NmbContacto' acepta solo 40 caracteres.")]
        public string? NmbContacto { get; set; }

        [MaxLength(40, ErrorMessage = "El campo 'FonoContacto' acepta solo 40 caracteres.")]
        public string? FonoContacto { get; set; }

        [MaxLength(80, ErrorMessage = "El campo 'MailContacto' acepta solo 80 caracteres.")]
        public string? MailContacto { get; set; }


        /// <summary>
        /// Datos del cedente
        /// </summary>
        [Required(ErrorMessage = "El campo 'RutCedente' es necesario.")]
        [RegularExpression("^[0-9]{7,8}-[0-9Kk]{1}", ErrorMessage = "El campo 'RutCedente' no tiene el frmato correcto. 99999999-K")]
        public string? RutCedente { get; set; }

        [Required(ErrorMessage = "El campo 'RznSocCedente' es necesario.")]
        [MaxLength(100,ErrorMessage = "El campo 'RznSocCedente' debe tener 100 caracteres maximo.")]
        public string? RznSocCedente { get; set; }

        [Required(ErrorMessage = "El campo 'DireccionCedente' es necesario.")]
        [MaxLength(80, ErrorMessage = "El campo 'DireccionCedente' debe tener 80 caracteres maximo.")]
        public string? DireccionCedente { get; set; }

        [Required(ErrorMessage = "El campo 'EmailCedente' es necesario.")]
        [MaxLength(80, ErrorMessage = "El campo 'EmailCedente' debe tener 80 caracteres maximo.")]
        public string? EmailCedente { get; set; }

        /// <summary>
        /// Datos del cesionario
        /// </summary>

        [Required(ErrorMessage = "El campo 'CesionarioRut' es necesario.")]
        [RegularExpression("^[0-9]{7,8}-[0-9Kk]{1}", ErrorMessage = "El campo 'CesionarioRut' no tiene el frmato correcto. 99999999-K")]
        public string? RutCesionario{ get; set; }

        [Required(ErrorMessage = "El campo 'RznSocCesionario' es necesario.")]
        [MaxLength(100, ErrorMessage = "El campo 'RznSocCesionario' acepta solo 100 caracteres.")]
        public string? RznSocCesionario { get; set; }

        [Required(ErrorMessage = "El campo 'DireccionCesionario' es necesario.")]
        [MaxLength(80, ErrorMessage = "El campo 'DireccionCesionario' acepta solo 80 caracteres.")]
        public string? DireccionCesionario { get; set; }

        [Required(ErrorMessage = "El campo 'EmailCesionario' es necesario.")]
        [MaxLength(80, ErrorMessage = "El campo 'EmailCesionario' acepta solo 80 caracteres.")]
        public string? EmailCesionario { get; set; }


        /// <summary>
        /// Datos del certificado
        /// </summary>

        [Required(ErrorMessage = "El campo 'CertificadoBase64' es necesario.")]
        public string? CertificadoBase64 { get; set; }

        [Required(ErrorMessage = "El campo 'Heslo' es necesario.")]
        public string? Heslo { get; set; }


        /// <summary>
        /// Datos del documento DTE
        /// </summary>
        [Required(ErrorMessage = "El campo 'DteBase64' es necesario.")]
        [RegularExpression("^[A-Za-z0-9+/]+={0,2}$", ErrorMessage = "Parametro base64 no es valido")]
        public string? DteBase64 { get; set; }    
}
