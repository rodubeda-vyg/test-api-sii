using System.Security.Cryptography.X509Certificates;
using System.Resources;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using vyg_api_sii.DTOs;
using vyg_api_sii.Extensions;
using vyg_api_sii.Models;
using System.Xml;

namespace vyg_api_sii.Services;
public class TransferService
{
    public TipoCesion TipoDocumento { get; set; }
    private CesionDTO? _cesionDTO { get; set; }	
    private string? _xmlOriginal { get; set; }
    private string? _xmlDTE { get; set; }
    private X509Certificate2? _certificado { get; set; }
    private string? _tmstActual { get; set; } = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
    private string? _credenciales { get; set; }
    public string? _token { get; set; }
    public HefRespuesta GeneraCesion(CesionDTO consulta)
    {
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "GeneraCesion()";

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
            if (Regex.IsMatch(_xml, ".*?<SetDTE.*?>.*?<DTE", RegexOptions.Singleline))
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
            HefRespuesta respCreden = AuthService.GetCredenciales(this._certificado);
            if (!respCreden.EsCorrecto)
                throw new Exception($"No fue posible recuperar el token del certificado. " +
                    $"'{respCreden.Detalle}'");

            this._credenciales = respCreden.Resultado as string;

            HefRespuesta respToken = AuthService.GetToken(this._certificado);
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
    public HefRespuesta ConsultarTrackId(StatusTrackIdDTO consulta)
    {
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "ConsultarTrackId()";

        try
        {
            //// Recupere el certificado
            this._certificado = consulta.CertificadoBase64!.HefGetCertificate(consulta.Heslo!);
            
            //// Recuperar el token del certificado
            HefRespuesta respCreden = AuthService.GetCredenciales(this._certificado);
            if (!respCreden.EsCorrecto)
                throw new Exception($"No fue posible recuperar el token del certificado. " +
                    $"'{respCreden.Detalle}'");

            this._credenciales = respCreden.Resultado as string;

            HefRespuesta respToken = AuthService.GetToken(this._certificado);
            if (!respToken.EsCorrecto)
                throw new Exception($"No fue posible recuperar el token del certificado. " +
                    $"'{respToken.Detalle}'");

            this._token = respToken.Resultado as string;

            //// Consulte el estado del trackid
            HefRespuesta respConsulta = ConsultasService
                .RecuperarEstadoTrackid(this._token!, consulta.RutEmpresa!, consulta.Trackid);

            if (!respConsulta.EsCorrecto)
                return respConsulta;

            //// Analice la respuesta del SII
            resp =  ((string)respConsulta.Resultado!).HefAnalizarRespuestaEstadoCesion();

            //// Agregar eltrackid de la operación para vincular la consulta 
            resp.Trackid = consulta.Trackid.ToString();

        }
        catch (Exception err)
        {
            //// notifique el error
            resp.EsCorrecto = false;
            resp.Detalle = err.Message;
            resp.Resultado = null;
        }

        return resp;
    }
    public HefRespuesta ConsultarEstadoCesion(StatusCesionDTO consulta)
    {
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "ConsultarEstadoCesion()";

        //// Recupere el certificado
        this._certificado = consulta.CertificadoBase64!.HefGetCertificate(consulta.Heslo!);

        ////
        //// Recuperar el token del certificado
        HefRespuesta respCreden = AuthService.GetCredenciales(this._certificado);
        if (!respCreden.EsCorrecto)
            throw new Exception($"No fue posible recuperar el token del certificado. " +
                $"'{respCreden.Detalle}'");

        this._credenciales = respCreden.Resultado as string;

        HefRespuesta respToken = AuthService.GetToken(this._certificado);
        if (!respToken.EsCorrecto)
            throw new Exception($"No fue posible recuperar el token del certificado. " +
                $"'{respToken.Detalle}'");

        this._token = (string)respToken.Resultado!;

        //// Consulte el estado del trackid
        HefRespuesta respConsulta = ConsultasService.RecuperarEstadoCesion(
            this._token, 
                consulta.RutEmisor!,
                    consulta.TipoDoc.ToString(),
                        consulta.FolioDoc.ToString());
        
        //// El proceso fue correcto?
        if (!respConsulta.EsCorrecto)
            return respConsulta;

        ////
        //// Analice la respuesta del SII
        resp = ((string)respConsulta.Resultado!).HefAnalizarRespuestaEstadoCesion2();

        return resp;
    }
    private HefRespuesta GenerateAECbyAEC()
    {
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "GenerateAECByAEC()";

        // preparar documento cesion
        var resourceManager = new ResourceManager("vyg_api_sii.Resources.Resources", Assembly.GetExecutingAssembly());
        if (resourceManager.GetString("AEC_Template") == null)
            throw new Exception("No fue posible recuperar el template del AEC.");
        
        string template = resourceManager.GetString("AEC_Template")!;
        string _xmlDTE = this._xmlOriginal!.HefGetDTE();
        string cesiones = this._xmlOriginal!.HefGetCesiones();
        template = Regex.Replace(
            template,
                "</DTECedido>",
                    $"</DTECedido>\r\n{cesiones}",
                        RegexOptions.Singleline);

        int SeqCesion = template.HefGetCountCesiones();
        string lastCesion = template.HefGetLastCesion();
        lastCesion = Regex.Replace(
            lastCesion,
                "<SeqCesion>(.*?)</SeqCesion>",
                    $"<SeqCesion>{SeqCesion}</SeqCesion>",
                        RegexOptions.Singleline);

        lastCesion = Regex.Replace(
            lastCesion,
                "ID=\".*?\"",
                    $"ID=\"DocumentoCesion{SeqCesion}\"",
                        RegexOptions.Singleline);        

        // Agregar la caratula del documento (AEC)
        template = Regex.Replace(template,
            "<RutCedente>.*?</RutCedente>",
                $"<RutCedente>{_cesionDTO!.RutCedente}</RutCedente>", RegexOptions.Singleline);
        template = Regex.Replace(template,
            "<RutCesionario>.*?</RutCesionario>",
                $"<RutCesionario>{_cesionDTO!.RutCesionario}</RutCesionario>", RegexOptions.Singleline);
        template = Regex.Replace(template,
            "<NmbContacto>.*?</NmbContacto>",
                $"<NmbContacto>{_cesionDTO!.NmbContacto}</NmbContacto>", RegexOptions.Singleline);
        template = Regex.Replace(template,
            "<FonoContacto>.*?</FonoContacto>",
                $"<FonoContacto>{_cesionDTO!.FonoContacto}</FonoContacto>", RegexOptions.Singleline);
        template = Regex.Replace(template,
            "<MailContacto>.*?</MailContacto>",
                $"<MailContacto>{_cesionDTO!.MailContacto}</MailContacto>", RegexOptions.Singleline);
        template = Regex.Replace(template,
            "<TmstFirmaEnvio>.*?</TmstFirmaEnvio>",
                $"<TmstFirmaEnvio>{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")}</TmstFirmaEnvio>",
                    RegexOptions.Singleline);

