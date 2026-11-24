namespace Xtml.BrowserApis.EventListeners;

public interface IToggleEventListeners
{
    /// <summary>
    /// Fired when the element is a popover or <dialog>, before it is hidden or shown.
    /// </summary>
    Action<Event.Toggle>? OnBeforeToggle { set; }
    
    /// <summary>
    /// Fired when the element is a popover, <dialog>, or <details> element, just after it is hidden or shown.
    /// </summary>
    Action<Event.Toggle>? OnToggle { set; }
}