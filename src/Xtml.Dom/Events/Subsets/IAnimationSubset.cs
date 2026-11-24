namespace Xtml.BrowserApis.Events.Subsets;

public interface IAnimationSubset : ISubset, IViewSubset
{
    new const string TRIM = "animationName,elapsedTime,propertyName,pseudoElement";

    /// <summary>
    /// A string containing the value of the animation-name that generated the animation.
    /// </summary>
    string AnimationName { get; }

    /// <summary>
    /// A float giving the amount of time the animation has been running, in seconds, 
    /// when this event fired, excluding any time the animation was paused. 
    /// For an animationstart event, elapsedTime is 0.0 unless there was a negative 
    /// value for animation-delay, in which case the event will be fired with 
    /// elapsedTime containing (-1 * delay).
    /// </summary>
    double ElapsedTime { get; }

    /// <summary>
    /// A string containing the name CSS property associated with the transition.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// A string, starting with '::', containing the name of the pseudo-element the 
    /// animation runs on. If the animation doesn't run on a pseudo-element but 
    /// on the element, an empty string: ''.
    /// </summary>
    string PseudoElement { get; }
}