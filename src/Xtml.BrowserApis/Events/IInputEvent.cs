using static Xtml.BrowserApis.Events.Aliases.Subsets;

namespace Xtml.BrowserApis.Events;

public interface IInputEvent<T>
    : IUIEvent, Data, DataTransfer, IsComposing
{
    /// <summary>
    /// Returns the type of change for editable content such as, for example, inserting, deleting, or formatting text.
    /// </summary>
    string InputType { get; }

    /// <summary>
    /// A reference to the object to which the event was originally dispatched.
    /// </summary>
    new EventTarget<T> Target { get; }
}
