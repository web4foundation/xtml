namespace Xtml.BrowserApis.Events.Subsets;

public interface IIsComposingSubset : ISubset, IViewSubset
{
    new const string TRIM = "isComposing";

    /// <summary>
    /// Returns a boolean value that is true if the event is fired between after compositionstart and before compositionend.
    /// </summary>
    bool IsComposing { get; }
}