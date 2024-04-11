using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vyg_api_sii.Models;

public class BookVenta
{
    public int detTipoDoc { get; set; }
    public string? detRutEmisor { get; set; }
    public int detRutDoc  { get; set; }
    public string? detDvDoc { get; set; }
    public string? detRznSoc { get; set; }
    public int detNroDoc { get; set; }
    public string? detFchDoc { get; set; }
    public string? detFecAcuse { get; set; }
    public string? detFecRecepcion { get; set; }
    public decimal detMntExe { get; set; }
    public decimal detMntNeto { get; set; }
    public decimal detMntIVA { get; set; }
    public decimal detMntTotal { get; set; }
    public decimal detTasaImp { get; set; }
    public string? detEventoReceptorLeyenda { get; set; }
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
