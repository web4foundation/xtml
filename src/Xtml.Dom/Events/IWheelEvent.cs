using static Xtml.BrowserApis.Events.Aliases.Subsets;

namespace Xtml.BrowserApis.Events;

public interface IWheelEvent
    : IMouseEvent, Deltas
{
}
