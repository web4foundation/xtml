using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Xtml.Dom;
using Xtml.Runtime;
using Xtml.Runtime.Utilities;
using Xtml.WebSocket.JsonRpc;

namespace Xtml.WebSocket;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on Bridge.
internal partial class Bridge : IKeyholesClient
{
    void IKeyholesClient.SetValue(byte[] key, in Keyhole keyhole) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setValue"))
        .Param(in keyhole)
        .AsNotification();

    void IKeyholesClient.SetValue(byte[] key, Span<Keyhole> keyholes) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setValue"))
        .Param(keyholes)
        .AsNotification();

    void IKeyholesClient.SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setNode"))
        .Param(snapshot, keyholes)
        .AsNotification();

    void IKeyholesClient.SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, byte[]> viewTransitionName) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setNode"))
        .Param(snapshot, keyholes)
        .Param(viewTransitionName)
        .AsNotification();

    void IKeyholesClient.SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, int> viewTransitionNameNew, ValueTuple<string, int> viewTransitionNameOld) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setNode"))
        .Param(snapshot, keyholes)
        .Param(viewTransitionNameNew)
        .Param(viewTransitionNameOld)
        .AsNotification();

    void IKeyholesClient.PushNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, byte[] newKey) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].pushNode"))
        .Param(snapshot, keyholes)
        .Param(newKey)
        .AsNotification();

    void IKeyholesClient.PushNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, byte[] newKey, ValueTuple<string, int> viewTransitionName) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].pushNode"))
        .Param(snapshot, keyholes)
        .Param(newKey)
        .Param(viewTransitionName)
        .AsNotification();

    void IKeyholesClient.PopNode(byte[] key) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].popNode"))
        .AsNotification();
    
    void IKeyholesClient.PopNode(byte[] key, ValueTuple<string, int> viewTransitionName) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].popNode"))
        .Param(viewTransitionName)
        .AsNotification();
}
