using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StardewUI.Layout;

/// <summary>
/// List wrapper that tracks whether changes have been made.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public class DirtyTrackingList<T> : IList<T>, IReadOnlyList<T>
{
    private readonly List<T> items = [];

    /// <summary>
    /// Whether changes have been made since the last call to <see cref="ResetDirty"/>.
    /// </summary>
    public bool IsDirty { get; private set; }

    /// <inheritdoc />
    public T this[int index]
    {
        get => this.items[index];
        set
        {
            if (!Equals(this.items[index], value))
            {
                this.items[index] = value;
                this.IsDirty = true;
            }
        }
    }

    /// <inheritdoc />
    public int Count => this.items.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public void Add(T item)
    {
        this.items.Add(item);
        this.IsDirty = true;
    }

    /// <inheritdoc />
    public void Clear()
    {
        if (this.items.Count == 0)
        {
            return;
        }

        this.items.Clear();
        this.IsDirty = true;
    }

    /// <inheritdoc />
    public bool Contains(T item)
    {
        return this.items.Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex)
    {
        this.items.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        return this.items.GetEnumerator();
    }

    /// <inheritdoc />
    public int IndexOf(T item)
    {
        return this.items.IndexOf(item);
    }

    /// <inheritdoc />
    public void Insert(int index, T item)
    {
        this.items.Insert(index, item);
        this.IsDirty = true;
    }

    /// <inheritdoc />
    public bool Remove(T item)
    {
        bool wasRemoved = this.items.Remove(item);
        if (wasRemoved)
        {
            this.IsDirty = true;
        }
        return wasRemoved;
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        this.items.RemoveAt(index);
        this.IsDirty = true;
    }

    /// <summary>
    /// Resets the dirty state; <see cref="IsDirty"/> will return <c>false</c> until another mutation occurs.
    /// </summary>
    public void ResetDirty()
    {
        this.IsDirty = false;
    }

    /// <summary>
    /// Replaces the entire list with the specified sequence.
    /// </summary>
    /// <param name="items">The new list items.</param>
    public bool SetItems(IEnumerable<T> items)
    {
        if (items.SequenceEqual(this.items))
        {
            return false;
        }
        this.items.Clear();
        this.items.AddRange(items);
        this.IsDirty = true;
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
