namespace vyg_api_sii.Models;
public class HefRespuesta
{
    public string? Fecha { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    public bool EsCorrecto { get; set; }
    public string? Mensaje { get; set; }
    public string? Detalle { get; set; }
    public string? CodigoSII { get; set; } = null;
    public string? Trackid { get; set; } = null;
    public object? Resultado { get; set; }
}

public enum HefOrigen
{
    Emitidos = 0,
    Recibidos = 1
}    

public class credencialSII
{
    public int rut { get; set; }
    public string DV { get; set; } = string.Empty;
    public string rutConDV { get; set; } = string.Empty;
    public string claveSII { get; set; } = string.Empty;
    public string cookie { get; set; } = string.Empty;
    public string token { get; set; } = string.Empty;
    public string conversationId { get; set; } = string.Empty;
    public string transactionId { get; set; } = string.Empty;
    public string dtPC { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
}
