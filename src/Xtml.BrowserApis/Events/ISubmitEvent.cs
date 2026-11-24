using static Xtml.BrowserApis.Events.Aliases.Subsets;

namespace Xtml.BrowserApis.Events;

public interface ISubmitEvent
    : IEvent, Submitter
{
}
