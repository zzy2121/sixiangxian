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

        public TagService()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TaskManager");
            Directory.CreateDirectory(appDataPath);
            _tagFilePath = Path.Combine(appDataPath, "tags.json");
            InitializePredefinedTags();
            LoadTags();
        }

        private void InitializePredefinedTags()
        {
            _predefinedTags = new List<TaskTag>
            {
                // 工作相关
                new TaskTag("会议", "#F44336", TaskCategory.Work),
                new TaskTag("项目", "#2196F3", TaskCategory.Work),
                new TaskTag("报告", "#FF9800", TaskCategory.Work),
                new TaskTag("邮件", "#9C27B0", TaskCategory.Work),
                new TaskTag("客户", "#E91E63", TaskCategory.Work),
                
                // 学习相关
                new TaskTag("阅读", "#4CAF50", TaskCategory.Study),
                new TaskTag("课程", "#00BCD4", TaskCategory.Study),
                new TaskTag("考试", "#FF5722", TaskCategory.Study),
                new TaskTag("研究", "#795548", TaskCategory.Study),
                
                // 生活相关
                new TaskTag("购物", "#CDDC39", TaskCategory.Life),
                new TaskTag("家务", "#607D8B", TaskCategory.Life),
                new TaskTag("出行", "#3F51B5", TaskCategory.Life),
                new TaskTag("社交", "#E91E63", TaskCategory.Life),
                
                // 健康相关
                new TaskTag("运动", "#4CAF50", TaskCategory.Health),
                new TaskTag("医疗", "#F44336", TaskCategory.Health),
                new TaskTag("休息", "#9E9E9E", TaskCategory.Health),
                
                // 娱乐相关
                new TaskTag("电影", "#673AB7", TaskCategory.Entertainment),
                new TaskTag("游戏", "#FF9800", TaskCategory.Entertainment),
                new TaskTag("音乐", "#E91E63", TaskCategory.Entertainment),
                
                // 其他
                new TaskTag("重要", "#F44336", TaskCategory.Other),
                new TaskTag("紧急", "#FF5722", TaskCategory.Other),
                new TaskTag("个人", "#9C27B0", TaskCategory.Other)
            };
        }

        public List<TaskTag> GetAllTags()
        {
            return _predefinedTags.ToList();
        }

        public List<TaskTag> GetTagsByCategory(TaskCategory category)
        {
            return _predefinedTags.Where(t => t.Category == category).ToList();
        }

        public TaskTag CreateCustomTag(string name, string color, TaskCategory category)
        {
            var tag = new TaskTag(name, color, category);
            _predefinedTags.Add(tag);
            SaveTags();
            return tag;
        }

        public void DeleteCustomTag(TaskTag tag)
        {
            _predefinedTags.Remove(tag);
            SaveTags();
        }

        public List<TaskTag> SearchTags(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAllTags();

            return _predefinedTags.Where(t => 
                t.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        private void LoadTags()
        {
            try
            {
                if (File.Exists(_tagFilePath))
                {
                    var json = File.ReadAllText(_tagFilePath);
                    var customTags = JsonConvert.DeserializeObject<List<TaskTag>>(json) ?? new List<TaskTag>();
                    _predefinedTags.AddRange(customTags);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"加载标签数据时出错: {ex.Message}", "错误", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void SaveTags()
        {
            try
            {
                // 只保存自定义标签（非预定义标签）
                var customTags = _predefinedTags.Skip(GetPredefinedTagCount()).ToList();
                var json = JsonConvert.SerializeObject(customTags, Formatting.Indented);
                File.WriteAllText(_tagFilePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"保存标签数据时出错: {ex.Message}", "错误", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private int GetPredefinedTagCount()
        {
            return 23; // 预定义标签的数量
        }
    }
}