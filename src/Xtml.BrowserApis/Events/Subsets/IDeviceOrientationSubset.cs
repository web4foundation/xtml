namespace Xtml.BrowserApis.Events.Subsets;

public interface IDeviceOrientationSubset : ISubset, IViewSubset
{
    new const string TRIM = "absolute,alpha,beta,gamma";

    /// <summary>
    /// A boolean that indicates whether or not the device is providing 
    /// orientation data absolutely.
    /// </summary>
    bool Absolute { get; }

    /// <summary>
    /// A number representing the motion of the device around the z axis, 
    /// express in degrees with values ranging from 0 (inclusive) to 
    /// 360 (exclusive).
    /// </summary>
    double Alpha { get; }

    /// <summary>
    /// A number representing the motion of the device around the x axis, 
    /// express in degrees with values ranging from -180 (inclusive) to 
    /// 180 (exclusive). This represents a front to back motion of the 
    /// device.
    /// </summary>
    double Beta { get; }

    /// <summary>
    /// A number representing the motion of the device around the y axis, 
    /// express in degrees with values ranging from -90 (inclusive) to 
    /// 90 (exclusive). This represents a left to right motion of the 
    /// device.
    /// </summary>
    double Gamma { get; }
}