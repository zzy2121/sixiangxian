using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Views
{
    public partial class TaskEditWindow : Window
    {
        private readonly DataService _dataService;
        private readonly TagService _tagService;
        private TaskItem _currentTask;
        private List<TaskTag> _selectedTags;
        private bool _isEditMode;

        // 构造函数 - 新建任务
        public TaskEditWindow(DataService dataService, TagService tagService, DateTime? defaultDate = null, int? defaultHour = null)
        {
            InitializeComponent();
            _dataService = dataService;
            _tagService = tagService;
            _selectedTags = new List<TaskTag>();
            _isEditMode = false;
            
            InitializeControls();
            
            if (defaultDate.HasValue)
            {
                dpDueDate.SelectedDate = defaultDate.Value;
                dpStartDate.SelectedDate = defaultDate.Value;
                dpEndDate.SelectedDate = defaultDate.Value;
                
                if (defaultHour.HasValue)
                {
                    chkAllDay.IsChecked = false;
                    cmbStartHour.SelectedValue = defaultHour.Value;
                    cmbEndHour.SelectedValue = defaultHour.Value + 1;
                }
            }
            
            txtWindowTitle.Text = "添加任务";
        }

        // 构造函数 - 编辑任务
        public TaskEditWindow(DataService dataService, TagService tagService, TaskItem task)
        {
            InitializeComponent();
            _dataService = dataService;
            _tagService = tagService;
            _currentTask = task;
            _selectedTags = new List<TaskTag>(task.Tags);
            _isEditMode = true;
            
            InitializeControls();
            LoadTaskData();
            
            txtWindowTitle.Text = "编辑任务";
        }

        private void InitializeControls()
        {
            // 初始化象限下拉框
            cmbQuadrant.Items.Add(new ComboBoxItem { Content = "紧急且重要", Tag = TaskQuadrant.UrgentImportant });
            cmbQuadrant.Items.Add(new ComboBoxItem { Content = "紧急但不重要", Tag = TaskQuadrant.UrgentNotImportant });
            cmbQuadrant.Items.Add(new ComboBoxItem { Content = "重要但不紧急", Tag = TaskQuadrant.ImportantNotUrgent });
            cmbQuadrant.Items.Add(new ComboBoxItem { Content = "既不紧急也不重要", Tag = TaskQuadrant.NotUrgentNotImportant });

            // 初始化分类下拉框
            foreach (TaskCategory category in Enum.GetValues<TaskCategory>())
            {
                cmbCategory.Items.Add(new ComboBoxItem 
                { 
                    Content = GetCategoryDescription(category), 
                    Tag = category 
                });
            }

            // 初始化状态下拉框
            cmbStatus.Items.Add(new ComboBoxItem { Content = "未开始", Tag = TaskStatus.NotStarted });
            cmbStatus.Items.Add(new ComboBoxItem { Content = "进行中", Tag = TaskStatus.InProgress });
            cmbStatus.Items.Add(new ComboBoxItem { Content = "已完成", Tag = TaskStatus.Completed });

            // 初始化优先级下拉框
            for (int i = 1; i <= 5; i++)
            {
                cmbPriority.Items.Add(new ComboBoxItem { Content = i.ToString(), Tag = i });
            }

            // 初始化时间下拉框
            for (int i = 0; i < 24; i++)
            {
                cmbStartHour.Items.Add(new ComboBoxItem { Content = i.ToString("00"), Tag = i });
                cmbEndHour.Items.Add(new ComboBoxItem { Content = i.ToString("00"), Tag = i });
            }

            for (int i = 0; i < 60; i += 15)
            {
                cmbStartMinute.Items.Add(new ComboBoxItem { Content = i.ToString("00"), Tag = i });
                cmbEndMinute.Items.Add(new ComboBoxItem { Content = i.ToString("00"), Tag = i });
            }

            // 设置默认值
            cmbQuadrant.SelectedIndex = 0;
            cmbCategory.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            cmbPriority.SelectedIndex = 0;
            cmbStartHour.SelectedIndex = 9; // 9:00
            cmbEndHour.SelectedIndex = 10; // 10:00
            cmbStartMinute.SelectedIndex = 0; // :00
            cmbEndMinute.SelectedIndex = 0; // :00
            
            dpDueDate.SelectedDate = DateTime.Today;
            dpStartDate.SelectedDate = DateTime.Today;
            dpEndDate.SelectedDate = DateTime.Today;

            LoadAvailableTags();
            UpdateSelectedTags();
        }

        private void LoadTaskData()
        {
            txtTitle.Text = _currentTask.Title;
            txtDescription.Text = _currentTask.Description;
            dpDueDate.SelectedDate = _currentTask.DueDate;
            
            // 设置象限
            foreach (ComboBoxItem item in cmbQuadrant.Items)
            {
                if ((TaskQuadrant)item.Tag == _currentTask.Quadrant)
                {
                    cmbQuadrant.SelectedItem = item;
                    break;
                }
            }

            // 设置分类
            foreach (ComboBoxItem item in cmbCategory.Items)
            {
                if ((TaskCategory)item.Tag == _currentTask.Category)
                {
                    cmbCategory.SelectedItem = item;
                    break;
                }
            }

            // 设置状态
            foreach (ComboBoxItem item in cmbStatus.Items)
            {
                if ((TaskStatus)item.Tag == _currentTask.Status)
                {
                    cmbStatus.SelectedItem = item;
                    break;
                }
            }

            // 设置优先级
            foreach (ComboBoxItem item in cmbPriority.Items)
            {
                if ((int)item.Tag == _currentTask.Priority)
                {
                    cmbPriority.SelectedItem = item;
                    break;
                }
            }

            // 设置时间
            chkAllDay.IsChecked = _currentTask.IsAllDay;
            if (!_currentTask.IsAllDay)
            {
                dpStartDate.SelectedDate = _currentTask.StartTime.Date;
                dpEndDate.SelectedDate = _currentTask.EndTime.Date;
                cmbStartHour.SelectedValue = _currentTask.StartTime.Hour;
                cmbEndHour.SelectedValue = _currentTask.EndTime.Hour;
                cmbStartMinute.SelectedValue = _currentTask.StartTime.Minute;
                cmbEndMinute.SelectedValue = _currentTask.EndTime.Minute;
            }

            UpdateSelectedTags();
        }

        private void LoadAvailableTags()
        {
            pnlAvailableTags.Children.Clear();
            var allTags = _tagService.GetAllTags();

            if (allTags == null || !allTags.Any())
            {
                var noTagsText = new TextBlock
                {
                    Text = "暂无可用标签，点击下方按钮创建新标签",
                    Foreground = Brushes.Gray,
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(5)
                };
                pnlAvailableTags.Children.Add(noTagsText);
                return;
            }

            foreach (var tag in allTags)
            {
                var button = new Button
                {
                    Content = tag.Name,
                    Margin = new Thickness(3, 3, 3, 3),
                    Padding = new Thickness(10, 6, 10, 6),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tag.Color)) { Opacity = 0.3 },
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tag.Color)),
                    BorderThickness = new Thickness(1),
                    Tag = tag,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    ToolTip = $"{tag.Name} ({tag.TagType})"
                };
                
                // 添加悬停效果
                button.MouseEnter += (s, e) => 
                {
                    if (s is Button btn)
                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tag.Color)) { Opacity = 0.6 };
                };
                button.MouseLeave += (s, e) => 
                {
                    if (s is Button btn)
                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tag.Color)) { Opacity = 0.3 };
                };
                
                button.Click += TagButton_Click;
                pnlAvailableTags.Children.Add(button);
            }
        }

        private void UpdateSelectedTags()
        {
            pnlSelectedTags.Children.Clear();

            foreach (var tag in _selectedTags)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tag.Color)) { Opacity = 0.5 },
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tag.Color)),
                    BorderThickness = new Thickness(1, 1, 1, 1),
                    CornerRadius = new CornerRadius(3),
                    Margin = new Thickness(2, 2, 2, 2),
                    Padding = new Thickness(6, 3, 6, 3)
                };

                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                
                var textBlock = new TextBlock 
                { 
                    Text = tag.Name, 
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                
                var removeButton = new Button
                {
                    Content = "×",
                    Width = 16,
                    Height = 16,
                    FontSize = 10,
                    Padding = new Thickness(0, 0, 0, 0),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0, 0, 0, 0),
                    Tag = tag
                };
                
                removeButton.Click += RemoveTag_Click;
                
                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(removeButton);
                border.Child = stackPanel;
                
                pnlSelectedTags.Children.Add(border);
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

        // 事件处理
        private void AllDay_Changed(object sender, RoutedEventArgs e)
        {
            pnlTimeRange.Visibility = chkAllDay.IsChecked == true ? Visibility.Collapsed : Visibility.Visible;
        }

        private void TagButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TaskTag tag)
            {
                if (!_selectedTags.Any(t => t.Name == tag.Name))
                {
                    _selectedTags.Add(tag);
                    UpdateSelectedTags();
                }
            }
        }

        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TaskTag tag)
            {
                _selectedTags.RemoveAll(t => t.Name == tag.Name);
                UpdateSelectedTags();
            }
        }

        private void CreateTag_Click(object sender, RoutedEventArgs e)
        {
            var createTagWindow = new CreateTagWindow(_tagService);
            if (createTagWindow.ShowDialog() == true)
            {
                LoadAvailableTags();
            }
        }

        private void ClearTags_Click(object sender, RoutedEventArgs e)
        {
            _selectedTags.Clear();
            UpdateSelectedTags();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                var task = _isEditMode ? _currentTask : new TaskItem();
                
                task.Title = txtTitle.Text.Trim();
                task.Description = txtDescription.Text.Trim();
                task.DueDate = dpDueDate.SelectedDate ?? DateTime.Today;
                task.Quadrant = (TaskQuadrant)((ComboBoxItem)cmbQuadrant.SelectedItem).Tag;
                task.Category = (TaskCategory)((ComboBoxItem)cmbCategory.SelectedItem).Tag;
                task.Status = (TaskStatus)((ComboBoxItem)cmbStatus.SelectedItem).Tag;
                task.Priority = (int)((ComboBoxItem)cmbPriority.SelectedItem).Tag;
                task.Tags = new List<TaskTag>(_selectedTags);
                task.IsAllDay = chkAllDay.IsChecked == true;

                if (!task.IsAllDay)
                {
                    var startDate = dpStartDate.SelectedDate ?? DateTime.Today;
                    var endDate = dpEndDate.SelectedDate ?? DateTime.Today;
                    var startHour = (int)((ComboBoxItem)cmbStartHour.SelectedItem).Tag;
                    var endHour = (int)((ComboBoxItem)cmbEndHour.SelectedItem).Tag;
                    var startMinute = (int)((ComboBoxItem)cmbStartMinute.SelectedItem).Tag;
                    var endMinute = (int)((ComboBoxItem)cmbEndMinute.SelectedItem).Tag;

                    task.StartTime = startDate.AddHours(startHour).AddMinutes(startMinute);
                    task.EndTime = endDate.AddHours(endHour).AddMinutes(endMinute);
                }

                if (_isEditMode)
                {
                    _dataService.UpdateTask(task);
                }
                else
                {
                    _dataService.AddTask(task);
                }

                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("请输入任务标题", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTitle.Focus();
                return false;
            }

            if (dpDueDate.SelectedDate == null)
            {
                MessageBox.Show("请选择截止日期", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpDueDate.Focus();
                return false;
            }

            if (chkAllDay.IsChecked == false)
            {
                if (dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null)
                {
                    MessageBox.Show("请选择开始和结束日期", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var startTime = (dpStartDate.SelectedDate ?? DateTime.Today)
                    .AddHours((int)((ComboBoxItem)cmbStartHour.SelectedItem).Tag)
                    .AddMinutes((int)((ComboBoxItem)cmbStartMinute.SelectedItem).Tag);
                
                var endTime = (dpEndDate.SelectedDate ?? DateTime.Today)
                    .AddHours((int)((ComboBoxItem)cmbEndHour.SelectedItem).Tag)
                    .AddMinutes((int)((ComboBoxItem)cmbEndMinute.SelectedItem).Tag);

                if (endTime <= startTime)
                {
                    MessageBox.Show("结束时间必须晚于开始时间", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }
    }
}