using Xtml.Dom;
using Xtml.WebSocket.JsonRpc;

namespace Xtml.WebSocket;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on Bridge.
public partial class Bridge : IConsole
{
    public IConsole Console => this;

    // TODO: How to support object?
    // TODO: Tests still needed for each one.
    void IConsole.Assert(bool assertion, string message) => JsonRpcWriter
        .Batch(this)
        .Method("console.assert")
        .Param(assertion)
        .Param(message)
        .AsNotification();
    void IConsole.Assert(bool assertion, string message, params Span<string> substitutions) => JsonRpcWriter
        .Batch(this)
        .Method("console.assert")
        .Param(assertion)
        .Param(message)
        .OptionalParams(substitutions)
        .AsNotification();
    void IConsole.Assert(bool assertion, object value) => throw new NotImplementedException();
    void IConsole.Assert(bool assertion, params Span<object> values) => throw new NotImplementedException();
    void IConsole.Clear() => JsonRpcWriter
        .Batch(this)
        .Method("console.clear")
        .AsNotification();
    void IConsole.Count() => JsonRpcWriter
        .Batch(this)
        .Method("console.count")
        .AsNotification();
    void IConsole.Count(object label) => throw new NotImplementedException();
    void IConsole.CountReset() => JsonRpcWriter
        .Batch(this)
        .Method("console.reset")
        .AsNotification();
    void IConsole.CountReset(object label) => throw new NotImplementedException();
    void IConsole.Debug(string message) => JsonRpcWriter
        .Batch(this)
        .Method("console.debug")
        .Param(message)
        .AsNotification();
    void IConsole.Debug(string message, params Span<string> substitutions) => JsonRpcWriter
        .Batch(this)
        .Method("console.debug")
        .Param(message)
        .OptionalParams(substitutions)
        .AsNotification();
    void IConsole.Debug(object value) => throw new NotImplementedException();
    void IConsole.Debug(params Span<object> values) => throw new NotImplementedException();
    void IConsole.Dir(object obj) => throw new NotImplementedException();
    void IConsole.Dir(object obj, IConsole.DirOptions options) => throw new NotImplementedException();
    void IConsole.Error(string message) => JsonRpcWriter
        .Batch(this)
        .Method("console.error")
        .Param(message)
        .AsNotification();
    void IConsole.Error(string message, params Span<string> substitutions) => JsonRpcWriter
        .Batch(this)
        .Method("console.error")
        .Param(message)
        .OptionalParams(substitutions)
        .AsNotification();
    void IConsole.Error(object value) => throw new NotImplementedException();
    void IConsole.Error(object value, params Span<object> values) => throw new NotImplementedException();
    void IConsole.Group() => JsonRpcWriter
        .Batch(this)
        .Method("console.group")
        .AsNotification();
    void IConsole.Group(object label) => throw new NotImplementedException();
    void IConsole.GroupCollapsed() => JsonRpcWriter
        .Batch(this)
        .Method("console.groupCollapsed")
        .AsNotification();
    void IConsole.GroupCollapsed(object label) => throw new NotImplementedException();
    void IConsole.GroupCollapsed(string message, params Span<string> substitutions) => JsonRpcWriter
        .Batch(this)
        .Method("console.groupCollapsed")
        .Param(message)
        .OptionalParams(substitutions)
        .AsNotification();
    void IConsole.GroupEnd() => JsonRpcWriter
        .Batch(this)
        .Method("console.groupEnd")
        .AsNotification();
    void IConsole.Info(string message) => JsonRpcWriter
        .Batch(this)
        .Method("console.info")
        .Param(message)
        .AsNotification();
    void IConsole.Info(string message, params Span<string> substitutions) => JsonRpcWriter
        .Batch(this)
        .Method("console.info")
        .Param(message)
        .OptionalParams(substitutions)
        .AsNotification();
    void IConsole.Info(object value) => throw new NotImplementedException();
    void IConsole.Info(object value, params Span<object> values) => throw new NotImplementedException();
    void IConsole.Log(string message) => JsonRpcWriter
        .Batch(this)
        .Method("console.log")
        .Param(message)
        .AsNotification();
    void IConsole.Log(string message, params Span<string> substitutions) => JsonRpcWriter
        .Batch(this)
        .Method("console.log")
        .Param(message)
        .OptionalParams(substitutions)
        .AsNotification();
    void IConsole.Log(object value) => throw new NotImplementedException();
    void IConsole.Log(object value, params Span<object> values) => throw new NotImplementedException();
    void IConsole.Table(object obj) => throw new NotImplementedException();
    void IConsole.Table(object obj, string[] columns) => throw new NotImplementedException();
    void IConsole.Time() => JsonRpcWriter
        .Batch(this)
        .Method("console.time")
        .AsNotification();
    void IConsole.Time(object label) => throw new NotImplementedException();
    void IConsole.TimeEnd() => JsonRpcWriter
        .Batch(this)
        .Method("console.timeEnd")
        .AsNotification();
    void IConsole.TimeEnd(object label) => throw new NotImplementedException();
    void IConsole.TimeLog() => JsonRpcWriter
        .Batch(this)
        .Method("console.timeLog")
        .AsNotification();
    void IConsole.TimeLog(object label) => throw new NotImplementedException();
    void IConsole.TimeLog(object label, params Span<object> values) => throw new NotImplementedException();
    void IConsole.Trace() => JsonRpcWriter
        .Batch(this)
        .Method("console.trace")
        .AsNotification();
    void IConsole.Trace(params Span<object> objects) => throw new NotImplementedException();
    void IConsole.Warn(string message) => JsonRpcWriter
        .Batch(this)
        .Method("console.warn")
        .Param(message)
        .AsNotification();
    void IConsole.Warn(string message, params Span<string> substitutions) => JsonRpcWriter
        .Batch(this)
        .Method("console.warn")
        .Param(message)
        .OptionalParams(substitutions)
        .AsNotification();
    void IConsole.Warn(object value) => throw new NotImplementedException();
    void IConsole.Warn(object value, params Span<object> values) => throw new NotImplementedException();
}