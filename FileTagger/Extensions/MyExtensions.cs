using System.Collections.Generic;
using Avalonia;
using Avalonia.VisualTree;

namespace FileTagger.Extensions;

public static class MyExtensions
{
    /// <summary>
    /// Extensions for Visual
    /// </summary>
    extension(Visual root)
    {
        /// <summary>
        /// Enumerates all descendants of a Visual of the given type T
        /// </summary>
        public IEnumerable<T> GetVisualDescendants<T>()
        {
            List<T> children = [];
            foreach (Visual child in root.GetVisualChildren())
            {
                if (child is T correctTypeChild)
                {
                    children.Add(correctTypeChild);
                }
                else
                {
                    children.AddRange(child.GetVisualDescendants<T>());
                }
            }
            return children;
        }
    }
}