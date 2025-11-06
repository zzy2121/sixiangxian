using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class DataService
    {
        private readonly string _dataFilePath;
        private List<TaskItem> _tasks;

        public DataService()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TaskManager");
            Directory.CreateDirectory(appDataPath);
            _dataFilePath = Path.Combine(appDataPath, "tasks.json");
            _tasks = new List<TaskItem>();
            LoadTasks();
        }

        public List<TaskItem> GetAllTasks()
        {
            return _tasks.ToList();
        }

        public List<TaskItem> GetTasksByQuadrant(TaskQuadrant quadrant)
        {
            return _tasks.Where(t => t.Quadrant == quadrant).ToList();
        }

        public List<TaskItem> GetTasksByDate(DateTime date)
        {
            return _tasks.Where(t => t.DueDate.Date == date.Date).ToList();
        }

        public List<TaskItem> GetTodayTasks()
        {
            return GetTasksByDate(DateTime.Today);
        }

        public void AddTask(TaskItem task)
        {
            _tasks.Add(task);
            SaveTasks();
        }

        public void UpdateTask(TaskItem task)
        {
            var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existingTask != null)
            {
                var index = _tasks.IndexOf(existingTask);
                task.ModifiedDate = DateTime.Now;
                _tasks[index] = task;
                SaveTasks();
            }
        }

        public void DeleteTask(string taskId)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                _tasks.Remove(task);
                SaveTasks();
            }
        }

        public List<TaskItem> SearchTasks(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAllTasks();

            return _tasks.Where(t => 
                t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        private void LoadTasks()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    var json = File.ReadAllText(_dataFilePath);
                    _tasks = JsonConvert.DeserializeObject<List<TaskItem>>(json) ?? new List<TaskItem>();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"加载任务数据时出错: {ex.Message}", "错误", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                _tasks = new List<TaskItem>();
            }
        }

        private void SaveTasks()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_tasks, Formatting.Indented);
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"保存任务数据时出错: {ex.Message}", "错误", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}