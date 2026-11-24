namespace Xtml.BrowserApis.Events.Subsets;

public interface IProgressSubset : ISubset, IViewSubset
{
    new const string TRIM = "lengthComputable,loaded,total";

    /// <summary>
    /// A boolean flag indicating if the ratio between the size of the data 
    /// already transmitted or processed (loaded), and the total size of the 
    /// data (total), is calculable. In other words, it tells if the 
    /// progress is measurable or not.
    /// </summary>
    bool LengthComputable { get; }

    /// <summary>
    /// A 64-bit unsigned integer indicating the size, in bytes, of the data 
    /// already transmitted or processed. The ratio can be calculated by 
    /// dividing ProgressEvent.total by the value of this property. When 
    /// downloading a resource using HTTP, this only counts the body of 
    /// the HTTP message, and doesn't include headers and other overhead. 
    /// Note that for compressed requests of unknown total size, loaded 
    /// might contain the size of the compressed, or decompressed, data, 
    /// depending on the browser. As of 2024, it contains the size of the 
    /// compressed data in Firefox, and the size of the uncompressed data 
    /// in Chrome.
    /// </summary>
    long Loaded { get; }

    /// <summary>
    /// A 64-bit unsigned integer indicating the total size, in bytes, 
    /// of the data being transmitted or processed. When downloading a 
    /// resource using HTTP, this value is taken from the Content-Length 
    /// response header. It only counts the body of the HTTP message, 
    /// and doesn't include headers and other overhead.
    /// </summary>
    long Total { get; }
}