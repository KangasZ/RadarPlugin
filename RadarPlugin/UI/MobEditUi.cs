using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using ImGuiNET;
using RadarPlugin.Constants;
using RadarPlugin.Enums;
using RadarPlugin.RadarLogic;

namespace RadarPlugin.UI;

public class MobEditUi : IDisposable
{
    private Configuration.Configuration configInterface;
    private readonly DalamudPluginInterface dalamudPluginInterface;
    private GameObject localObject;

    private bool mobEditVisible = false;
    private readonly TypeConfigurator typeConfiguration;
    private readonly RadarModules radarModules;

    public MobEditUi(DalamudPluginInterface dalamudPluginInterface, Configuration.Configuration configInterface, TypeConfigurator typeConfigurator, RadarModules radarModules)
    {
        this.configInterface = configInterface;
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.dalamudPluginInterface.UiBuilder.Draw += DrawMobEditWindow;
        this.typeConfiguration = typeConfigurator;
        this.radarModules = radarModules;
    }

    private void DrawMobEditWindow()
    {
        if (!mobEditVisible)
        {
            return;
        }
        
        var size = new Vector2(600, 300);
        ImGui.SetNextWindowSize(size, ImGuiCond.Appearing);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin Modify Mobs Window", ref mobEditVisible))
        {
#if DEBUG
            Dalamud.Utility.Util.ShowObject(localObject);
#endif
            ImGui.Columns(2);
            var utilIgnored = MobConstants.DataIdIgnoreList.Contains(localObject.DataId);
            var defaulParams = radarModules.radarConfigurationModule.GetParams(localObject);
            var mobOvveride = radarModules.radarConfigurationModule.GetParamsWithOverride(localObject);
            var isUsingCustomEspOption = defaulParams != mobOvveride;

            ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() / 2);
            // Setup First column
            ImGui.Text("Information Table");
            ImGui.BeginTable("localobjecttable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("Setting");
            ImGui.TableSetupColumn("Value");
            ImGui.TableHeadersRow();
            ImGui.TableNextColumn();
            ImGui.Text("Name");
            ImGui.TableNextColumn();
            ImGui.Text($"{localObject.Name}");
            ImGui.TableNextColumn();
            ImGui.Text("Given Name");
            ImGui.TableNextColumn();
            ImGui.Text($"{radarModules.radarConfigurationModule.GetText(localObject, mobOvveride)}");
            ImGui.TableNextColumn();
            ImGui.Text("Data ID");
            ImGui.TableNextColumn();
            ImGui.Text($"{localObject.DataId}");
            ImGui.TableNextColumn();
            ImGui.Text("Type");
            ImGui.TableNextColumn();
            ImGui.Text($"{localObject.ObjectKind}");
            ImGui.EndTable();

            ImGui.Text("Disabled table");
            ImGui.BeginTable("disabledbylocalobjecttable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("Source");
            ImGui.TableSetupColumn("Value");
            ImGui.TableHeadersRow();
            ImGui.TableNextColumn();
            ImGui.Text("Utility");
            ImGui.TableNextColumn();
            ImGui.Text($"{utilIgnored}");
            ImGui.TableNextColumn();
            ImGui.Text("User");
            ImGui.TableNextColumn();
            string userCustomOptionsText;
            if (isUsingCustomEspOption)
            {
                userCustomOptionsText = mobOvveride!.Enabled.ToString();
            }
            else
            {
                userCustomOptionsText = "N/A";
            }
            ImGui.Text($"{userCustomOptionsText}");
            ImGui.TableNextColumn();
            ImGui.Text("Overall");
            ImGui.TableNextColumn();
            
            ImGui.Text($"NOT WORKING ATM, figure it out :)");
            ImGui.TableNextColumn();
            ImGui.Text("Disablable?");
            ImGui.TableNextColumn();
            ImGui.Text($"{localObject.DataId != 0}");
            ImGui.EndTable();

            // Setup second column
            ImGui.NextColumn();
            ImGui.Text("You cannot modify a mob with a data id of 0");
            if (localObject.DataId != 0)
            {
                //TODO: Grab the custom overridden thingy
                ImGui.TableNextColumn();
                if (ImGui.Checkbox($"Custom Settings Enable##custom-settings-{localObject.Address}", ref isUsingCustomEspOption))
                {
                    configInterface.CustomizeMob(localObject, isUsingCustomEspOption, defaulParams);
                }

                if (isUsingCustomEspOption)
                {
                    var configBlocked = mobOvveride.Enabled;
                    var overriddenOption = configInterface.cfg.OptionOverride[localObject.DataId];
                    if (ImGui.Checkbox($"Enabled##{localObject.Address}", ref configBlocked))
                    {
                        configInterface.cfg.OptionOverride[localObject.DataId].Enabled = configBlocked;
                    }
                    var colorChange = ImGui.ColorConvertU32ToFloat4(mobOvveride.ColorU);
                    if (ImGui.ColorEdit4($"Color##{localObject.Address}-color", ref colorChange,
                            ImGuiColorEditFlags.NoInputs))
                    {
                        configInterface.cfg.OptionOverride[localObject.DataId].ColorU = ImGui.ColorConvertFloat4ToU32(colorChange);
                        configInterface.Save();
                    }
                    // TODO: display origination helpers
                    typeConfiguration.OpenUiWithType(ref mobOvveride, localObject.Name.TextValue.ToString() ?? "Unknown", overriddenOption.MobTypeValue, DisplayOrigination.DeepDungeon);
                }
            }
            
        }

        ImGui.End();
    }

    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= DrawMobEditWindow;
    }

    public void Show(GameObject gameObject)
    {
        this.localObject = gameObject;
        mobEditVisible = true;
    }
} 