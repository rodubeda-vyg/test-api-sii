using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using vyg_api_sii.Extensions;

namespace vyg_api_sii.Models;
public class BookCompra
{
    public string? EstadoDocumento { get; set; }
    [JsonPropertyName("rutReceptor")]
    public string? detRutReceptor { get; set; }
    [JsonPropertyName("rutEmisor")]
    public int detRutDoc { get; set; }
    [JsonPropertyName("DVEmisor")]
    public string? detDvDoc { get; set; }
    [JsonPropertyName("razonSocialEmisor")]
    public string? detRznSoc { get; set; }
    [JsonPropertyName("tipoDTE")]
    public string? detTipoDoc { get; set; }
    [JsonPropertyName("folio")]
    public string? detNroDoc { get; set; }
    [JsonPropertyName("tipoDTEReferencia")]
    public string? detTipoDocRef { get; set; }
    [JsonPropertyName("folioReferencia")]
    public string? detFolioDocRef { get; set; }
    [JsonPropertyName("fechaDocumento")]
    public string? detFchDoc { get; set; }
    [JsonPropertyName("fechaRecepcion")]
    public string? detFecRecepcion { get; set; }
    [JsonPropertyName("montoExento")]
    [Column(TypeName = "decimal(18,0)")] 
    public decimal detMntExe { get; set; }
    [JsonPropertyName("montoNeto")]
    [Column(TypeName = "decimal(18,0)")] 
    public decimal detMntNeto { get; set; }
    [JsonPropertyName("IVA")]
    [Column(TypeName = "decimal(18,0)")] 
    public decimal detMntIVA { get; set; }
    [JsonPropertyName("montoTotal")]
    [Column(TypeName = "decimal(18,0)")] 
    public decimal detMntTotal { get; set; }
    [JsonPropertyName("periodoCarga")]
    public string? detPcarga { get; set; }
}
public class RootResponseCompra
{
    public List<BookCompra>? data { get; set; } = new List<BookCompra>();
    public bool esDocPapel { get; set; }
    public object? dataCabecera { get; set; }
    public MetaData? metaData { get; set; }
    public RespEstado? respEstado { get; set; }
}