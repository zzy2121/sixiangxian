using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class SystemCalendarIntegration
    {
        public void ExportTasksToSystemCalendar(List<TaskItem> tasks)
        {
            try
            {
                // 创建ICS文件内容
                var icsContent = GenerateIcsContent(tasks);
                
                // 保存到临时文件
                var tempPath = Path.GetTempFileName();
                var icsPath = Path.ChangeExtension(tempPath, ".ics");
                
                File.WriteAllText(icsPath, icsContent, Encoding.UTF8);
                
                // 打开系统默认日历应用
                Process.Start(new ProcessStartInfo
                {
                    FileName = icsPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"导出到系统日历失败: {ex.Message}");
            }
        }

        private string GenerateIcsContent(List<TaskItem> tasks)
        {
            var sb = new StringBuilder();
            
            // ICS文件头
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//四象限任务管理器//TaskManager//CN");
            sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine("METHOD:PUBLISH");
            
            foreach (var task in tasks)
            {
                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine($"UID:{task.Id}@taskmanager.local");
                sb.AppendLine($"DTSTAMP:{DateTime.Now:yyyyMMddTHHmmssZ}");
                
                if (task.IsAllDay)
                {
                    sb.AppendLine($"DTSTART;VALUE=DATE:{task.DueDate:yyyyMMdd}");
                    sb.AppendLine($"DTEND;VALUE=DATE:{task.DueDate.AddDays(1):yyyyMMdd}");
                }
                else
                {
                    sb.AppendLine($"DTSTART:{task.StartTime:yyyyMMddTHHmmssZ}");
                    sb.AppendLine($"DTEND:{task.EndTime:yyyyMMddTHHmmssZ}");
                }
                
                sb.AppendLine($"SUMMARY:{EscapeIcsText(task.Title)}");
                
                if (!string.IsNullOrEmpty(task.Description))
                {
                    sb.AppendLine($"DESCRIPTION:{EscapeIcsText(task.Description)}");
                }
                
                // 添加分类信息
                sb.AppendLine($"CATEGORIES:{GetQuadrantDescription(task.Quadrant)},{GetCategoryDescription(task.Category)}");
                
                // 添加标签信息
                if (task.Tags.Any())
                {
                    var tagNames = string.Join(",", task.Tags.Select(t => t.Name));
                    sb.AppendLine($"X-TAGS:{EscapeIcsText(tagNames)}");
                }
                
                // 设置优先级
                var priority = task.Quadrant switch
                {
                    TaskQuadrant.UrgentImportant => 1,
                    TaskQuadrant.UrgentNotImportant => 2,
                    TaskQuadrant.ImportantNotUrgent => 3,
                    TaskQuadrant.NotUrgentNotImportant => 4,
                    _ => 5
                };
                sb.AppendLine($"PRIORITY:{priority}");
                
                // 设置状态
                var status = task.Status switch
                {
                    TaskStatus.NotStarted => "NEEDS-ACTION",
                    TaskStatus.InProgress => "IN-PROCESS",
                    TaskStatus.Completed => "COMPLETED",
                    _ => "NEEDS-ACTION"
                };
                sb.AppendLine($"STATUS:{status}");
                
                sb.AppendLine("END:VEVENT");
            }
            
            sb.AppendLine("END:VCALENDAR");
            
            return sb.ToString();
        }

        private string EscapeIcsText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
                
            return text.Replace("\\", "\\\\")
                      .Replace(",", "\\,")
                      .Replace(";", "\\;")
                      .Replace("\n", "\\n")
                      .Replace("\r", "");
        }

        private string GetQuadrantDescription(TaskQuadrant quadrant)
        {
            return quadrant switch
            {
                TaskQuadrant.UrgentImportant => "紧急且重要",
                TaskQuadrant.UrgentNotImportant => "紧急但不重要",
                TaskQuadrant.ImportantNotUrgent => "重要但不紧急",
                TaskQuadrant.NotUrgentNotImportant => "既不紧急也不重要",
                _ => "未分类"
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

        public void ImportTasksFromSystemCalendar()
        {
            // 这个功能比较复杂，需要读取系统日历数据
            // 可以作为未来的扩展功能
            throw new NotImplementedException("从系统日历导入功能正在开发中");
        }
    }
}