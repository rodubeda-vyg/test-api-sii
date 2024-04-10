using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Http.Metadata;
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
        var group = routes.MapGroup("/api/sii/transfer");
        
        group.MapPost(
            "/document",
            async (CesionDTO cederRequest, CesionService cederService) =>
            {
                HefRespuesta resp = new HefRespuesta
                {
                    Mensaje = "transfer/document"
                };

                try 
                {
                    resp = cederService.GeneraCesion(cederRequest!);
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
            });

        group.MapPost(
            "/trackid/status",
            async (StatusTrackIdDTO trackId, CesionService cederService) =>
            {
                HefRespuesta resp = new HefRespuesta
                {
                    Mensaje = "/transfer/trackid"
                };

                try 
                {
                    resp = cederService.ConsultarTrackId(trackId!);
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
            });

        return group;
    }
}
