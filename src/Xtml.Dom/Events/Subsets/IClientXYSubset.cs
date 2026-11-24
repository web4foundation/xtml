namespace Xtml.BrowserApis.Events.Subsets;

public interface IClientXYSubset : ISubset, IViewSubset
{
    new const string TRIM = "clientX,clientY";

    /// <summary>
    /// The X coordinate of the mouse pointer in viewport coordinates.
    /// </summary>
    double ClientX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer in viewport coordinates.
    /// </summary>
    double ClientY { get; }
}