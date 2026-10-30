using Xtml.Runtime.Utilities;

namespace Xtml.Runtime;

public ref struct Reconciler(IRpcClient mutator, Keyhole[] oldBuffer, Keyhole[] newBuffer)
{
    public static readonly byte[] ROOT_KEY = "root"u8.ToArray();

    public static void Diff(IRpcClient mutator, Keyhole[] oldBuffer, Keyhole[] newBuffer)
    {
        using var perf = Perf.Measure("DiffUtil.Diff"); // TODO: Remove PerfCheck

        ref Keyhole oldFirst = ref oldBuffer[0];
        ref Keyhole newFirst = ref newBuffer[0];

        var diffUtil = new Reconciler(mutator, oldBuffer, newBuffer);
        diffUtil.Recurse(ref oldFirst, ref newFirst);
    }

    private readonly bool Recurse(ref Keyhole oldParent, ref Keyhole newParent)
    {
        if (CompareLengths(ref oldParent, ref newParent)) return false;
        if (CompareImmutables(ref oldParent, ref newParent)) return false;
        if (CompareMutables(ref oldParent, ref newParent)) return false;
        return false;
    }

    /// <summary>
    /// The first thing to compare is the easiest (and fastest).
    /// If the two spans have a different quantity of keyholes, 
    /// then it's impossible that they are the same.  
    /// So replace the whole span.
    /// </summary>
    private readonly bool CompareLengths(ref Keyhole oldParent, ref Keyhole newParent)
    {
        Span<Keyhole> oldSpan = oldBuffer.AsSpan(oldParent.Sequence);
        Span<Keyhole> newSpan = newBuffer.AsSpan(newParent.Sequence);

        if (oldSpan.Length != newSpan.Length)
        {
            if (newParent.Type == KeyholeType.HtmlRaw)
                mutator.SetValue(
                    newParent.Key,
                    newSpan
                );
            else if (newParent.TransitionModifier is null)
                mutator.SetNode(
                    newBuffer,
                    newParent.Key ?? ROOT_KEY, // Only HotReload can change the root string literal.,
                    newSpan
                );
            else
                mutator.SetNode(
                    newBuffer,
                    newParent.Key,
                    newSpan,
                    (oldParent.RelativeOrder <= newParent.RelativeOrder ? "xtml-fwd-" : "xtml-rev-", newParent.Key)
                );

            // Short circuit.  No need to finish diffing this span or traverse deeper
            // since this whole span (and possibly its children) will be sent to the browser.
            return true;
        }

        // Allow deeper recursion.
        return false;
    }

    /// <summary>
    /// IMMUTABLES (string literals)
    /// Traverse every even index – these are guaranteed to be string literals only.
    /// If any of the string literals do not match up that means this whole sequence 
    /// and all its children must be replaced.
    /// Usually this is the result of switch-expressions or ternary-conditionals:
    ///   • $"<div>{ value switch { 1 => MyComponent1(), 2 => MyComponent2(), ... } }</div>"
    ///   • $"<div>{ (condition ? MyComponent1() : MyComponent2()) }</div>" 
    /// Caveat: Don't forget about Hot Reload (DEBUG only) which can cheat the
    /// compiler guarantees that InterpolatedStringHandlers gives us where
    /// we can expect the exact same string literals at each keyhole every time.
    /// </summary>
    private readonly bool CompareImmutables(ref Keyhole oldParent, ref Keyhole newParent)
    {
        Span<Keyhole> oldSpan = oldBuffer.AsSpan(oldParent.Sequence);
        Span<Keyhole> newSpan = newBuffer.AsSpan(newParent.Sequence);

        for (int i = 0; i < newSpan.Length; i += 2)
        {
            ref Keyhole oldKeyhole = ref oldSpan[i];
            ref Keyhole newKeyhole = ref newSpan[i];

            // Note: We must use `Object.ReferenceEquals` and NOT `str1 == str2` since the latter 
            // will try comparing char by char if the pointers don't match.  Thanks to the 
            // compiler guarantees around string literals, string interning, and InterpolatedStringHandler,
            // comparing pointers is enough to guarantee equality in this rare context. 
            // (This is especially helpful when the two strings are several kilobytes in length!)
            if (!Object.ReferenceEquals(oldKeyhole.StringLiteral, newKeyhole.StringLiteral))
            {
                if (newParent.Type == KeyholeType.HtmlRaw)
                    mutator.SetValue(
                        newParent.Key,
                        newSpan
                    );
                else if (newParent.TransitionModifier == null)
                    mutator.SetNode(
                        newBuffer,
                        newParent.Key ?? ROOT_KEY, // Only HotReload can change the root string literal.
                        newSpan
                    );
                else
                    mutator.SetNode(
                        newBuffer,
                        newParent.Key,
                        newSpan,
                        (oldParent.RelativeOrder <= newParent.RelativeOrder ? "xtml-fwd-" : "xtml-rev-", newParent.Key)
                    );

                // Short circuit.  This whole segment (and possibly its children) will be replaced 
                // so there's no need to diff its mutables or traverse deeper.
                return true;
            }
        }

        // Allow deeper recursion.
        return false;
    }

    /// <summary>
    /// MUTABLES (state)
    /// Traverse every odd index – these are guaranteed to be a mutable keyhole value, i.e. state.
    /// </summary>
    private readonly bool CompareMutables(ref Keyhole oldParent, ref Keyhole newParent)
    {
        Span<Keyhole> oldSpan = oldBuffer.AsSpan(oldParent.Sequence);
        Span<Keyhole> newSpan = newBuffer.AsSpan(newParent.Sequence);

        for (int i = 1; i < newSpan.Length; i += 2)
        {
            ref Keyhole oldKeyhole = ref oldSpan[i];
            ref Keyhole newKeyhole = ref newSpan[i];

            bool shortCircuit = newKeyhole.Type switch
            {
                KeyholeType.String or
                KeyholeType.Boolean or
                KeyholeType.Color or
                KeyholeType.Uri or
                KeyholeType.Integer or
                KeyholeType.Long or
                KeyholeType.Float or
                KeyholeType.Double or
                KeyholeType.Decimal or
                KeyholeType.DateTime or
                KeyholeType.DateOnly or
                KeyholeType.TimeSpan or
                KeyholeType.TimeOnly =>
                    CompareMutable(ref oldParent, ref newParent, ref oldKeyhole, ref newKeyhole),
                KeyholeType.Iterator =>
                    CompareIterator(ref oldParent, ref newParent, ref oldKeyhole, ref newKeyhole),
                KeyholeType.Html or
                KeyholeType.HtmlRaw =>
                    Recurse(ref oldKeyhole, ref newKeyhole),
                KeyholeType.EventListener =>
                    false, // Event listeners never need to be diff'd, their only purpose is for lookup.
                KeyholeType.StringLiteral =>
                    throw new InvalidOperationException($"It should be impossible to find a StringLiteral in an odd index: {i}"),
                _ =>
                    throw new InvalidOperationException("Unnamed enum values are not supported")
            };

            if (shortCircuit)
                return true;
        }

        // Allow deeper recursion.
        return false;
    }

    private readonly bool CompareMutable(ref Keyhole oldParent, ref Keyhole newParent, ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        if (!Keyhole.Equals(ref oldKeyhole, ref newKeyhole))
        {
            if (newParent.Type == KeyholeType.HtmlRaw)
            {
                // This keyhole's value is part of a sequence of keyholes that comprises this attribute 
                // (or raw text element like <title> or <textarea>).
                // Find the start of this sequence, then grab the sequence's full span.
                ref var startKeyhole = ref newBuffer[newParent.SequenceStart];
                mutator.SetValue(newParent.Key, newBuffer.AsSpan(startKeyhole.Sequence));

                // Short circuit.  No need to diff the rest of this span.
                // This whole attribute sequence will be updated.
                return true;
            }
            else
            {
                mutator.SetValue(newKeyhole.Key, in newKeyhole);
            }
        }

        // Allow deeper recursion.
        return false;
    }

    private readonly bool CompareIterator(ref Keyhole oldParent, ref Keyhole newParent, ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        var oldItems = oldBuffer.AsSpan(oldKeyhole.Sequence);
        var newItems = newBuffer.AsSpan(newKeyhole.Sequence);
        var minLength = Math.Min(oldItems.Length, newItems.Length);

        var tagChanges = oldItems.Length != newItems.Length ? 1 : 0;
        if (newKeyhole.TransitionModifier is not null)
        {
            for (var d = 1; d < minLength; d += 2)
            {
                ref var oldItem = ref oldItems[d];
                ref var newItem = ref newItems[d];
                if (oldItem.Tag != newItem.Tag)
                    if (++tagChanges > 1)
                        break;
            }
        }
        bool shouldUseTransition = tagChanges > 1;

        for (var d = 1; d < minLength; d += 2)
        {
            ref var oldItem = ref oldItems[d];
            ref var newItem = ref newItems[d];

            if (shouldUseTransition && oldItem.Tag != newItem.Tag && oldItem.Tag is not null && newItem.Tag is not null)
            {
                mutator.SetNode(
                    newBuffer,
                    newItem.Key,
                    newBuffer.AsSpan(newItem.Sequence),
                    ("xtml-move-", newItem.Tag.GetHashCode()),
                    ("xtml-move-", oldItem.Tag.GetHashCode())
                );
            }
            else
            {
                Recurse(ref oldItem, ref newItem);
            }
        }

        if (oldItems.Length < newItems.Length)
        {
            // The new iterator has more items than the old one. 
            // These items can simply be appended to the end.  
            // This will not violate any keyhole's positional stability
            // or cause any keyname collisions.
            for (var d = minLength; d < newItems.Length; d += 2)
            {
                ref var newItem = ref newItems[d];
                var newItemSpan = newBuffer.AsSpan(newItem.Sequence);

                if (newKeyhole.TransitionModifier is null || newItem.Tag is null)
                    mutator.PushNode(newBuffer, newKeyhole.Key, newItemSpan, newItem.Key);
                else
                    mutator.PushNode(newBuffer, newKeyhole.Key, newItemSpan, newItem.Key, ("xtml-move-", newItem.Tag.GetHashCode()));
            }
        }
        else if (oldItems.Length > newItems.Length)
        {
            // The old iterator has more items than the new one. 
            // These items can simply be removed.  
            // This will not violate any keyhole's positional stability.
            for (var d = minLength; d < oldItems.Length; d += 2)
            {
                ref var item = ref oldItems[d];
                if (newKeyhole.TransitionModifier is null || item.Tag is null)
                    mutator.PopNode(item.Key);
                else
                    mutator.PopNode(item.Key, ("xtml-move-", item.Tag.GetHashCode()));
            }
        }

        // Allow deeper recursion.
        return false;
    }
}
