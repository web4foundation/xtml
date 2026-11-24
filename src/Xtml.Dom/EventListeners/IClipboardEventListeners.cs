namespace Xtml.BrowserApis.EventListeners;

public interface IClipboardListeners
{
    /// <summary>
    /// An event fired whenever the user initiates a copy action.
    /// </summary>
    Action<Event.Clipboard>? OnCopy { set; }
    
    /// <summary>
    /// An event fired whenever the user initiates a cut action.
    /// </summary>
    Action<Event.Clipboard>? OnCut { set; }
    
    /// <summary>
    /// An event fired whenever the user initiates a paste action.
    /// </summary>
    Action<Event.Clipboard>? OnPaste { set; }
    
}