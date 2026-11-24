using static Xtml.BrowserApis.Events.Aliases.Subsets;

namespace Xtml.BrowserApis.Events;

public interface IMouseEvent
    : IUIEvent, Buttons, Coordinates, Modifiers, RelatedTarget
{
}
