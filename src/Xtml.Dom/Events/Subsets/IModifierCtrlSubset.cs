namespace Xtml.BrowserApis.Events.Subsets;

public interface IModifierCtrlSubset : ISubset, IViewSubset
{
    new const string TRIM = "ctrlKey";

    /// <summary>
    /// Returns true if the control key was down when the event was fired.
    /// </summary>
    bool CtrlKey { get; }
}