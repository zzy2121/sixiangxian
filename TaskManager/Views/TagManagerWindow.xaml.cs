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
            var isCustomTag = hasSelection && dgTags.SelectedItem is TaskTag selectedTag && _tagService.IsCustomTag(selectedTag);
            
            btnEditTag.IsEnabled = isCustomTag;
            btnDeleteTag.IsEnabled = isCustomTag;
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
                if (!_tagService.IsCustomTag(selectedTag))
                {
                    MessageBox.Show("只能删除自定义标签，预定义标签无法删除", "提示", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show($"确定要删除自定义标签 '{selectedTag.Name}' 吗？", 
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