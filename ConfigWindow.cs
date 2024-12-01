using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using ImGuiNET;
using System.Numerics;

namespace Interjection;

public class ConfigWindow : Window
{
    public ConfigWindow() : base(Plugin.Name + " Configuration")
    {
        Size = new Vector2(369, 225);
        SizeCondition = ImGuiCond.FirstUseEver;
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

        needSave |= ImGui.Checkbox("Enabled", ref Plugin.Config.Enabled);
        ImGui.SameLine();
        if (ImGui.Button("Reset Config to Defaults"))
        {
            Plugin.Config.SetToDefaults();
            needSave = true;
        }
        using (ImRaii.Disabled(!Plugin.Config.Enabled))
        {
            using (ImRaii.Disabled(!Plugin.Config.OverrideNormalCastColor))
            {
                needSave |= ByteColorEdit("##ucastcolor", ref Plugin.Config.NormalCastColor, ImGuiColorEditFlags.NoInputs);
                ImGui.SameLine();
            }
            needSave |= ImGui.Checkbox("Recolor Default Castbar", ref Plugin.Config.OverrideNormalCastColor);
            Tooltip("Change the enemy list castbar color.");

            using (ImRaii.Disabled(!Plugin.Config.OverrideInterruptableCastColor))
            {
                needSave |= ByteColorEdit("##icastcolor", ref Plugin.Config.InterruptableCastColor, ImGuiColorEditFlags.NoInputs);
                ImGui.SameLine();
            }
            needSave |= ImGui.Checkbox("Recolor Interruptable Castbar", ref Plugin.Config.OverrideInterruptableCastColor);
            Tooltip("Change the enemy list castbar color for interruptible spells.");

            using (ImRaii.Disabled(!Plugin.Config.OverrideTankTargetEnmityGemColor))
            {
                needSave |= ByteColorEdit("##tankgemcolor", ref Plugin.Config.TankGemColor, ImGuiColorEditFlags.NoInputs);
                ImGui.SameLine();
            }
            needSave |= ImGui.Checkbox("Recolor Enmity Gem for Tanks", ref Plugin.Config.OverrideTankTargetEnmityGemColor);
            Tooltip("Change the enmity gem color when the enemy is targetting a tank that is not you.");
        }

        if (needSave)
        {
            Plugin.Config.Save();
        }
    }
}
