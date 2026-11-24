namespace Xtml.BrowserApis.Events.Subsets;

public interface IScreenXYSubset : ISubset, IViewSubset
{
    new const string TRIM = "screenX,screenY";

    /// <summary>
    /// The X coordinate of the mouse pointer in screen coordinates.
    /// </summary>
    double ScreenX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer in screen coordinates.
    /// </summary>
    double ScreenY { get; }
}