using System.Collections.Generic;
using System.Reflection;
using ATL;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using FileTagger.Models;
using FileTagger.Services;

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
            PropertyInfo maximisedProperty = typeof(Preferences).GetProperty(nameof(Preferences.IsMaximised))!;
            preferenceService.StorePreferenceItem(maximisedProperty, isMaximised);
            if (!isMaximised)
            {
                PropertyInfo sizeProperty = typeof(Preferences).GetProperty(nameof(Preferences.WindowSize))!;
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
            if(pictureInfo.PictureHash == 0) pictureInfo.ComputePicHash();
            if(other.PictureHash == 0) other.ComputePicHash();
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