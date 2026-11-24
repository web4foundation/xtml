namespace Xtml.BrowserApis.Events;

public enum Button : int
{
    /// <summary>
    /// Main button pressed, usually the left button or the un-initialized state
    /// </summary>
    Main = 0,
    /// <summary>
    /// Auxiliary button pressed, usually the wheel button or the middle button (if present)
    /// </summary>
    Auxiliary = 1,
    /// <summary>
    /// Secondary button pressed, usually the right button
    /// </summary>
    Secondary = 2,
    /// <summary>
    /// Fourth button, typically the Browser Back button
    /// </summary>
    Fourth = 3,
    /// <summary>
    /// Fifth button, typically the Browser Forward button
    /// </summary>
    Fifth = 4
}

[Flags]
public enum ButtonFlag : int
{
    /// <summary>
    /// No button or un-initialized
    /// </summary>
    None = 0,
    /// <summary>
    /// Primary button (usually the left button)
    /// </summary>
    Primary = 1,
    /// <summary>
    /// Secondary button (usually the right button)
    /// </summary>
    Secondary = 2,
    /// <summary>
    /// Auxiliary button (usually the mouse wheel button or middle button)
    /// </summary>
    Auxiliary = 4,
    /// <summary>
    /// 4th button (typically the "Browser Back" button)
    /// </summary>
    Fourth = 8,
    /// <summary>
    /// 5th button (typically the "Browser Forward" button)
    /// </summary>
    Fifth = 16
}

public enum DeltaMode : int
{
    /// <summary>
    /// The delta* values are specified in pixels.
    /// </summary>
    Pixel = 0,
    /// <summary>
    /// The delta* values are specified in lines. Each mouse click scrolls a line of content, 
    /// where the method used to calculate line height is browser dependent.
    /// </summary>
    Line = 1,
    /// <summary>
    /// The delta* values are specified in pages. Each mouse click scrolls a page of content.
    /// </summary>
    Page = 2
}

public enum EventPhase : int
{
    /// <summary>
    /// The event is not being processed at this time.
    /// </summary>
    None = 0,
    /// <summary>
    /// The event is being propagated through the target's ancestor objects. 
    /// This process starts with the Window, then Document, then the HTMLHtmlElement, 
    /// and so on through the elements until the target's parent is reached. 
    /// Event listeners registered for capture mode when EventTarget.addEventListener() 
    /// was called are triggered during this phase.
    /// </summary>
    CapturingPhase = 1,
    /// <summary>
    /// The event has arrived at the event's target. Event listeners registered for 
    /// this phase are called at this time. If Event.bubbles is false, processing 
    /// the event is finished after this phase is complete.
    /// </summary>
    AtTarget = 2,
    /// <summary>
    /// The event is propagating back up through the target's ancestors in reverse order, 
    /// starting with the parent, and eventually reaching the containing Window. 
    /// This is known as bubbling, and occurs only if Event.bubbles is true. 
    /// Event listeners registered for this phase are triggered during this process.
    /// </summary>
    BubblingPhase = 3
}

public enum KeyLocation : int
{
    /// <summary>
    /// The key described by the event is not identified as being located in a particular 
    /// area of the keyboard; it is not located on the numeric keypad (unless it's the NumLock key), 
    /// and for keys that are duplicated on the left and right sides of the keyboard, the key is, 
    /// for whatever reason, not to be associated with that location.  
    /// <br/><br/>Examples include alphanumeric keys on the standard PC 101 US keyboard, 
    /// the NumLock key, and the space bar.
    /// </summary>
    Standard = 0,
    /// <summary>
    /// The key is one which may exist in multiple locations on the keyboard and, in this instance, 
    /// is on the left side of the keyboard.
    /// <br/><br/>Examples include the left Control key, the left Command key on a Macintosh 
    /// keyboard, or the left Shift key.
    /// </summary>
    Left = 1,
    /// <summary>
    /// The key is one which may exist in multiple positions on the keyboard and, in this case, 
    /// is located on the right side of the keyboard.
    /// <br/><br/>Examples include the right Shift key and the right Alt key (Option on a Mac keyboard).
    /// </summary>
    Right = 2,
    /// <summary>
    /// The key is located on the numeric keypad, or is a virtual key associated with the numeric 
    /// keypad if there's more than one place the key could originate from. The NumLock key does 
    /// not fall into this group and is always encoded with the location DOM_KEY_LOCATION_STANDARD.
    /// <br/><br/>Examples include the digits on the numeric keypad, the keypad's Enter key, and 
    /// the decimal point on the keypad.
    /// </summary>
    NumPad = 3,
}