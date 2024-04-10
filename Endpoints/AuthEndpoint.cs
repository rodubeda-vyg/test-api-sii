using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Http.Metadata;
using Swashbuckle.AspNetCore.Annotations;
using vyg_api_sii.DTOs;
using vyg_api_sii.Models;
using vyg_api_sii.Services;

namespace vyg_api_sii.Endpoints;
public static class AuthEndpoint
{
    public static RouteGroupBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/sii/auth");
        
        group.MapPost(
            "/simple",
            async (CesionDTO cederRequest, CesionService cederService) =>
            {
                HefRespuesta resp = new HefRespuesta
                {
                    Mensaje = "Ceder"
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
            "/cert",
            async (StatusTrackIdDTO trackId, CesionService cederService) =>
            {
                HefRespuesta resp = new HefRespuesta
                {
                    Mensaje = "TrackId"
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
