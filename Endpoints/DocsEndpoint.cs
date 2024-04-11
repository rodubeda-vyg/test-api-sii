using Swashbuckle.AspNetCore.Annotations;
using vyg_api_sii.DTOs;
using vyg_api_sii.Models;
using vyg_api_sii.Services;

namespace vyg_api_sii.Endpoints;
public static class DocsEndpoint
{
    public static RouteGroupBuilder MapDocsEndpoints(
        this IEndpointRouteBuilder routes)
    {
        string[] EndpointTag = new[] { "Documents" };
        var group = routes.MapGroup("/api/sii/document");
        
        group
            .MapPost(
                "/status",
                async (StatusCesionDTO documento, TransferService transferService) =>
                {
                    HefRespuesta resp = new HefRespuesta
                    {
                        Mensaje = "EstadoDocumento"
                    };

                    try 
                    {
                        resp = transferService.ConsultarEstadoCesion(documento!);
                    }
                    catch (Exception err)
                    {
                        resp.EsCorrecto = false;
                        resp.Detalle = err.Message;
                        resp.Resultado = null;
                        return Results.BadRequest(resp);
                    }

                    await Task.CompletedTask;
                    return Results.Ok(resp);
                }
            )
            .WithMetadata(
                new SwaggerOperationAttribute { Tags = EndpointTag }
            );

        // get xml from sii


        // get aec from sii
        group
            .MapPost(
                "/download/aec",
                async (DocumentAECDTO documento, SIIService siiService) =>
                {
                    DocumentDownload resp = new DocumentDownload();

                    try 
                    {
                        resp = await siiService.RecuperaDocumentosAecs_RecibidosNew(
                            documento.Credencial!,
                            documento.RutEmisor!,
                            documento.TipoDTE!,
                            documento.Folio!
                        );
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err);
                        return Results.BadRequest(resp);
                    }
                    return Results.Ok(resp);
                }
            )
            .WithMetadata(
                new SwaggerOperationAttribute { Tags = EndpointTag }
            );

                

        return group;
    }
}
