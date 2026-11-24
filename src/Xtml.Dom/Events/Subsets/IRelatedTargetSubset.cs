namespace Xtml.BrowserApis.Events.Subsets;

public interface IRelatedTargetSubset : ISubset, IViewSubset
{
    new const string TRIM = "relatedTarget";

    /// <summary>
    /// The secondary target for the event, if there is one.
    /// </summary>
    EventTarget RelatedTarget { get; }
}