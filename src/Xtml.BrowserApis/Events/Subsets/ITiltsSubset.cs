namespace Xtml.BrowserApis.Events.Subsets;

public interface ITiltsSubset : ISubset, IViewSubset
{
    new const string TRIM = "tiltX,tiltY";

    /// <summary>
    /// The plane angle (in degrees, in the range of -90 to 90) between 
    /// the Y–Z plane and the plane containing both the pointer 
    /// (e.g. pen stylus) axis and the Y axis.
    /// </summary>
    double TiltX { get; }

    /// <summary>
    /// The plane angle (in degrees, in the range of -90 to 90) between 
    /// the X–Z plane and the plane containing both the pointer 
    /// (e.g. pen stylus) axis and the X axis.
    /// </summary>
    double TiltY { get; }
}