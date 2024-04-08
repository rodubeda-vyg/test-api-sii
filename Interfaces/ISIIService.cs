using vyg_api_sii.Models;

namespace vyg_api_sii.Interfaces;
public interface ISIIService
{
    HefRespuesta SII_Datos_Generales(string credenciales);
    HefRespuesta RecuperarDatosCertificado(string credenciales);
    HefRespuesta RecuperarEstadoTrackid(string token, string rutEmpresa, long trackid);
    HefRespuesta RecuperarEstadoCesion(string token, string rutEmpresa, string tipoDoc, string folioDoc, string idCesion="");
}
