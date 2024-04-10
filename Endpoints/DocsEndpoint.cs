using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Http.Metadata;
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
        var group = routes.MapGroup("/api/sii/docs");
        
        // get xml from sii
        // get aec from sii
        

        group.MapPost(
            "/status",
            async (StatusCesionDTO documento, CesionService cederService) =>
            {
                HefRespuesta resp = new HefRespuesta
                {
                    Mensaje = "EstadoDocumento"
                };

                try 
                {
                    resp = cederService.ConsultarEstadoCesion(documento!);
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
            });

        return group;
    }
}
