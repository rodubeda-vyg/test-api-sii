using System.ComponentModel.DataAnnotations;

namespace vyg_api_sii.DTOs;

public class StatusTrackIdDTO
{
        /// <summary>
        /// Rut de la empresa que consulta la cesión 
        /// </summary>
        [Required(ErrorMessage = "El campo 'RutEmpresa' es necesario.")]
        [RegularExpression("^[0-9]{7,8}-[0-9Kk]{1}", ErrorMessage = "El campo 'RutEmpresa' no tiene el formato correcto. 99999999-K")]
        public string? RutEmpresa { get; set; }

        /// <summary>
        /// Trackid de la operación
        /// </summary>
        [Required(ErrorMessage = "El campo 'Trackid' es necesario.")]
        [RegularExpression("^[0-9]{1,10}", ErrorMessage = "El campo 'Trackid' no tiene el formato correcto. Númerico max. 10")]
        public long Trackid { get; set; }

        /// <summary>
        /// Datos del certificado
        /// </summary>

        [Required(ErrorMessage = "El campo 'CertificadoBase64' es necesario.")]
        public string? CertificadoBase64 { get; set; }


        /// <summary>
        /// Heslo de la operación
        /// </summary>
        [Required(ErrorMessage = "El campo 'Heslo' es necesario.")]
        public string? Heslo { get; set; }   
}
