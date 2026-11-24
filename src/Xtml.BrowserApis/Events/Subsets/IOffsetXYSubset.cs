namespace Xtml.BrowserApis.Events.Subsets;

public interface IOffsetXYSubset : ISubset, IViewSubset
{
    new const string TRIM = "offsetX,offsetY";
    
    /// <summary>
    /// The X coordinate of the mouse pointer relative to the position of the padding edge of the target node.
    /// </summary>
    double OffsetX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the position of the padding edge of the target node.
    /// </summary>
    double OffsetY { get; }
}