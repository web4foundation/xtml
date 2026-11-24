namespace Xtml.BrowserApis.EventListeners;

public interface ITransitionEventListeners
{
    /// <summary>
    /// An Event fired when a CSS transition has been cancelled.
    /// </summary>
    Action<Event.Transition>? OnTransitionCancel { set; }
    
    /// <summary>
    /// An Event fired when a CSS transition has finished playing.
    /// </summary>
    Action<Event.Transition>? OnTransitionEnd { set; }
    
    /// <summary>
    /// An Event fired when a CSS transition is created (i.e., when it is added to a set of running transitions), though not necessarily started.
    /// </summary>
    Action<Event.Transition>? OnTransitionRun { set; }
    
    /// <summary>
    /// An Event fired when a CSS transition has started transitioning.
    /// </summary>
    Action<Event.Transition>? OnTransitionStart { set; }
}