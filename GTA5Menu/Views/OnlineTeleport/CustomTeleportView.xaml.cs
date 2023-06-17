﻿using GTA5Menu.Utils;
using GTA5Menu.Config;
using GTA5Menu.Models;

using GTA5Core.Features;
using GTA5Core.RAGE.Teleports;
using GTA5Shared.Helper;

namespace GTA5Menu.Views.OnlineTeleport;

/// <summary>
/// CustomTeleportView.xaml 的交互逻辑
/// </summary>
public partial class CustomTeleportView : UserControl
{
    public ObservableCollection<TeleportInfoModel> CustomTeleports { get; set; } = new();

    public CustomTeleportView()
    {
        InitializeComponent();
        GTA5MenuWindow.WindowClosingEvent += GTA5MenuWindow_WindowClosingEvent;

        // 如果配置文件存在就读取
        if (File.Exists(GTA5Util.File_Config_Teleports))
        {
            var teleports = JsonHelper.ReadFile<Teleports>(GTA5Util.File_Config_Teleports);

            foreach (var custom in teleports.CustomLocations)
            {
                CustomTeleports.Add(new()
                {
                    Name = custom.Name,
                    X = custom.X,
                    Y = custom.Y,
                    Z = custom.Z
                });
            }
        }
    }

    private void GTA5MenuWindow_WindowClosingEvent()
    {
        SaveConfig();
    }

    /////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 保存配置文件
    /// </summary>
    private void SaveConfig()
    {
        if (!Directory.Exists(FileHelper.Dir_Config))
            return;

        var teleports = new Teleports
        {
            CustomLocations = new()
        };
        // 加载到配置文件
        foreach (var info in CustomTeleports)
        {
            teleports.CustomLocations.Add(new()
            {
                Name = info.Name,
                X = info.X,
                Y = info.Y,
                Z = info.Z,
                Pitch = 0.0f,
                Yaw = 0.0f,
                Roll = 0.0f,
            });
        }
        // 写入到Json文件
        JsonHelper.WriteFile(GTA5Util.File_Config_Teleports, teleports);
    }

    private void ListBox_CustomTeleports_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        Button_Teleport_Click(null, null);
    }

    private void ListBox_CustomTeleports_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var index = ListBox_CustomTeleports.SelectedIndex;
        if (index == -1)
            return;

        var info = CustomTeleports[index];

        TextBox_CustomName.Text = info.Name;
        TextBox_Position_X.Text = info.X.ToString("0.000");
        TextBox_Position_Y.Text = info.Y.ToString("0.000");
        TextBox_Position_Z.Text = info.Z.ToString("0.000");
    }

    /// <summary>
    /// 增加
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_AddCustomTeleport_Click(object sender, RoutedEventArgs e)
    {
        var vector3 = Teleport.GetPlayerPosition();

        CustomTeleports.Add(new()
        {
            Name = $"保存点 : {DateTime.Now:yyyyMMdd_HHmmss_ffff}",
            X = vector3.X,
            Y = vector3.Y,
            Z = vector3.Z
        });

        ListBox_CustomTeleports.SelectedIndex = ListBox_CustomTeleports.Items.Count - 1;
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_EditCustomTeleport_Click(object sender, RoutedEventArgs e)
    {
        var tempName = TextBox_CustomName.Text.Trim();
        var tempX = TextBox_Position_X.Text.Trim();
        var tempY = TextBox_Position_Y.Text.Trim();
        var tempZ = TextBox_Position_Z.Text.Trim();

        if (string.IsNullOrEmpty(tempName))
        {
            NotifierHelper.Show(NotifierType.Warning, "坐标名称不能为空，操作取消");
            return;
        }

        if (!float.TryParse(tempX, out float x) ||
            !float.TryParse(tempY, out float y) ||
            !float.TryParse(tempZ, out float z))
        {
            NotifierHelper.Show(NotifierType.Warning, "坐标数据不合法，操作取消");
            return;
        }

        var index = ListBox_CustomTeleports.SelectedIndex;
        if (index == -1)
        {
            NotifierHelper.Show(NotifierType.Warning, "当前自定义传送坐标选中项为空");
            return;
        }

        CustomTeleports[index].Name = tempName;
        CustomTeleports[index].X = x;
        CustomTeleports[index].Y = y;
        CustomTeleports[index].Z = z;

        NotifierHelper.Show(NotifierType.Success, "修改自定义传送坐标数据成功");
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_DeleteCustomTeleport_Click(object sender, RoutedEventArgs e)
    {
        var index = ListBox_CustomTeleports.SelectedIndex;
        if (index == -1)
        {
            NotifierHelper.Show(NotifierType.Warning, "当前自定义传送坐标选中项为空");
            return;
        }

        CustomTeleports.RemoveAt(index);
    }

    private void Button_Teleport_Click(object sender, RoutedEventArgs e)
    {
        var index = ListBox_CustomTeleports.SelectedIndex;
        if (index == -1)
        {
            NotifierHelper.Show(NotifierType.Warning, "当前自定义传送坐标选中项为空");
            return;
        }

        var info = CustomTeleports[index];

        Teleport.SetTeleportPosition(new()
        {
            X = info.X,
            Y = info.Y,
            Z = info.Z
        });
    }

    /////////////////////////////////////////////////////////////////////////////

    private void Button_ToWaypoint_Click(object sender, RoutedEventArgs e)
    {
        Teleport.ToWaypoint();
    }

    private void Button_ToObjective_Click(object sender, RoutedEventArgs e)
    {
        Teleport.ToObjective();
    }

    /////////////////////////////////////////////////////////////////////////////

    private void Button_MoveDistance_Click(object sender, RoutedEventArgs e)
    {
        var moveDistance = (float)Slider_MoveDistance.Value;

        if (sender is Button button)
        {
            switch (button.Content.ToString())
            {
                case "向前":
                    Teleport.MoveFoward(moveDistance);
                    break;
                case "向后":
                    Teleport.MoveBack(moveDistance);
                    break;
                case "向左":
                    Teleport.MoveLeft(moveDistance);
                    break;
                case "向右":
                    Teleport.MoveRight(moveDistance);
                    break;
                case "向上":
                    Teleport.MoveUp(moveDistance);
                    break;
                case "向下":
                    Teleport.MoveDown(moveDistance);
                    break;
            }
        }
    }
}