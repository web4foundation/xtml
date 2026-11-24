namespace Xtml.BrowserApis.Events.Subsets;

public interface IModifierShiftSubset : ISubset, IViewSubset
{
    new const string TRIM = "shiftKey";

    /// <summary>
    /// Returns true if the shift key was down when the event was fired.
    /// </summary>
    bool ShiftKey { get; }
}