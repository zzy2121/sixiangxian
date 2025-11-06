using System;
using System.Linq;
using System.Text;
using System.Windows;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager
{
    public class TestTagService
    {
        public static void TestTagFunctionality()
        {
            var result = new StringBuilder();
            result.AppendLine("=== 测试标签服务功能 ===");
            
            try
            {
                var tagService = new TagService();
                
                // 测试获取所有标签
                result.AppendLine("\n1. 获取所有标签:");
                var allTags = tagService.GetAllTags();
                result.AppendLine($"总共有 {allTags.Count} 个标签");
                
                // 显示前5个标签
                foreach (var tag in allTags.Take(5))
                {
                    result.AppendLine($"  - {tag.Name} ({tag.Category}) [{tag.TagType}] {tag.Color}");
                }
                
                // 测试创建自定义标签
                result.AppendLine("\n2. 创建自定义标签:");
                var customTag = tagService.CreateCustomTag("测试标签", "#FF0000", TaskCategory.Other);
                result.AppendLine($"创建了自定义标签: {customTag.Name} ({customTag.TagType})");
                
                // 测试获取更新后的标签列表
                result.AppendLine("\n3. 更新后的标签总数:");
                allTags = tagService.GetAllTags();
                result.AppendLine($"现在总共有 {allTags.Count} 个标签");
                
                // 测试按分类获取标签
                result.AppendLine("\n4. 按分类获取标签 (工作类):");
                var workTags = tagService.GetTagsByCategory(TaskCategory.Work);
                foreach (var tag in workTags)
                {
                    result.AppendLine($"  - {tag.Name} [{tag.TagType}]");
                }
                
                // 测试搜索标签
                result.AppendLine("\n5. 搜索标签 (包含'会议'):");
                var searchResults = tagService.SearchTags("会议");
                foreach (var tag in searchResults)
                {
                    result.AppendLine($"  - {tag.Name} [{tag.TagType}]");
                }
                
                // 测试检查是否为自定义标签
                result.AppendLine("\n6. 检查标签类型:");
                var testTag = allTags.FirstOrDefault(t => t.Name == "测试标签");
                if (testTag != null)
                {
                    result.AppendLine($"'{testTag.Name}' 是自定义标签: {tagService.IsCustomTag(testTag)}");
                }
                
                var predefinedTag = allTags.FirstOrDefault(t => t.Name == "会议");
                if (predefinedTag != null)
                {
                    result.AppendLine($"'{predefinedTag.Name}' 是自定义标签: {tagService.IsCustomTag(predefinedTag)}");
                }
                
                // 测试更新自定义标签
                result.AppendLine("\n7. 更新自定义标签:");
                if (testTag != null)
                {
                    bool updated = tagService.UpdateCustomTag(testTag, "更新的测试标签", "#00FF00", TaskCategory.Study);
                    result.AppendLine($"标签更新结果: {updated}");
                    
                    // 验证更新
                    allTags = tagService.GetAllTags();
                    var updatedTag = allTags.FirstOrDefault(t => t.Name == "更新的测试标签");
                    if (updatedTag != null)
                    {
                        result.AppendLine($"更新后的标签: {updatedTag.Name} ({updatedTag.Category}) {updatedTag.Color}");
                    }
                }
                
                // 测试删除自定义标签
                result.AppendLine("\n8. 删除自定义标签:");
                var tagToDelete = allTags.FirstOrDefault(t => t.Name == "更新的测试标签");
                if (tagToDelete != null)
                {
                    tagService.DeleteCustomTag(tagToDelete);
                    result.AppendLine("自定义标签已删除");
                    
                    // 验证删除
                    allTags = tagService.GetAllTags();
                    result.AppendLine($"删除后标签总数: {allTags.Count}");
                }
                
                result.AppendLine("\n=== 标签服务测试完成 ===");
                result.AppendLine("✅ 所有测试通过！");
            }
            catch (Exception ex)
            {
                result.AppendLine($"\n❌ 测试失败: {ex.Message}");
                result.AppendLine($"详细错误: {ex}");
            }
            
            // 显示测试结果
            MessageBox.Show(result.ToString(), "标签服务测试结果", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}