using System.Xml.Serialization;
using FastEndpoints;

namespace Frontline.Extensions;

public static class EndpointExtensions
{
    public static async Task SendXmlAsync<TResponse>(this IEndpoint ep,
        TResponse response,
        int statusCode = 200,
        string contentType = "application/xml",
        CancellationToken cancellationToken = default)
    {
        ep.HttpContext.MarkResponseStart();
        ep.HttpContext.Response.StatusCode = statusCode;
        ep.HttpContext.Response.ContentType = contentType;
        var xmlSerializer = new XmlSerializer(typeof(TResponse));
        using var stream = new MemoryStream();
        xmlSerializer.Serialize(stream, response);
        stream.Position = 0;
        await stream.CopyToAsync(ep.HttpContext.Response.Body, cancellationToken);
    }
}