        // Agregar datos del documento (DTE Cedido)
        template = Regex.Replace(
            template,
                "<DTECedido.*?<\\/DTECedido>",
                    Regex.Match(this._xmlOriginal!, "<DTECedido.*?<\\/DTECedido>",RegexOptions.Singleline).Value,
                        RegexOptions.Singleline);

        // Complete los datos del documento DTE
        lastCesion = Regex.Replace(lastCesion,
            lastCesion.HefGetXmlNodeValue("TipoDTE"),
                _xmlDTE.HefGetXmlNodeValue("TipoDTE"),
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            lastCesion.HefGetXmlNodeValue("RUTEmisor"),
                _xmlDTE.HefGetXmlNodeValue("RUTEmisor"),
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            lastCesion.HefGetXmlNodeValue("RUTReceptor"),
                $"<RUTReceptor>{_xmlDTE.HefGetXmlNodeValue2("RUTRecep")}</RUTReceptor>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            lastCesion.HefGetXmlNodeValue("Folio"),
                _xmlDTE.HefGetXmlNodeValue("Folio"),
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            lastCesion.HefGetXmlNodeValue("FchEmis"),
                _xmlDTE.HefGetXmlNodeValue("FchEmis"),
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            lastCesion.HefGetXmlNodeValue("MntTotal"),
                _xmlDTE.HefGetXmlNodeValue("MntTotal"),
                    RegexOptions.Singleline);

        // Complete los datos del cedente
        lastCesion = Regex.Replace(lastCesion,
            "<RUT>CCCC</RUT>",
                $"<RUT>{_cesionDTO.RutCedente}</RUT>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<RazonSocial>CCCC</RazonSocial>",
                $"<RazonSocial>{_cesionDTO.RznSocCedente}</RazonSocial>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<Direccion>CCCC</Direccion>",
                $"<Direccion>{_cesionDTO.DireccionCedente}</Direccion>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<eMail>CCCC</eMail>",
                $"<eMail>{_cesionDTO.EmailCedente}</eMail>",
                    RegexOptions.Singleline);

