namespace Xtml.BrowserApis.Events.Subsets;

public interface IAnglesSubset : ISubset, IViewSubset
{
    new const string TRIM = "altitudeAngle,azimuthAngle";

    /// <summary>
    /// Represents the angle between a transducer (a pointer or stylus) axis 
    /// and the X-Y plane of a device screen.
    /// </summary>
    double AltitudeAngle { get; }

    /// <summary>
    /// Represents the angle between the Y-Z plane and the plane containing 
    /// both the transducer (a pointer or stylus) axis and the Y axis.
    /// </summary>
    double AzimuthAngle { get; }
}