using COG.Config.Impl;
using COG.UI.CustomOption;

namespace COG.UI.SidebarText.Impl;

public class CrewmateSettings : SidebarText
{
    public CrewmateSettings() : base(LanguageConfig.Instance.SidebarTextCrewmate)
    {
    }

    public override void ForResult(ref string result)
    {
        Objects.Clear();
        Objects.AddRange(new[]
        {
            HudStringPatch.GetOptByType(CustomOption.CustomOption.TabType.Crewmate)
        });
    }
}