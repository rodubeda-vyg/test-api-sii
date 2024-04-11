using System.ComponentModel.DataAnnotations.Schema;

namespace vyg_api_sii.Models;

public class BookCompra
{
    public string? detRutReceptor { get; set; }
    public int detRutDoc { get; set; }
    public string? detDvDoc { get; set; }
    public string? detRznSoc { get; set; }
    public double detNroDoc { get; set; }
    public string? detFchDoc { get; set; }
    public string? detTipoDocRef { get; set; }
    public string? detFolioDocRef { get; set; }
    public string? detFecRecepcion { get; set; }
    [Column(TypeName = "decimal(18,0)")] 
    public decimal detMntExe { get; set; }
    [Column(TypeName = "decimal(18,0)")] 
    public decimal detMntNeto { get; set; }
    [Column(TypeName = "decimal(18,0)")] 
    public decimal detMntIVA { get; set; }
    [Column(TypeName = "decimal(18,0)")] 
    public decimal detMntTotal { get; set; }
}

public class RootResponseCompra
{
    public List<BookCompra>? data { get; set; } = new List<BookCompra>();
    public bool esDocPapel { get; set; }
    public object? dataCabecera { get; set; }
    public MetaData? metaData { get; set; }
    public RespEstado? respEstado { get; set; }
}