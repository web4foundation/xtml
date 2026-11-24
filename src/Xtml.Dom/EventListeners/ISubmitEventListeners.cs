namespace Xtml.BrowserApis.EventListeners;

public interface ISubmitEventListeners
{
    /// <summary>
    /// The submit event fires when a <form> is submitted.
    /// </summary>
    Action<Event.Submit>? OnSubmit { set; }
}