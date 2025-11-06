using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class TagService
    {
        private readonly string _tagFilePath;
        private List<TaskTag> _predefinedTags;
        private List<TaskTag> _customTags;

        public TagService()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TaskManager");
            Directory.CreateDirectory(appDataPath);
            _tagFilePath = Path.Combine(appDataPath, "tags.json");
            InitializePredefinedTags();
            LoadCustomTags();
        }

        private void InitializePredefinedTags()
        {
            _predefinedTags = new List<TaskTag>
            {
                // 工作相关
                new TaskTag("会议", "#F44336", TaskCategory.Work, false),
                new TaskTag("项目", "#2196F3", TaskCategory.Work, false),
                new TaskTag("报告", "#FF9800", TaskCategory.Work, false),
                new TaskTag("邮件", "#9C27B0", TaskCategory.Work, false),
                new TaskTag("客户", "#E91E63", TaskCategory.Work, false),
                
                // 学习相关
                new TaskTag("阅读", "#4CAF50", TaskCategory.Study, false),
                new TaskTag("课程", "#00BCD4", TaskCategory.Study, false),
                new TaskTag("考试", "#FF5722", TaskCategory.Study, false),
                new TaskTag("研究", "#795548", TaskCategory.Study, false),
                
                // 生活相关
                new TaskTag("购物", "#CDDC39", TaskCategory.Life, false),
                new TaskTag("家务", "#607D8B", TaskCategory.Life, false),
                new TaskTag("出行", "#3F51B5", TaskCategory.Life, false),
                new TaskTag("社交", "#E91E63", TaskCategory.Life, false),
                
                // 健康相关
                new TaskTag("运动", "#4CAF50", TaskCategory.Health, false),
                new TaskTag("医疗", "#F44336", TaskCategory.Health, false),
                new TaskTag("休息", "#9E9E9E", TaskCategory.Health, false),
                
                // 娱乐相关
                new TaskTag("电影", "#673AB7", TaskCategory.Entertainment, false),
                new TaskTag("游戏", "#FF9800", TaskCategory.Entertainment, false),
                new TaskTag("音乐", "#E91E63", TaskCategory.Entertainment, false),
                
                // 其他
                new TaskTag("重要", "#F44336", TaskCategory.Other, false),
                new TaskTag("紧急", "#FF5722", TaskCategory.Other, false),
                new TaskTag("个人", "#9C27B0", TaskCategory.Other, false)
            };
        }

        public List<TaskTag> GetAllTags()
        {
            var allTags = new List<TaskTag>(_predefinedTags);
            allTags.AddRange(_customTags);
            return allTags;
        }

        public List<TaskTag> GetTagsByCategory(TaskCategory category)
        {
            return GetAllTags().Where(t => t.Category == category).ToList();
        }

        public TaskTag CreateCustomTag(string name, string color, TaskCategory category)
        {
            var tag = new TaskTag(name, color, category, true);
            _customTags.Add(tag);
            SaveCustomTags();
            return tag;
        }

        public bool UpdateCustomTag(TaskTag originalTag, string newName, string newColor, TaskCategory newCategory)
        {
            var existingTag = _customTags.FirstOrDefault(t => t.Name == originalTag.Name && t.Color == originalTag.Color);
            if (existingTag != null)
            {
                existingTag.Name = newName;
                existingTag.Color = newColor;
                existingTag.Category = newCategory;
                SaveCustomTags();
                return true;
            }
            return false;
        }

        public void DeleteCustomTag(TaskTag tag)
        {
            // 只能删除自定义标签，不能删除预定义标签
            var customTag = _customTags.FirstOrDefault(t => t.Name == tag.Name && t.Color == tag.Color);
            if (customTag != null)
            {
                _customTags.Remove(customTag);
                SaveCustomTags();
            }
        }

        public bool IsCustomTag(TaskTag tag)
        {
            return _customTags.Any(t => t.Name == tag.Name && t.Color == tag.Color);
        }

        public List<TaskTag> SearchTags(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAllTags();

            return GetAllTags().Where(t => 
                t.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        private void LoadCustomTags()
        {
            try
            {
                _customTags = new List<TaskTag>();
                if (File.Exists(_tagFilePath))
                {
                    var json = File.ReadAllText(_tagFilePath);
                    _customTags = JsonConvert.DeserializeObject<List<TaskTag>>(json) ?? new List<TaskTag>();
                }
            }
            catch (Exception ex)
            {
                _customTags = new List<TaskTag>();
                System.Windows.MessageBox.Show($"加载自定义标签数据时出错: {ex.Message}", "错误", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void SaveCustomTags()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_customTags, Formatting.Indented);
                File.WriteAllText(_tagFilePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"保存自定义标签数据时出错: {ex.Message}", "错误", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}