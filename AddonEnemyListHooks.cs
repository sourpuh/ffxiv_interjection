﻿using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
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
        AddonEnemyList* thisPtr = (AddonEnemyList*)args.Addon;

        if (thisPtr == null || !Plugin.Config.Enabled)
        {
            _enemyList.Dispose();
            return;
        }

        if (!_enemyList.Built)
        {
            _enemyList.Initialize(thisPtr);
        }

        var numArrayHolder = Framework.Instance()->GetUIModule()->GetRaptureAtkModule()->AtkModule.AtkArrayDataHolder;
        if (numArrayHolder.NumberArrayCount <= 21)
        {
            return;
        }

        var enemyListArray = numArrayHolder.NumberArrays[21];
        for (var i = 0; i < thisPtr->EnemyCount; i++)
        {
            uint enemyObjectId = (uint)enemyListArray->IntArray[8 + i * 6];
            BattleChara* enemyChara = CharacterManager.Instance()->LookupBattleCharaByEntityId(enemyObjectId);

            if (enemyChara is null) continue;

            BattleChara* targetChara = CharacterManager.Instance()->LookupBattleCharaByEntityId((uint)enemyChara->Character.GetTargetId());

            bool isTargetTank = targetChara != null && TankClasses.Contains(targetChara->Character.CharacterData.ClassJob);
            bool isTargetLocalPlayer = Plugin.ClientState.LocalPlayer?.EntityId == enemyChara->Character.GetTargetId();

            var castinfo = enemyChara->GetCastInfo();
            _enemyList.UpdateIndex(i, castinfo, isTargetTank && !isTargetLocalPlayer);
        }
    }
}
