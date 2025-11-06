using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Views
{
    public partial class TagManagerWindow : Window
    {
        private readonly TagService _tagService;

        public TagManagerWindow(TagService tagService)
        {
            InitializeComponent();
            _tagService = tagService;
            LoadTags();
            
            dgTags.SelectionChanged += DgTags_SelectionChanged;
        }

        private void LoadTags()
        {
            var tags = _tagService.GetAllTags();
            dgTags.ItemsSource = tags;
        }

        private void DgTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var hasSelection = dgTags.SelectedItem != null;
            btnEditTag.IsEnabled = hasSelection;
            btnDeleteTag.IsEnabled = hasSelection;
        }

        private void CreateTag_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new CreateTagWindow(_tagService);
            if (createWindow.ShowDialog() == true)
            {
                LoadTags();
            }
        }

        private void EditTag_Click(object sender, RoutedEventArgs e)
        {
            if (dgTags.SelectedItem is TaskTag selectedTag)
            {
                var editWindow = new CreateTagWindow(_tagService, selectedTag);
                if (editWindow.ShowDialog() == true)
                {
                    LoadTags();
                }
            }
        }

        private void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            if (dgTags.SelectedItem is TaskTag selectedTag)
            {
                var result = MessageBox.Show($"确定要删除标签 '{selectedTag.Name}' 吗？", 
                    "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _tagService.DeleteCustomTag(selectedTag);
                    LoadTags();
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTags();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}