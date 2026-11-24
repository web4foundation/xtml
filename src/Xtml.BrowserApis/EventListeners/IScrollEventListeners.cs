namespace Xtml.BrowserApis.EventListeners;

public interface IScrollEventListeners
{
    /// <summary>
    /// Fired when the document view or an element has been scrolled.
    /// </summary>
    Action<Event>? OnScroll { set; }
    
    /// <summary>
    /// Fires when the document view has completed scrolling.
    /// </summary>
    Action<Event>? OnScrollEnd { set; }
}