using Xtml.Runtime.Composers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xtml.Runtime;
using System.Diagnostics;
using System.IO.Pipelines;
using Xtml.Templating;
using Xtml.Templating.Composers;
using Xtml.Runtime.Assets;

namespace Xtml.WebSocket;

public static partial class Extensions
{
    /// <summary>
    /// Adds a RouteEndpoint for the specified pattern that establishes a 
    /// XTML connection enabling the handling of events and manipulation of the DOM.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="template">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="WindowBuilder"/> that can be used to listen to events or further customize the endpoint.</returns>
    public static WindowBuilder MapWindow(
        this WebApplication app,
        [StringSyntax("Route")] string pattern,
        Func<Html> template)
    {
        var applicationBuilder = app.UseWebSockets();
        var windowBuilder = new WindowBuilder(template);
        var group = app.MapGroup(pattern);

        group.Map("/", async httpContext =>
        {
            // TODO: Make ContentSecurityPolicy a part of config?
            // img-src *;                           No restrictions on image sources
            // style-src 'self' 'unsafe-inline';    Allows inline and same-origin styles
            // script-src-elem 'self';              Scripts are same-origin only (<script src="..."> and <script>...</script>)
            // script-src-attr 'unsafe-inline';     Inline event handlers are allowed (onclick="..." etc)
            httpContext.Response.Headers.ContentSecurityPolicy = "img-src *; style-src 'self' 'unsafe-inline'; script-src-elem 'self'; script-src-attr 'unsafe-inline';";
            httpContext.Response.ContentType = "text/html; charset=utf-8";

            var pipeWriter = httpContext.Response.BodyWriter;
            var composer = HtmlKeyComposer.Reuse(pipeWriter, windowBuilder);
            await httpContext.WriteAsync(composer, windowBuilder.Template);
        });

        group.Map("/ui.ws", async httpContext =>
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                var logger = app.Services.GetRequiredService<ILogger<Bridge>>();
                await Bridge.Bind(
                    httpContext,
                    windowBuilder,
                    logger,
                    app.Lifetime.ApplicationStopping
                );
            }
        });

        if (!applicationBuilder.Properties.TryGetValue("IS_XTML_MAPPED", out _))
        {
            applicationBuilder.Properties["IS_XTML_MAPPED"] = true;

            app.Map("/_app/websocket/ui.js", (HttpContext context) => {
                context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                WriteAsset(context, "text/javascript", AssetsHelper.GetJs(context.Request.Headers.AcceptEncoding.ToString()));
            });

            app.Map("/_app/base/ui.css", (HttpContext context) => {
                context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                WriteAsset(context, "text/css", AssetsHelper.GetCss(context.Request.Headers.AcceptEncoding.ToString()));
            });

            app.Map("/_app/websocket/sw.js", (HttpContext context) => {
                context.Response.Headers["Service-Worker-Allowed"] = "/";
                WriteAsset(context, "text/javascript", AssetsHelper.GetSw(context.Request.Headers.AcceptEncoding.ToString()));
            });

            app.Map("/_app/alive", async httpContext =>
            {
                if (httpContext.WebSockets.IsWebSocketRequest)
                    await httpContext.WebSockets.AcceptWebSocketAsync();
            });
        }

        return windowBuilder;
    }

    private static void WriteAsset(HttpContext context, string contentType, (byte[] Body, string? ContentEncoding) asset)
    {
        context.Response.ContentType = contentType;
        context.Response.Headers.Vary = "Accept-Encoding";
        if (asset.ContentEncoding is not null)
            context.Response.Headers.ContentEncoding = asset.ContentEncoding;
        context.Response.ContentLength = asset.Body.Length;
        context.Response.BodyWriter.Write(asset.Body);
    }

    private static ValueTask<FlushResult> WriteAsync<T>(
        this HttpContext httpContext,
        T composer,
        Func<Html> template,
        bool includeServerTiming = false) // TODO: Move `includeServerTiming` to Config
            where T : BaseComposer, IStreamingComposer
    {
        var pipeWriter = httpContext.Response.BodyWriter;
        if (!includeServerTiming)
        {
            pipeWriter.Write(composer, $"{template()}");
            return pipeWriter.FlushAsync(httpContext.RequestAborted);
        }
        else
        {
            long gc1 = GC.GetAllocatedBytesForCurrentThread();
            long stopwatch = Stopwatch.GetTimestamp();

            pipeWriter.Write(composer, $"{template()}");

            var elapsed = Stopwatch.GetElapsedTime(stopwatch);
            long gc2 = GC.GetAllocatedBytesForCurrentThread();

            // This allocates.  Boo!  But it occurs after measurement.
            httpContext.Response.Headers["Server-Timing"] = $"""
                allocations;desc="Allocations: {gc2 - gc1}b", render;desc="XTML.Render";dur={elapsed.TotalNanoseconds / 1_000_000d}
                """;

            return pipeWriter.FlushAsync(httpContext.RequestAborted);
        }
    }
}
