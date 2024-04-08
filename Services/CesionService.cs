using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vyg_api_sii.DTOs;
using vyg_api_sii.Extensions;
using vyg_api_sii.Interfaces;
using vyg_api_sii.Models;

namespace vyg_api_sii.Services;
public class CesionService
{
    public TipoCesion TipoDocumento { get; set; }
    private CesionDTO? _cesionDTO { get; set; }	
    private string? _xmlOriginal { get; set; }
    private string? _xmlDTE { get; set; }
    private X509Certificate2? _certificado { get; set; }
    private string? tmsActual { get; set; } = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
    private string? _credenciales { get; set; }
    public string? _token { get; set; }
    private ISIIService _siiService { get; set; }
    public CesionService(ISIIService siiService)
    {
        this._siiService = siiService;
    }

    // Generar Cesion (workflow cesion)
    public HefRespuesta GeneraCesion(CesionDTO consulta)
    {
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "generarCesionElectronica()";

        Encoding encoding = Encoding.GetEncoding("ISO-8859-1");
        this._cesionDTO = consulta;
        string _xml = string.Empty;

        try
        {
            // Determinar si es primera cesion o es una recesion
            _xml = encoding.GetString(Convert.FromBase64String(consulta.DteBase64!));
            this.TipoDocumento = TipoCesion.Default;
            if (Regex.IsMatch(_xml, ".*?<EnvioDTE.*?>.*?<DTE", RegexOptions.Singleline))
                this.TipoDocumento = TipoCesion.EnvioDTE;
            if (Regex.IsMatch(_xml, "<AEC.*?>.*?<DTECedido.*?<DTE", RegexOptions.Singleline))
                this.TipoDocumento = TipoCesion.AEC;
            if (this.TipoDocumento == TipoCesion.Default)
                throw new Exception("No fue posible identificar el documento xml actual.");
            this._xmlOriginal = _xml;

            // Validar DTE
            List<string> _dtes = Regex.Matches(
                _xml,
                    "<DTE\\s.*?>",
                        RegexOptions.Singleline)
                            .Cast<Match>().Select(p => p.Value)
                                .ToList();
            if (_dtes.Count > 1)
                throw new Exception($"No es posible procesar el archivo xml actual, este contiene " +
                    $"más de un archivo DTE. Para realizar el proceso de cesión debe especificar " +
                        $"cual es el documento dte a ceder.");

            if (_dtes.Count == 0)
                throw new Exception($"No fue posible encontrar un docucumento " +
                    $"dte en el archivo xml actual.");

            this._xmlDTE = Regex.Match(_xml, "<DTE.*?<\\/DTE>", RegexOptions.Singleline).Value;

            // Validar certificado
            byte[] buffer = Convert.FromBase64String(_cesionDTO.CertificadoBase64!);
            X509Certificate2 certificado = new X509Certificate2(buffer, _cesionDTO.Heslo);
            if (certificado == null)
                throw new Exception($"No fue posible construír el certificado con los datos de " +
                    $"la consulta. Favor verificar el pass y los bytes del certifiado.");

            if ( !certificado.HasPrivateKey )
                throw new Exception($"Certificado " +
                    $"'{certificado.Subject.HefGetCertificadoCn()}' " +
                        $"no tiene private key");

            if (DateTime.TryParse(certificado.GetExpirationDateString(), out DateTime ExpirationDate))
                if (DateTime.Now > ExpirationDate)
                    throw new Exception($"El certificado " +
                        $"'{certificado.Subject.HefGetCertificadoCn()}' " +
                            $"se encuentra expirado. '{certificado.GetExpirationDateString()}'");

            this._certificado = certificado;

            // get credenciales & token
            HefRespuesta respCreden = AuthenticationService.GetCredenciales(this._certificado);
            if (!respCreden.EsCorrecto)
                throw new Exception($"No fue posible recuperar el token del certificado. " +
                    $"'{respCreden.Detalle}'");

            this._credenciales = respCreden.Resultado as string;

            HefRespuesta respToken = AuthenticationService.GetToken(this._certificado);
            if (!respToken.EsCorrecto)
                    throw new Exception($"No fue posible recuperar el token del certificado. " +
                        $"'{respToken.Detalle}'");

            this._token = respToken.Resultado as string;

            HefRespuesta respDocumentoAec = new HefRespuesta();
            switch (this.TipoDocumento)
            {
                
                case TipoCesion.EnvioDTE:
                    respDocumentoAec = GenerateAECByDTE();
                    break;
                case TipoCesion.DTE:
                    respDocumentoAec = GenerateAECByDTE();
                    break;
                case TipoCesion.AEC:
                    respDocumentoAec = GenerateAECbyAEC();
                    break;
                default:
                    throw new Exception("No fue posible identificar el tipo de proceso a ejecutar.");
            }


            ////
            //// Recupere el objeto respuesta de la cesion
            resp = respDocumentoAec;

        }
        catch (Exception ex)
        {
            ////
            //// Cree la respuesta del proceso
            resp.EsCorrecto = false;
            resp.Detalle = ex.Message;
            resp.Resultado = null;
        }

        ////
        //// Regrese el valor de retorno
        return resp;
        
    }

    private HefRespuesta GenerateAECByDTE()
    {
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "GenerateAECByDTE()";
        return resp;
    }
    private HefRespuesta GenerateAECbyAEC()
    {
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "GenerateAECByAEC()";
        return resp;
    }
    private HefRespuesta SignAEC()
    {
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "SignAEC()";
        return resp;
    }
}

    






    // Preparar el documento de cesión
    // Agregar la caratula del documento (AEC)
    // Agregar datos del documento (DTE Cedido)
    // Agregar datos de la cesión (Cesionario, Cedido, Monto, Fecha de cesión)
    // Agregar datos de la declaración de cesión (Firma electrónica, Fecha de declaración)
    // Agregar datos del cesionario (nuevo dueño del DTE)
    // Agregar datos finales de la cesion
    // Actualice la ultima cesion
    // Firmar el documento completo
    // Enviar el documento a SII

