namespace Xtml.BrowserApis.Events.Subsets;

public interface IDetailSubset : ISubset, IViewSubset
{
    new const string TRIM = "detail";

    /// <summary>
    /// Returns a long with details about the event, depending on the event type.
    /// </summary>
    long Detail { get; }
}