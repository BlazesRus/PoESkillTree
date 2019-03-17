﻿using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.ItemFilter.Model;

namespace PoESkillTree.ItemFilter.Views
{
    /// <summary>
    /// Interaction logic for ItemFilterEditWindow.xaml
    /// </summary>
    public partial class ItemFilterEditWindow : MetroWindow
    {
        public Filter Filter { get; set; }

        public ItemFilterEditWindow(Filter filter)
        {
            Filter = filter;

            InitializeComponent();

            DataContext = this;

            Title = Filter.Name + " - " + Title;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private async void Description_Click(object sender, RoutedEventArgs e)
        {
            string description = (sender as Button).Tag.ToString();

            await this.ShowInfoAsync(description);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
