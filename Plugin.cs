using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Interjection;

public class Plugin : IDalamudPlugin
{
    public static string Name => "Interjection";

    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IDalamudPluginInterface Interface { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    public static AddonEnemyListHooks Hooks { get; private set; } = null!;
    public static ConfigWindow ConfigWindow { get; private set; } = null!;
    public static Config Config { get; private set; } = null!;
    public WindowSystem WindowSystem = new(Name);

    public Plugin()
    {
        Config = Interface.GetPluginConfig() as Config ?? new Config();
        Config.Initialize(Interface);
        Hooks = new AddonEnemyListHooks();

        ConfigWindow = new ConfigWindow();
        WindowSystem.AddWindow(ConfigWindow);
        Interface.UiBuilder.Draw += DrawUI;
        Interface.UiBuilder.OpenConfigUi += OpenConfigUI;
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
        CommandManager.RemoveHandler("/interjection");
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
