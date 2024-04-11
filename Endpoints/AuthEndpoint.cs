using System.Security.Cryptography.X509Certificates;
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
        string[] EndpointTag = new[] { "Authentication" };
        var group = routes.MapGroup("/api/sii/auth");
        
        group
            .MapPost(
                "/simple",
                async (AuthSimpleDTO credencial, AuthService authService) =>
                {
                    credencialSII resp = new credencialSII(){
                        rutConDV = credencial.Rut!,
                        rut = int.Parse(credencial.Rut!.Substring(0, credencial.Rut!.Length - 2)),
                        DV = credencial.Rut!.Substring(credencial.Rut!.Length - 1),
                        claveSII = credencial.Psswrd!
                    };

                    try 
                    {
                        resp = await authService.GetTokenSimple(resp);
                    }
                    catch (Exception err)
                    {
                        return Results.BadRequest(err.Message);
                    }
                    return Results.Ok(resp) ;
                }
            )
            .WithMetadata(
                new SwaggerOperationAttribute { Tags = EndpointTag }
            );

        group
            .MapPost(
                "/cert",
                (AuthCertDTO credencial, AuthService authService) =>
                {
                    HefRespuesta resp = new HefRespuesta
                    {
                        Mensaje = "auth cert"
                    };

                    byte[] certificado = Convert.FromBase64String(credencial.CertificadoBase64!);
                    string password = credencial.Psswrd!;
                    var cert = authService.RecuperarCertificado(certificado, password);
                    if (!cert.EsCorrecto)
                        return Results.BadRequest(resp);
                    
                    X509Certificate2? getCertificado = (X509Certificate2)cert?.Resultado!;

                    try 
                    {
                        resp = authService.GetTokenCert(getCertificado);
                    }
                    catch (Exception err)
                    {
                        resp.EsCorrecto = false;
                        resp.Detalle = err.Message;
                        resp.Resultado = null;
                        return Results.BadRequest(resp);
                    }
                    return Results.Ok(resp) ;
                }
            )
            .WithMetadata(new SwaggerOperationAttribute { Tags = EndpointTag }
            );
            
        return group;
    }
}
