using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dalamud;
using Dalamud.Game.Text.SeStringHandling;
using GatherBuddy.Alarms;
using GatherBuddy.Classes;
using GatherBuddy.Config;
using GatherBuddy.GatherHelper;
using GatherBuddy.Interfaces;
using GatherBuddy.Plugin;
using GatherBuddy.Structs;
using ImGuiNET;
using ImRaii = OtterGui.Raii.ImRaii;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private const string AutomaticallyGenerated = "Automatically generated from context menu.";

    private void DrawAddAlarm(IGatherable item)
    {
        // Only timed items.
        if (item.InternalLocationId <= 0)
            return;

        var current = _alarmCache.Selector.EnsureCurrent();
        if (ImGui.Selectable("添加到闹钟"))
        {
            if (current == null)
            {
                Plugin.AlarmManager.AddGroup(new AlarmGroup()
                {
                    Description = AutomaticallyGenerated,
                    Enabled     = true,
                    Alarms      = new List<Alarm> { new(item) { Enabled = true } },
                });
                current = _alarmCache.Selector.EnsureCurrent();
            }
            else
            {
                Plugin.AlarmManager.AddAlarm(current, new Alarm(item));
            }
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(
                $"添加 {item.Name[GatherBuddy.Language]} 到 {(current == null ? "新的闹钟" : CheckUnnamed(current.Name))}");
    }

    private void DrawAddToGatherGroup(IGatherable item)
    {
        var       current = _gatherGroupCache.Selector.EnsureCurrent();
        using var color   = ImRaii.PushColor(ImGuiCol.Text, ColorId.DisabledText.Value(), current == null);
        if (ImGui.Selectable("添加到采集分类") && current != null)
            if (Plugin.GatherGroupManager.ChangeGroupNode(current, current.Nodes.Count, item, null, null, null, false))
                Plugin.GatherGroupManager.Save();

        color.Pop();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(current == null
                ? "需要设置并选择一个采集分类组。"
                : $"添加 {item.Name[GatherBuddy.Language]} 到 {current.Name}");
    }

    private void DrawAddGatherWindow(IGatherable item)
    {
        var current = _gatherWindowCache.Selector.EnsureCurrent();

        if (ImGui.Selectable("添加到采集悬浮窗"))
        {
            if (current == null)
                Plugin.GatherWindowManager.AddPreset(new GatherWindowPreset
                {
                    Enabled     = true,
                    Items       = new List<IGatherable> { item },
                    Description = AutomaticallyGenerated,
                });
            else
                Plugin.GatherWindowManager.AddItem(current, item);
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(
                $"添加 {item.Name[GatherBuddy.Language]} 到 {(current == null ? "新的采集悬浮窗" : CheckUnnamed(current.Name))}");
    }

    private static string TeamCraftAddressEnd(string type, uint id)
    {
        var lang = GatherBuddy.Language switch
        {
            ClientLanguage.English  => "en",
            ClientLanguage.German   => "de",
            ClientLanguage.French   => "fr",
            ClientLanguage.Japanese => "ja",
            ClientLanguage.ChineseSimplified => "cn",
            _                       => "en",
        };

        return $"db/{lang}/{type}/{id}";
    }

    private static string TeamCraftAddressEnd(FishingSpot s)
        => s.Spearfishing
            ? TeamCraftAddressEnd("spearfishing-spot", s.SpearfishingSpotData!.GatheringPointBase.Row)
            : TeamCraftAddressEnd("fishing-spot",      s.Id);

    // 在wiki中打开
    private static string WikiAddress(string itemName)
        => $"https://ff14.huijiwiki.com/wiki/物品:{itemName}";
    
    private static void DrawOpenInWiki(string itemName)
    {
        if (itemName == "")
            return;

        if (!ImGui.Selectable("在Wiki中打开 "))
            return;

        try
        {
            Process.Start(new ProcessStartInfo(WikiAddress(itemName)) { UseShellExecute = true });
        }
        catch (Exception e)
        {
            GatherBuddy.Log.Error($"Could not open Wiki:\n{e.Message}");
        }
    }
    
    // 在garland中打开
    private static string GarlandToolsItemAddress(uint itemId)
        => $"https://www.garlandtools.cn/db/#item/{itemId}";

    private static void DrawOpenInGarlandTools(uint itemId)
    {
        if (itemId == 0)
            return;

        if (!ImGui.Selectable("在Garland中打开 "))
            return;

        try
        {
            Process.Start(new ProcessStartInfo(GarlandToolsItemAddress(itemId)) { UseShellExecute = true });
        }
        catch (Exception e)
        {
            GatherBuddy.Log.Error($"Could not open GarlandTools:\n{e.Message}");
        }
    }
    
    //在鱼糕中打开
    private static string MomolaItemAddress(uint itemId)
        => $"https://fish.ffmomola.com/#/wiki?fishId={itemId}";
    
    private static void DrawOpenMomolaItem(uint itemId)
    {
        if (itemId == 0)
            return;

        if (!ImGui.Selectable("在鱼糕中打开 "))
            return;

        try
        {
            Process.Start(new ProcessStartInfo(MomolaItemAddress(itemId)) { UseShellExecute = true });
        }
        catch (Exception e)
        {
            GatherBuddy.Log.Error($"Could not open Momola:\n{e.Message}");
        }
    }

    //在TeamCraft中打开
    private static void DrawOpenInTeamCraft(uint itemId)
    {
        if (itemId == 0)
            return;

        if (ImGui.Selectable("在TeamCraft中打开 "))
            OpenInTeamCraftWeb(TeamCraftAddressEnd("item", itemId));

    }

    private static void OpenInTeamCraftWeb(string addressEnd)
    {
        Process.Start(new ProcessStartInfo($"https://www.ffxivteamcraft.com/{addressEnd}")
        {
            UseShellExecute = true,
        });
    }

    private static void OpenInTeamCraftLocal(string addressEnd)
    {
        Task.Run(() =>
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:14500/{addressEnd}");
                using var response = GatherBuddy.HttpClient.Send(request);
            }
            catch
            {
                try
                {
                    if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ffxiv-teamcraft")))
                        Process.Start(new ProcessStartInfo($"teamcraft:///{addressEnd}")
                        {
                            UseShellExecute = true,
                        });
                }
                catch
                {
                    GatherBuddy.Log.Error("Could not open local teamcraft.");
                }
            }
        });
    }

    private static void DrawOpenInTeamCraft(FishingSpot fs)
    {
        if (fs.Id == 0)
            return;

        if (ImGui.Selectable("在TeamCraft中打开 "))
            OpenInTeamCraftWeb(TeamCraftAddressEnd(fs));
    }
    
    //复制到剪贴板
    private static void CopyToClipboard(string itemName)
    {
        if (ImGui.Selectable("复制到剪贴板"))
        {
            ImGui.SetClipboardText(itemName);
        }
    }

    //物品鱼类右键菜单
    public void CreateContextMenu(IGatherable item)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup(item.Name[GatherBuddy.Language]);

        using var popup = ImRaii.Popup(item.Name[GatherBuddy.Language]);
        if (!popup)
            return;

        CopyToClipboard(item.Name[GatherBuddy.Language]);
        if (ImGui.Selectable("创建聊天框链接"))
            Communicator.Print(SeString.CreateItemLink(item.ItemId));
        
        ImGui.Separator();
        DrawAddAlarm(item);
        DrawAddToGatherGroup(item);
        DrawAddGatherWindow(item);
        
        ImGui.Separator();
        DrawOpenInWiki(item.Name[GatherBuddy.Language]);
        DrawOpenInGarlandTools(item.ItemId);
        if (item is Fish)
        {
            DrawOpenMomolaItem(item.ItemId);
        }
    }

    public static void CreateGatherWindowContextMenu(IGatherable item, bool clicked)
    {
        if (clicked)
            ImGui.OpenPopup(item.Name[GatherBuddy.Language]);

        using var popup = ImRaii.Popup(item.Name[GatherBuddy.Language]);
        if (!popup)
            return;
        
        CopyToClipboard(item.Name[GatherBuddy.Language]);
        if (ImGui.Selectable("创建聊天框链接"))
            Communicator.Print(SeString.CreateItemLink(item.ItemId));
        ImGui.Separator();
        DrawOpenInWiki(item.Name[GatherBuddy.Language]);
        DrawOpenInGarlandTools(item.ItemId);
        if (item is Fish)
        {
            DrawOpenMomolaItem(item.ItemId);
        }
    }

    //鱼饵右键
    public static void CreateContextMenu(Bait bait)
    {
        if (bait.Id == 0)
            return;

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup(bait.Name);

        using var popup = ImRaii.Popup(bait.Name);
        if (!popup)
            return;
        
        CopyToClipboard(bait.Name);
        if (ImGui.Selectable("创建聊天框链接"))
            Communicator.Print(SeString.CreateItemLink(bait.Id));
        ImGui.Separator();
        DrawOpenInWiki(bait.Name);
        DrawOpenInGarlandTools(bait.Id);
    }

    //钓鱼点右键
    public static void CreateContextMenu(FishingSpot? spot)
    {
        // if (spot == null)
        //     return;
        //
        // if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        //     ImGui.OpenPopup(spot.Name);
        //
        // using var popup = ImRaii.Popup(spot.Name);
        // if (!popup)
        //     return;
        //
        // DrawOpenMomolaSpot(spot.Id);
    }
}
