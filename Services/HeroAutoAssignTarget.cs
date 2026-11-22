using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

namespace HeroPathAutoBuildAssignment.Models;

public class HeroAutoAssignTarget : IAutoAssignTarget
{
    private readonly Hero _hero;

    public HeroAutoAssignTarget(Hero hero)
    {
        _hero = hero;
    }

    public Hero Hero => _hero;

    public int UnspentAttributePoints =>
        _hero.HeroDeveloper.UnspentAttributePoints;

    public int UnspentFocusPoints =>
        _hero.HeroDeveloper.UnspentFocusPoints;

    public int GetAttributeValue(CharacterAttribute attribute) =>
        _hero.GetAttributeValue(attribute);

    public int GetFocus(SkillObject skill) =>
        _hero.HeroDeveloper.GetFocus(skill);

    public bool GetPerkValue(PerkObject perk) =>
        _hero.HeroDeveloper.GetPerkValue(perk);

    public IReadOnlyPropertyOwner<CharacterAttribute> GetCharacterAttributes() =>
        _hero.CharacterAttributes;

    public bool CanAddFocusToSkill(SkillObject skill) =>
        _hero.HeroDeveloper.CanAddFocusToSkill(skill);

    public void AddAttribute(CharacterAttribute attribute, int amount) =>
        _hero.HeroDeveloper.AddAttribute(attribute, amount);

    public void AddFocus(SkillObject skill, int amount) =>
        _hero.HeroDeveloper.AddFocus(skill, amount);

    public void AddPerk(PerkObject perk) =>
        _hero.HeroDeveloper.AddPerk(perk);
}