namespace Xtml.BrowserApis.Events.Subsets;

public interface IPreventDefaultSubset : ISubset
{
    const string TRIM = "preventDefault";

    /// <summary>
    /// Cancels the event (if it is cancelable).
    /// Important difference in Xtml: preventDefault() is called first thing
    /// on the developer's behalf before the rest of the event handler is executed.
    /// This is necessary since it's a synchronous API trying to execute remotely.
    /// </summary>
    void PreventDefault()
    {
    }
}