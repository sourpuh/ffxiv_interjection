using ImGuiNET;
using System.Numerics;
using Dalamud.Interface.Windowing;
using System;
using FFXIVClientStructs.FFXIV.Client.Graphics;

namespace Interjection;

public class ConfigWindow : Window
{
    private readonly Plugin _plugin;
    public ConfigWindow(Plugin p) : base(Plugin.Name + " Configuration")
    {
        _plugin = p;

        Size = new Vector2(369, 225);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    private void DisableableBlock(bool enabled, Action fun)
    {
        if (!enabled) ImGui.BeginDisabled();
        fun();
        if (!enabled) ImGui.EndDisabled();
    }

    private void Tooltip(string tooltip)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }
    }

    private bool ByteColorEdit(string label, ref ByteColor color, ImGuiColorEditFlags flags)
    {
        Vector4 v = color.ToVector4();
        if (ImGui.ColorEdit4(label, ref v, flags))
        {
            color = v.ToByteColor();
            return true;
        }
        return false;
    }

    public override void Draw()
    {
        bool needSave = false;

        needSave |= ImGui.Checkbox("Enabled", ref _plugin.Config.Enabled);
        ImGui.SameLine();
        if (ImGui.Button("Reset Config to Defaults"))
        {
            _plugin.Config.SetToDefaults();
            needSave = true;
        }
        DisableableBlock(_plugin.Config.Enabled, () =>
        {
            DisableableBlock(_plugin.Config.OverrideNormalCastColor, () =>
            {
                needSave |= ByteColorEdit("##ucastcolor", ref _plugin.Config.NormalCastColor, ImGuiColorEditFlags.NoInputs);
                ImGui.SameLine();
            });
            needSave |= ImGui.Checkbox("Recolor Default Castbar", ref _plugin.Config.OverrideNormalCastColor);
            Tooltip("Change the enemy list castbar color.");

            DisableableBlock(_plugin.Config.OverrideInterruptableCastColor, () =>
            {
                needSave |= ByteColorEdit("##icastcolor", ref _plugin.Config.InterruptableCastColor, ImGuiColorEditFlags.NoInputs);
                ImGui.SameLine();
            });
            needSave |= ImGui.Checkbox("Recolor Interruptable Castbar", ref _plugin.Config.OverrideInterruptableCastColor);
            Tooltip("Change the enemy list castbar color for interruptible spells.");

            DisableableBlock(_plugin.Config.OverrideTankTargetEnmityGemColor, () =>
            {
                needSave |= ByteColorEdit("##tankgemcolor", ref _plugin.Config.TankGemColor, ImGuiColorEditFlags.NoInputs);
                ImGui.SameLine();
            });
            needSave |= ImGui.Checkbox("Recolor Enmity Gem for Tanks", ref _plugin.Config.OverrideTankTargetEnmityGemColor);
            Tooltip("Change the enmity gem color when the enemy is targetting a tank that is not you.");
        });

        if (needSave)
        {
            _plugin.Config.Save();
        }
    }
}
