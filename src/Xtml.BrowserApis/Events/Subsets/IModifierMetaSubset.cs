namespace Xtml.BrowserApis.Events.Subsets;

public interface IModifierMetaSubset : ISubset, IViewSubset
{
    new const string TRIM = "metaKey";

    /// <summary>
    /// Returns true if the meta key was down when the event was fired.
    /// </summary>
    bool MetaKey { get; }
}