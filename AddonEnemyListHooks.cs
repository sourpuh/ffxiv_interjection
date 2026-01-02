using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Linq;

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
    private readonly EnemyList _enemyList;

    public AddonEnemyListHooks()
    {
        Plugin.AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "_EnemyList", AddonPostDraw);
        _enemyList = new();
    }
    public void Dispose()
    {
        Plugin.AddonLifecycle.UnregisterListener(AddonPostDraw);
        _enemyList.Dispose();
    }

    public void AddonPostDraw(AddonEvent type, AddonArgs args)
    {
        AddonEnemyList* thisPtr = (AddonEnemyList*)args.Addon.Address;
        if (thisPtr == null || !Plugin.Config.Enabled)
        {
            _enemyList.Dispose();
            return;
        }

        if (!_enemyList.Built)
        {
            _enemyList.Initialize(thisPtr);
        }

        var enemyListArray = AtkStage.Instance()->GetNumberArrayData(NumberArrayType.EnemyList);
        for (var i = 0; i < thisPtr->EnemyCount; i++)
        {
            uint enemyObjectId = (uint)enemyListArray->IntArray[8 + i * 6];
            BattleChara* enemyChara = CharacterManager.Instance()->LookupBattleCharaByEntityId(enemyObjectId);

            if (enemyChara is null) continue;

            BattleChara* targetChara = CharacterManager.Instance()->LookupBattleCharaByEntityId((uint)enemyChara->Character.GetTargetId());

            bool isTargetTank = targetChara != null && TankClasses.Contains(targetChara->Character.CharacterData.ClassJob);
            bool isTargetLocalPlayer = Plugin.ObjectTable.LocalPlayer?.EntityId == enemyChara->Character.GetTargetId();

            _enemyList.UpdateIndex(i, enemyChara->IsCasting ? enemyChara->GetCastInfo() : null, isTargetTank && !isTargetLocalPlayer);
        }
    }
}
