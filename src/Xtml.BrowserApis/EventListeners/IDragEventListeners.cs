namespace Xtml.BrowserApis.EventListeners;

public interface IDragEventListeners
{
    /// <summary>
    /// This event is fired when an element or text selection is being dragged.
    /// </summary>
    Action<Event.Drag>? OnDrag { set; }
    
    /// <summary>
    /// This event is fired when a drag operation is being ended (by releasing a mouse button or hitting the escape key).
    /// </summary>
    Action<Event.Drag>? OnDragEnd { set; }
    
    /// <summary>
    /// This event is fired when a dragged element or text selection enters a valid drop target.
    /// </summary>
    Action<Event.Drag>? OnDragEnter { set; }
    
    /// <summary>
    /// This event is fired when a dragged element or text selection leaves a valid drop target.
    /// </summary>
    Action<Event.Drag>? OnDragLeave { set; }
    
    /// <summary>
    /// This event is fired continuously when an element or text selection is being dragged and the mouse pointer is over a valid drop target (every 50 ms WHEN mouse is not moving ELSE much faster between 5 ms (slow movement) and 1ms (fast movement) approximately. This firing pattern is different than mouseover ).
    /// </summary>
    Action<Event.Drag>? OnDragOver { set; }
    
    /// <summary>
    /// This event is fired when the user starts dragging an element or text selection.
    /// </summary>
    Action<Event.Drag>? OnDragStart { set; }
    
    /// <summary>
    /// This event is fired when an element or text selection is dropped on a valid drop target.
    /// </summary>
    Action<Event.Drag>? OnDrop { set; }
    

}