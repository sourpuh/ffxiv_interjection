using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace Interjection;

public unsafe class EnemyList : IDisposable
{
    private static readonly ByteColor White = new ByteColor()
    {
        R = 255, G = 255, B = 255, A = 255,
    };
    private static readonly ByteColor BrightMultiplier = new ByteColor()
    {
        R = 200, G = 200, B = 200,
    };
    private static readonly ByteColor DefaultMultiplier = new ByteColor()
    {
        R = 100, G = 100, B = 100,
    };

    private Plugin _plugin;
    private EnemyButton[] _enemyButtons;
    public EnemyList()
    {
        _enemyButtons = new EnemyButton[AddonEnemyList.MaxEnemyCount];
    }

    public bool Built { get; private set; }

    public void Initialize(Plugin plugin, AddonEnemyList* enemyList)
    {
        if (Built) return;
        _plugin = plugin;

        for (byte i = 0; i < AddonEnemyList.MaxEnemyCount; i++)
        {
            var enemyComponent = *(&enemyList->EnemyOneComponent)[i];
            _enemyButtons[i] = new(plugin, enemyComponent);
        }
        Built = true;
    }

    public void Dispose()
    {
        if (!Built) return;

        for (int i = 0; i < _enemyButtons.Length; i++)
        {
            _enemyButtons[i].Dispose();
        }
        Built = false;
    }

    public void UpdateIndex(int i, CastInfo* castinfo, bool isTargettingTank)
    {
        if (castinfo != null && castinfo->IsCasting > 0)
        {
            if (_plugin.Config.OverrideInterruptableCastColor && castinfo->Interruptible > 0)
            {
                _enemyButtons[i].ColorCastBarInterruptable();
            }
            else if (_plugin.Config.OverrideNormalCastColor)
            {
                _enemyButtons[i].ColorCastBarNormal();
            } else
            {
                _enemyButtons[i].ResetCastBar();
            }
        }
        else
        {
            _enemyButtons[i].ResetCastBar();
        }

        if (_plugin.Config.OverrideTankTargetEnmityGemColor && isTargettingTank)
        {
            _enemyButtons[i].ColorGemBlue();
        }
        else
        {
            _enemyButtons[i].ResetGem();
        }
    }

    public class EnemyButton : IDisposable
    {
        private Plugin _plugin;
        private CastState _castState;
        private readonly AtkResNode* _castbarBackground;
        private readonly AtkResNode* _castbarForeground;
        private readonly AtkResNode* _abilityName;
        private readonly AtkResNode* _enemyButtonBackground;
        private readonly AtkResNode* _gem;

        private enum CastState
        {
            NotCasting,
            CastingInterruptible,
            CastingUninterruptible,
        };

        public EnemyButton(Plugin plugin, AtkComponentButton* enemyComponent)
        {
            _plugin = plugin;
            if (enemyComponent == null)
            {
                throw new ArgumentNullException();
            }
            if (enemyComponent->AtkComponentBase.UldManager.NodeListCount <= 16)
            {
                throw new ArgumentException("" + enemyComponent->AtkComponentBase.UldManager.NodeListCount);
            }

            _castState = CastState.NotCasting;
            _castbarBackground = enemyComponent->AtkComponentBase.UldManager.NodeList[12];
            _castbarForeground = enemyComponent->AtkComponentBase.UldManager.NodeList[13];
            _abilityName = enemyComponent->AtkComponentBase.UldManager.NodeList[16];
            _enemyButtonBackground = enemyComponent->AtkComponentBase.UldManager.NodeList[2];
            _gem = enemyComponent->AtkComponentBase.UldManager.NodeList[6];
        }

        internal void ColorGemBlue()
        {
            AtkImageNode* gemImage = (AtkImageNode*)_gem->ChildNode;
            gemImage->PartId = 3;
            MultiplyColor(_gem, _plugin.Config.TankGemColor);
        }

        internal void ResetGem()
        {
            MultiplyColor(_gem, DefaultMultiplier);
        }

        internal void ColorCastBarNormal()
        {
            _castState = CastState.CastingUninterruptible;

            MultiplyColor(_enemyButtonBackground, DefaultMultiplier);
            _enemyButtonBackground->Color = _plugin.Config.NormalCastColor;

            MultiplyColor(_castbarBackground, BrightMultiplier);
            _castbarBackground->Color = _plugin.Config.NormalCastColor;

            MultiplyColor(_castbarForeground, BrightMultiplier);
            _castbarForeground->Color = Max(DefaultMultiplier, _plugin.Config.NormalCastColor);
        }
        internal void ColorCastBarInterruptable()
        {
            _castState = CastState.CastingInterruptible;

            MultiplyColor(_enemyButtonBackground, BrightMultiplier);
            _enemyButtonBackground->Color = _plugin.Config.InterruptableCastColor;

            MultiplyColor(_castbarBackground, BrightMultiplier);
            _castbarBackground->Color = _plugin.Config.InterruptableCastColor;

            MultiplyColor(_castbarForeground, BrightMultiplier);
            _castbarForeground->Color = Max(DefaultMultiplier, _plugin.Config.InterruptableCastColor);
        }

        internal void ResetCastBar()
        {
            _castState = CastState.NotCasting;

            MultiplyColor(_enemyButtonBackground, DefaultMultiplier);
            _enemyButtonBackground->Color = White;

            MultiplyColor(_castbarBackground, DefaultMultiplier);
            _castbarBackground->Color = White;

            MultiplyColor(_castbarForeground, DefaultMultiplier);
            _castbarForeground->Color = White;
        }

        private static unsafe void MultiplyColor(AtkResNode* n, ByteColor c)
        {
            n->MultiplyRed = c.R;
            n->MultiplyGreen = c.G;
            n->MultiplyBlue = c.B;
        }

        private static ByteColor Max(ByteColor a, ByteColor b)
        {
            ByteColor c = new()
            {
                R = Math.Max(a.R, b.R),
                G = Math.Max(a.G, b.G),
                B = Math.Max(a.B, b.B),
                A = Math.Max(a.A, b.A)
            };
            return c;
        }

        public void Dispose()
        {
            ResetCastBar();
            ResetGem();
        }
    }
}
