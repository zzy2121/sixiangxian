using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Views
{
    public partial class CalendarView : Window
    {
        private readonly DataService _dataService;
        private readonly TagService _tagService;
        private DateTime _currentDate;
        private string _currentViewMode = "Month";
        private List<TaskItem> _allTasks;

        public CalendarView(DataService dataService, TagService tagService)
        {
            InitializeComponent();
            _dataService = dataService;
            _tagService = tagService;
            _currentDate = DateTime.Today;
            
            InitializeFilters();
            cmbViewMode.SelectedIndex = 0; // 默认月视图
            LoadTasks();
            UpdateCalendarView();
        }

        private void InitializeFilters()
        {
            // 初始化标签筛选
            var allTags = _tagService.GetAllTags();
            foreach (var tag in allTags)
            {
                var item = new ComboBoxItem 
                { 
                    Content = tag.Name, 
                    Tag = tag,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tag.Color))
                };
                cmbTagFilter.Items.Add(item);
            }

            // 初始化分类筛选
            foreach (TaskCategory category in Enum.GetValues<TaskCategory>())
            {
                cmbCategoryFilter.Items.Add(new ComboBoxItem 
                { 
                    Content = GetCategoryDescription(category), 
                    Tag = category 
                });
            }

            cmbTagFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndex = 0;
        }

        private void LoadTasks()
        {
            _allTasks = _dataService.GetAllTasks();
        }

        private void UpdateCalendarView()
        {
            txtCurrentDate.Text = _currentDate.ToString("yyyy年MM月", CultureInfo.CurrentCulture);
            
            switch (_currentViewMode)
            {
                case "Month":
                    ShowMonthView();
                    break;
                case "Week":
                    ShowWeekView();
                    break;
                case "Day":
                    ShowDayView();
                    break;
            }
        }

        private void ShowMonthView()
        {
            MonthView.Visibility = Visibility.Visible;
            WeekView.Visibility = Visibility.Collapsed;
            DayView.Visibility = Visibility.Collapsed;

            CalendarGrid.Children.Clear();
            
            var firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            var startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);

            for (int i = 0; i < 42; i++) // 6周 x 7天
            {
                var date = startDate.AddDays(i);
                var dayPanel = CreateDayPanel(date, date.Month == _currentDate.Month);
                CalendarGrid.Children.Add(dayPanel);
            }
        }

        private Border CreateDayPanel(DateTime date, bool isCurrentMonth)
        {
            var border = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5),
                Background = isCurrentMonth ? Brushes.White : Brushes.LightGray,
                Margin = new Thickness(1, 1, 1, 1)
            };

            var stackPanel = new StackPanel();
            
            // 日期标题
            var dateText = new TextBlock
            {
                Text = date.Day.ToString(),
                FontWeight = date.Date == DateTime.Today ? FontWeights.Bold : FontWeights.Normal,
                Foreground = date.Date == DateTime.Today ? Brushes.Red : (isCurrentMonth ? Brushes.Black : Brushes.Gray),
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5, 2, 5, 2)
            };
            stackPanel.Children.Add(dateText);

            // 获取当天任务
            var dayTasks = _allTasks.Where(t => t.DueDate.Date == date.Date).Take(3).ToList();
            foreach (var task in dayTasks)
            {
                var taskBlock = new TextBlock
                {
                    Text = task.Title,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(GetQuadrantColor(task.Quadrant)),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(2, 1, 2, 1),
                    MaxHeight = 15
                };
                stackPanel.Children.Add(taskBlock);
            }

            if (dayTasks.Count > 3)
            {
                var moreText = new TextBlock
                {
                    Text = $"还有{_allTasks.Count(t => t.DueDate.Date == date.Date) - 3}个任务...",
                    FontSize = 9,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(2, 1, 2, 1)
                };
                stackPanel.Children.Add(moreText);
            }

            border.Child = stackPanel;
            border.MouseLeftButtonDown += (s, e) => DayPanel_Click(date);
            
            return border;
        }

        private void ShowWeekView()
        {
            MonthView.Visibility = Visibility.Collapsed;
            WeekView.Visibility = Visibility.Visible;
            DayView.Visibility = Visibility.Collapsed;

            // 获取当前周的开始日期（周日）
            var startOfWeek = _currentDate.AddDays(-(int)_currentDate.DayOfWeek);
            
            // 更新周标题
            txtWeekDay1.Text = $"{startOfWeek:MM/dd}\n周日";
            txtWeekDay2.Text = $"{startOfWeek.AddDays(1):MM/dd}\n周一";
            txtWeekDay3.Text = $"{startOfWeek.AddDays(2):MM/dd}\n周二";
            txtWeekDay4.Text = $"{startOfWeek.AddDays(3):MM/dd}\n周三";
            txtWeekDay5.Text = $"{startOfWeek.AddDays(4):MM/dd}\n周四";
            txtWeekDay6.Text = $"{startOfWeek.AddDays(5):MM/dd}\n周五";
            txtWeekDay7.Text = $"{startOfWeek.AddDays(6):MM/dd}\n周六";

            // 创建时间网格
            CreateWeekTimeGrid(startOfWeek);
        }

        private void CreateWeekTimeGrid(DateTime startOfWeek)
        {
            WeekTimeGrid.Children.Clear();
            WeekTimeGrid.RowDefinitions.Clear();
            WeekTimeGrid.ColumnDefinitions.Clear();

            // 创建列定义
            WeekTimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            for (int i = 0; i < 7; i++)
            {
                WeekTimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // 创建24小时的行
            for (int hour = 0; hour < 24; hour++)
            {
                WeekTimeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
                
                // 时间标签
                var timeLabel = new TextBlock
                {
                    Text = $"{hour:00}:00",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 10
                };
                Grid.SetRow(timeLabel, hour);
                Grid.SetColumn(timeLabel, 0);
                WeekTimeGrid.Children.Add(timeLabel);

                // 为每一天创建时间槽
                for (int day = 0; day < 7; day++)
                {
                    var dayDate = startOfWeek.AddDays(day);
                    var timeSlot = CreateTimeSlot(dayDate, hour);
                    Grid.SetRow(timeSlot, hour);
                    Grid.SetColumn(timeSlot, day + 1);
                    WeekTimeGrid.Children.Add(timeSlot);
                }
            }
        }

        private Border CreateTimeSlot(DateTime date, int hour)
        {
            var border = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5),
                Background = Brushes.White
            };

            var stackPanel = new StackPanel();
            
            // 获取该时间段的任务
            var hourTasks = _allTasks.Where(t => 
                t.DueDate.Date == date.Date && 
                !t.IsAllDay && 
                t.StartTime.Hour == hour).ToList();

            foreach (var task in hourTasks)
            {
                var taskBlock = new TextBlock
                {
                    Text = task.Title,
                    FontSize = 9,
                    Foreground = new SolidColorBrush(GetQuadrantColor(task.Quadrant)),
                    Background = new SolidColorBrush(GetQuadrantColor(task.Quadrant)) { Opacity = 0.1 },
                    Padding = new Thickness(2, 2, 2, 2),
                    Margin = new Thickness(1, 1, 1, 1),
                    TextWrapping = TextWrapping.Wrap
                };
                stackPanel.Children.Add(taskBlock);
            }

            border.Child = stackPanel;
            border.MouseLeftButtonDown += (s, e) => TimeSlot_Click(date, hour);
            
            return border;
        }

        private void ShowDayView()
        {
            MonthView.Visibility = Visibility.Collapsed;
            WeekView.Visibility = Visibility.Collapsed;
            DayView.Visibility = Visibility.Visible;

            txtCurrentDate.Text = _currentDate.ToString("yyyy年MM月dd日 dddd", CultureInfo.CurrentCulture);
        }

        private Color GetQuadrantColor(TaskQuadrant quadrant)
        {
            return quadrant switch
            {
                TaskQuadrant.UrgentImportant => Colors.Red,
                TaskQuadrant.UrgentNotImportant => Colors.Orange,
                TaskQuadrant.ImportantNotUrgent => Colors.Blue,
                TaskQuadrant.NotUrgentNotImportant => Colors.Gray,
                _ => Colors.Black
            };
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
        private void PrevMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentViewMode switch
            {
                "Month" => _currentDate.AddMonths(-1),
                "Week" => _currentDate.AddDays(-7),
                "Day" => _currentDate.AddDays(-1),
                _ => _currentDate
            };
            UpdateCalendarView();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentViewMode switch
            {
                "Month" => _currentDate.AddMonths(1),
                "Week" => _currentDate.AddDays(7),
                "Day" => _currentDate.AddDays(1),
                _ => _currentDate
            };
            UpdateCalendarView();
        }

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = DateTime.Today;
            UpdateCalendarView();
        }

        private void ViewMode_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (cmbViewMode.SelectedItem is ComboBoxItem item)
            {
                _currentViewMode = item.Tag.ToString();
                UpdateCalendarView();
            }
        }

        private void TagFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CategoryFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            LoadTasks(); // 重新加载任务
            
            // 应用标签筛选
            if (cmbTagFilter.SelectedItem is ComboBoxItem tagItem && tagItem.Tag is TaskTag selectedTag)
            {
                _allTasks = _allTasks.Where(t => t.Tags.Any(tag => tag.Name == selectedTag.Name)).ToList();
            }

            // 应用分类筛选
            if (cmbCategoryFilter.SelectedItem is ComboBoxItem categoryItem && categoryItem.Tag is TaskCategory selectedCategory)
            {
                _allTasks = _allTasks.Where(t => t.Category == selectedCategory).ToList();
            }

            UpdateCalendarView();
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            // 打开任务添加对话框，预设当前选中的日期
            var addTaskWindow = new TaskEditWindow(_dataService, _tagService, _currentDate);
            if (addTaskWindow.ShowDialog() == true)
            {
                LoadTasks();
                UpdateCalendarView();
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks();
            UpdateCalendarView();
        }

        private void DayPanel_Click(DateTime date)
        {
            _currentDate = date;
            _currentViewMode = "Day";
            cmbViewMode.SelectedIndex = 2;
            ShowDayView();
        }

        private void TimeSlot_Click(DateTime date, int hour)
        {
            var addTaskWindow = new TaskEditWindow(_dataService, _tagService, date, hour);
            if (addTaskWindow.ShowDialog() == true)
            {
                LoadTasks();
                UpdateCalendarView();
            }
        }
    }
}