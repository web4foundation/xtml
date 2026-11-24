using static Xtml.BrowserApis.Events.Aliases.Subsets;

namespace Xtml.BrowserApis.Events.Subsets;

public interface IModifiersSubset : ISubset, ModifierAlt, ModifierCtrl, ModifierMeta, ModifierShift, IViewSubset
{
    new const string TRIM = $"{ModifierAlt.TRIM},{ModifierCtrl.TRIM},{ModifierMeta.TRIM},{ModifierShift.TRIM}";
}