namespace Xtml.BrowserApis.EventListeners;

public interface IKeyboardEventListeners
{
    /// <summary>
    /// A key has been pressed.
    /// </summary>
    Action<Event.Keyboard>? OnKeyDown { set; }
    
    /// <summary>
    /// A key has been released.
    /// </summary>
    Action<Event.Keyboard>? OnKeyUp { set; }
}
