namespace Xtml.BrowserApis.EventListeners;

public interface IDocumentEventListeners
{
    /// <summary>
    /// Returns loading status of the document.
    /// </summary>
    Action<Event>? OnReadStateChange { set; }

    /// <summary>
    /// Fired when the current text selection on a document is changed.
    /// </summary>
    Action<Event>? OnSelectionChange { set; }

    /// <summary>
    /// Fired when the content of a tab has become visible or has been hidden.
    /// </summary>
    Action<Event>? OnVisibilityChange { set; }
}