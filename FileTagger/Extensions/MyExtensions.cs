using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ATL;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using FileTagger.Models;
using FileTagger.Services;

namespace FileTagger.Extensions;

public static class MyExtensions
{
    /// <summary>
    /// Extensions for List
    /// </summary>
    extension<T>(List<T> collection)
    {
        /// <summary>
        /// Returns the indices of all items that match the predicate.
        /// </summary>
        public IEnumerable<int> FindIndices(Func<T, bool> match)
        {
            IEnumerable<int> indices = [];
            for (int i = 0; i < collection.Count; i++)
            {
                if (match(collection[i])) indices = indices.Append(i);
            }

            return indices;
        }
    }

    /// <summary>
    /// Extensions for ILogical
    /// </summary>
    extension(ILogical logical)
    {
        /// <summary>
        /// Gets the first ILogical sibling of type <see cref="T"/>
        /// </summary>
        public T? GetFirstSibling<T>() where T : class, ILogical
        {
            IEnumerable<ILogical> siblings = logical.GetLogicalSiblings();
            foreach (ILogical sibling in siblings)
            {
                if (sibling is T t)
                {
                    return t;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the first ILogical sibling of type <see cref="T"/> that matches the given predicate
        /// </summary>
        public T? GetFirstSibling<T>(Func<T, bool> match) where T : class, ILogical
        {
            IEnumerable<ILogical> siblings = logical.GetLogicalSiblings();
            foreach (ILogical sibling in siblings)
            {
                if (sibling is T t && match(t))
                {
                    return t;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Extensions for Window
    /// </summary>
    extension(Window window)
    {
        /// <summary>
        /// Stores the state of the window into the given <see cref="IPreferenceService"/>.
        /// Stores whether the given window is maximised, and if it isn't, stores its size
        /// </summary>
        public void StoreWindowState(IPreferenceService preferenceService)
        {
            bool isMaximised = window.WindowState == WindowState.Maximized;
            PropertyInfo maximisedProperty = typeof(SystemPreferences).GetProperty(nameof(SystemPreferences.IsMaximised))!;
            preferenceService.StorePreferenceItem(maximisedProperty, isMaximised);
            if (!isMaximised)
            {
                PropertyInfo sizeProperty = typeof(SystemPreferences).GetProperty(nameof(SystemPreferences.WindowSize))!;
                preferenceService.StorePreferenceItem(sizeProperty, (window.Width, window.Height));
            }
        }
    }

    /// <summary>
    /// Extensions for PictureInfo
    /// </summary>
    extension(PictureInfo pictureInfo)
    {
        /// <summary>
        /// Test the equality of the images in two PictureInfo
        /// </summary>
        public bool PicturesEqual(PictureInfo other)
        {
            if (pictureInfo.PictureHash == 0) pictureInfo.ComputePicHash();
            if (other.PictureHash == 0) other.ComputePicHash();
            return pictureInfo.PictureHash == other.PictureHash;
        }

        /// <summary>
        /// Test the equality of two PictureInfo regardless of how they're represented internally (native codes or generic enum)
        /// , while also testing the equality of their images
        /// </summary>
        public bool TrueEqual(PictureInfo other)
        {
            return pictureInfo.PicturesEqual(other) && pictureInfo.EqualsProper(other);
        }
    }
}