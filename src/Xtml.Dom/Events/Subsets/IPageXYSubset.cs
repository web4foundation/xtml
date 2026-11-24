namespace Xtml.BrowserApis.Events.Subsets;

public interface IPageXYSubset : ISubset, IViewSubset
{
    new const string TRIM = "pageX,pageY";

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the whole document.
    /// </summary>
    double PageX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the whole document.
    /// </summary>
    double PageY { get; }
}