namespace Xtml.BrowserApis.Events.Subsets;

public interface ISubmitterSubset : ISubset, IViewSubset
{
    new const string TRIM = "submitter";

    /// <summary>
    /// An HTMLElement object which identifies the button or other element 
    /// which was invoked to trigger the form being submitted.
    /// </summary>
    EventTarget Submitter { get; }
}