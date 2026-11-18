using TaleWorlds.Core;

namespace HeroPathAutoBuildAssignment.Models;

public class TemplateManagerCharacterSkill
{
    public string StringId { get; set; }
    public float SkillPriority { get; set; }
    
    public SkillObject Skill { get; set; }
    
    public TemplateManagerCharacterSkill(SkillObject skillObject, string stringId, float skillPriority)
    {
        Skill = skillObject;
        StringId = stringId;
        SkillPriority = skillPriority;
    }
}