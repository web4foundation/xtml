namespace Xtml.BrowserApis.EventListeners;

public interface IPointerEventListeners
{
    /// <summary>
    /// Fired when an element captures a pointer using setPointerCapture().
    /// </summary>
    Action<Event.Pointer>? OnGotPointerCapture { set; }    

    /// <summary>
    /// Fired when a captured pointer is released.


    /// </summary>
    Action<Event.Pointer>? OnLostPointerCapture { set; }    

    /// <summary>
    /// Fired when a pointer event is canceled.
    /// </summary>
    Action<Event.Pointer>? OnPointerCancel { set; }    

    /// <summary>
    /// Fired when a pointer becomes active.
    /// </summary>
    Action<Event.Pointer>? OnPointerDown { set; }    

    /// <summary>
    /// Fired when a pointer is moved into the hit test boundaries of an element or one of its descendants.
    /// </summary>
    Action<Event.Pointer>? OnPointerEnter { set; }    

    /// <summary>
    /// Fired when a pointer is moved out of the hit test boundaries of an element.
    /// </summary>
    Action<Event.Pointer>? OnPointerLeave { set; }    

    /// <summary>
    /// Fired when a pointer changes coordinates.
    /// </summary>
    Action<Event.Pointer>? OnPointerMove { set; }    

    /// <summary>
    /// Fired when a pointer is moved out of the hit test boundaries of an element (among other reasons).
    /// </summary>
    Action<Event.Pointer>? OnPointerOut { set; }    

    /// <summary>
    /// Fired when a pointer is moved into an element's hit test boundaries.
    /// </summary>
    Action<Event.Pointer>? OnPointerOver { set; }    

    /// <summary>
    /// Fired when a pointer is no longer active.
    /// </summary>
    Action<Event.Pointer>? OnPointerUp { set; }
}