        // Agregar declaración jurada
        string datosCertificado = this._credenciales!.HefGetDatosCertificado();

        lastCesion = Regex.Replace(lastCesion,
            "<RUT>GGGG<\\/RUT>",
                $"<RUT>{datosCertificado.Split('|')[0]}</RUT>",
                    RegexOptions.Singleline);

        lastCesion = Regex.Replace(lastCesion,
            "<Nombre>GGGG<\\/Nombre>",
                $"<Nombre>{datosCertificado.Split('|')[1]}</Nombre>",
                    RegexOptions.Singleline);

        string rutReceptor = Regex.Match(_xmlDTE,
            "<RUTRecep>(.*?)<\\/RUTRecep>",
                RegexOptions.Singleline).Groups[1].Value;

        string RznSocRecep = Regex.Match(_xmlDTE,
            "<RznSocRecep>(.*?)<\\/RznSocRecep>",
                RegexOptions.Singleline).Groups[1].Value;

        string declaracion = string.Empty;
        declaracion += "Se declara bajo juramento que " + _cesionDTO.RznSocCedente + ", RUT " + _cesionDTO.RutCedente + " ha puesto a ";
        declaracion += "disposición del cesionario " + _cesionDTO.RznSocCesionario + ", RUT " + _cesionDTO.RutCesionario + ", el ";
        declaracion += "o los documentos donde constan los recibos de las mercaderías entregadas o ";
        declaracion += "servicios prestados, entregados por parte del deudor de la factura ";
        declaracion += $"{RznSocRecep}, RUT {rutReceptor}, de acuerdo a lo establecido en la Ley N° 19.983";

        lastCesion = Regex.Replace(lastCesion,
            "<DeclaracionJurada/>",
                $"<DeclaracionJurada>{declaracion}</DeclaracionJurada>",
                    RegexOptions.Singleline);

        // Complete los datos del cesionario
        lastCesion = Regex.Replace(lastCesion,
            "<RUT>DDDD</RUT>",
                $"<RUT>{_cesionDTO.RutCesionario}</RUT>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<RazonSocial>DDDD</RazonSocial>",
                $"<RazonSocial>{_cesionDTO.RznSocCesionario}</RazonSocial>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<Direccion>DDDD</Direccion>",
                $"<Direccion>{_cesionDTO.DireccionCesionario}</Direccion>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<eMail>DDDD</eMail>",
                $"<eMail>{_cesionDTO.EmailCesionario}</eMail>",
                    RegexOptions.Singleline);

        // Complete los datos de el monto de la cesión.
        lastCesion = Regex.Replace(lastCesion,
            "<MontoCesion>EEEE</MontoCesion>",
                $"<MontoCesion>{_xmlDTE.HefGetXmlNodeValue2("MntTotal")}</MontoCesion>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<UltimoVencimiento>EEEE</UltimoVencimiento>",
                $"<UltimoVencimiento>{_xmlDTE.HefGetUltimoVencimientoDTE()}</UltimoVencimiento>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<TmstCesion>EEEE</TmstCesion>",
                $"<TmstCesion>{this._tmstActual}</TmstCesion>",
                    RegexOptions.Singleline);

        // Recuperar la ultima cesion (template)
        string currentlastCesion = template.HefGetLastCesion();
        template = Regex.Replace(
            template,
                currentlastCesion,
                    lastCesion,
                        RegexOptions.Singleline);

        // Firmar el documento completo
        HefRespuesta respSignature = SignAEC(template);
        if (!respSignature.EsCorrecto)
            throw new Exception($"No fue posible firmar el documento completo AEC. " +
                $"'{respSignature.Detalle}'");

        XmlDocument xAEC = new XmlDocument();
        xAEC.PreserveWhitespace = true;
        if (respSignature.Resultado == null)
            throw new Exception("No fue posible recuperar el documento firmado.");

        xAEC.LoadXml((string)respSignature.Resultado!);

