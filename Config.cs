using Dalamud.Configuration;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using System;

namespace Interjection;

[Serializable]
public class Config : IPluginConfiguration
{
    private static readonly ByteColor DefaultCastColor = new()
    {
        R = 247, G = 154, B = 0, A = 255
    };
    private static readonly ByteColor DefaultInterruptableCastColor = new()
    {
        R = 247, G = 97, B = 107, A = 255
    };
    private static readonly ByteColor DefaultTankGemColor = new()
    {
        R = 0, G = 125, B = 255, A = 255
    };

    public int Version { get; set; } = 0;

    public bool Enabled;
    public bool OverrideNormalCastColor;
    public ByteColor NormalCastColor;
    public bool OverrideInterruptableCastColor;
    public ByteColor InterruptableCastColor;
    public bool OverrideTankTargetEnmityGemColor;
    public ByteColor TankGemColor;

    public Config()
    {
        SetToDefaults();
    }
    public void SetToDefaults()
    {
        Enabled = true;
        OverrideNormalCastColor = true;
        NormalCastColor = DefaultCastColor;
        OverrideInterruptableCastColor = true;
        InterruptableCastColor = DefaultInterruptableCastColor;
        OverrideTankTargetEnmityGemColor = true;
        TankGemColor = DefaultTankGemColor;
    }

    [NonSerialized] private DalamudPluginInterface? _pluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
    }

    public void Save()
    {
        _pluginInterface?.SavePluginConfig(this);
    }
}
