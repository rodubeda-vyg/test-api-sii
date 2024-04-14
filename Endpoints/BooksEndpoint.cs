using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Metadata;
using Swashbuckle.AspNetCore.Annotations;
using vyg_api_sii.DTOs;
using vyg_api_sii.Models;
using vyg_api_sii.Services;

namespace vyg_api_sii.Endpoints;
public static class BooksEndpoint
{
    public static RouteGroupBuilder MapBooksEndpoints(
        this IEndpointRouteBuilder routes)
    {
        string[] EndpointTag = new[] { "Books" };
        var group = routes.MapGroup("/api/sii/books");
        
        // get xml from sii
        // get aec from sii
        

        group
            .MapPost(
                "/venta",
                async (BookDTO credencial, BooksService booksService) =>
                {
                    List<BookVenta> resp = new List<BookVenta>();
                    Match matchToken = Regex.Match(credencial.Cookie!, @"TOKEN=([\w\d]+);");
                    string tokenSII = string.Empty;
                    if (matchToken.Success) { tokenSII = matchToken.Groups[1].Value; }

                    if (string.IsNullOrEmpty(tokenSII))
                        return Results.BadRequest("No se ha encontrado el token de autenticación");
                    
                    credencialSII credencialSII = new credencialSII
                    {
                        rutConDV = credencial.Rut!,
                        rut = int.Parse(credencial.Rut!.Substring(0, credencial.Rut!.Length - 2)),
                        DV = credencial.Rut!.Substring(credencial.Rut!.Length - 1),
                        cookie = credencial.Cookie!,
                        token = tokenSII,
                        conversationId = tokenSII,
                        transactionId = Guid.NewGuid().ToString(),
                        dtPC = "20$69871136_496h103vSNCSJEFFRMOVPUCHRDPRHDHKFUODMPSB-0e0"
                    };

                    try 
                    {
                        resp = await booksService.GetBookVentaAsync(credencialSII, credencial.Periodo);
                    }
                    catch (Exception err)
                    {
                        return Results.BadRequest(err.Message);
                    }

                    return Results.Ok(resp);
                }
            )
            .WithMetadata(new SwaggerOperationAttribute { Tags = EndpointTag }
            );

        group
            .MapPost(
                "/compra",
                async (BookDTO credencial, BooksService booksService) =>
                {
                    List<BookCompra> resp = new List<BookCompra>();
                    Match matchToken = Regex.Match(credencial.Cookie!, @"TOKEN=([\w\d]+);");
                    string tokenSII = string.Empty;
                    if (matchToken.Success) { tokenSII = matchToken.Groups[1].Value; }

                    if (string.IsNullOrEmpty(tokenSII))
                        return Results.BadRequest("No se ha encontrado el token de autenticación");
                    
                    credencialSII credencialSII = new credencialSII
                    {
                        rutConDV = credencial.Rut!,
                        rut = int.Parse(credencial.Rut!.Substring(0, credencial.Rut!.Length - 2)),
                        DV = credencial.Rut!.Substring(credencial.Rut!.Length - 1),
                        cookie = credencial.Cookie!,
                        token = tokenSII,
                        conversationId = tokenSII,
                        transactionId = Guid.NewGuid().ToString(),
                        dtPC = "20$69871136_496h103vSNCSJEFFRMOVPUCHRDPRHDHKFUODMPSB-0e0"
                    };

                    try 
                    {
                        resp = await booksService.GetBookCompraAsync(credencialSII, credencial.Periodo);
                    }
                    catch (Exception err)
                    {
                        return Results.BadRequest(err.Message);
                    }

                    return Results.Ok(resp);
                }
            )
            .WithMetadata(new SwaggerOperationAttribute { Tags = EndpointTag }
            );

        return group;
    }
}
