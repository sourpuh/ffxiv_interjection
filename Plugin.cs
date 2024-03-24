using Dalamud.Game.Command;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Windowing;

namespace Interjection;

public class Plugin : IDalamudPlugin
{
    public static string Name => "Interjection";

    public IClientState ClientState { get; private set; } = null!;
    public static ICommandManager CommandManager { get; private set; } = null!;
    public DalamudPluginInterface Interface { get; private set; } = null!;
    public IDataManager DataManager { get; private set; } = null!;
    public IPluginLog PluginLog { get; private set; } = null!;
    public IAddonLifecycle AddonLifecycle { get; private set; }
    public static AddonEnemyListHooks Hooks { get; private set; } = null!;
    public ConfigWindow ConfigWindow { get; private set; } = null!;
    public Config Config { get; private set; } = null!;
    public WindowSystem WindowSystem = new(Name);

    internal bool InPvp;

    public Plugin(
        IClientState clientState,
        ICommandManager commandManager, 
        DalamudPluginInterface pluginInterface, 
        IDataManager dataManager,
        IPluginLog pluginLog,
        IAddonLifecycle addonLifecycle)
    {
        ClientState = clientState;
        CommandManager = commandManager;
        DataManager = dataManager;
        Interface = pluginInterface;
        PluginLog = pluginLog;
        AddonLifecycle = addonLifecycle;

        Config = pluginInterface.GetPluginConfig() as Config ?? new Config();
        Config.Initialize(pluginInterface);
        Hooks = new AddonEnemyListHooks(this);

        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);
        pluginInterface.UiBuilder.Draw += DrawUI;
        pluginInterface.UiBuilder.OpenConfigUi += OpenConfigUI;
        ClientState.TerritoryChanged += OnTerritoryChange;
        CommandManager.AddHandler("/interjection", new CommandInfo(ToggleConfig)
        {
            HelpMessage = "Toggles config window."
        });
    }
    public void Dispose()
    {
        Config.Enabled = false;
        Hooks.Dispose();

        WindowSystem.RemoveAllWindows();
        Interface.UiBuilder.Draw -= DrawUI;
        Interface.UiBuilder.OpenConfigUi -= OpenConfigUI;
        ClientState.TerritoryChanged -= OnTerritoryChange;
        CommandManager.RemoveHandler("/interjection");
    }

    private void OnTerritoryChange(ushort e)
    {
        try
        {
            var territory = DataManager.GetExcelSheet<TerritoryType>()?.GetRow(e);
            if (territory != null) InPvp = territory.IsPvpZone;
        }
        catch (KeyNotFoundException)
        {
            PluginLog.Warning("Could not get territory for current zone");
            InPvp = false;
        }
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    public void OpenConfigUI()
    {
        ConfigWindow.IsOpen = true;
    }

    private void ToggleConfig(string command, string args)
    {
        ConfigWindow.IsOpen = !ConfigWindow.IsOpen;
    }
}
