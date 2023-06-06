using System;
using System.Diagnostics;
using ImGuiNET;
using ImRaii = OtterGui.Raii.ImRaii;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private static void DrawChildWindow()
    {
        static void RawGithub()
        {
            if (ImGui.Button("github原版"))
            {
                try
                {
                    Process.Start(new ProcessStartInfo("https://github.com/Ottermandias/GatherBuddy") { UseShellExecute = true });
                }
                catch (Exception e)
                {
                    GatherBuddy.Log.Error($"Could not open github:\n{e.Message}");
                }
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("打开国际服原版github源码链接");
            }
        }

        static void CnGithub()
        {
            if (ImGui.Button("github国服库"))
            {
                try
                {
                    Process.Start(new ProcessStartInfo("https://github.com/wozaiha/GatherBuddy") { UseShellExecute = true });
                }
                catch (Exception e)
                {
                    GatherBuddy.Log.Error($"Could not open github:\n{e.Message}");
                }
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("打开国服版（未汉化）github源码链接");
            }
        }

        static void ZhGithub()
        {
            if (ImGui.Button("github国服汉化"))
            {
                try
                {
                    Process.Start(new ProcessStartInfo("https://github.com/gogofishman/GatherBuddy") { UseShellExecute = true });
                }
                catch (Exception e)
                {
                    GatherBuddy.Log.Error($"Could not open github:\n{e.Message}");
                }
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("打开国服汉化版github源码链接");
            }
        }

        static void Email()
        {
            if (ImGui.Button("向我发送邮件，点击复制邮箱"))
            {
                ImGui.SetClipboardText("gogofishman@qq.com");
            }
        }

        ImGui.Text("   ");
        ImGui.SameLine();
        ImGui.BeginChild("##123");

        ImGui.Text("");
        ImGui.Text("汉化：yoyokity");

        ImGui.Text("");
        ImGui.Text("额外改动：");
        ImGui.Bullet();
        ImGui.Text("优化钓鱼表格列间距");
        ImGui.Bullet();
        ImGui.Text("添加物品右键菜单分割线");
        ImGui.Bullet();
        ImGui.Text("添加物品右键菜单“复制到剪切板”");
        ImGui.Bullet();
        ImGui.Text("添加物品右键菜单“在Wiki中打开”");
        ImGui.Bullet();
        ImGui.Text("添加鱼类右键菜单“在鱼糕中打开”");
        ImGui.Bullet();
        ImGui.Text("调整物品右键菜单“在Garland中打开”链接为国内站点");
        ImGui.Bullet();
        ImGui.Text("在钓鱼计时器窗口中添加了右键菜单");
        ImGui.Bullet();
        ImGui.Text("格式化时间显示，使其更符合国人习惯");

        ImGui.Text("");
        RawGithub();
        ImGui.SameLine();
        CnGithub();
        ImGui.SameLine();
        ZhGithub();
        ImGui.Text("");
        ImGui.Text("当你发现某些翻译不太恰当或者有更好的翻译方法，可以通过以下两种方式联系我：");
        ImGui.Bullet();
        ImGui.Text("点击上方第三个按钮，在github国服汉化项目中发起issue");
        ImGui.Bullet();
        Email();

        ImGui.EndChild();
    }


    private static void DrawAbout()
    {
        using var id  = ImRaii.PushId("About");
        using var tab = ImRaii.TabItem("关于汉化");

        if (!tab)
            return;

        DrawChildWindow();
    }
}
