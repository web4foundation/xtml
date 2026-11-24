namespace Xtml.BrowserApis.Events.Subsets;

public interface IXYSubset : ISubset, IViewSubset
{
    new const string TRIM = "x,y";

    /// <summary>
    /// Alias for MouseEvent.clientX.
    /// </summary>
    double X { get; }

    /// <summary>
    /// Alias for MouseEvent.clientY.
    /// </summary>
    double Y { get; }
}