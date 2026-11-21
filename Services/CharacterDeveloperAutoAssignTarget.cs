using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Core;

namespace HeroPathAutoBuildAssignment.Models;

public class CharacterDeveloperAutoAssignTarget : IAutoAssignTarget
{
    private readonly CharacterDeveloperHeroItemVM _vm;
    
    public CharacterDeveloperAutoAssignTarget(CharacterDeveloperHeroItemVM vm)
    {
        _vm = vm;
    }

    public Hero Hero => _vm.Hero;
    
    public int UnspentAttributePoints =>
        _vm.UnspentAttributePoints;

    public int UnspentFocusPoints =>
        _vm.UnspentCharacterPoints;

    public int GetAttributeValue(CharacterAttribute attribute) => 
        GetAttributeVM(attribute)?.AttributeValue ?? 0;

    public int GetFocus(SkillObject skill) =>
        GetSkillVM(skill)?.CurrentFocusLevel ?? 0;

    public bool GetPerkValue(PerkObject perk)
    {
        return _vm.PerkSelection.IsPerkSelected(perk);
    }

    public IReadOnlyPropertyOwner<CharacterAttribute> GetCharacterAttributes()
    {
        return _vm.CharacterAttributes;
    }

    public bool CanAddFocusToSkill(SkillObject skill)
    {
        var skillVM = GetSkillVM(skill);

        if (skillVM == null)
        {
            return false;
        }

        return skillVM.CanAddFocus;
    }

    public void AddAttribute(CharacterAttribute attribute, int amount)
    {
        var attributeVM = GetAttributeVM(attribute);

        if (attributeVM == null)
        {
            return;
        }
        attributeVM.ExecuteAddAttributePoint();
    }

    public void AddFocus(SkillObject skill, int amount)
    {
        var skillVM = GetSkillVM(skill);

        if (skillVM == null)
        {
            return;
        }
        
        skillVM.ExecuteAddFocus();
    }

    public void AddPerk(PerkObject perk)
    {
        var perkVM = GetPerkVM(perk);

        if (perkVM == null)
        {
            return;
        }
        
        perkVM.ExecuteStartSelection();
    }

    private CharacterAttributeItemVM GetAttributeVM(CharacterAttribute attribute)
    {
        return _vm.Attributes.FirstOrDefault(x => x.AttributeType == attribute);
    }
    
    private SkillVM GetSkillVM(SkillObject skill)
    {
        return _vm.Skills.FirstOrDefault(x => x.Skill == skill);
    }

    private PerkVM GetPerkVM(PerkObject perk)
    {
        return _vm.Skills
            .SelectMany(skillVM => skillVM.Perks)
            .FirstOrDefault(perkVM => perkVM.Perk == perk);
    }
}