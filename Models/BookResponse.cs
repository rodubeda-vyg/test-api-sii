namespace vyg_api_sii.Models;
public class BookResponse
{
    public class MetaData
    {
        public string? Namespace { get; set; }
        public string? conversationId { get; set; }
        public string? transactionId { get; set; }
        public object? page { get; set; }
    }
    public class Data
    {
        public string? rutEmisor { get; set; }
        public string? dvEmisor { get; set; }
        public string? ptributario { get; set; }
        public string? codTipoDoc { get; set; }
        public string? operacion { get; set; }
        public string? estadoContab { get; set; }
    }
    public MetaData? metaData { get; set; }
    public Data? data { get; set; }
}
public class MetaData
{
    public string? conversationId { get; set; }
    public string? transactionId { get; set; }
}
public class RespEstado
{
    public int codRespuesta { get; set; }
}