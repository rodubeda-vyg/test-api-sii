using Swashbuckle.AspNetCore.Annotations;
using vyg_api_sii.DTOs;
using vyg_api_sii.Models;
using vyg_api_sii.Services;

namespace vyg_api_sii.Endpoints;
public static class TransEndpoint
{
    public static RouteGroupBuilder MapTransEndpoints(
        this IEndpointRouteBuilder routes)
    {
        string[] EndpointTag = new[] { "Transfer Document" };
        var group = routes.MapGroup("/api/sii/transfer");
        
        group
            .MapPost(
                "/document",
                async (CesionDTO cederRequest, TransferService transferService) =>
                {
                    HefRespuesta resp = new HefRespuesta
                    {
                        Mensaje = "transfer/document"
                    };

                    try 
                    {
                        resp = transferService.GeneraCesion(cederRequest!);
                    }
                    catch (Exception err)
                    {
                        resp.EsCorrecto = false;
                        resp.Detalle = err.Message;
                        resp.Resultado = null;
                        return Results.BadRequest(resp);
                    }

                    await Task.CompletedTask;
                    return Results.Ok(resp) ;
                }
            )
            .WithMetadata(
                new SwaggerOperationAttribute { Tags = EndpointTag }
            );

        group
            .MapPost(
                "/trackid/status",
                async (StatusTrackIdDTO trackId, TransferService transferService) =>
                {
                    HefRespuesta resp = new HefRespuesta
                    {
                        Mensaje = "/transfer/trackid"
                    };

                    try 
                    {
                        resp = transferService.ConsultarTrackId(trackId!);
                    }
                    catch (Exception err)
                    {
                        resp.EsCorrecto = false;
                        resp.Detalle = err.Message;
                        resp.Resultado = null;
                        return Results.BadRequest(resp);
                    }

                    await Task.CompletedTask;
                    return Results.Ok(resp) ;
                }
            )
            .WithMetadata(
                new SwaggerOperationAttribute { Tags = EndpointTag }
            );

        return group;
    }
}
