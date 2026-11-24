namespace Xtml.BrowserApis.Events.Subsets;

public interface IDeviceMotionSubset : ISubset, IViewSubset
{
    new const string TRIM = "acceleration,acceleratinIncludingGravity,rotationRate,interval";

    /// <summary>
    /// An object giving the acceleration of the device on the three axis X, Y and Z. 
    /// Acceleration is expressed in m/s².
    /// </summary>
    XYZ Acceleration { get; }

    /// <summary>
    /// An object giving the acceleration of the device on the three axis X, Y and Z 
    /// with the effect of gravity. Acceleration is expressed in m/s².
    /// </summary>
    XYZ AccelerationIncludingGravity { get; }

    /// <summary>
    /// An object giving the rate of change of the device's orientation on the three 
    /// orientation axis alpha, beta and gamma. Rotation rate is expressed in 
    /// degrees per seconds.
    /// </summary>
    ABG RotationRate { get; }

    /// <summary>
    /// A number representing the interval of time, in milliseconds, at which data 
    /// is obtained from the device.
    /// </summary>
    double Interval { get; }
}