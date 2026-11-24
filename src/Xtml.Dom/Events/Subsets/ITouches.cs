namespace Xtml.BrowserApis.Events.Subsets;

public interface ITouchesSubset : ISubset, IViewSubset
{
    new const string TRIM = "changedTouches,targetTouches,touches";

    /// <summary>
    /// A TouchList of all the Touch objects representing individual points of contact 
    /// whose states changed between the previous touch event and this one.
    /// </summary>
    TouchPoint[] ChangedTouches { get; }

    /// <summary>
    /// A TouchList of all the Touch objects that are both currently in contact with 
    /// the touch surface and were also started on the same element that is the target 
    /// of the event.
    /// </summary>
    TouchPoint[] TargetTouches { get; }

    /// <summary>
    /// A TouchList of all the Touch objects representing all current points of 
    /// contact with the surface, regardless of target or changed status.
    /// </summary>
    TouchPoint[] Touches { get; }
}