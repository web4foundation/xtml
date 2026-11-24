using static Xtml.BrowserApis.Events.Aliases.Subsets;

namespace Xtml.BrowserApis.Events;

public interface IPointerEvent
    : IMouseEvent, Pointer, Angles, WidthHeight, Pressures, Tilts
{
}
