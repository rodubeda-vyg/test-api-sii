using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using vyg_api_sii.Models;

namespace vyg_api_sii.Services;
public class SIIService
{
    public static HefRespuesta SII_Datos_Generales(string credenciales)
    {
        ////
        //// Cree la respuesta
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "SII_Datos_Generales";

        ////
        //// Variables
        string data = string.Empty;

        ////
        //// Identifique la url donde recuperar la información
        string uriSIITarget = "https://misiir.sii.cl/cgi_misii/siihome.cgi";

        ////
        //// Inicie el proceso
        try
        {
            ////
            //// Genera la consulta al SII
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uriSIITarget);
            req.Method = "GET";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7";
            req.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
            req.Headers.Add("Accept-Language", "es-419,es;q=0.9,es-ES;q=0.8,en;q=0.7,en-GB;q=0.6,en-US;q=0.5");
            req.Headers.Add("Cookie", credenciales);
            req.Host = "misiir.sii.cl";
            req.Referer = "https://herculesr.sii.cl/";
            req.Headers.Add("Sec-Ch-Ua", "\"Microsoft Edge\";v=\"123\", \"Not:A-Brand\";v=\"8\", \"Chromium\";v=\"123\"");
            req.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
            req.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
            req.Headers.Add("Sec-Fetch-Dest", "document");
            req.Headers.Add("Sec-Fetch-Mode", "navigate");
            req.Headers.Add("Sec-Fetch-Site", "same-site");
            req.Headers.Add("Sec-Fetch-User", "?1");
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0";
            req.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                ////
                //// Si no hay respuesta del servidor
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(
                        $"El SII no ha contestado nuestra solicitud. status code : {response.StatusCode.ToString()}");

                ////
                //// recupere la respuesta del SII
                using (Stream receiveStream = response.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.GetEncoding("ISO-8859-1")))
                    {
                        data = readStream.ReadToEnd();
                    }

                }

            }

            ////
            //// Complete la respuesta
            resp.EsCorrecto = true;
            resp.Detalle = "Proceso ejecutado OK";
            resp.Resultado = data;

        }
        catch (Exception err)
        {
            ////
            //// Notifique el error
            resp.EsCorrecto = false;
            resp.Detalle = err.Message;
            resp.Resultado = null;
        }

        ////
        //// Regrese el valor de retorno
        return resp;

    }
    public static HefRespuesta RecuperarDatosCertificado(string credenciales)
    {
        ////
        //// Cree la respuesta
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "MyMethod";

        ////
        //// Inicie el proceso
        try
        {
            ////
            //// Complete la respuesta
            resp.EsCorrecto = true;
            resp.Detalle = "Proceso ejecutado OK";
            resp.Resultado = null;

        }
        catch (Exception err)
        {
            ////
            //// Notifique el error
            resp.EsCorrecto = false;
            resp.Detalle = err.Message;
            resp.Resultado = null;

        }

        ////
        //// Regrese el valor de retorno
        return resp;

    }
    public static HefRespuesta RecuperarEstadoTrackid(string token, string rutEmpresa, long trackid)
    {
        ////
        //// Cree la respuesta
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "RecuperarTokenSiiDte";

        ////
        //// Protocolos se transmision
        System.Net.ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls;

        ////
        //// Variables
        string respuesta = string.Empty;

        ////
        //// Inicie el proceso
        try
        {
            #region RECUPERAR TOKEN DESDE EL SII

            ////
            //// Recupere la semilla desde el SII
            //// string urlSemilla = "https://maullin.sii.cl/DTEWS/services/wsRPETCConsulta";
            string urlSemilla = "https://palena.sii.cl/DTEWS/services/wsRPETCConsulta";

            ////
            //// Consulte al SII
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(urlSemilla);
            req.Method = "POST";
            req.Headers.Add("Accept-Encoding", "gzip,deflate");
            req.ContentType = "text/xml;charset=UTF-8";
            req.Headers.Add("SOAPAction", "");
            req.Host = "palena.sii.cl";
            req.UserAgent = "Apache-HttpClient/4.5.5(Java/16.0.1)";

            ////
            //// Escriba la consulta ( POST ) y su largo en bytes
            string postData = $@"
            <soapenv:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:def=""http://DefaultNamespace"">
            <soapenv:Header/>
            <soapenv:Body>
                <def:getEstEnvio soapenv:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                    <Token xsi:type=""xsd:string"">{token}</Token>
                    <TrackId xsi:type=""xsd:string"">{trackid}</TrackId>
                </def:getEstEnvio>
            </soapenv:Body>
            </soapenv:Envelope>";


            byte[] postBytes = Encoding.UTF8.GetBytes(postData);
            req.ContentLength = postBytes.Length;

            ////
            //// Escriba los bytes en el request stream
            using (Stream postStream = req.GetRequestStream())
            {
                postStream.Write(postBytes, 0, postBytes.Length);
                postStream.Flush();
                postStream.Close();
            }

            ////
            //// Recupere la respuesta de la consulta ( response )
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                ////
                //// Recuperar el token
                if (response.StatusCode == HttpStatusCode.OK)
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        respuesta = reader.ReadToEnd();
                    }

            }

            ////
            //// Compruebe la semilla
            Match mRespuesta = Regex.Match(
                respuesta, 
                    "getEstEnvioReturn.*?>(.*?)<", 
                        RegexOptions.Singleline);

            ////
            //// Valide la semilla
            if (!mRespuesta.Success)
                throw new Exception($"El servidor del SII no regreso ningun estado. Respuesta = '{respuesta}'");

            ////
            //// Recupere la respuesta xml de la semilla
            respuesta = WebUtility.HtmlDecode(mRespuesta.Groups[1].Value);

            #endregion

            ////
            //// Complete la respuesta
            resp.EsCorrecto = true;
            resp.Detalle = "Recuperción respuesta OK";
            resp.Trackid = trackid.ToString();
            resp.Resultado = respuesta;

        }
        catch (Exception err)
        {
            ////
            //// Notifique el error
            resp.EsCorrecto = false;
            resp.Detalle = err.Message;
            resp.CodigoSII = null;
            resp.Trackid = trackid.ToString();
            resp.Resultado = null;

        }

        ////
        //// Regrese el valor de retorno
        return resp;

    }
    public static HefRespuesta RecuperarEstadoCesion(string token, string rutEmpresa, string tipoDoc, string folioDoc, string idCesion="")
    {
        ////
        //// Cree la respuesta
        HefRespuesta resp = new HefRespuesta();
        resp.EsCorrecto = false;
        resp.Mensaje = "RecuperarEstadoCesion";

        ////
        //// Protocolos se transmision
        System.Net.ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls;

        ////
        //// Variables
        string respuesta = string.Empty;

        ////
        //// Inicie el proceso
        try
        {

            #region RECUPERAR TOKEN DESDE EL SII

            ////
            //// Recupere la semilla desde el SII
            string urlSemilla = "https://palena.sii.cl/DTEWS/services/wsRPETCConsulta";

            ////
            //// Consulte al SII
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(urlSemilla);
            req.Method = "POST";
            req.Headers.Add("Accept-Encoding", "gzip,deflate");
            req.ContentType = "text/xml;charset=UTF-8";
            req.Headers.Add("SOAPAction", "");
            req.Host = "palena.sii.cl";
            req.UserAgent = "Apache-HttpClient/4.5.5(Java/16.0.1)";

            ////
            //// Escriba la consulta ( POST ) y su largo en bytes
            string postData = $@"
            <soapenv:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:def=""http://DefaultNamespace"">
            <soapenv:Header/>
            <soapenv:Body>
                <def:getEstCesion soapenv:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                    <Token xsi:type=""xsd:string"">{token}</Token>
                    <RutEmisor xsi:type=""xsd:string"">{rutEmpresa.Split('-')[0]}</RutEmisor>
                    <DVEmisor xsi:type=""xsd:string"">{rutEmpresa.Split('-')[1]}</DVEmisor>
                    <TipoDoc xsi:type=""xsd:string"">{tipoDoc}</TipoDoc>
                    <FolioDoc xsi:type=""xsd:string"">{folioDoc}</FolioDoc>
                    <IdCesion xsi:type=""xsd:string""></IdCesion>
                </def:getEstCesion>
            </soapenv:Body>
            </soapenv:Envelope>";


            byte[] postBytes = Encoding.UTF8.GetBytes(postData);
            req.ContentLength = postBytes.Length;

            ////
            //// Escriba los bytes en el request stream
            using (Stream postStream = req.GetRequestStream())
            {
                postStream.Write(postBytes, 0, postBytes.Length);
                postStream.Flush();
                postStream.Close();
            }

            ////
            //// Recupere la respuesta de la consulta ( response )
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                ////
                //// Recuperar el token
                if (response.StatusCode == HttpStatusCode.OK)
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        respuesta = reader.ReadToEnd();
                    }

            }

            ////
            //// Compruebe la semilla
            Match mRespuesta = Regex.Match(
                respuesta,
                    "getEstCesionReturn.*?>(.*?)<",
                        RegexOptions.Singleline);

            ////
            //// Valide la semilla
            if (!mRespuesta.Success)
                throw new Exception($"El servidor del SII no regreso ningun estado. Respuesta = '{respuesta}'");

            ////
            //// Recupere la respuesta xml de la semilla
            respuesta = WebUtility.HtmlDecode(mRespuesta.Groups[1].Value);

            #endregion

            ////
            //// Complete la respuesta
            resp.EsCorrecto = true;
            resp.Detalle = "Recuperción respuesta OK";
            resp.Resultado = respuesta;

        }
        catch (Exception err)
        {
            ////
            //// Notifique el error
            resp.EsCorrecto = false;
            resp.Detalle = err.Message;
            resp.CodigoSII = "-1";
            resp.Resultado = null;

        }

        ////
        //// Regrese el valor de retorno
        return resp;
    }
    public async Task<HefRespuesta> RecuperaDocumentosAecs_RecibidosNew(string credenciales, string rutEmpresa, string tipoDte, string folio)
    {
        HefRespuesta resp = new HefRespuesta();
        resp.Mensaje = "RecuperaDocumentosAecs_Recibidos";

        string uriSIITarget = "https://palena.sii.cl/cgi_rtc/RTC/RTCDescargarXmlCons.cgi";
        string uriSIIRefere = "https://palena.sii.cl/rtc/RTC/RTCConsultas.html";

        string sDocumento = string.Empty;

        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                client.DefaultRequestHeaders.Add("Accept-Language", "es,es-ES;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
                client.DefaultRequestHeaders.Add("Origin", "https://palena.sii.cl");
                client.DefaultRequestHeaders.Add("sec-ch-ua", "\" Not; A Brand\";v=\"99\", \"Microsoft Edge\"; v=\"91\", \"Chromium\"; v=\"91\"");
                client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
                client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.77 Safari/537.36 Edg/91.0.864.37");
                client.DefaultRequestHeaders.Add("Referer", uriSIIRefere);
                client.DefaultRequestHeaders.Add("Cookie", credenciales);

                var postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("rut_emisor", rutEmpresa.Split('-')[0]));
                postData.Add(new KeyValuePair<string, string>("dv_emisor", rutEmpresa.Split('-')[1]));
                postData.Add(new KeyValuePair<string, string>("tipo_docto", tipoDte));
                postData.Add(new KeyValuePair<string, string>("folio", folio));
                postData.Add(new KeyValuePair<string, string>("clave", ""));
                postData.Add(new KeyValuePair<string, string>("botonxml", "xml"));

                HttpContent content = new FormUrlEncodedContent(postData);

                HttpResponseMessage response = await client.PostAsync(uriSIITarget, content);

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("No fue posible comunicarse con el servidor remoto.");

                //sDocumento = await response.Content.ReadAsStringAsync();
                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                sDocumento = Encoding.GetEncoding("ISO-8859-1").GetString(bytes);
            }

            resp.EsCorrecto = true;
            resp.Mensaje = "Proceso ejecutado correctamente";
            resp.Resultado = sDocumento;
        }
        catch (Exception ex)
        {
            resp.EsCorrecto = false;
            resp.Detalle = ex.Message;
            resp.Resultado = null;
        }

        return resp;
    }
    public async Task<HefRespuesta> RecuperarDocumentoNew(string rutEmpresa, string token, string TipoDTE, string Folio, string FchDesde, HefOrigen Origen)
    {
        HefRespuesta resp = new HefRespuesta();
        resp.Mensaje = "RecuperarDocumento DAL";

        string ORIGEN = string.Empty;
        if (Origen == HefOrigen.Emitidos)
            ORIGEN = "ENV";
        if (Origen == HefOrigen.Recibidos)
            ORIGEN = "RCP";

        string uriSIITarget = "https://www1.sii.cl/cgi-bin/Portal001/download.cgi";
        uriSIITarget += "?RUT_EMP=" + rutEmpresa.Split('-')[0];
        uriSIITarget += "&DV_EMP=" + rutEmpresa.Split('-')[1].ToUpper();
        uriSIITarget += "&ORIGEN=" + ORIGEN;
        uriSIITarget += "&RUT_RECP=";
        uriSIITarget += "&FOLIO=" + Folio;
        uriSIITarget += "&FOLIOHASTA=";
        uriSIITarget += "&RZN_SOC=";
        uriSIITarget += "&FEC_DESDE=" + FchDesde;
        uriSIITarget += "&FEC_HASTA=";
        uriSIITarget += "&TPO_DOC=" + TipoDTE;
        uriSIITarget += "&ESTADO=";
        uriSIITarget += "&ORDEN=";
        uriSIITarget += "&DOWNLOAD=XML";

        string uriReference = "https://www1.sii.cl/cgi-bin/Portal001/lista_documentos.cgi";

        try
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Accept-Language", "es,es-ES;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            client.DefaultRequestHeaders.Add("Cookie", token);
            client.DefaultRequestHeaders.Add("Origin", "https://www1.sii.cl");
            client.DefaultRequestHeaders.Add("Referer", uriReference);
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "same-origin");
            client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.57");

            HttpResponseMessage response = await client.GetAsync(uriSIITarget);

            if (!response.IsSuccessStatusCode)
                throw new Exception("El SII no ha contestado nuestra solicitud.");

            string my_data = await response.Content.ReadAsStringAsync();

            resp.EsCorrecto = true;
            resp.Detalle = "Recuperación OK";
            resp.Resultado = my_data;
        }
        catch (Exception ex)
        {
            resp.EsCorrecto = false;
            resp.Detalle = "No fue posible recuperar la información.\r\n" + ex.Message;
            resp.Resultado = null;
        }

        return resp;
    }
    public async Task<credencialSII> GetTokenSimple(credencialSII credencial)
    {
        credencialSII respuesta = new credencialSII();
        respuesta = credencial;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        string url = "https://zeusr.sii.cl/cgi_AUT2000/CAutInicio.cgi";
        try
        {
            HttpWebRequest? request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = true;
            request.Method = "POST";
            request.Accept = "text / html, application / xhtml + xml, */*";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Host = "zeusr.sii.cl";

            string bodyData = string.Format("rut={0}&dv={1}&referencia=https%3A%2F%2Fmisiir.sii.cl%2Fcgi_misii%2Fsiihome.cgi&rutcntr={2}&clave={3}"
                , respuesta.rut.ToString()
                , respuesta.DV
                , respuesta.rutConDV
                , respuesta.claveSII
                );

            byte[] bodyByte = Encoding.UTF8.GetBytes(bodyData);
            request.ContentLength = bodyByte.Length;

            Stream postStream = request.GetRequestStream();
            postStream.Write(bodyByte, 0, bodyByte.Length);
            postStream.Flush();
            postStream.Close();

            HttpWebResponse? response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                bool HasCookies = response.Headers.AllKeys.Contains("Set-Cookie");
                if (!HasCookies) {
                    respuesta.status = respuesta.status + " / La consulta actual no regresó cookies";
                    respuesta.token = "Error";
                    throw new Exception(respuesta.status);
                }
                string? cookies = response.Headers["Set-Cookie"];
                if (string.IsNullOrEmpty(cookies)) {
                    respuesta.status = respuesta.status + " / La consulta actual regresó cookies sin valores";
                    respuesta.token = "Error";
                    throw new Exception(respuesta.status);
                }
                string[] Items = cookies.Split(',');
                string sCookies = string.Empty;
                foreach (string Item in Items)
                {
                    sCookies += Item.Split(';')[0] + "; ";
                }

                sCookies = sCookies.Substring(0, sCookies.Length - 2);
                respuesta.cookie = sCookies;
                string tokenSII = string.Empty;
                Match matchToken = Regex.Match(sCookies, @"TOKEN=([\w\d]+);");
                if (matchToken.Success) { tokenSII = matchToken.Groups[1].Value; }
                respuesta.token = tokenSII;
                respuesta.conversationId = matchToken.Groups[1].Value;
                respuesta.transactionId = Guid.NewGuid().ToString();
                respuesta.dtPC = "20$69871136_496h103vSNCSJEFFRMOVPUCHRDPRHDHKFUODMPSB-0e0";

                respuesta.status = "Cookies OK";
            }
            else
            {
                respuesta.cookie = string.Empty;
                respuesta.status = "Bad Request.";
                respuesta.token = "Error";
                throw new Exception(respuesta.status);
            }

            request = null;
            response.Close();
            response = null;

        }
        catch (Exception ex)
        {
            respuesta.status = respuesta.status + " / " + ex.Message;
        }

        await Task.Delay(1000);
        return respuesta;

    }
    public HefRespuesta GetTokenCert(X509Certificate2 certificado)
    {
        ////
        //// Cree la entidad para recuperar la respuesta
        HefRespuesta resp = new HefRespuesta();
        resp.Mensaje = "GetTokenCert";

        ////
        //// Target donde apunta la autenticación del SII
        string uriSIITarget = "https://herculesr.sii.cl/cgi_AUT2000/CAutInicio.cgi?";
        uriSIITarget += "https://misiir.sii.cl/cgi_misii/siihome.cgi";

        ////
        //// Inicie el proceso
        try
        {

            ////
            //// Consulta al SII para autenticar al cliente actual ( certificado )
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uriSIITarget);
            req.PreAuthenticate = true;
            req.AllowAutoRedirect = true;
            req.ClientCertificates.Add(certificado);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            ////
            //// Escriba la consulta ( POST ) y su largo en bytes
            string postData = "referencia=https%3A%2F%2Fmisiir.sii.cl%2Fcgi_misii%2Fsiihome.cgi";
            byte[] postBytes = Encoding.UTF8.GetBytes(postData);
            req.ContentLength = postBytes.Length;

            ////
            //// Escriba los bytes en el request stream
            Stream postStream = req.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Flush();
            postStream.Close();

            ////
            //// Recupere la respuesta de la consulta ( response )
            HttpWebResponse response = (HttpWebResponse)req.GetResponse();

            ////
            //// Recupere la respuesta del SII
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("No fue posible comunicarse con el servidor remoto.");

            ////
            //// Recupere las cookies generadas por el SII
            bool HasCookies = response.Headers.AllKeys.Contains("Set-Cookie");
            if (!HasCookies)
                throw new Exception("La consulta actual no regresó cookies.");

            ////
            //// Recupere las cookies del proceso
            string cookies = response.Headers["Set-Cookie"]!;
            if (string.IsNullOrEmpty(cookies))
                throw new Exception("La consulta actual no regresó ninguna cookies. ( cookies= null )");

            ////
            //// Cree el arreglo de cookies
            string[] Items = cookies.Split(',');

            ////
            //// Por cada item del arreglo solo recupere el value de la cookie
            string sCookies = string.Empty;
            foreach (string Item in Items)
            {
                sCookies += Item.Split(';')[0] + "; ";
            }

            ////
            //// Limpie la cadena de los caracteres no validos
            sCookies = sCookies.Substring(0, sCookies.Length - 2);

            ////
            //// Complete la respuesta del proceso
            resp.EsCorrecto = true;
            resp.Mensaje = "Conectar()";
            resp.Detalle = "Operación ejecutada correctamente.";
            resp.Resultado = sCookies;

        }
        catch (Exception Ex)
        {
            resp.EsCorrecto = false;
            resp.Detalle = "No fue posible realizar conección al SII.\r\n" + Ex.Message;
            resp.Resultado = null;
        }

        ////
        //// Regrese el valor de retorno
        return resp;
    }
}
