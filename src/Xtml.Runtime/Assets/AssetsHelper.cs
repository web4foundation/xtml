using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Xtml.Runtime.Assets;

public static class AssetsHelper
{
    private static readonly Lazy<byte[]> Js = new(() => Load("ui.js"));
    private static readonly Lazy<byte[]> JsGzip = new(() => CompressGZip(Js.Value));
    private static readonly Lazy<byte[]> JsBr = new(() => CompressBrotli(Js.Value));

    private static readonly Lazy<byte[]> Css = new(() => Load("ui.css"));
    private static readonly Lazy<byte[]> CssGzip = new(() => CompressGZip(Css.Value));
    private static readonly Lazy<byte[]> CssBr = new(() => CompressBrotli(Css.Value));

    private static readonly Lazy<byte[]> Sw = new(() => Load("sw.js"));
    private static readonly Lazy<byte[]> SwGzip = new(() => CompressGZip(Sw.Value));
    private static readonly Lazy<byte[]> SwBr = new(() => CompressBrotli(Sw.Value));

    public static byte[] JS => Js.Value;
    public static byte[] JS_GZIP => JsGzip.Value;
    public static byte[] JS_BR => JsBr.Value;

    public static byte[] CSS => Css.Value;
    public static byte[] CSS_GZIP => CssGzip.Value;
    public static byte[] CSS_BR => CssBr.Value;

    public static byte[] SW => Sw.Value;
    public static byte[] SW_GZIP => SwGzip.Value;
    public static byte[] SW_BR => SwBr.Value;

    public static (byte[] Body, string? ContentEncoding) GetJs(string? acceptEncoding)
        => acceptEncoding switch
        {
            string s when s.Contains("br") => (JS_BR, "br"),
            string s when s.Contains("gzip") => (JS_GZIP, "gzip"),
            _ => (JS, null),
        };

    public static (byte[] Body, string? ContentEncoding) GetCss(string? acceptEncoding)
        => acceptEncoding switch
        {
            string s when s.Contains("br") => (CSS_BR, "br"),
            string s when s.Contains("gzip") => (CSS_GZIP, "gzip"),
            _ => (CSS, null),
        };

    public static (byte[] Body, string? ContentEncoding) GetSw(string? acceptEncoding)
        => acceptEncoding switch
        {
            string s when s.Contains("br") => (SW_BR, "br"),
            string s when s.Contains("gzip") => (SW_GZIP, "gzip"),
            _ => (SW, null),
        };

    private static byte[] Load(string resourceName) =>
        Encoding.UTF8.GetBytes(new StreamReader(typeof(AssetsHelper).Assembly
            .GetManifestResourceStream($"{typeof(AssetsHelper).Assembly.GetName().Name}.Assets.{resourceName}")!
        ).ReadToEnd());

    private static byte[] CompressGZip(byte[] data)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize))
            gzip.Write(data);
        return output.ToArray();
    }

    private static byte[] CompressBrotli(byte[] data)
    {
        using var output = new MemoryStream();
        using (var brotli = new BrotliStream(output, CompressionLevel.SmallestSize))
            brotli.Write(data);
        return output.ToArray();
    }
}