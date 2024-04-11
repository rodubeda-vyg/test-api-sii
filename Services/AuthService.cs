using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using vyg_api_sii.Models;

namespace vyg_api_sii.Services;
public class AuthService
{
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
    public static HefRespuesta GetCredenciales(X509Certificate2 certificado)
    {
        ////
        //// Cree la entidad para recuperar la respuesta
        HefRespuesta resp = new HefRespuesta();
        resp.Mensaje = "GetToken";
        resp.Detalle = "Recupera el token desde el SII";

        ////
        //// Protocolos se transmision
        System.Net.ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls;

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
            string cookies = response.Headers["Set-Cookie"];
            if (string.IsNullOrEmpty(cookies))
                throw new Exception("La consulta actual no regresó ninguna cookies. ( cookies= null )");

            ////
            //// Cree el arreglo de cookies
            string sCookies = string.Join("; ", cookies.Split(',')
                .Cast<string>().Select(p=> p.Split(';')[0].Trim() ).ToArray());

            ////
            //// Complete la respuesta del proceso
            resp.EsCorrecto = true;
            resp.Resultado = sCookies;

        }
        catch (Exception Ex)
        {
            resp.EsCorrecto = false;
            resp.Mensaje = "No fue posible realizar conección al SII.\r\n" + Ex.Message;
        }

