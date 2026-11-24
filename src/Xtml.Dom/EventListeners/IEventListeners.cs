namespace Xtml.BrowserApis.EventListeners;

public interface IEventListeners
{
    /// <summary>
    /// Fired when the value of an input element is about to be modified.
    /// </summary>
    Action<Event>? OnBeforeInput { set; }

    /// <summary>
    /// Fires when an element's value is changed as a direct result of a user action.
    /// </summary>
    Action<Event>? OnInput { set; }

    /// <summary>
    /// Fired when a Content Security Policy is violated.
    /// </summary>
    Action<Event>? OnSecurityPolicyViolation { set; }

    /// <summary>
    /// Fired when the value of an <input>, <select>, or <textarea> element has been changed and committed by the user. Unlike the input event, the change event is not necessarily fired for each alteration to an element's value.
    /// </summary>
    Action<Event>? OnChange { set; }
    
    /// <summary>
    /// Fires for elements containing a resource when the resource has successfully loaded.
    /// </summary>
    Action<Event>? OnLoad { set; }
    
    /// <summary>
    /// The reset event fires when a <form> is reset.
    /// </summary>
    Action<Event>? OnReset { set; }

    /// <summary>
    /// The cancel event fires on an <input> element when the user cancels the file picker dialog via the Esc key or the cancel button and when the user re-selects the same files that were previously selected of type="file".
    /// </summary>
    Action<Event>? OnCancel { set; }
    
    /// <summary>
    /// Fired when an element does not satisfy its constraints during constraint validation.
    /// </summary>
    Action<Event>? OnInvalid { set; }

    /// <summary>
    /// Fired when some text has been selected.
    /// </summary>
    Action<Event>? OnSelect { set; }

    /// <summary>
    /// The slotchange event is fired on an HTMLSlotElement instance (<slot> element) when the node(s) contained in that slot change.
    /// </summary>
    Action<Event>? OnSlotChange { set; }

    /// <summary>
    /// The selectstart event of the Selection API is fired when a user starts a new selection.
    /// </summary>
    Action<Event>? OnSelectStart { set; }

    /// <summary>
    /// The cuechange event fires when a TextTrack has changed the currently displaying cues. The event is fired on both the TextTrack and the HTMLTrackElement in which it's being presented, if any.
    /// </summary>
    Action<Event>? OnCueChange { set; }
}