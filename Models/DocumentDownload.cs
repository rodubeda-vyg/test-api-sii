namespace vyg_api_sii.Models;
public class DocumentDownload
{
    public bool Success { get; set; }
    public string? Description { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public string? Encoding { get; set; }
    public string? FileAsBase64 { get; set;}
}