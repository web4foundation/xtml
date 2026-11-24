namespace Xtml.BrowserApis.EventListeners;

public interface IFocusEventListeners
{
    /// <summary>
    /// Fired when an element has lost focus.
    /// </summary>
    Action<Event.Focus>? OnBlur { set; }
    
    /// <summary>
    /// Fired when an element has gained focus.
    /// </summary>
    Action<Event.Focus>? OnFocus { set; }
    
    /// <summary>
    /// Fired when an element has gained focus, after focus.
    /// </summary>
    Action<Event.Focus>? OnFocusIn { set; }
    
    /// <summary>
    /// Fired when an element has lost focus, after blur.
    /// </summary>
    Action<Event.Focus>? OnFocusOut { set; }
}