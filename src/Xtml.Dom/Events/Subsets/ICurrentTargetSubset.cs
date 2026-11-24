namespace Xtml.BrowserApis.Events.Subsets;

public interface ICurrentTargetSubset : ISubset, IViewSubset
{
    new const string TRIM = "currentTarget";

    /// <summary>
    /// A reference to the currently registered target for the event. This is the 
    /// object to which the event is currently slated to be sent. It's possible 
    /// this has been changed along the way through retargeting.
    /// </summary>
    EventTarget CurrentTarget { get; }
}