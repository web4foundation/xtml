namespace Xtml.BrowserApis.EventListeners;

public interface IFormDataEventListeners
{
    /// <summary>
    /// The formdata event fires after the entry list representing the form's data is constructed. This happens when the form is submitted, but can also be triggered by the invocation of a FormData() constructor.
    /// </summary>
    Action<Event.FormData>? OnFormData { set; }
}