        // Enviar AEC al SII
        lastCesion = xAEC.OuterXml.HefGetLastCesion();
        string eMail = Regex.Match(
            lastCesion,
                "Cedente.*?eMail>(.*?)<",
                    RegexOptions.Singleline).Groups[1].Value;
        string rutCedente = Regex.Match(
            lastCesion,
                "Cedente.*?RUT>(.*?)<",
                    RegexOptions.Singleline).Groups[1].Value;
        string nombreArchivo = $"HefestoCesion_R" +
            $"{rutCedente.Replace("-", "")}" +
                $"_F{DateTime.Now.ToString("yyyyMMddHHmmss")}.xml";

        resp = PublicacionService
                .PublicarDocumentoProduccion(
                    xAEC.OuterXml,
                        this._token!,
                            eMail,
                                rutCedente,
                                    nombreArchivo);

        if (resp.EsCorrecto)
            resp.Resultado = ((string)resp.Resultado!).HefGetBase64();

        //// Test
        //// Guardar el documento en disco
        if (resp.EsCorrecto)
            File.WriteAllText($"Files/HefCesion{resp.Trackid}.xml", xAEC.OuterXml, Encoding.GetEncoding("ISO-8859-1"));

        return resp;
    }
    private HefRespuesta GenerateAECByDTE()
    {
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "GenerateAECByAEC()";

        // preparar documento cesion
        var resourceManager = new ResourceManager("vyg_api_sii.Resources.Resources", Assembly.GetExecutingAssembly());
        if (resourceManager.GetString("AEC_Template") == null)
            throw new Exception("No fue posible recuperar el template del AEC.");
        
        string template = resourceManager.GetString("AEC_Template")!;
        string _xmlDTE = this._xmlOriginal!.HefGetDTE();
        string cesiones = this._xmlOriginal!.HefGetCesiones();
        if ( !string.IsNullOrEmpty(cesiones) )
            template = Regex.Replace(
                template,
                    "</DTECedido>",
                        $"</DTECedido>\r\n{cesiones}",
                            RegexOptions.Singleline);

        int SeqCesion = template.HefGetCountCesiones();
        string lastCesion = template.HefGetLastCesion();
        lastCesion = Regex.Replace(
            lastCesion,
                "<SeqCesion>(.*?)</SeqCesion>",
                    $"<SeqCesion>{SeqCesion}</SeqCesion>",
                        RegexOptions.Singleline);

        lastCesion = Regex.Replace(
            lastCesion,
                "ID=\".*?\"",
                    $"ID=\"DocumentoCesion{SeqCesion}\"",
                        RegexOptions.Singleline);        

        // Agregar la caratula del documento (AEC)
        template = Regex.Replace(template,
            "<RutCedente>.*?</RutCedente>",
                $"<RutCedente>{_cesionDTO!.RutCedente}</RutCedente>", RegexOptions.Singleline);
        template = Regex.Replace(template,
            "<RutCesionario>.*?</RutCesionario>",
                $"<RutCesionario>{_cesionDTO!.RutCesionario}</RutCesionario>", RegexOptions.Singleline);
        template = Regex.Replace(template,
            "<NmbContacto>.*?</NmbContacto>",
                $"<NmbContacto>{_cesionDTO!.NmbContacto}</NmbContacto>", RegexOptions.Singleline);
        template = Regex.Replace(template,
            "<FonoContacto>.*?</FonoContacto>",
                $"<FonoContacto>{_cesionDTO!.FonoContacto}</FonoContacto>", RegexOptions.Singleline);
        template = Regex.Replace(template,
            "<MailContacto>.*?</MailContacto>",
                $"<MailContacto>{_cesionDTO!.MailContacto}</MailContacto>", RegexOptions.Singleline);
        template = Regex.Replace(template,
            "<TmstFirmaEnvio>.*?</TmstFirmaEnvio>",
                $"<TmstFirmaEnvio>{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")}</TmstFirmaEnvio>",
                    RegexOptions.Singleline);

        // Agregar datos del documento (DTE Cedido)
        template = Regex.Replace(
            template,
                "<DTE/>",
                    _xmlDTE,
                        RegexOptions.Singleline);

        template = Regex.Replace(
            template,
                "<TmstFirma>.*?<\\/TmstFirma>",
                    $"<TmstFirma>{this._tmstActual}</TmstFirma>",
                        RegexOptions.Singleline);

        // Complete los datos del documento DTE
        lastCesion = Regex.Replace(lastCesion,
            lastCesion.HefGetXmlNodeValue("TipoDTE"),
                _xmlDTE.HefGetXmlNodeValue("TipoDTE"),
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            lastCesion.HefGetXmlNodeValue("RUTEmisor"),
                _xmlDTE.HefGetXmlNodeValue("RUTEmisor"),
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            lastCesion.HefGetXmlNodeValue("RUTReceptor"),
                $"<RUTReceptor>{_xmlDTE.HefGetXmlNodeValue2("RUTRecep")}</RUTReceptor>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            lastCesion.HefGetXmlNodeValue("Folio"),
                _xmlDTE.HefGetXmlNodeValue("Folio"),
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            lastCesion.HefGetXmlNodeValue("FchEmis"),
                _xmlDTE.HefGetXmlNodeValue("FchEmis"),
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            lastCesion.HefGetXmlNodeValue("MntTotal"),
                _xmlDTE.HefGetXmlNodeValue("MntTotal"),
                    RegexOptions.Singleline);

        // Complete los datos del cedente
        lastCesion = Regex.Replace(lastCesion,
            "<RUT>CCCC</RUT>",
                $"<RUT>{_cesionDTO.RutCedente}</RUT>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<RazonSocial>CCCC</RazonSocial>",
                $"<RazonSocial>{_cesionDTO.RznSocCedente}</RazonSocial>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<Direccion>CCCC</Direccion>",
                $"<Direccion>{_cesionDTO.DireccionCedente}</Direccion>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<eMail>CCCC</eMail>",
                $"<eMail>{_cesionDTO.EmailCedente}</eMail>",
                    RegexOptions.Singleline);

        // Agregar declaración jurada
        string datosCertificado = this._credenciales!.HefGetDatosCertificado();

        lastCesion = Regex.Replace(lastCesion,
            "<RUT>GGGG<\\/RUT>",
                $"<RUT>{datosCertificado.Split('|')[0]}</RUT>",
                    RegexOptions.Singleline);

        lastCesion = Regex.Replace(lastCesion,
            "<Nombre>GGGG<\\/Nombre>",
                $"<Nombre>{datosCertificado.Split('|')[1]}</Nombre>",
                    RegexOptions.Singleline);

        string rutReceptor = Regex.Match(_xmlDTE,
            "<RUTRecep>(.*?)<\\/RUTRecep>",
                RegexOptions.Singleline).Groups[1].Value;

        string RznSocRecep = Regex.Match(_xmlDTE,
            "<RznSocRecep>(.*?)<\\/RznSocRecep>",
                RegexOptions.Singleline).Groups[1].Value;

        string declaracion = string.Empty;
        declaracion += "Se declara bajo juramento que " + _cesionDTO.RznSocCedente + ", RUT " + _cesionDTO.RutCedente + " ha puesto a ";
        declaracion += "disposición del cesionario " + _cesionDTO.RznSocCesionario + ", RUT " + _cesionDTO.RutCesionario + ", el ";
        declaracion += "o los documentos donde constan los recibos de las mercaderías entregadas o ";
        declaracion += "servicios prestados, entregados por parte del deudor de la factura ";
        declaracion += $"{RznSocRecep}, RUT {rutReceptor}, de acuerdo a lo establecido en la Ley N° 19.983";

        lastCesion = Regex.Replace(lastCesion,
            "<DeclaracionJurada/>",
                $"<DeclaracionJurada>{declaracion}</DeclaracionJurada>",
                    RegexOptions.Singleline);

        // Complete los datos del cesionario
        lastCesion = Regex.Replace(lastCesion,
            "<RUT>DDDD</RUT>",
                $"<RUT>{_cesionDTO.RutCesionario}</RUT>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<RazonSocial>DDDD</RazonSocial>",
                $"<RazonSocial>{_cesionDTO.RznSocCesionario}</RazonSocial>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<Direccion>DDDD</Direccion>",
                $"<Direccion>{_cesionDTO.DireccionCesionario}</Direccion>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<eMail>DDDD</eMail>",
                $"<eMail>{_cesionDTO.EmailCesionario}</eMail>",
                    RegexOptions.Singleline);

        // Complete los datos de el monto de la cesión.
        lastCesion = Regex.Replace(lastCesion,
            "<MontoCesion>EEEE</MontoCesion>",
                $"<MontoCesion>{_xmlDTE.HefGetXmlNodeValue2("MntTotal")}</MontoCesion>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<UltimoVencimiento>EEEE</UltimoVencimiento>",
                $"<UltimoVencimiento>{_xmlDTE.HefGetUltimoVencimientoDTE()}</UltimoVencimiento>",
                    RegexOptions.Singleline);
        lastCesion = Regex.Replace(lastCesion,
            "<TmstCesion>EEEE</TmstCesion>",
                $"<TmstCesion>{this._tmstActual}</TmstCesion>",
                    RegexOptions.Singleline);

        // Recuperar la ultima cesion (template)
        string currentlastCesion = template.HefGetLastCesion();
        template = Regex.Replace(
            template,
                currentlastCesion,
                    lastCesion,
                        RegexOptions.Singleline);

        // Firmar el documento completo
        HefRespuesta respSignature = SignAEC(template);
        if (!respSignature.EsCorrecto)
            throw new Exception($"No fue posible firmar el documento completo AEC. " +
                $"'{respSignature.Detalle}'");

        XmlDocument xAEC = new XmlDocument();
        xAEC.PreserveWhitespace = true;
        if (respSignature.Resultado == null)
            throw new Exception("No fue posible recuperar el documento firmado.");

        xAEC.LoadXml((string)respSignature.Resultado!);

        // Enviar AEC al SII
        lastCesion = xAEC.OuterXml.HefGetLastCesion();
        string eMail = Regex.Match(
            lastCesion,
                "Cedente.*?eMail>(.*?)<",
                    RegexOptions.Singleline).Groups[1].Value;
        string rutCedente = Regex.Match(
            lastCesion,
                "Cedente.*?RUT>(.*?)<",
                    RegexOptions.Singleline).Groups[1].Value;
        string nombreArchivo = $"HefestoCesion_R" +
            $"{rutCedente.Replace("-", "")}" +
                $"_F{DateTime.Now.ToString("yyyyMMddHHmmss")}.xml";

        resp = PublicacionService
                .PublicarDocumentoProduccion(
                    xAEC.OuterXml,
                        this._token!,
                            eMail,
                                rutCedente,
                                    nombreArchivo);

        if (resp.EsCorrecto)
            resp.Resultado = ((string)resp.Resultado!).HefGetBase64();

        //// Test
        //// Guardar el documento en disco
        if (resp.EsCorrecto)
            File.WriteAllText($"Files/HefCesion{resp.Trackid}.xml", xAEC.OuterXml, Encoding.GetEncoding("ISO-8859-1"));

        return resp;
    }
    private HefRespuesta SignAEC(string template)
    {
        //// Inicie la respuesta
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "SignAEC()";

        try
        {
            XmlDocument xAEC = new XmlDocument();
            xAEC.PreserveWhitespace = true;
            xAEC.LoadXml(template);

            XmlElement xDTECedido = (XmlElement)xAEC.DocumentElement?.GetElementsByTagName("DTECedido")[0]!;
            HefRespuesta firma1 = Extensions.HefSignatureExtension.HefFirmar(xDTECedido.OuterXml, this._certificado!);
            //Extensiones.HefSignature.HefFirmar(xDTECedido.OuterXml, this._certificado);

            XmlElement xLastCesion = xAEC.DocumentElement?.GetElementsByTagName("Cesion")
                .Cast<XmlElement>()
                    .LastOrDefault()!;

            HefRespuesta firma2 = Extensions.HefSignatureExtension.HefFirmar(xLastCesion.OuterXml, this._certificado!);

            if (template.HefGetCountCesiones() == 1)
                template = Regex.Replace(
                    template,
                        "<DTECedido.*?<\\/DTECedido>",
                            (string)firma1.Resultado!,
                                RegexOptions.Singleline);

            template = Regex.Replace(
                template,
                    template.HefGetLastCesion(),
                        (string)firma2.Resultado!,
                            RegexOptions.Singleline);

            xAEC = new XmlDocument();
            xAEC.PreserveWhitespace = true;
            xAEC.LoadXml(template);

            ////
            //// Firme el documento completo
            HefRespuesta firma3 = Extensions.HefSignatureExtension.HefFirmar(xAEC.OuterXml, this._certificado!);

            ////
            //// firme el documento completo
            xAEC = new XmlDocument();
            xAEC.PreserveWhitespace = true;
            xAEC.LoadXml((string)firma3.Resultado!);

            ////
            //// regrese la respuesta
            resp.EsCorrecto = true;
            resp.Detalle = "Documento firmado correctamente";
            resp.Resultado = xAEC.OuterXml;


        }
        catch (Exception err)
        {
            ////
            //// Notficar el error
            resp.EsCorrecto = false;
            resp.Detalle = err.Message;

            
        }

        ////
        //// regrese el valor de retorno
        return resp;
    }
}
