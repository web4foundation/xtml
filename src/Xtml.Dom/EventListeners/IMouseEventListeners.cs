namespace Xtml.BrowserApis.EventListeners;

public interface IMouseEventListeners
{
    /// <summary>
    /// Fired when a non-primary pointing device button (e.g., any mouse button other than the left button) has been pressed and released on an element.
    /// </summary>
    Action<Event.Mouse>? OnAuxClick { set; }    

    /// <summary>
    /// Fired when a pointing device button (e.g., a mouse's primary button) is pressed and released on a single element.
    /// </summary>
    Action<Event.Mouse>? OnClick { set; }    

    /// <summary>
    /// Fired when the user attempts to open a context menu.
    /// </summary>
    Action<Event.Mouse>? OnContextMenu { set; }    

    /// <summary>
    /// Fired when a pointing device button (e.g., a mouse's primary button) is clicked twice on a single element.
    /// </summary>
    Action<Event.Mouse>? OnDblClick { set; }    

    /// <summary>
    /// Fired when a pointing device button is pressed on an element.
    /// </summary>
    Action<Event.Mouse>? OnMouseDown { set; }    

    /// <summary>
    /// Fired when a pointing device (usually a mouse) is moved over the element that has the listener attached.
    /// </summary>
    Action<Event.Mouse>? OnMouseEnter { set; }    

    /// <summary>
    /// Fired when the pointer of a pointing device (usually a mouse) is moved out of an element that has the listener attached to it.
    /// </summary>
    Action<Event.Mouse>? OnMouseLeave { set; }    

    /// <summary>
    /// Fired when a pointing device (usually a mouse) is moved while over an element.
    /// </summary>
    Action<Event.Mouse>? OnMouseMove { set; }    

    /// <summary>
    /// Fired when a pointing device (usually a mouse) is moved off the element to which the listener is attached or off one of its children.
    /// </summary>
    Action<Event.Mouse>? OnMouseOut { set; }    

    /// <summary>
    /// Fired when a pointing device is moved onto the element to which the listener is attached or onto one of its children.
    /// </summary>
    Action<Event.Mouse>? OnMouseOver { set; }    

    /// <summary>
    /// Fired when a pointing device button is released on an element.
    /// </summary>
    Action<Event.Mouse>? OnMouseUp { set; }
}