using System.Collections.Generic;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.HManager;
using COG.Listener.Event.Impl.Meeting;
using COG.Role.Impl.Neutral;
using COG.States;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using COG.Utils.Coding;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

[NotUsed]
[Unfinished]
public class Eraser : CustomRole, IListener
{
#pragma warning disable CS8618
    public CustomOption? InitialEraseCooldown { get; }
    public CustomOption? IncreaseCooldownAfterErasing { get; }
    public CustomOption? CanEraseImpostors { get; }
    public CustomButton EraseButton { get; }
    public static PlayerControl? CurrentTarget { get; set; }
    public static Dictionary<PlayerControl, CustomRole> TempErasedPlayerRoles { get; set; }

    public Eraser() : base(LanguageConfig.Instance.EraserName, Palette.ImpostorRed, CampType.Impostor)
    {
        BaseRoleType = RoleTypes.Impostor;
        Description = LanguageConfig.Instance.EraserDescription;
        TempErasedPlayerRoles = new Dictionary<PlayerControl, CustomRole>();

        if (ShowInOptions)
        {
            var type = ToCustomOption(this);
            InitialEraseCooldown = CustomOption.Create(type, LanguageConfig.Instance.EraserInitialEraseCd, 30f, 10f,
                60f, 5f, MainRoleOption);
            IncreaseCooldownAfterErasing = CustomOption.Create(type,
                LanguageConfig.Instance.EraserIncreaseCdAfterErasing, 10f, 5f, 15f, 5f, MainRoleOption);
            CanEraseImpostors = CustomOption.Create(type, LanguageConfig.Instance.EraserCanEraseImpostors, false,
                MainRoleOption);
        }

        EraseButton = CustomButton.Create(() =>
            {
                var role = CurrentTarget!.GetMainRole();
                CustomRole? newRole = role!.CampType switch
                {
                    CampType.Crewmate => CustomRoleManager.GetManager().GetTypeRoleInstance<Crewmate.Crewmate>(),
                    CampType.Neutral => CustomRoleManager.GetManager().GetTypeRoleInstance<Opportunist>(),
                    CampType.Impostor => CustomRoleManager.GetManager().GetTypeRoleInstance<Impostor>(),
                    _ => null
                };

                if (newRole != null) TempErasedPlayerRoles.Add(CurrentTarget!, newRole);

                var currentCd = EraseButton!.Cooldown();
                EraseButton.SetCooldown(currentCd + (IncreaseCooldownAfterErasing?.GetFloat() ?? 10f));
            },
            () => EraseButton?.ResetCooldown(),
            () =>
            {
                if (!CanEraseImpostors?.GetBool() ?? (false && CurrentTarget))
                    return CurrentTarget!.GetMainRole()?.CampType != CampType.Impostor;
                return CurrentTarget;
            },
            () => true,
            null!,
            2,
            KeyCode.E,
            LanguageConfig.Instance.EraseAction,
            () => InitialEraseCooldown?.GetFloat() ?? 30f,
            -1);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnHudUpdate(HudManagerUpdateEvent @event)
    {
        if (!(GameStates.InGame && PlayerControl.LocalPlayer.IsRole(this))) return;
        CurrentTarget = PlayerControl.LocalPlayer.SetClosestPlayerOutline(Color);
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnMeetingEnd(MeetingVotingCompleteEvent @event)
    {
        TempErasedPlayerRoles.ForEach(kvp =>
        {
            var (player, role) = kvp;
            player.RpcSetCustomRole(role);
        });

        TempErasedPlayerRoles.Clear();
        return true;
    }

    public override void ClearRoleGameData()
    {
        TempErasedPlayerRoles.Clear();
        CurrentTarget = null;
    }

    public override IListener GetListener()
    {
        return this;
    }

#pragma warning restore CS8618
}