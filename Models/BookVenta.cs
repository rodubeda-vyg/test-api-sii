using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace vyg_api_sii.Models;
public class BookVenta
{
    [JsonPropertyName("rutEmisor")]
    public string? detRutEmisor { get; set; }
    [JsonPropertyName("rutReceptor")]
    public int detRutDoc  { get; set; }
    [JsonPropertyName("DVReceptor")]
    public string? detDvDoc { get; set; }
    [JsonPropertyName("razonSocialReceptor")]
    public string? detRznSoc { get; set; }
    [JsonPropertyName("tipoDTE")]
    public int detTipoDoc { get; set; }
    [JsonPropertyName("folio")]
    public int detNroDoc { get; set; }
    [JsonPropertyName("tipoDTEReferencia")]
    public string? detTipoDocRef { get; set; }
    [JsonPropertyName("folioReferencia")]
    public string? detFolioDocRef { get; set; }

    [JsonPropertyName("fechaDocumento")]
    public string? detFchDoc { get; set; }
    [JsonPropertyName("fechaAcuseRecibo")]
    public string? detFecAcuse { get; set; }
    [JsonPropertyName("fechaRecepcion")]
    public string? detFecRecepcion { get; set; }
    [JsonPropertyName("montoExento")]
    [Column(TypeName = "decimal(18,0)")] 
    public decimal detMntExe { get; set; }
    [JsonPropertyName("montoNeto")]
    [Column(TypeName = "decimal(18,0)")] 
    public decimal detMntNeto { get; set; }
    [JsonPropertyName("TasaImpuesto")]
    [Column(TypeName = "decimal(18,0)")] 
    public decimal detTasaImp { get; set; }
    [JsonPropertyName("IVA")]
    [Column(TypeName = "decimal(18,0)")] 
    public decimal detMntIVA { get; set; }
    [JsonPropertyName("montoTotal")]
    [Column(TypeName = "decimal(18,0)")] 
    public decimal detMntTotal { get; set; }
    [JsonPropertyName("descripcionEventoReceptor")]
    public string? detEventoReceptorLeyenda { get; set; }
    [JsonPropertyName("periodoCarga")]
    public int detPcarga { get; set; }
}
public class RootResponseVta
{
    public List<BookVenta>? data { get; set; } = new List<BookVenta>();
    public bool esDocPapel { get; set; }
    public object? dataCabecera { get; set; }
    public MetaData? metaData { get; set; }
    public RespEstado? respEstado { get; set; }
}