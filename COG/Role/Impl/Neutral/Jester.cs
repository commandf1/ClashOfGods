using System.Linq;
using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.States;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Neutral;

public class Jester : CustomRole, IListener, IWinnable
{
    private readonly CustomOption? _allowReportDeadBody;
    private readonly CustomOption? _allowStartMeeting;

    public Jester() : base(LanguageConfig.Instance.JesterName, Color.magenta, CampType.Neutral)
    {
        Description = LanguageConfig.Instance.JesterDescription;
        _allowStartMeeting = CustomOption.Create(
            ToCustomOption(this), LanguageConfig.Instance.AllowStartMeeting, true,
            MainRoleOption);
        _allowReportDeadBody = CustomOption.Create(
            ToCustomOption(this), LanguageConfig.Instance.AllowReportDeadBody, true,
            MainRoleOption);

        CustomWinnerManager.RegisterWinnableInstance(this);
    }

    public bool CanWin()
    {
        var jester = DeadPlayerManager.DeadPlayers.FirstOrDefault(dp =>
            dp.Role == CustomRoleManager.GetManager().GetTypeRoleInstance<Jester>() &&
            dp.DeathReason == Utils.DeathReason.Exiled);
        if (jester == null) return false;

        CustomWinnerManager.RegisterWinningPlayer(jester.Player);
        GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
        return true;
    }

    public ulong GetWeight()
    {
        return IWinnable.GetOrder(5);
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnPlayerReportDeadBody(PlayerReportDeadBodyEvent @event)
    {
        if (!GameStates.InGame) return true;
        var reportedPlayer = @event.Target;
        var player = @event.Player;
        if (!Id.Equals(player.GetMainRole()?.Id)) return true;
        var result1 = _allowStartMeeting.GetBool();
        var result2 = _allowReportDeadBody.GetBool();
        if (!result1 && reportedPlayer == null) return false;
        return result2 || reportedPlayer == null;
    }

    public override IListener GetListener()
    {
        return this;
    }
}