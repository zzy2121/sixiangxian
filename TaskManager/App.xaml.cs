using System;
using System.Linq;
using System.Windows;

namespace TaskManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 检查命令行参数
            if (e.Args.Length > 0 && e.Args[0] == "test-tags")
            {
                // 运行标签测试
                TestTagService.TestTagFunctionality();
                Shutdown();
                return;
            }
            
            // 正常启动WPF应用程序
            base.OnStartup(e);
        }
    }
}