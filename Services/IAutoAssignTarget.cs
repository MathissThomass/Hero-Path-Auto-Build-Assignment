using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

namespace HeroPathAutoBuildAssignment.Models;

public interface IAutoAssignTarget
{
    Hero Hero { get; }

    int UnspentAttributePoints { get; }
    int UnspentFocusPoints { get; }

    int GetAttributeValue(CharacterAttribute attribute);
    int GetFocus(SkillObject skill);
    bool GetPerkValue(PerkObject perk);
    IReadOnlyPropertyOwner<CharacterAttribute> GetCharacterAttributes();

    bool CanAddFocusToSkill(SkillObject skill);

    void AddAttribute(CharacterAttribute attribute, int amount);
    void AddFocus(SkillObject skill, int amount);
    void AddPerk(PerkObject perk);
}