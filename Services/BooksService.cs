using System.Net;
using System.Text;
using Newtonsoft.Json;
using vyg_api_sii.Models;

namespace vyg_api_sii.Services;

public class BooksService
{
    public async Task<List<BookVenta>> GetBookVentaAsync(credencialSII credencial, int periodoDTE)
    {
        List<BookVenta> respuesta = new List<BookVenta>();
        
        // Tipo de Documentos a obtener informacion
        int[] tiposDTE = [33, 34, 61, 56];

        // Agregar logica para obtener periodos desde base de dato
        // Mientras obtendra información del mes actual y anterior
        
        // Ultimos dos meses
        // int periodoDTE;
        // periodoDTE = DateTime.Now.Year * 100 + DateTime.Now.Month;

        // Obtener información por periodo/documento y guardar datos en BD
        respuesta.Clear();
        foreach (var tipo in tiposDTE) {
            var RecuperaLibro = await GetBookVenta(credencial, tipo, periodoDTE);
            respuesta.AddRange(RecuperaLibro);
            await Task.Delay(500);
        }
        return respuesta;
    }
    public async Task<List<BookCompra>> GetBookCompraAsync(credencialSII credencial, int periodoDTE)
    {
        List<BookCompra> respuesta = new List<BookCompra>();
        // int periodoDTE;
        // peridoDTE = DateTime.Now.Year * 100 + DateTime.Now.Month;
        int[] tiposDTE = { 33, 34, 61, 56 };
        string[] estadoDocumento = { "REGISTRO", "PENDIENTE" };
        foreach (var tipoDTE in tiposDTE)
        {
            var libroCompraSII = await GetBookCompra(credencial, periodoDTE, tipoDTE, estadoDocumento[0]);
            await Task.Delay(500);
            if (libroCompraSII.Count > 0)
            {
                respuesta.AddRange(libroCompraSII);
            }

            libroCompraSII = await GetBookCompra(credencial, periodoDTE, tipoDTE, estadoDocumento[1]);
            await Task.Delay(500);
            if (libroCompraSII.Count > 0)
            {
                respuesta.AddRange(libroCompraSII);
            }
        }
        return respuesta;
    }
    private async Task<List<BookVenta>> GetBookVenta(credencialSII credencial, int tipoDTE, int periodoDTE)
    {
        List<BookVenta> respuesta = new();
        RootResponseVta responseSII = new();

        string url = "https://www4.sii.cl/consdcvinternetui/services/data/facadeService/getDetalleVenta";

        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Cookie", credencial.cookie);
            request.Headers.Add("Origin", "https://www4.sii.cl");

            var requestData = new BookResponse
            {
                metaData = new BookResponse.MetaData
                {
                    Namespace = "cl.sii.sdi.lob.diii.consdcv.data.api.interfaces.FacadeService/getDetalleVenta",
                    conversationId = credencial.conversationId,
                    transactionId = credencial.transactionId,
                    page = null
                },
                data = new BookResponse.Data
                {
                    rutEmisor = credencial.rut.ToString(),
                    dvEmisor = credencial.DV,
                    ptributario = periodoDTE.ToString(),
                    codTipoDoc = tipoDTE.ToString(),
                    operacion = "",
                    estadoContab = ""
                }
            };

            string jsonData = JsonConvert.SerializeObject(requestData);
            jsonData = jsonData.Replace("Namespace", "namespace");
            var content = new StringContent(jsonData, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var bodyResponse = await response.Content.ReadAsStringAsync();
            responseSII = JsonConvert.DeserializeObject<RootResponseVta>(bodyResponse)!;

            if (responseSII.data!.Count > 0)
            {
                foreach (var ventas in responseSII.data)
                {
                    BookVenta detalle = new ();
                    //detalle.fechaConsulta = DateTime.Now;
                    detalle = ventas;
                    respuesta.Add(detalle);
                    respuesta.ForEach( x => x.detRutEmisor = credencial.rutConDV);
                };
            }

            request = null;
            response.Dispose();
            response = null;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return respuesta;
    }
    private async Task<List<BookCompra>> GetBookCompra(credencialSII credencial, int periodo, int tipoDTE, string estadoDocumento)
    {
        List<BookCompra> respuesta = new List<BookCompra>();
        RootResponseCompra responseSII = new RootResponseCompra();

        //estadoContab = REGISTRO (documetos aceptados)
        //estadoContab = PENDIENTE (aun pendiente de aceptar)

        string url = "https://www4.sii.cl/consdcvinternetui/services/data/facadeService/getDetalleCompra";

        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = true;
            request.Method = "POST";
            request.Accept = "application/json, text/plain, */*";
            request.ContentType = "application/json";
            request.Host = "www4.sii.cl";
            request.Headers.Add("Cookie", credencial.cookie);
            request.Headers.Add("x-dtpc", credencial.dtPC);

            String bodyData = String.Format(
                @"{{" + "\n" +
                @"    ""metaData"": {{" + "\n" +
                @"        ""namespace"": ""cl.sii.sdi.lob.diii.consdcv.data.api.interfaces.FacadeService/getDetalleCompra""," + "\n" +
                @"        ""conversationId"": ""{0}""," + "\n" +
                @"        ""transactionId"": ""{1}""," + "\n" +
                @"        ""page"": null" + "\n" +
                @"    }}," + "\n" +
                @"    ""data"": {{" + "\n" +
                @"        ""rutEmisor"": ""{2}""," + "\n" +
                @"        ""dvEmisor"": ""{3}""," + "\n" +
                @"        ""ptributario"": ""{4}""," + "\n" +
                @"        ""codTipoDoc"": ""{5}""," + "\n" +
                @"        ""operacion"": ""COMPRA""," + "\n" +
                @"        ""estadoContab"": ""{6}""" + "\n" +
                @"    }}" + "\n" +
                @"}}"
                , credencial.conversationId
                , credencial.transactionId
                , credencial.rut
                , credencial.DV
                , periodo
                , tipoDTE
                , estadoDocumento
                );

            byte[] bodyByte = Encoding.UTF8.GetBytes(bodyData);
            request.ContentLength = bodyByte.Length;

            Stream postStream = request.GetRequestStream();
            postStream.Write(bodyByte, 0, bodyByte.Length);
            postStream.Flush();
            postStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var bodyResponse = await reader.ReadToEndAsync();
                    responseSII = JsonConvert.DeserializeObject<RootResponseCompra>(bodyResponse)!;
                    //responseSII = System.Text.Json.JsonSerializer.Deserialize<RootResponseCompra>(bodyResponse)!;
                }
            }

            if (responseSII.data!.Count > 0)
            {
                foreach (var compras in responseSII.data)
                {
                    BookCompra detalle = new BookCompra();
                    detalle = compras;
                    respuesta.Add(detalle);
                    respuesta.ForEach( x => 
                        {
                            x.detRutReceptor = credencial.rutConDV;
                            x.detTipoDoc = tipoDTE.ToString();
                            x.detPcarga = periodo.ToString();
                            x.EstadoDocumento = estadoDocumento;
                        }
                    );
                };
            }

            request = null;
            response.Close();
            response = null;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return respuesta;

    }
}