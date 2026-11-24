namespace Xtml.BrowserApis.Events.Subsets;

public interface IMovementXYSubset : ISubset, IViewSubset
{
    new const string TRIM = "movementX,movementY";

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the position of the last mousemove event.
    /// </summary>
    double MovementX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the position of the last mousemove event.
    /// </summary>
    double MovementY { get; }
}