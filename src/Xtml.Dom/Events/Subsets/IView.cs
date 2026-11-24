namespace Xtml.BrowserApis.Events.Subsets;

public interface IViewSubset : ISubset
{
    const string TRIM = "view";

    /// <summary>
    /// Read-only property returns the WindowProxy object from which the event was generated. This is the Window object the event happened in.
    /// </summary>
    IWindow View { get; }
}