using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Views
{
    public partial class MiniQuadrantWidget : Window
    {
        private readonly DataService _dataService;
        private readonly TagService _tagService;
        private readonly MainWindow _mainWindow;

        public MiniQuadrantWidget(DataService dataService, TagService tagService, MainWindow mainWindow)
        {
            InitializeComponent();
            _dataService = dataService;
            _tagService = tagService;
            _mainWindow = mainWindow;
            
            // 设置初始位置（右上角）
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width - 20;
            this.Top = 50;
            
            LoadTodayTasks();
        }

        private void LoadTodayTasks()
        {
            var todayTasks = _dataService.GetTodayTasks();
            
            // 按象限分组
            var quadrant1Tasks = todayTasks.Where(t => t.Quadrant == TaskQuadrant.UrgentImportant).ToList();
            var quadrant2Tasks = todayTasks.Where(t => t.Quadrant == TaskQuadrant.UrgentNotImportant).ToList();
            var quadrant3Tasks = todayTasks.Where(t => t.Quadrant == TaskQuadrant.ImportantNotUrgent).ToList();
            var quadrant4Tasks = todayTasks.Where(t => t.Quadrant == TaskQuadrant.NotUrgentNotImportant).ToList();

            // 更新四象限显示
            lstMiniQuadrant1.ItemsSource = quadrant1Tasks;
            lstMiniQuadrant2.ItemsSource = quadrant2Tasks;
            lstMiniQuadrant3.ItemsSource = quadrant3Tasks;
            lstMiniQuadrant4.ItemsSource = quadrant4Tasks;
        }

        private void Widget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 允许拖拽移动窗口
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void TaskItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is TaskItem task)
            {
                // 打开任务编辑窗口
                var editWindow = new TaskEditWindow(_dataService, _tagService, task);
                if (editWindow.ShowDialog() == true)
                {
                    LoadTodayTasks();
                    // 通知主窗口刷新
                    _mainWindow?.RefreshTasks();
                }
            }
        }



        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTodayTasks();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        // 公共方法供主窗口调用
        public void RefreshTasks()
        {
            LoadTodayTasks();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // 阻止真正关闭，只是隐藏
            e.Cancel = true;
            this.Hide();
        }
    }
}