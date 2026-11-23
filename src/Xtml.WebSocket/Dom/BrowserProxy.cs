using Xtml.Dom;

namespace Xtml.WebSocket.BrowserApis;

internal partial class BrowserProxy(Bridge bridge)
{
    public Lazy<Window> Window = new(() => new Window(bridge));
    public Lazy<Console> Console = new(() => new Console(bridge));
    public Lazy<Document> Document = new(() => new Document(bridge));

    public bool IsPropagationStopped => _currentPropagationID == _suppressPropagationID && _currentPropagationLevel >= _suppressPropagationLevel;
    private int _currentPropagationID;
    private int _currentPropagationLevel;
    private int _suppressPropagationID;
    private int _suppressPropagationLevel;

    public void StopPropagation()
    {
        _suppressPropagationID = _currentPropagationID;
        _suppressPropagationLevel = _currentPropagationLevel + 1;
    }

    public void StopImmediatePropagation()
    {
        _suppressPropagationID = _currentPropagationID;
        _suppressPropagationLevel = 0;
    }

    public void SetCurrentPropagation(int id, int level)
    {
        _currentPropagationID = id;
        _currentPropagationLevel = level;
    }
}