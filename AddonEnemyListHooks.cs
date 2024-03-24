using System;
using System.Linq;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Interjection;

public unsafe class AddonEnemyListHooks : IDisposable
{
    private readonly byte[] TankClasses = [
        /*GLA*/ 1,
        /*MRD*/ 3,
        /*PLD*/ 19,
        /*WAR*/ 21,
        /*DRK*/ 32,
        /*GNB*/ 37
    ];
    private readonly Plugin _plugin;
    private readonly EnemyList _enemyList;

    public AddonEnemyListHooks(Plugin p)
    {
        _plugin = p;
        _plugin.AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "_EnemyList", AddonPostDraw);
        _enemyList = new();
    }
    public void Dispose()
    {
        _plugin.AddonLifecycle.UnregisterListener(AddonPostDraw);
        _enemyList.Dispose();
    }

    public void AddonPostDraw(AddonEvent type, AddonArgs args)
    {
        AddonEnemyList* thisPtr = (AddonEnemyList*)args.Addon;

        if (thisPtr == null || !_plugin.Config.Enabled || _plugin.InPvp)
        {
            _enemyList.Dispose();
            return;
        }

        if (!_enemyList.Built)
        {
            _enemyList.Initialize(_plugin, thisPtr);
        }

        var numArrayHolder = Framework.Instance()->GetUiModule()->GetRaptureAtkModule()->AtkModule.AtkArrayDataHolder;
        if (numArrayHolder.NumberArrayCount <= 21)
        {
            return;
        }

        var enemyListArray = numArrayHolder.NumberArrays[21];
        for (var i = 0; i < thisPtr->EnemyCount; i++)
        {
            uint enemyObjectId = (uint) enemyListArray->IntArray[8 + i * 6];
            BattleChara* enemyChara = CharacterManager.Instance()->LookupBattleCharaByObjectId(enemyObjectId);

            if (enemyChara is null) continue;

            BattleChara* targetChara = CharacterManager.Instance()->LookupBattleCharaByObjectId((uint)enemyChara->Character.GetTargetId());

            bool isTargetTank = targetChara != null && TankClasses.Contains(targetChara->Character.CharacterData.ClassJob);
            bool isTargetLocalPlayer = _plugin.ClientState.LocalPlayer?.ObjectId == enemyChara->Character.GetTargetId();

            var castinfo = enemyChara->GetCastInfo;
            _enemyList.UpdateIndex(i, castinfo, isTargetTank && !isTargetLocalPlayer);
        }
    }
}
