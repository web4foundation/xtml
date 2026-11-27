using Xtml.Runtime;
using Xtml.WebSocket.JsonRpc;

namespace Xtml.WebSocket.BrowserApis;

internal ref struct KeyholesClient(Bridge bridge) : IKeyholesClient
{
    void IKeyholesClient.SetValue(byte[] key, in Keyhole keyhole) => JsonRpcWriter
        .Current(bridge)
        .Method(("keyholes['", key, "'].setValue"))
        .Param(in keyhole)
        .AsNotification();

    void IKeyholesClient.SetValue(byte[] key, Span<Keyhole> keyholes) => JsonRpcWriter
        .Current(bridge)
        .Method(("keyholes['", key, "'].setValue"))
        .Param(keyholes)
        .AsNotification();

    void IKeyholesClient.SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes) => JsonRpcWriter
        .Current(bridge)
        .Method(("keyholes['", key, "'].setNode"))
        .Param(snapshot, keyholes)
        .AsNotification();

    void IKeyholesClient.SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, byte[]> viewTransitionName) => JsonRpcWriter
        .Current(bridge)
        .Method(("keyholes['", key, "'].setNode"))
        .Param(snapshot, keyholes)
        .Param(viewTransitionName)
        .AsNotification();

    void IKeyholesClient.SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, int> viewTransitionNameNew, ValueTuple<string, int> viewTransitionNameOld) => JsonRpcWriter
        .Current(bridge)
        .Method(("keyholes['", key, "'].setNode"))
        .Param(snapshot, keyholes)
        .Param(viewTransitionNameNew)
        .Param(viewTransitionNameOld)
        .AsNotification();

    void IKeyholesClient.PushNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, byte[] newKey) => JsonRpcWriter
        .Current(bridge)
        .Method(("keyholes['", key, "'].pushNode"))
        .Param(snapshot, keyholes)
        .Param(newKey)
        .AsNotification();

    void IKeyholesClient.PushNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, byte[] newKey, ValueTuple<string, int> viewTransitionName) => JsonRpcWriter
        .Current(bridge)
        .Method(("keyholes['", key, "'].pushNode"))
        .Param(snapshot, keyholes)
        .Param(newKey)
        .Param(viewTransitionName)
        .AsNotification();

    void IKeyholesClient.PopNode(byte[] key) => JsonRpcWriter
        .Current(bridge)
        .Method(("keyholes['", key, "'].popNode"))
        .AsNotification();
    
    void IKeyholesClient.PopNode(byte[] key, ValueTuple<string, int> viewTransitionName) => JsonRpcWriter
        .Current(bridge)
        .Method(("keyholes['", key, "'].popNode"))
        .Param(viewTransitionName)
        .AsNotification();
}
