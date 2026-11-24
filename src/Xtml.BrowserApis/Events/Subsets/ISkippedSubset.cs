namespace Xtml.BrowserApis.Events.Subsets;

public interface ISkippedSubset : ISubset, IViewSubset
{
    new const string TRIM = "skipped";

    /// <summary>
    /// Returns true if the user agent is skipping the element's rendering, or false otherwise.
    /// </summary>
    bool Skipped { get; }
}