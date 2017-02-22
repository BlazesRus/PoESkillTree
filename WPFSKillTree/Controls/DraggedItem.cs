﻿using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls;
using POESKillTree.Model.Items;
using POESKillTree.Utils;

namespace POESKillTree.Controls
{
    /// <summary>
    /// Represents an <see cref="Item"/> of an <see cref="ItemVisualizer"/> that is currently dragged.
    /// Saves useful information and shows the item image on the cursor while dragging.
    /// <para>
    /// This class need to be disposed after the drag operation is completed for the item image overlay to
    /// be removed.
    /// </para>
    /// </summary>
    /// <remarks>For usage examples, see <see cref="Inventory"/> or <see cref="Stash"/>.</remarks>
    public class DraggedItem : IDisposable
    {
        /// <summary>
        /// Gets or sets the <see cref="DragDropEffects"/> that are used if this item is dropped into the stash
        /// (only a single value is supported). The default value is <see cref="DragDropEffects.Move"/>.
        /// </summary>
        public DragDropEffects DropOnStashEffect { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DragDropEffects"/> that are used if this item is dropped into an inventory slot
        /// (only a single value is supported). The default value is <see cref="DragDropEffects.Copy"/>.
        /// </summary>
        public DragDropEffects DropOnInventoryEffect { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DragDropEffects"/> that are used if this item is dropped into the bin
        /// (only a single value is supported). The default value is <see cref="DragDropEffects.Move"/>.
        /// </summary>
        public DragDropEffects DropOnBinEffect { get; set; }

        /// <summary>
        /// Gets the <see cref="DragDropEffects"/> that can possibly be used by this item.
        /// </summary>
        public DragDropEffects AllowedEffects
        {
            get { return DropOnBinEffect | DropOnStashEffect | DropOnInventoryEffect; }
        }

        /// <summary>
        /// Gets the <see cref="ItemVisualizer"/> from which this item was dragged.
        /// </summary>
        public ItemVisualizer SourceItemVisualizer { get; }

        /// <summary>
        /// Gets <see cref="ItemVisualizer.Item"/> of <see cref="SourceItemVisualizer"/>.
        /// </summary>
        public Item Item
        {
            get { return SourceItemVisualizer.Item; }
        }

        private readonly DragAdorner _dragAdorner;

        private readonly AdornerLayer _adornerLayer;

        private readonly Matrix _uiScaling;

        /// <summary>
        /// Gets the <see cref="Point"/> relative to <see cref="SourceItemVisualizer"/> where
        /// the mouse was pointed at when the drag operation was initiated.
        /// </summary>
        public Point DragStart { get; }

        private readonly Point _offset;

        public DraggedItem(ItemVisualizer sourceItemVisualizer)
        {
            DropOnStashEffect = DragDropEffects.Move;
            DropOnInventoryEffect = DragDropEffects.Copy;
            DropOnBinEffect = DragDropEffects.Move;
            SourceItemVisualizer = sourceItemVisualizer;

            var mainWindow = (MetroWindow) Application.Current.MainWindow;
            var dragScope = (UIElement)mainWindow.Content;

            var imageSource = sourceItemVisualizer.Item.Image.ImageSource.Result;
            var brush = new ImageBrush(imageSource) { Opacity = 0.5 };
            _dragAdorner = new DragAdorner(dragScope, new Size(imageSource.Width, imageSource.Height), brush);
            DragStart = Mouse.GetPosition(SourceItemVisualizer);
            // DragStart is subtracted for the cursor to be on the item image at the same position as where the drag
            // was initiated.
            // The window position is subtracted because the cursor position is retrieved as an absolute value.
            // The title bar height is subtracted because the content used for the adorner does not include it.
            var winPos = GetWindowPosition(mainWindow);
            _offset.X = -DragStart.X - winPos.X;
            _offset.Y = -DragStart.Y - winPos.Y - mainWindow.TitlebarHeight;

            _adornerLayer = AdornerLayer.GetAdornerLayer(dragScope);
            _adornerLayer.Add(_dragAdorner);

            // Win32.GetCursorPos() returns logical pixels, WPF uses physical/device pixels.
            // Need to scale from logical to physical if user has a custom UI scaling.
            _uiScaling = PresentationSource.FromVisual(_dragAdorner).CompositionTarget.TransformToDevice;

            DragDrop.AddGiveFeedbackHandler(sourceItemVisualizer, GiveFeedback);
        }

        private void GiveFeedback(object sender, GiveFeedbackEventArgs giveFeedbackEventArgs)
        {
            var w32Mouse = new Win32.Point();
            Win32.GetCursorPos(ref w32Mouse);
            _dragAdorner.OffsetLeft = w32Mouse.X / _uiScaling.M11 + _offset.X;
            _dragAdorner.OffsetTop = w32Mouse.Y / _uiScaling.M22 + _offset.Y;
        }

        private static Point GetWindowPosition(Window window)
        {
            // Window.Left and Top are incorrect when maximized
            if (window.WindowState == WindowState.Maximized)
            {
                var leftField = typeof(Window).GetField("_actualLeft",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var topField = typeof(Window).GetField("_actualTop",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return new Point((double) leftField.GetValue(window), (double) topField.GetValue(window));
            }
            else
            {
                return new Point(window.Left, window.Top);
            }
        }

        public void Dispose()
        {
            DragDrop.RemoveGiveFeedbackHandler(SourceItemVisualizer, GiveFeedback);
            _adornerLayer.Remove(_dragAdorner);
        }
    }
}