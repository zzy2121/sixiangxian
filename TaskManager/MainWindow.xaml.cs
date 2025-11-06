using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using TaskManager.Models;
using TaskManager.Services;
using TaskManager.Views;

namespace TaskManager
{
    public partial class MainWindow : Window
    {
        private readonly DataService _dataService;
        private readonly ExportService _exportService;
        private readonly TagService _tagService;
        private List<TaskItem> _currentTasks;
        private TaskItem _selectedTask;
        private bool _isQuadrantView = true;
        private CalendarView _calendarView;
        private MiniQuadrantWidget _miniWidget;
        private List<TaskTag> _quickSelectedTags;

        public MainWindow()
        {
            InitializeComponent();
            _dataService = new DataService();
            _exportService = new ExportService();
            _tagService = new TagService();
            _currentTasks = new List<TaskItem>();
            _quickSelectedTags = new List<TaskTag>();
            
            InitializeComboBoxes();
            LoadTodayTasksToQuadrants();
            UpdateDateTitle();
            UpdateStatus("应用程序已启动");
        }

        private void InitializeComboBoxes()
        {
            // 初始化象限下拉框 (列表视图用)
            cmbQuadrant.Items.Add(new ComboBoxItem { Content = "紧急且重要", Tag = TaskQuadrant.UrgentImportant });
            cmbQuadrant.Items.Add(new ComboBoxItem { Content = "紧急但不重要", Tag = TaskQuadrant.UrgentNotImportant });
            cmbQuadrant.Items.Add(new ComboBoxItem { Content = "重要但不紧急", Tag = TaskQuadrant.ImportantNotUrgent });
            cmbQuadrant.Items.Add(new ComboBoxItem { Content = "既不紧急也不重要", Tag = TaskQuadrant.NotUrgentNotImportant });

            // 初始化状态下拉框
            cmbStatus.Items.Add(new ComboBoxItem { Content = "未开始", Tag = TaskStatus.NotStarted });
            cmbStatus.Items.Add(new ComboBoxItem { Content = "进行中", Tag = TaskStatus.InProgress });
            cmbStatus.Items.Add(new ComboBoxItem { Content = "已完成", Tag = TaskStatus.Completed });

            // 初始化优先级下拉框
            for (int i = 1; i <= 5; i++)
            {
                cmbPriority.Items.Add(new ComboBoxItem { Content = i.ToString(), Tag = i });
            }

            // 初始化快速添加的象限下拉框
            cmbQuickQuadrant.Items.Add(new ComboBoxItem { Content = "紧急且重要", Tag = TaskQuadrant.UrgentImportant });
            cmbQuickQuadrant.Items.Add(new ComboBoxItem { Content = "紧急但不重要", Tag = TaskQuadrant.UrgentNotImportant });
            cmbQuickQuadrant.Items.Add(new ComboBoxItem { Content = "重要但不紧急", Tag = TaskQuadrant.ImportantNotUrgent });
            cmbQuickQuadrant.Items.Add(new ComboBoxItem { Content = "既不紧急也不重要", Tag = TaskQuadrant.NotUrgentNotImportant });

            // 初始化快速添加的分类下拉框
            foreach (TaskCategory category in Enum.GetValues<TaskCategory>())
            {
                cmbQuickCategory.Items.Add(new ComboBoxItem 
                { 
                    Content = GetCategoryDescription(category), 
                    Tag = category 
                });
            }

            // 设置默认值
            cmbQuadrant.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            cmbPriority.SelectedIndex = 0;
            cmbQuickQuadrant.SelectedIndex = 0;
            cmbQuickCategory.SelectedIndex = 0;
            dpDueDate.SelectedDate = DateTime.Today;
            dpQuickDueDate.SelectedDate = DateTime.Today;

            // 初始化快速添加的标签
            LoadQuickAddTags();
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

        private void UpdateDateTitle()
        {
            txtDateTitle.Text = DateTime.Today.ToString("yyyy年MM月dd日 dddd");
        }

        private void LoadTodayTasksToQuadrants()
        {
            var todayTasks = _dataService.GetTodayTasks();
            
            // 按象限分组任务
            var quadrant1Tasks = todayTasks.Where(t => t.Quadrant == TaskQuadrant.UrgentImportant).ToList();
            var quadrant2Tasks = todayTasks.Where(t => t.Quadrant == TaskQuadrant.UrgentNotImportant).ToList();
            var quadrant3Tasks = todayTasks.Where(t => t.Quadrant == TaskQuadrant.ImportantNotUrgent).ToList();
            var quadrant4Tasks = todayTasks.Where(t => t.Quadrant == TaskQuadrant.NotUrgentNotImportant).ToList();

            // 设置ItemsControl的数据源
            lstQuadrant1.ItemsSource = quadrant1Tasks;
            lstQuadrant2.ItemsSource = quadrant2Tasks;
            lstQuadrant3.ItemsSource = quadrant3Tasks;
            lstQuadrant4.ItemsSource = quadrant4Tasks;

            UpdateTaskCount(todayTasks.Count);
            UpdateStatus($"已加载今日任务 {todayTasks.Count} 个");
        }

        private void UpdateTaskCount(int count)
        {
            txtTaskCount.Text = $"今日任务: {count} 个";
        }

        private void LoadAllTasks()
        {
            _currentTasks = _dataService.GetAllTasks();
            RefreshTaskList();
            UpdateStatus($"已加载 {_currentTasks.Count} 个任务");
        }

        private void RefreshTaskList()
        {
            dgTasks.ItemsSource = null;
            dgTasks.ItemsSource = _currentTasks;
        }

        private void UpdateStatus(string message)
        {
            txtStatus.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        // 菜单和按钮事件处理
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new TaskEditWindow(_dataService, _tagService);
            if (addWindow.ShowDialog() == true)
            {
                RefreshCurrentView();
                UpdateStatus("任务已添加");
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTask != null)
            {
                var editWindow = new TaskEditWindow(_dataService, _tagService, _selectedTask);
                if (editWindow.ShowDialog() == true)
                {
                    RefreshCurrentView();
                    UpdateStatus("任务已更新");
                }
            }
            else
            {
                MessageBox.Show("请先选择要编辑的任务", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTask != null)
            {
                var result = MessageBox.Show($"确定要删除任务 \"{_selectedTask.Title}\" 吗？", 
                    "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _dataService.DeleteTask(_selectedTask.Id);
                    _selectedTask = null;
                    
                    // 更新按钮状态
                    if (btnUpdateTask != null)
                        btnUpdateTask.IsEnabled = false;
                    if (btnDeleteTask != null)
                        btnDeleteTask.IsEnabled = false;
                    
                    RefreshCurrentView();
                    UpdateStatus("任务已删除");
                }
            }
            else
            {
                MessageBox.Show("请先选择要删除的任务", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // 任务选择事件
        private void TaskItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is TaskItem task)
            {
                UpdateTaskSelection(task);
                UpdateStatus($"已选择任务: {task.Title}");
            }
        }

        private void UpdateTaskSelection(TaskItem task)
        {
            _selectedTask = task;
            // 更新列表视图中的按钮状态
            if (btnUpdateTask != null)
                btnUpdateTask.IsEnabled = task != null;
            if (btnDeleteTask != null)
                btnDeleteTask.IsEnabled = task != null;
        }

        private void RefreshCurrentView()
        {
            if (_isQuadrantView)
            {
                LoadTodayTasksToQuadrants();
            }
            else
            {
                RefreshTaskList();
            }
        }

        // 视图切换事件
        private void ShowAllTasks_Click(object sender, RoutedEventArgs e)
        {
            LoadAllTasks();
        }

        private void ShowTodayTasks_Click(object sender, RoutedEventArgs e)
        {
            _currentTasks = _dataService.GetTodayTasks();
            RefreshTaskList();
            UpdateStatus($"显示今日任务 ({_currentTasks.Count} 个)");
        }

        private void ShowQuadrantView_Click(object sender, RoutedEventArgs e)
        {
            _isQuadrantView = true;
            QuadrantView.Visibility = Visibility.Visible;
            ListView.Visibility = Visibility.Collapsed;
            LoadTodayTasksToQuadrants();
        }

        private void ShowListView_Click(object sender, RoutedEventArgs e)
        {
            _isQuadrantView = false;
            QuadrantView.Visibility = Visibility.Collapsed;
            ListView.Visibility = Visibility.Visible;
            LoadAllTasks();
        }

        // 快速添加任务
        private void QuickAddTask_Click(object sender, RoutedEventArgs e)
        {
            ShowQuickAddDialog(TaskQuadrant.UrgentImportant);
        }

        private void AddTaskToQuadrant1_Click(object sender, RoutedEventArgs e)
        {
            ShowQuickAddDialog(TaskQuadrant.UrgentImportant);
        }

        private void AddTaskToQuadrant2_Click(object sender, RoutedEventArgs e)
        {
            ShowQuickAddDialog(TaskQuadrant.UrgentNotImportant);
        }

        private void AddTaskToQuadrant3_Click(object sender, RoutedEventArgs e)
        {
            ShowQuickAddDialog(TaskQuadrant.ImportantNotUrgent);
        }

        private void AddTaskToQuadrant4_Click(object sender, RoutedEventArgs e)
        {
            ShowQuickAddDialog(TaskQuadrant.NotUrgentNotImportant);
        }

        private void ShowQuickAddDialog(TaskQuadrant quadrant)
        {
            // 设置默认象限
            foreach (ComboBoxItem item in cmbQuickQuadrant.Items)
            {
                if ((TaskQuadrant)item.Tag == quadrant)
                {
                    cmbQuickQuadrant.SelectedItem = item;
                    break;
                }
            }

            QuickAddPanel.Visibility = Visibility.Visible;
            txtQuickTitle.Focus();
        }

        private void ConfirmQuickAdd_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateQuickAddInput())
            {
                var task = CreateQuickTask();
                _dataService.AddTask(task);
                
                ClearQuickAddForm();
                QuickAddPanel.Visibility = Visibility.Collapsed;
                RefreshCurrentView();
                UpdateStatus("快速任务已添加");
            }
        }

        private void CancelQuickAdd_Click(object sender, RoutedEventArgs e)
        {
            QuickAddPanel.Visibility = Visibility.Collapsed;
        }

        private bool ValidateQuickAddInput()
        {
            if (string.IsNullOrWhiteSpace(txtQuickTitle.Text))
            {
                MessageBox.Show("请输入任务标题", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuickTitle.Focus();
                return false;
            }
            return true;
        }

        private TaskItem CreateQuickTask()
        {
            return new TaskItem
            {
                Title = txtQuickTitle.Text.Trim(),
                Description = txtQuickDescription.Text.Trim(),
                Quadrant = (TaskQuadrant)((ComboBoxItem)cmbQuickQuadrant.SelectedItem).Tag,
                Category = (TaskCategory)((ComboBoxItem)cmbQuickCategory.SelectedItem).Tag,
                DueDate = dpQuickDueDate.SelectedDate ?? DateTime.Today,
                Tags = new List<TaskTag>(_quickSelectedTags)
            };
        }

        private void ClearQuickAddForm()
        {
            txtQuickTitle.Text = "";
            txtQuickDescription.Text = "";
            dpQuickDueDate.SelectedDate = DateTime.Today;
            _quickSelectedTags.Clear();
            UpdateQuickSelectedTags();
        }

        private void LoadQuickAddTags()
        {
            // 如果控件存在才清空和添加
            if (pnlQuickTags != null)
            {
                pnlQuickTags.Children.Clear();
                var allTags = _tagService.GetAllTags();

                if (allTags == null || !allTags.Any())
                {
                    var noTagsText = new TextBlock
                    {
                        Text = "暂无可用标签，点击下方按钮创建新标签",
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(5),
                        FontSize = 11,
                        TextWrapping = TextWrapping.Wrap
                    };
                    pnlQuickTags.Children.Add(noTagsText);
                    return;
                }

                foreach (var tag in allTags)
                {
                    var button = new Button
                    {
                        Content = tag.Name,
                        Margin = new Thickness(3, 3, 3, 3),
                        Padding = new Thickness(8, 4, 8, 4),
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tag.Color)) { Opacity = 0.3 },
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tag.Color)),
                        BorderThickness = new Thickness(1),
                        Tag = tag,
                        FontSize = 11,
                        Cursor = System.Windows.Input.Cursors.Hand,
                        ToolTip = $"{tag.Name} ({tag.TagType})"
                    };
                    
                    // 添加悬停效果
                    button.MouseEnter += (s, e) => 
                    {
                        if (s is Button btn && btn.Tag is TaskTag hoverTag)
                            btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hoverTag.Color)) { Opacity = 0.6 };
                    };
                    button.MouseLeave += (s, e) => 
                    {
                        if (s is Button btn && btn.Tag is TaskTag leaveTag)
                            btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(leaveTag.Color)) { Opacity = 0.3 };
                    };
                    
                    button.Click += QuickTagButton_Click;
                    pnlQuickTags.Children.Add(button);
                }
            }
        }

        private void QuickTagButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TaskTag tag)
            {
                if (!_quickSelectedTags.Any(t => t.Name == tag.Name))
                {
                    _quickSelectedTags.Add(tag);
                    UpdateQuickSelectedTags();
                }
            }
        }

        private void UpdateQuickSelectedTags()
        {
            // 如果控件存在才清空和添加
            if (pnlQuickSelectedTags != null)
            {
                pnlQuickSelectedTags.Children.Clear();

                foreach (var tag in _quickSelectedTags)
                {
                    var border = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tag.Color)) { Opacity = 0.5 },
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tag.Color)),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(3),
                        Margin = new Thickness(2, 2, 2, 2),
                        Padding = new Thickness(4, 2, 4, 2)
                    };

                    var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    
                    var textBlock = new TextBlock 
                    { 
                        Text = tag.Name, 
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 3, 0),
                        FontSize = 10
                    };
                    
                    var removeButton = new Button
                    {
                        Content = "×",
                        Width = 14,
                        Height = 14,
                        FontSize = 8,
                        Padding = new Thickness(0),
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        Tag = tag
                    };
                    
                    removeButton.Click += QuickRemoveTag_Click;
                    
                    stackPanel.Children.Add(textBlock);
                    stackPanel.Children.Add(removeButton);
                    border.Child = stackPanel;
                    
                    pnlQuickSelectedTags.Children.Add(border);
                }
            }
        }

        private void QuickRemoveTag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TaskTag tag)
            {
                _quickSelectedTags.RemoveAll(t => t.Name == tag.Name);
                UpdateQuickSelectedTags();
            }
        }

        private void QuickCreateTag_Click(object sender, RoutedEventArgs e)
        {
            var createTagWindow = new CreateTagWindow(_tagService);
            if (createTagWindow.ShowDialog() == true)
            {
                LoadQuickAddTags();
            }
        }

        private void QuickClearTags_Click(object sender, RoutedEventArgs e)
        {
            _quickSelectedTags.Clear();
            UpdateQuickSelectedTags();
        }

        // 导出功能
        private void ExportTodayToWord_Click(object sender, RoutedEventArgs e)
        {
            ExportTasks("word");
        }

        private void ExportTodayToPdf_Click(object sender, RoutedEventArgs e)
        {
            ExportTasks("pdf");
        }

        private void ExportTasks(string format)
        {
            try
            {
                var todayTasks = _dataService.GetTodayTasks();
                if (!todayTasks.Any())
                {
                    MessageBox.Show("今日没有任务可导出", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = format.ToLower() == "word" ? "Word文档 (*.docx)|*.docx" : "PDF文档 (*.pdf)|*.pdf",
                    FileName = $"今日任务_{DateTime.Today:yyyyMMdd}.{(format.ToLower() == "word" ? "docx" : "pdf")}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    if (format.ToLower() == "word")
                    {
                        _exportService.ExportToWord(todayTasks, saveDialog.FileName);
                    }
                    else
                    {
                        _exportService.ExportToPdf(todayTasks, saveDialog.FileName);
                    }
                    
                    UpdateStatus($"任务已导出到 {format.ToUpper()} 文件");
                    MessageBox.Show($"任务已成功导出到 {saveDialog.FileName}", "导出成功", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus($"导出失败: {ex.Message}");
            }
        }

        // 其他功能
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("四象限任务管理器 v1.0\n\n基于四象限紧急程度管理的工作任务软件\n\n功能特点:\n• 四象限任务分类\n• 任务搜索和筛选\n• 导出Word/PDF文档\n• 本地数据存储", 
                "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowCalendarView_Click(object sender, RoutedEventArgs e)
        {
            if (_calendarView == null)
            {
                _calendarView = new CalendarView(_dataService, _tagService);
                _calendarView.Closed += (s, args) => _calendarView = null;
            }
            _calendarView.Show();
            _calendarView.Activate();
        }

        private void ToggleMiniWidget_Click(object sender, RoutedEventArgs e)
        {
            if (_miniWidget == null)
            {
                _miniWidget = new MiniQuadrantWidget(_dataService, _tagService, this);
                _miniWidget.Closed += (s, args) => 
                {
                    _miniWidget = null;
                    mnuMiniWidget.IsChecked = false;
                };
                _miniWidget.Show();
                mnuMiniWidget.IsChecked = true;
            }
            else
            {
                _miniWidget.Close();
                _miniWidget = null;
                mnuMiniWidget.IsChecked = false;
            }
        }

        private void ToggleTopmost_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
            mnuTopmost.IsChecked = this.Topmost;
        }

        private void ManageTags_Click(object sender, RoutedEventArgs e)
        {
            var tagManagerWindow = new TagManagerWindow(_tagService);
            tagManagerWindow.ShowDialog();
            LoadQuickAddTags(); // 刷新标签列表
        }

        private void ShowTaskFilter_Click(object sender, RoutedEventArgs e)
        {
            var filterWindow = new TaskFilterWindow(_dataService, _tagService);
            if (filterWindow.ShowDialog() == true)
            {
                // 应用筛选结果
                var filteredTasks = filterWindow.FilteredTasks;
                if (filteredTasks != null)
                {
                    _currentTasks = filteredTasks;
                    _isQuadrantView = false;
                    ShowListView_Click(null, null);
                    UpdateStatus($"筛选结果: {_currentTasks.Count} 个任务");
                }
            }
        }

        private void IntegrateSystemCalendar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var todayTasks = _dataService.GetTodayTasks();
                if (!todayTasks.Any())
                {
                    MessageBox.Show("今日没有任务可导出到系统日历", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "日历文件 (*.ics)|*.ics",
                    FileName = $"今日任务_{DateTime.Today:yyyyMMdd}.ics"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var integration = new SystemCalendarIntegration();
                    integration.ExportTasksToSystemCalendar(todayTasks);
                    
                    UpdateStatus("任务已导出到系统日历文件");
                    MessageBox.Show($"任务已成功导出到日历文件 {saveDialog.FileName}\n\n您可以将此文件导入到Outlook、Google日历等应用中。", 
                        "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出到系统日历失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus($"系统日历集成失败: {ex.Message}");
            }
        }

        // 缺少的方法实现
        private void RefreshTasks_Click(object sender, RoutedEventArgs e)
        {
            RefreshCurrentView();
            UpdateStatus("任务列表已刷新");
        }

        private void ShowQuadrant1_Click(object sender, RoutedEventArgs e)
        {
            if (_isQuadrantView)
            {
                UpdateStatus("显示紧急且重要任务");
            }
            else
            {
                _currentTasks = _dataService.GetTasksByQuadrant(TaskQuadrant.UrgentImportant);
                RefreshTaskList();
                UpdateStatus($"显示紧急且重要任务 ({_currentTasks.Count} 个)");
            }
        }

        private void ShowQuadrant2_Click(object sender, RoutedEventArgs e)
        {
            if (_isQuadrantView)
            {
                UpdateStatus("显示紧急但不重要任务");
            }
            else
            {
                _currentTasks = _dataService.GetTasksByQuadrant(TaskQuadrant.UrgentNotImportant);
                RefreshTaskList();
                UpdateStatus($"显示紧急但不重要任务 ({_currentTasks.Count} 个)");
            }
        }

        private void ShowQuadrant3_Click(object sender, RoutedEventArgs e)
        {
            if (_isQuadrantView)
            {
                UpdateStatus("显示重要但不紧急任务");
            }
            else
            {
                _currentTasks = _dataService.GetTasksByQuadrant(TaskQuadrant.ImportantNotUrgent);
                RefreshTaskList();
                UpdateStatus($"显示重要但不紧急任务 ({_currentTasks.Count} 个)");
            }
        }

        private void ShowQuadrant4_Click(object sender, RoutedEventArgs e)
        {
            if (_isQuadrantView)
            {
                UpdateStatus("显示既不紧急也不重要任务");
            }
            else
            {
                _currentTasks = _dataService.GetTasksByQuadrant(TaskQuadrant.NotUrgentNotImportant);
                RefreshTaskList();
                UpdateStatus($"显示既不紧急也不重要任务 ({_currentTasks.Count} 个)");
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "搜索任务...")
            {
                txtSearch.Text = "";
                txtSearch.Foreground = Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "搜索任务...";
                txtSearch.Foreground = Brushes.Gray;
            }
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (txtSearch.Text != "搜索任务..." && !string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchText = txtSearch.Text.ToLower();
                _currentTasks = _dataService.GetAllTasks()
                    .Where(t => t.Title.ToLower().Contains(searchText) || 
                               t.Description.ToLower().Contains(searchText))
                    .ToList();
                RefreshTaskList();
                UpdateStatus($"搜索结果: {_currentTasks.Count} 个任务");
            }
        }

        private void TaskList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgTasks.SelectedItem is TaskItem task)
            {
                EditTask_Click(sender, e);
            }
        }

        private void UpdateTask_Click(object sender, RoutedEventArgs e)
        {
            // 这个方法在新版本中被EditTask_Click替代
            EditTask_Click(sender, e);
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearQuickAddForm();
        }

        // 添加RefreshTasks方法供MiniWidget调用
        public void RefreshTasks()
        {
            RefreshCurrentView();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // 关闭所有子窗口
            _calendarView?.Close();
            _miniWidget?.Close();
            base.OnClosing(e);
        }
    }
}