        ////
        //// Regrese el valor de retorno
        return resp;

    }
    public static HefRespuesta GetToken(X509Certificate2 certificado)
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
        string semilla = string.Empty;

        ////
        //// Inicie el proceso
        try
        {

            #region RECUPERAR SEMILLA DESDE EL SII

            ////
            //// Recupere la semilla desde el SII
            string urlSemilla = "https://palena.sii.cl/DTEWS/CrSeed.jws";

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
            string postData = @$"<soapenv:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:def=""http://DefaultNamespace"">
                <soapenv:Header/>
                <soapenv:Body>
                    <def:getSeed soapenv:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/""/>
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
                        semilla = reader.ReadToEnd();
                    }

            }

            ////
            //// recupere la información de la respuesta
            Match mSemillaRespuesta = Regex.Match(
                semilla,
                    "getSeedReturn.*?>(.*?)<",
                        RegexOptions.Singleline);

            ////
            //// Valide la semilla
            if (!mSemillaRespuesta.Success)
                throw new Exception($"El servidor del SII no regreso ninguna semilla. Respuesta = '{semilla}'");

            ////
            //// Recupere la respuesta xml de la semilla
            semilla = WebUtility.HtmlDecode(mSemillaRespuesta.Groups[1].Value);

            ////
            //// Recupere la semilla
            Match mSemilla = Regex.Match(semilla, "<SEMILLA>(.*?)<\\/SEMILLA>", RegexOptions.Singleline);
            if (!mSemilla.Success)
                throw new Exception($"El servidor del SII no regreso ninguna semilla. Respuesta = '{semilla}'");

            ////
            //// recupere la semilla
            semilla = mSemilla.Groups[1].Value;


            #endregion

            #region CREAR SOBRE CON SEMILLA FIRMADA

            ////
            //// Firme la semilla
            semilla = FirmarDocumentoSemilla(semilla, certificado);

            #endregion

            #region RECUPERAR TOKEN DESDE EL SII

            ////
            //// Recupere la semilla desde el SII
            urlSemilla = "https://palena.sii.cl/DTEWS/GetTokenFromSeed.jws";

            ////
            //// Consulte al SII
            req = (HttpWebRequest)WebRequest.Create(urlSemilla);
            req.Method = "POST";
            req.Headers.Add("Accept-Encoding", "gzip,deflate");
            req.ContentType = "text/xml;charset=UTF-8";
            req.Headers.Add("SOAPAction", "");
            req.Host = "palena.sii.cl";
            req.UserAgent = "Apache-HttpClient/4.5.5(Java/16.0.1)";

            ////
            //// Escriba la consulta ( POST ) y su largo en bytes
            postData = @$"<soapenv:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:def=""http://DefaultNamespace"">
                <soapenv:Header/>
                <soapenv:Body>
                    <def:getToken soapenv:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                        <pszXml xsi:type=""xsd:string""><![CDATA[{semilla}]]></pszXml>
                    </def:getToken>
                </soapenv:Body>
            </soapenv:Envelope>";

            postBytes = Encoding.UTF8.GetBytes(postData);
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
                        semilla = reader.ReadToEnd();
                    }

            }

            ////
            //// Compruebe la semilla
            Match mTokenRespuesta = Regex.Match(semilla, "getTokenReturn.*?>(.*?)<", RegexOptions.Singleline);

            ////
            //// Valide la semilla
            if (!mTokenRespuesta.Success)
                throw new Exception($"El servidor del SII no regreso ninguna token. Respuesta = '{semilla}'");

            ////
            //// Recupere la respuesta xml de la semilla
            semilla = WebUtility.HtmlDecode(mTokenRespuesta.Groups[1].Value);

            ////
            //// Recupere la semilla
            Match mToken = Regex.Match(semilla, "<TOKEN>(.*?)<\\/TOKEN>", RegexOptions.Singleline);
            if (!mToken.Success)
                throw new Exception($"El servidor del SII no regreso ninguna token. Respuesta = '{semilla}'");

            #endregion

            ////
            //// Complete la respuesta
            resp.EsCorrecto = true;
            resp.Detalle = "Proceso ejecutado OK";
            resp.Resultado = mToken.Groups[1].Value;

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
    public HefRespuesta RecuperarCertificado(byte[] bytesCertificado, string pass)
    {
        ////
        //// Inicie la respuesta del proceso
        HefRespuesta resp = new HefRespuesta();
        resp.Mensaje = "RecuperarCertificado";

        ////
        //// Iniciar
        try
        {
            ////
            //// Recupere el certificado
            X509Certificate2 certificado = new X509Certificate2(
                bytesCertificado,
                    pass,
                        X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.UserKeySet);

            ////
            //// El certificado tiene pk?
            if (!certificado.HasPrivateKey)
                throw new Exception("El certificado no tiene PK.");

            ////
            //// El certificado esta expirado?
            DateTime dtExpira;
            if (!DateTime.TryParse(certificado.GetExpirationDateString(), out dtExpira))
                throw new Exception("No fue posible encontrar la fecha de expiración del certificado");

            ////
            //// Esta expirado?
            if (DateTime.Now > dtExpira)
                throw new Exception("El certificado se encuentra expirado.");

            ////
            //// Cree la respuesta
            resp.EsCorrecto = true;
            resp.Detalle = "Certificado recuperado correctamente";
            resp.Resultado = certificado;

        }
        catch (Exception ex)
        {
            ////
            //// Cree la respuesta
            resp.EsCorrecto = false;
            resp.Detalle = ex.Message;
            resp.Resultado = null;
        }

        ////
        //// Regrese el valor de retorno 
        return resp;
    }
    internal static string FirmarDocumentoSemilla(string semilla, X509Certificate2 certificado)
    {
        ////
        //// Construya el sobre para colocar la semilla
        string body = $"<getToken><item><Semilla>{semilla}</Semilla></item></getToken>";

        ////
        //// Cree un nuevo documento xml y defina sus caracteristicas
        XmlDocument doc = new XmlDocument();
        doc.PreserveWhitespace = false;
        doc.LoadXml(body);

        // Create a SignedXml object.
        SignedXml signedXml = new SignedXml(doc);

        // Add the key to the SignedXml document.  'key'
        signedXml.SigningKey = certificado.PrivateKey;

        // Get the signature object from the SignedXml object.
        Signature XMLSignature = signedXml.Signature;

        // Create a reference to be signed.  Pass "" 
        // to specify that all of the current XML
        // document should be signed.
        Reference reference = new Reference("");

        // Add an enveloped transformation to the reference.
        XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
        reference.AddTransform(env);

        reference.DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";

        // Add the Reference object to the Signature object.
        XMLSignature.SignedInfo.AddReference(reference);

        // Add an RSAKeyValue KeyInfo (optional; helps recipient find key to validate).
        KeyInfo keyInfo = new KeyInfo();
        keyInfo.AddClause(new RSAKeyValue((RSA)certificado.PrivateKey));


        ////
        //// Agregar información del certificado x509
        //// X509Certificate MSCert = X509Certificate.CreateFromCertFile(Certificate);
        keyInfo.AddClause(new KeyInfoX509Data(certificado));


        // Add the KeyInfo object to the Reference object.
        XMLSignature.KeyInfo = keyInfo;


        ////
        //// ASignar el metodo de firma
        signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";


        // Compute the signature.
        signedXml.ComputeSignature();

        // Get the XML representation of the signature and save
        // it to an XmlElement object.
        XmlElement xmlDigitalSignature = signedXml.GetXml();

        // Append the element to the XML document.
        doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));


        if (doc.FirstChild is XmlDeclaration)
        {
            doc.RemoveChild(doc.FirstChild);
        }

        // Save the signed XML document to a file specified
        // using the passed string.
        //XmlTextWriter xmltw = new XmlTextWriter(@"d:\ResultadoFirma.xml", new UTF8Encoding(false));
        //doc.WriteTo(xmltw);
        //xmltw.Close();
        return doc.InnerXml;


    }
}
