namespace Xtml.BrowserApis.EventListeners;

public interface ITouchEventListeners
{
    /// <summary>
    /// Fired when one or more touch points have been disrupted in an implementation-specific manner (for example, too many touch points are created).
    /// </summary>
    Action<Event.Touch>? OnTouchCancel { set; }
    
    /// <summary>
    /// Fired when one or more touch points are removed from the touch surface.
    /// </summary>
    Action<Event.Touch>? OnTouchEnd { set; }
    
    /// <summary>
    /// Fired when one or more touch points are moved along the touch surface.
    /// </summary>
    Action<Event.Touch>? OnTouchMove { set; }
    
    /// <summary>
    /// Fired when one or more touch points are placed on the touch surface.
    /// </summary>
    Action<Event.Touch>? OnTouchStart { set; }
}