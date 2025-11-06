using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace TaskManager.Models
{
    public enum TaskQuadrant
    {
        [Description("紧急且重要")]
        UrgentImportant = 1,
        [Description("紧急但不重要")]
        UrgentNotImportant = 2,
        [Description("重要但不紧急")]
        ImportantNotUrgent = 3,
        [Description("既不紧急也不重要")]
        NotUrgentNotImportant = 4
    }

    public enum TaskStatus
    {
        [Description("未开始")]
        NotStarted = 0,
        [Description("进行中")]
        InProgress = 1,
        [Description("已完成")]
        Completed = 2
    }

    public enum TaskCategory
    {
        [Description("工作")]
        Work = 1,
        [Description("生活")]
        Life = 2,
        [Description("学习")]
        Study = 3,
        [Description("健康")]
        Health = 4,
        [Description("娱乐")]
        Entertainment = 5,
        [Description("其他")]
        Other = 6
    }

    public class TaskTag
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public TaskCategory Category { get; set; }
        public bool IsCustom { get; set; } = false;

        public string TagType => IsCustom ? "自定义" : "预定义";

        public TaskTag()
        {
            Color = "#2196F3"; // 默认蓝色
        }

        public TaskTag(string name, string color, TaskCategory category, bool isCustom = false)
        {
            Name = name;
            Color = color;
            Category = category;
            IsCustom = isCustom;
        }
    }

    public class TaskItem : INotifyPropertyChanged
    {
        private string _id;
        private string _title;
        private string _description;
        private TaskQuadrant _quadrant;
        private DateTime _dueDate;
        private DateTime _startTime;
        private DateTime _endTime;
        private TaskStatus _status;
        private int _priority;
        private DateTime _createdDate;
        private DateTime _modifiedDate;
        private List<TaskTag> _tags;
        private TaskCategory _category;
        private bool _isAllDay;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public TaskQuadrant Quadrant
        {
            get => _quadrant;
            set { _quadrant = value; OnPropertyChanged(nameof(Quadrant)); }
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set { _dueDate = value; OnPropertyChanged(nameof(DueDate)); }
        }

        public TaskStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public int Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(nameof(Priority)); }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set { _createdDate = value; OnPropertyChanged(nameof(CreatedDate)); }
        }

        public DateTime ModifiedDate
        {
            get => _modifiedDate;
            set { _modifiedDate = value; OnPropertyChanged(nameof(ModifiedDate)); }
        }

        public DateTime StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(nameof(StartTime)); }
        }

        public DateTime EndTime
        {
            get => _endTime;
            set { _endTime = value; OnPropertyChanged(nameof(EndTime)); }
        }

        public List<TaskTag> Tags
        {
            get => _tags ?? (_tags = new List<TaskTag>());
            set { _tags = value; OnPropertyChanged(nameof(Tags)); }
        }

        public TaskCategory Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }

        public bool IsAllDay
        {
            get => _isAllDay;
            set { _isAllDay = value; OnPropertyChanged(nameof(IsAllDay)); }
        }

        public string TagsDisplay => Tags.Any() ? string.Join(", ", Tags.Select(t => t.Name)) : "";

        public TaskItem()
        {
            Id = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            Status = TaskStatus.NotStarted;
            Priority = 1;
            Tags = new List<TaskTag>();
            Category = TaskCategory.Work;
            IsAllDay = true;
            StartTime = DateTime.Today.AddHours(9); // 默认上午9点
            EndTime = DateTime.Today.AddHours(10); // 默认1小时
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}