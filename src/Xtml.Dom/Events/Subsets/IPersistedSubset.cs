namespace Xtml.BrowserApis.Events.Subsets;

public interface IPersistedSubset : ISubset, IViewSubset
{
    new const string TRIM = "persisted";

    /// <summary>
    /// Indicates if the document is loading from a cache.
    /// </summary>
    bool Persisted { get; }
}