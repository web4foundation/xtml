namespace Xtml.BrowserApis.EventListeners;

public interface IContentVisibilityEventListeners
{
    /// <summary>
    /// Fires on any element with content-visibility: auto set on it when it starts or stops being relevant to the user and skipping its contents.
    /// </summary>
    Action<Event.ContentVisibilityAutoStateChange>? OnContentVisibilityAutoStateChange { set; }
}