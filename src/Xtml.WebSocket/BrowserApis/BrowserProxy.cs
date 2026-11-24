using Xtml.BrowserApis;

namespace Xtml.WebSocket.BrowserApis;

internal partial class BrowserProxy(Bridge bridge)
{
    public IWindow Window => this;
    public IConsole Console => this;
    public IDocument Document => this;

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