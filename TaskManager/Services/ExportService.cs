using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using TaskManager.Models;
using PdfDocument = iTextSharp.text.Document;
using PdfParagraph = iTextSharp.text.Paragraph;
using PdfPageSize = iTextSharp.text.PageSize;
using PdfElement = iTextSharp.text.Element;
using PdfFont = iTextSharp.text.Font;
using iTextSharp.text.pdf;

namespace TaskManager.Services
{
    public class ExportService
    {
        public void ExportToWord(List<TaskItem> tasks, string filePath, string title = "任务列表")
        {
            try
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    // 添加标题
                    Paragraph titlePara = body.AppendChild(new Paragraph());
                    Run titleRun = titlePara.AppendChild(new Run());
                    titleRun.AppendChild(new Text($"{title} - {DateTime.Now:yyyy年MM月dd日}"));

                    // 添加空行
                    body.AppendChild(new Paragraph());

                    // 按象限分组显示任务
                    var groupedTasks = tasks.GroupBy(t => t.Quadrant).OrderBy(g => (int)g.Key);

                    foreach (var group in groupedTasks)
                    {
                        // 象限标题
                        Paragraph quadrantPara = body.AppendChild(new Paragraph());
                        Run quadrantRun = quadrantPara.AppendChild(new Run());
                        quadrantRun.AppendChild(new Text($"【{GetQuadrantDescription(group.Key)}】"));

                        // 任务列表
                        foreach (var task in group.OrderBy(t => t.Priority).ThenBy(t => t.DueDate))
                        {
                            Paragraph taskPara = body.AppendChild(new Paragraph());
                            Run taskRun = taskPara.AppendChild(new Run());
                            
                            string statusIcon = task.Status switch
                            {
                                TaskStatus.Completed => "✓",
                                TaskStatus.InProgress => "◐",
                                _ => "○"
                            };

                            string taskText = $"{statusIcon} {task.Title}";
                            if (task.DueDate != DateTime.MinValue)
                            {
                                taskText += $" (截止: {task.DueDate:MM-dd})";
                            }
                            if (!string.IsNullOrEmpty(task.Description))
                            {
                                taskText += $" - {task.Description}";
                            }

                            taskRun.AppendChild(new Text(taskText));
                        }

                        body.AppendChild(new Paragraph()); // 空行
                    }

                    mainPart.Document.Save();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"导出Word文档失败: {ex.Message}");
            }
        }

        public void ExportToPdf(List<TaskItem> tasks, string filePath, string title = "任务列表")
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    PdfDocument document = new PdfDocument(PdfPageSize.A4, 50, 50, 50, 50);
                    PdfWriter writer = PdfWriter.GetInstance(document, stream);
                    document.Open();

                    // 设置中文字体
                    BaseFont baseFont = BaseFont.CreateFont("c:\\windows\\fonts\\simsun.ttc,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                    PdfFont titleFont = new PdfFont(baseFont, 16, PdfFont.BOLD);
                    PdfFont normalFont = new PdfFont(baseFont, 12);
                    PdfFont headerFont = new PdfFont(baseFont, 14, PdfFont.BOLD);

                    // 添加标题
                    PdfParagraph titlePara = new PdfParagraph($"{title} - {DateTime.Now:yyyy年MM月dd日}", titleFont);
                    titlePara.Alignment = PdfElement.ALIGN_CENTER;
                    document.Add(titlePara);
                    document.Add(new PdfParagraph(" ", normalFont)); // 空行

                    // 按象限分组显示任务
                    var groupedTasks = tasks.GroupBy(t => t.Quadrant).OrderBy(g => (int)g.Key);

                    foreach (var group in groupedTasks)
                    {
                        // 象限标题
                        PdfParagraph quadrantPara = new PdfParagraph($"【{GetQuadrantDescription(group.Key)}】", headerFont);
                        document.Add(quadrantPara);

                        // 任务列表
                        foreach (var task in group.OrderBy(t => t.Priority).ThenBy(t => t.DueDate))
                        {
                            string statusIcon = task.Status switch
                            {
                                TaskStatus.Completed => "✓",
                                TaskStatus.InProgress => "◐",
                                _ => "○"
                            };

                            string taskText = $"{statusIcon} {task.Title}";
                            if (task.DueDate != DateTime.MinValue)
                            {
                                taskText += $" (截止: {task.DueDate:MM-dd})";
                            }
                            if (!string.IsNullOrEmpty(task.Description))
                            {
                                taskText += $" - {task.Description}";
                            }

                            PdfParagraph taskPara = new PdfParagraph(taskText, normalFont);
                            taskPara.IndentationLeft = 20;
                            document.Add(taskPara);
                        }

                        document.Add(new PdfParagraph(" ", normalFont)); // 空行
                    }

                    document.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"导出PDF文档失败: {ex.Message}");
            }
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
    }
}