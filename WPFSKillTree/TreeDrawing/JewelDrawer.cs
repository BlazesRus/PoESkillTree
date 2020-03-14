﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.ViewModels.Equipment;

namespace PoESkillTree.TreeDrawing
{
    public class JewelDrawer
    {
        private enum JewelType
        {
            Abyss,
            Blue,
            Green,
            Red,
            Prismatic,
        }

        private static readonly IReadOnlyDictionary<JewelType, string> AssetNames = new Dictionary<JewelType, string>
        {
            {JewelType.Abyss, "JewelSocketActiveAbyss"},
            {JewelType.Blue, "JewelSocketActiveBlue"},
            {JewelType.Green, "JewelSocketActiveGreen"},
            {JewelType.Red, "JewelSocketActiveRed"},
            {JewelType.Prismatic, "JewelSocketActivePrismatic"},
        };

        private readonly IReadOnlyDictionary<string, BitmapImage> _assets;
        private readonly IReadOnlyDictionary<ushort, SkillNode> _skillNodes;

        private readonly Dictionary<(JewelType type, bool isClusterSocket), (Size, ImageBrush)> _brushes =
            new Dictionary<(JewelType, bool), (Size, ImageBrush)>();

        private IReadOnlyList<InventoryItemViewModel> _jewelViewModels;

        public JewelDrawer(
            IReadOnlyDictionary<string, BitmapImage> assets, IReadOnlyDictionary<ushort, SkillNode> skillNodes)
        {
            _assets = assets;
            _skillNodes = skillNodes;
            Visual = new DrawingVisual();
            _jewelViewModels = Array.Empty<InventoryItemViewModel>();
        }

        public DrawingVisual Visual { get; }

        public IReadOnlyList<InventoryItemViewModel> JewelViewModels
        {
            get => _jewelViewModels;
            set
            {
                foreach (var oldVm in _jewelViewModels)
                {
                    oldVm.PropertyChanged -= JewelViewModelOnPropertyChanged;
                }
                foreach (var newVm in value)
                {
                    newVm.PropertyChanged += JewelViewModelOnPropertyChanged;
                }
                _jewelViewModels = value;
            }
        }

        private void JewelViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(InventoryItemViewModel.Item))
            {
                Draw();
            }
        }

        public void Draw()
        {
            using var dc = Visual.RenderOpen();
            foreach (var item in JewelViewModels.Select(vm => vm.Item).WhereNotNull())
            {
                if (item.Socket.HasValue)
                {
                    Draw(dc, item.Socket.Value, GetJewelType(item.Tags));
                }
            }
        }

        private void Draw(DrawingContext drawingContext, ushort nodeId, JewelType jewelType)
        {
            var node = _skillNodes[nodeId];
            var (size, brush) = _brushes.GetOrAdd((jewelType, node.ExpansionJewel != null), CreateBrush);
            drawingContext.DrawRectangle(brush, null,
                new Rect(node.Position.X - size.Width,
                    node.Position.Y - size.Height,
                    size.Width * 2,
                    size.Height * 2));
        }

        private static JewelType GetJewelType(Tags tags)
        {
            if (tags.HasFlag(Tags.AbyssJewel))
                return JewelType.Abyss;
            if (tags.HasFlag(Tags.StrJewel) && tags.HasFlag(Tags.DexJewel) && tags.HasFlag(Tags.IntJewel))
                return JewelType.Prismatic;
            if (tags.HasFlag(Tags.StrJewel))
                return JewelType.Red;
            if (tags.HasFlag(Tags.DexJewel))
                return JewelType.Green;
            if (tags.HasFlag(Tags.IntJewel))
                return JewelType.Blue;
            return JewelType.Red;
        }

        private (Size, ImageBrush) CreateBrush((JewelType type, bool isClusterSocket) key)
        {
            var assetName = AssetNames[key.type];
            if (key.isClusterSocket)
            {
                assetName += "Alt";
            }
            var image = _assets[assetName];
            var size = new Size(image.PixelWidth, image.PixelHeight);
            var brush = new ImageBrush
            {
                Stretch = Stretch.Uniform,
                ImageSource = image,
            };
            return (size, brush);
        }
    }
}