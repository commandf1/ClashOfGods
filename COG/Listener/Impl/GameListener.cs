using System.Collections.Generic;
using COG.Config.Impl;
using COG.Exception;
using COG.Role;
using COG.States;
using COG.Utils;

namespace COG.Listener.Impl;

public class GameListener : IListener
{
    private static readonly List<IListener> RegisteredListeners = new();

    public void OnCoBegin()
    {
        GameStates.InGame = true;
    }

    public void OnSelectRoles()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        RegisteredListeners.Clear();
        GameUtils.Data.Clear();
        var players = PlayerUtils.GetAllPlayers().Disarrange();
        var maxImpostors = GameUtils.GetGameOptions().NumImpostors;

        var getter = Role.RoleManager.GetManager().NewGetter();
        foreach (var player in players)
        {
            if (!getter.HasNext()) break;

            if (maxImpostors > 0)
            {
                maxImpostors--;

                Role.Role? impostorRole;

                try
                {
                    impostorRole = ((Role.RoleManager.RoleGetter)getter).GetNextTypeCampRole(CampType.Impostor);
                }
                catch (GetterCanNotGetException)
                {
                    impostorRole = Role.RoleManager.GetManager().GetTypeCampRoles(CampType.Impostor)[0];
                }

                // 如果没有内鬼职业会出BUG
                try
                {
                    RoleManager.Instance.SetRole(player, impostorRole!.BaseRoleType);
                    GameUtils.Data.Add(player, impostorRole);
                    RegisteredListeners.Add(impostorRole.GetListener(player));
                }
                catch
                {
                    RoleManager.Instance.SetRole(player, AmongUs.GameOptions.RoleTypes.Impostor);
                }
                continue;
            }
            setRoles:
            int crewmateCount = 0;
            while (crewmateCount < maxImpostors)
            {
                var role = getter.GetNext() ?? Role.RoleManager.GetManager().GetTypeCampRoles(CampType.Crewmate)[0];
                if (role!.CampType != CampType.Impostor)
                {
                    RoleManager.Instance.SetRole(player, role.BaseRoleType);
                    GameUtils.Data.Add(player, role);
                    RegisteredListeners.Add(role.GetListener(player));
                    crewmateCount++;
                }
            }
            if (crewmateCount < maxImpostors) goto setRoles;
        }
        
        ListenerManager.GetManager().RegisterListeners(RegisteredListeners.ToArray());
    }

    public void OnGameEnd(AmongUsClient client, EndGameResult endGameResult)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        GameStates.InGame = false;
        foreach (var registeredListener in RegisteredListeners)
        {
            ListenerManager.GetManager().UnregisterListener(registeredListener);
        }
    }

    public void OnGameStart(GameStartManager manager)
    {
        // 改变按钮颜色
        manager.MakePublicButton.color = Palette.DisabledClear;
        manager.privatePublicText.color = Palette.DisabledClear;
    }

    public bool OnMakePublic(GameStartManager manager)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        GameUtils.SendGameMessage(LanguageConfig.Instance.MakePublicMessage);
        // 禁止设置为公开
        return false;
    }

    public void OnSetUpRoleText(IntroCutscene intro)
    {
        var player = PlayerControl.LocalPlayer;
        
        if (!AmongUsClient.Instance.AmHost)
        {
            // something about rpc
            
            return;
        }

        Role.Role? role = null;
        
        foreach (var keyValuePair in GameUtils.Data)
        {
            if (keyValuePair.Key.FriendCode.Equals(player.FriendCode))
            {
                role = keyValuePair.Value;
            }
        }
        
        if (role == null) return;
        
        // 游戏开始的时候显示角色信息
        
        // Main.Logger.LogInfo(intro.YouAreText.text + " " + intro.RoleText.text + " " + intro.RoleBlurbText.text);
        /*
        intro.YouAreText.color = role.Color;
        intro.RoleText.text = role.Name;
        intro.RoleText.color = role.Color;
        intro.RoleBlurbText.color = role.Color;
        intro.RoleBlurbText.text = role.Description;
        */
    }

    public void OnSetUpTeamText(IntroCutscene intro)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            // something about rpc
            
            return;
        }

        PlayerControl? player = null;
        Role.Role? role = null;

        foreach (var keyValuePair in GameUtils.Data)
        {
            var target = keyValuePair.Key;
            if (intro.PlayerPrefab.name.Equals(target.name))
            {
                player = target;
                role = keyValuePair.Value;
            }
        }
        
        if (role == null || player == null) return;
        
        // 游戏开始的时候显示陣營信息
        intro.TeamTitle.color = role.Color;
        if (role.CampType is CampType.Crewmate or CampType.Impostor) return;
        /*
         *  其他陣營文本預留
         */
    }

    public void OnGameEndSetEverythingUp(EndGameManager manager)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            // something about rpc
            
            return;
        }
        Role.Role? role = null;
        foreach (var keyValuePair in GameUtils.Data)
        {
            if (keyValuePair.Key.name.Equals(manager.PlayerPrefab.name))
            {
                role = keyValuePair.Value;
                break;
            }
        }
        if (role == null) return;
        manager.WinText.text = role.CampType.GetCampString() + " 胜利！"; // 暂定 等待后续修改
        manager.WinText.color = role.Color;
    }
}