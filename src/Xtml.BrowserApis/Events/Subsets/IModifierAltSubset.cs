namespace Xtml.BrowserApis.Events.Subsets;

public interface IModifierAltSubset : ISubset, IViewSubset
{
    new const string TRIM = "altKey";

    /// <summary>
    /// Returns true if the alt key was down when the event was fired.
    /// </summary>
    bool AltKey { get; }
}