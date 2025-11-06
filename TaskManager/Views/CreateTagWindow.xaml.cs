using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Views
{
    public partial class CreateTagWindow : Window
    {
        private readonly TagService _tagService;
        private string _selectedColor = "#2196F3";
        private TaskTag _editingTag = null;
        private bool _isEditMode = false;

        public CreateTagWindow(TagService tagService)
        {
            InitializeComponent();
            _tagService = tagService;
            InitializeControls();
        }

        // 编辑标签的构造函数
        public CreateTagWindow(TagService tagService, TaskTag existingTag) : this(tagService)
        {
            _editingTag = existingTag;
            _isEditMode = true;
            Title = "编辑标签";
            btnCreate.Content = "保存";
            
            txtTagName.Text = existingTag.Name;
            _selectedColor = existingTag.Color;
            rectColorPreview.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(existingTag.Color));
            
            foreach (ComboBoxItem item in cmbTagCategory.Items)
            {
                if ((TaskCategory)item.Tag == existingTag.Category)
                {
                    cmbTagCategory.SelectedItem = item;
                    break;
                }
            }
        }

        private void InitializeControls()
        {
            // 初始化分类下拉框
            foreach (TaskCategory category in Enum.GetValues<TaskCategory>())
            {
                cmbTagCategory.Items.Add(new ComboBoxItem 
                { 
                    Content = GetCategoryDescription(category), 
                    Tag = category 
                });
            }
            cmbTagCategory.SelectedIndex = 0;

            // 初始化预设颜色
            var presetColors = new[]
            {
                "#F44336", "#E91E63", "#9C27B0", "#673AB7", "#3F51B5", "#2196F3",
                "#03A9F4", "#00BCD4", "#009688", "#4CAF50", "#8BC34A", "#CDDC39",
                "#FFEB3B", "#FFC107", "#FF9800", "#FF5722", "#795548", "#9E9E9E",
                "#607D8B", "#000000"
            };

            foreach (var color in presetColors)
            {
                var border = new Border
                {
                    Width = 35,
                    Height = 35,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(3),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = color,
                    ToolTip = $"颜色: {color}"
                };

                border.MouseLeftButtonDown += PresetColor_Click;
                border.MouseEnter += (s, e) => border.BorderBrush = Brushes.Black;
                border.MouseLeave += (s, e) => border.BorderBrush = Brushes.Gray;
                
                pnlPresetColors.Children.Add(border);
            }
        }

        private string GetCategoryDescription(TaskCategory category)
        {
            return category switch
            {
                TaskCategory.Work => "工作",
                TaskCategory.Life => "生活",
                TaskCategory.Study => "学习",
                TaskCategory.Health => "健康", 
                TaskCategory.Entertainment => "娱乐",
                TaskCategory.Other => "其他",
                _ => "未知"
            };
        }

        private void PresetColor_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string color)
            {
                _selectedColor = color;
                rectColorPreview.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            }
        }

        private void SelectColor_Click(object sender, RoutedEventArgs e)
        {
            // 简化的颜色选择，使用预设颜色
            MessageBox.Show("请从下方预设颜色中选择，或者手动输入颜色代码", "颜色选择", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                var tagName = txtTagName.Text.Trim();
                var category = (TaskCategory)((ComboBoxItem)cmbTagCategory.SelectedItem).Tag;
                
                try
                {
                    if (_isEditMode)
                    {
                        // 编辑模式：更新现有标签
                        if (_tagService.IsCustomTag(_editingTag))
                        {
                            _tagService.UpdateCustomTag(_editingTag, tagName, _selectedColor, category);
                            DialogResult = true;
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("只能编辑自定义标签，预定义标签无法修改", "提示", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        // 创建模式：创建新标签
                        _tagService.CreateCustomTag(tagName, _selectedColor, category);
                        MessageBox.Show($"标签 '{tagName}' 创建成功！", "创建成功", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{(_isEditMode ? "更新" : "创建")}标签失败: {ex.Message}", "错误", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTagName.Text))
            {
                MessageBox.Show("请输入标签名称", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTagName.Focus();
                return false;
            }

            // 检查标签名称是否已存在（编辑模式下排除当前标签）
            var existingTags = _tagService.GetAllTags();
            var duplicateTag = existingTags.FirstOrDefault(t => 
                t.Name.Equals(txtTagName.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            
            if (duplicateTag != null)
            {
                // 如果是编辑模式且重复的标签就是当前编辑的标签，则允许
                if (_isEditMode && _editingTag != null && 
                    duplicateTag.Name == _editingTag.Name && duplicateTag.Color == _editingTag.Color)
                {
                    return true;
                }
                
                MessageBox.Show("该标签名称已存在", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTagName.Focus();
                return false;
            }

            return true;
        }
    }
}