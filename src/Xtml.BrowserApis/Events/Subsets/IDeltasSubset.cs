namespace Xtml.BrowserApis.Events.Subsets;

public interface IDeltasSubset : ISubset, IViewSubset
{
    new const string TRIM = "deltaX,deltaY,deltaZ,deltaMode";

    /// <summary>
    /// Returns a double representing the horizontal scroll amount.
    /// </summary>
    double DeltaX { get; }

    /// <summary>
    /// Returns a double representing the vertical scroll amount.
    /// </summary>
    double DeltaY { get; }

    /// <summary>
    /// Returns a double representing the scroll amount for the z-axis.
    /// </summary>
    double DeltaZ { get; }

    /// <summary>
    /// Returns an unsigned long representing the unit of the delta* values' scroll amount.
    /// </summary>
    DeltaMode DeltaMode { get; }
}