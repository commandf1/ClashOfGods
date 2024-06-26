using COG.Config.Impl;
using COG.Listener;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Crewmate : CustomRole
{
    public Crewmate() : base(LanguageConfig.Instance.CrewmateName, Color.white, CampType.Crewmate, false)
    {
        IsBaseRole = true;
        Description = LanguageConfig.Instance.CrewmateDescription;
    }

    public override IListener GetListener()
    {
        return IListener.EmptyListener;
    }
}