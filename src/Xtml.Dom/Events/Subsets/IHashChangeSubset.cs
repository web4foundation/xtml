namespace Xtml.BrowserApis.Events.Subsets;

public interface IHashChangeSubset : ISubset, IViewSubset
{
    new const string TRIM = "newUrl,oldUrl";

    /// <summary>
    /// The new URL to which the window is navigating.
    /// </summary>
    string NewUrl { get; }

    /// <summary>
    /// The previous URL from which the window was navigated.
    /// </summary>
    string OldUrl { get; }

}