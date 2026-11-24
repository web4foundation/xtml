namespace Xtml.BrowserApis.Events.Subsets;

public interface IStatesSubset : ISubset, IViewSubset
{
    new const string TRIM = "newState,oldState";

    /// <summary>
    /// A string (either "open" or "closed"), representing the state the element is transitioning to.
    /// </summary>
    string NewState { get; }

    /// <summary>
    /// A string (either "open" or "closed"), representing the state the element is transitioning from.
    /// </summary>
    string OldState { get; }
}