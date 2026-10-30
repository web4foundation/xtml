using Xtml.Dom;

namespace Xtml.Runtime;

public interface IRpcClient
{
    void SetValue(byte[] key, in Keyhole keyhole);
    void SetValue(byte[] key, Span<Keyhole> keyholes);
    void SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes);
    void SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, byte[]> viewTransitionName);
    void SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, int> viewTransitionNameNew, ValueTuple<string, int> viewTransitionNameOld);
    void PushNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, byte[] newKey);
    void PushNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, byte[] newKey, ValueTuple<string, int> viewTransitionName);
    void PopNode(byte[] key);
    void PopNode(byte[] key, ValueTuple<string, int> viewTransitionName);
}
