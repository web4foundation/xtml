namespace Xtml.BrowserApis.Events.Subsets;

public interface IDataSubset : ISubset, IViewSubset
{
    new const string TRIM = "data";

    /// <summary>
    /// Returns a string with the inserted characters. This may be an empty string if the change doesn't insert text (for example, when deleting characters).
    /// </summary>
    string Data { get; }
}