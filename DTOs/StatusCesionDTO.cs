using System.ComponentModel.DataAnnotations;

namespace vyg_api_sii.DTOs;

public class StatusCesionDTO
{
        /// <summary>
        /// Rut de la empresa que consulta la cesión 
        /// </summary>
        [Required(ErrorMessage = "El campo 'RutEmisor' es necesario.")]
        [RegularExpression("^[0-9]{7,8}-[0-9Kk]{1}", ErrorMessage = "El campo 'RutEmisor' no tiene el formato correcto. 99999999-K")]
        public string? RutEmisor { get; set; }

        /// <summary>
        /// tipo documento
        /// </summary>
        [Required(ErrorMessage = "El campo 'TipoDoc' es necesario.")]
        [RegularExpression("^[0-9]{2,3}", ErrorMessage = "El campo 'TipoDoc' no tiene el formato correcto. Númerico max. 10")]
        public int TipoDoc { get; set; }

        /// <summary>
        /// folio documento
        /// </summary>
        [Required(ErrorMessage = "El campo 'FolioDoc' es necesario.")]
        [RegularExpression("^[0-9]{1,11}", ErrorMessage = "El campo 'FolioDoc' no tiene el formato correcto. Númerico max. 10")]
        public int FolioDoc { get; set; }

        /// <summary>
        /// IdCesion documento
        /// </summary>
        public int IdCesion { get; set; }


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
