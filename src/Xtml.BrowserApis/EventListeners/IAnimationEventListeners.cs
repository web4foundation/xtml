namespace Xtml.BrowserApis.EventListeners;

public interface IAnimationEventListeners
{
    /// <summary>
    /// Fired when an animation unexpectedly aborts.
    /// </summary>
    Action<Event.Animation>? OnAnimationCancel { set; }
    
    /// <summary>
    /// Fired when an animation has completed normally.
    /// </summary>
    Action<Event.Animation>? OnAnimationEnd { set; }
    
    /// <summary>
    /// Fired when an animation iteration has completed.
    /// </summary>
    Action<Event.Animation>? OnAnimationIteration { set; }
    
    /// <summary>
    /// Fired when an animation starts.
    /// </summary>
    Action<Event.Animation>? OnAnimationStart { set; }
}