using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileTagger.Extensions;

/// <summary>
/// Implementing several List/Collection methods for ObservableCollection
/// </summary>
public static class ObservableCollectionExtensions
{
    /// <summary>
    /// Extensions for ObservableCollection
    /// </summary>
    extension<T>(ObservableCollection<T> collection)
    {
        /// <summary>
        /// Returns the index of the first item that matches the predicate.
        /// Returns -1 if no match is found
        /// </summary>
        public int FindIndex(Func<T, bool> match)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if (match(collection[i])) return i;
            }

            return -1;
        }

        /// <summary>
        /// Removes all items that match the predicate. Returns the number of items removed.
        /// </summary>
        public int RemoveAll(Func<T, bool> match)
        {
            int removed = 0;
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                T item = collection[i];
                if (match(item))
                {
                    removed++;
                    collection.RemoveAt(i);
                }
            }

            return removed;
        }

        /// <summary>
        /// Adds a collection of items to the end of the list.
        /// </summary>
        public void AddRange(IEnumerable<T> newItems)
        {
            foreach (T x1 in newItems)
            {
                collection.Add(x1);
            }
        }

        /// <summary>
        /// Inserts a collection of items into the list at the specified index.
        /// </summary>
        public void InsertRange(int index, IEnumerable<T> newItems)
        {
            List<T> enumeratedItems = newItems.ToList();
            for (int i = enumeratedItems.Count - 1; i >= 0; i--)
            {
                collection.Insert(index, enumeratedItems.ElementAt(i));
            }
        }
    }
}