using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Views
{
    public partial class TaskFilterWindow : Window
    {
        private readonly DataService _dataService;
        private readonly TagService _tagService;
        public List<TaskItem> FilteredTasks { get; private set; }

        public TaskFilterWindow(DataService dataService, TagService tagService)
        {
            InitializeComponent();
            _dataService = dataService;
            _tagService = tagService;
            
            InitializeFilters();
        }

        private void InitializeFilters()
        {
            // 初始化象限筛选
            cmbQuadrantFilter.Items.Add(new ComboBoxItem { Content = "全部象限", Tag = "" });
            cmbQuadrantFilter.Items.Add(new ComboBoxItem { Content = "紧急且重要", Tag = TaskQuadrant.UrgentImportant });
            cmbQuadrantFilter.Items.Add(new ComboBoxItem { Content = "紧急但不重要", Tag = TaskQuadrant.UrgentNotImportant });
            cmbQuadrantFilter.Items.Add(new ComboBoxItem { Content = "重要但不紧急", Tag = TaskQuadrant.ImportantNotUrgent });
            cmbQuadrantFilter.Items.Add(new ComboBoxItem { Content = "既不紧急也不重要", Tag = TaskQuadrant.NotUrgentNotImportant });

            // 初始化状态筛选
            cmbStatusFilter.Items.Add(new ComboBoxItem { Content = "全部状态", Tag = "" });
            cmbStatusFilter.Items.Add(new ComboBoxItem { Content = "未开始", Tag = TaskStatus.NotStarted });
            cmbStatusFilter.Items.Add(new ComboBoxItem { Content = "进行中", Tag = TaskStatus.InProgress });
            cmbStatusFilter.Items.Add(new ComboBoxItem { Content = "已完成", Tag = TaskStatus.Completed });

            // 初始化分类筛选
            cmbCategoryFilter.Items.Add(new ComboBoxItem { Content = "全部分类", Tag = "" });
            foreach (TaskCategory category in Enum.GetValues<TaskCategory>())
            {
                cmbCategoryFilter.Items.Add(new ComboBoxItem 
                { 
                    Content = GetCategoryDescription(category), 
                    Tag = category 
                });
            }

            // 初始化标签筛选
            cmbTagFilter.Items.Add(new ComboBoxItem { Content = "全部标签", Tag = "" });
            var allTags = _tagService.GetAllTags();
            foreach (var tag in allTags)
            {
                cmbTagFilter.Items.Add(new ComboBoxItem { Content = tag.Name, Tag = tag });
            }

            // 设置默认选择
            cmbQuadrantFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndex = 0;
            cmbTagFilter.SelectedIndex = 0;

            // 设置日期范围
            dpStartDate.SelectedDate = DateTime.Today.AddDays(-30);
            dpEndDate.SelectedDate = DateTime.Today.AddDays(30);
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

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            var allTasks = _dataService.GetAllTasks();
            FilteredTasks = allTasks.AsEnumerable().ToList();

            // 应用象限筛选
            if (cmbQuadrantFilter.SelectedItem is ComboBoxItem quadrantItem && 
                quadrantItem.Tag is TaskQuadrant quadrant)
            {
                FilteredTasks = FilteredTasks.Where(t => t.Quadrant == quadrant).ToList();
            }

            // 应用状态筛选
            if (cmbStatusFilter.SelectedItem is ComboBoxItem statusItem && 
                statusItem.Tag is TaskStatus status)
            {
                FilteredTasks = FilteredTasks.Where(t => t.Status == status).ToList();
            }

            // 应用分类筛选
            if (cmbCategoryFilter.SelectedItem is ComboBoxItem categoryItem && 
                categoryItem.Tag is TaskCategory category)
            {
                FilteredTasks = FilteredTasks.Where(t => t.Category == category).ToList();
            }

            // 应用标签筛选
            if (cmbTagFilter.SelectedItem is ComboBoxItem tagItem && 
                tagItem.Tag is TaskTag selectedTag)
            {
                FilteredTasks = FilteredTasks.Where(t => t.Tags.Any(tag => tag.Name == selectedTag.Name)).ToList();
            }

            // 应用日期范围筛选
            if (dpStartDate.SelectedDate.HasValue)
            {
                FilteredTasks = FilteredTasks.Where(t => t.DueDate >= dpStartDate.SelectedDate.Value).ToList();
            }

            if (dpEndDate.SelectedDate.HasValue)
            {
                FilteredTasks = FilteredTasks.Where(t => t.DueDate <= dpEndDate.SelectedDate.Value).ToList();
            }

            // 应用关键词筛选
            if (!string.IsNullOrWhiteSpace(txtKeyword.Text))
            {
                var keyword = txtKeyword.Text.Trim();
                FilteredTasks = FilteredTasks.Where(t => 
                    t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            // 显示筛选结果
            dgFilterResults.ItemsSource = FilteredTasks;
            txtResultCount.Text = $"找到 {FilteredTasks.Count} 个匹配的任务";
        }

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            cmbQuadrantFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndex = 0;
            cmbTagFilter.SelectedIndex = 0;
            dpStartDate.SelectedDate = DateTime.Today.AddDays(-30);
            dpEndDate.SelectedDate = DateTime.Today.AddDays(30);
            txtKeyword.Text = "";
            
            dgFilterResults.ItemsSource = null;
            txtResultCount.Text = "";
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (FilteredTasks != null && FilteredTasks.Any())
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("请先应用筛选条件", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}