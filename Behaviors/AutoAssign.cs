using System;
using System.Collections.Generic;
using System.Linq;
using HeroPathAutoBuildAssignment.Models;
using HeroPathAutoBuildAssignment.Settings;
using HeroPathAutoBuildAssignment.Utils;
using MCM.Abstractions.Base.Global;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

namespace HeroPathAutoBuildAssignment.Behaviors;

public class AutoAssign
{
    private static readonly Random _rng = new Random();
    private const double LearningRateCapMultiplier = 9.0;

    
    public static void AutoAssignFocusPoint(IAutoAssignTarget target, TemplateManagerCharacter template)
    {

        while (target.UnspentFocusPoints > 0)
        {
            var weightList = new Dictionary<SkillObject, double>();

            
            foreach (var templateSkill in template.SkillList)
            {
                var skill = templateSkill.Skill;
                if (!target.CanAddFocusToSkill(skill))
                {
                    weightList[skill] = 0;
                    continue;
                }

                if (!HasRoomForFocusPoint(target, template, skill))
                {
                    weightList[skill] = 0;
                    continue;
                }

                var skillPriority = templateSkill.SkillPriority;
                var learningRate = CharacterUtils.GetLearningRate(target, skill);

                var baseWeight = 1.0 / learningRate;
                var mult = 1.0 + skillPriority;
                double weight = baseWeight;
                var learningRateAfterApplyFocus = CharacterUtils.GetLearningRate(target, skill, 1);
                if ( learningRateAfterApplyFocus > LearningRateCapMultiplier)
                {
                    weight /= learningRateAfterApplyFocus;
                }
                else
                {
                    weight *= mult;
                }

                weightList[skill] = weight;
            }
            
            var pick = CharacterUtils.PickSkillByWeight(weightList);

            if (pick == null)
            {
                break;
            }
            
            target.AddFocus(pick, 1);
        }
    }
    
    public static void AutoAssignAttributPoint(IAutoAssignTarget target, TemplateManagerCharacter template)
    {
        var attributesWithoutRoom = new HashSet<CharacterAttribute>();

        while(target.UnspentAttributePoints > 0)
        {
            var weightList = new Dictionary<SkillObject, double>();

            
            foreach (var templateSkill in template.SkillList)
            {
                var skill = templateSkill.Skill;
                if (!target.CanAddFocusToSkill(skill))
                {
                    weightList[skill] = 0;
                    continue;
                }

                var skillPriority = templateSkill.SkillPriority;
                var learningRate = CharacterUtils.GetLearningRate(target, skill);

                var baseWeight = 1.0 / learningRate;
                var mult = 1.0 + skillPriority;
                double weight = baseWeight * mult;

                weightList[skill] = weight;
            }
            
            var pick = CharacterUtils.PickAttributByWeight(weightList, attributesWithoutRoom);

            if (pick == null)
            {
                break;
            }

            if (!HasRoomForAttributePoint(target, template, pick))
            {
                attributesWithoutRoom.Add(pick);
                continue;
            }
            
            target.AddAttribute(pick, 1);
        }
    }

    public static void CatchUpAssignedHero(IAutoAssignTarget target, TemplateManagerCharacter template)
    {
        var settings = GlobalSettings<HeroPathSettings>.Instance;
        
        if (target == null || template == null || settings == null)
        {
            return;
        }

        foreach (var skill in CharacterUtils.GetSkillsWithWarSails())
        {
            if (skill == null)
            {
                continue;
            }

            AutoAssignPerkPoint(target, template, skill, 0);
        }

        if (settings.AttributAutomaticAssignment && target.UnspentAttributePoints > 0)
        {
            AutoAssignAttributPoint(target, template);
        }

        if (settings.FocusAutomaticAssignment && target.UnspentFocusPoints > 0)
        {
            AutoAssignFocusPoint(target, template);
        }
    }

    public static void AutoAssignPerkPoint(IAutoAssignTarget target, TemplateManagerCharacter template, SkillObject skill,
        int initialLvl)
    {
        var unlockedPerks = CharacterUtils.GetPerksForSkillInRange(skill, initialLvl, target.Hero.GetSkillValue(skill));
        var templatePerkList = template.PerkList.FindAll(perk => perk.Enable).Select(perk => perk.StringId).ToList();
        var unlockedPerkIdInTemplate =
            unlockedPerks.Select(perkObject => perkObject.StringId).Intersect(templatePerkList);
        var perkInTemplateToAdd = PerkObject.All
            .Where(perkObject => unlockedPerkIdInTemplate.Contains(perkObject.StringId))
            .ToList();
        var settings = GlobalSettings<HeroPathSettings>.Instance;

        foreach (var perk in from perk in perkInTemplateToAdd
                 let perkValue = target.GetPerkValue(perk)
                 let alternativePerkValue = target.GetPerkValue(perk.AlternativePerk)
                 where !perkValue && !alternativePerkValue
                 select perk)
        {
            target.AddPerk(ChoosePerkRespectingCaps(target, perk));
        }

        if (settings is { UnSelectedPerkRandomAssignment: false })
        {
            return;
        }
        
        var perkToRandomize = unlockedPerks.Except(perkInTemplateToAdd);
        foreach (var perk in perkToRandomize)
        {
            if (!target.GetPerkValue(perk) && perk.AlternativePerk == null)
            {
                target.AddPerk(ChoosePerkRespectingCaps(target, perk));
                continue;
            }

            if (target.GetPerkValue(perk) || perk.AlternativePerk == null ||
                target.GetPerkValue(perk.AlternativePerk)) continue;
            var rand = _rng.Next(2);

            var picked = rand == 0 ? perk : perk.AlternativePerk;
            target.AddPerk(ChoosePerkRespectingCaps(target, picked));
        }
    }

    private static bool HasRoomForFocusPoint(IAutoAssignTarget target, TemplateManagerCharacter template, SkillObject skill)
    {
        var maxFocus = Campaign.Current.Models.CharacterDevelopmentModel.MaxFocusPerSkill;
        var currentFocus = target.GetFocus(skill);
        return currentFocus + 1 + CountPendingFocusBonuses(target, template, skill) <= maxFocus;
    }

    private static bool HasRoomForAttributePoint(IAutoAssignTarget target, TemplateManagerCharacter template, CharacterAttribute attribute)
    {
        var maxAttribute = Campaign.Current.Models.CharacterDevelopmentModel.MaxAttribute;
        var currentAttribute = target.GetAttributeValue(attribute);
        return currentAttribute + 1 + CountPendingAttributeBonuses(target, template, attribute) <= maxAttribute;
    }

    private static int CountPendingFocusBonuses(IAutoAssignTarget target, TemplateManagerCharacter template, SkillObject skill)
    {
        if (skill != DefaultSkills.OneHanded && skill != DefaultSkills.TwoHanded)
        {
            return 0;
        }

        return IsTemplatePerkStillPending(target, template, DefaultPerks.Crafting.WeaponMasterSmith) ? 1 : 0;
    }

    private static int CountPendingAttributeBonuses(IAutoAssignTarget target, TemplateManagerCharacter template, CharacterAttribute attribute)
    {
        var count = 0;
        foreach (var perk in EnumerateAttributeGrantingPerks(attribute))
        {
            if (IsTemplatePerkStillPending(target, template, perk))
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsTemplatePerkStillPending(IAutoAssignTarget target, TemplateManagerCharacter template, PerkObject? perk)
    {
        if (perk == null || !template.GetPerkValue(perk) || target.GetPerkValue(perk))
        {
            return false;
        }

        var alternative = perk.AlternativePerk;
        return alternative == null || !target.GetPerkValue(alternative);
    }

    private static PerkObject ChoosePerkRespectingCaps(IAutoAssignTarget target, PerkObject perk)
    {
        if (!GrantsBlockedBonus(target, perk))
        {
            return perk;
        }

        var alternative = perk.AlternativePerk;
        if (alternative == null || GrantsSameBlockedBonus(target, perk, alternative))
        {
            return perk;
        }

        return alternative;
    }

    private static bool GrantsBlockedBonus(IAutoAssignTarget target, PerkObject perk)
    {
        foreach (var bonus in GetPerkBonuses(perk))
        {
            if (IsBonusAtCap(target, bonus))
            {
                return true;
            }
        }

        return false;
    }

    private static bool GrantsSameBlockedBonus(IAutoAssignTarget target, PerkObject chosen, PerkObject alternative)
    {
        foreach (var blocked in GetPerkBonuses(chosen))
        {
            if (!IsBonusAtCap(target, blocked))
            {
                continue;
            }

            foreach (var other in GetPerkBonuses(alternative))
            {
                if (blocked.Attribute != null && blocked.Attribute == other.Attribute)
                {
                    return true;
                }

                if (blocked.Skill != null && blocked.Skill == other.Skill)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsBonusAtCap(IAutoAssignTarget target, PerkBonus bonus)
    {
        if (bonus.Attribute != null)
        {
            return target.GetAttributeValue(bonus.Attribute) >=
                   Campaign.Current.Models.CharacterDevelopmentModel.MaxAttribute;
        }

        if (bonus.Skill != null)
        {
            return target.GetFocus(bonus.Skill) >=
                   Campaign.Current.Models.CharacterDevelopmentModel.MaxFocusPerSkill;
        }

        return false;
    }

    private static IEnumerable<PerkObject> EnumerateAttributeGrantingPerks(CharacterAttribute attribute)
    {
        if (attribute == DefaultCharacterAttributes.Vigor)
        {
            yield return DefaultPerks.Crafting.VigorousSmith;
            yield return DefaultPerks.Athletics.Strong;
        }
        else if (attribute == DefaultCharacterAttributes.Control)
        {
            yield return DefaultPerks.Crafting.StrongSmith;
            yield return DefaultPerks.Athletics.Steady;
        }
        else if (attribute == DefaultCharacterAttributes.Endurance)
        {
            yield return DefaultPerks.Crafting.EnduringSmith;
            yield return DefaultPerks.Athletics.Durable;
        }
    }

    private static IEnumerable<PerkBonus> GetPerkBonuses(PerkObject perk)
    {
        if (perk == DefaultPerks.Crafting.VigorousSmith || perk == DefaultPerks.Athletics.Strong)
        {
            yield return new PerkBonus(DefaultCharacterAttributes.Vigor, null);
        }
        else if (perk == DefaultPerks.Crafting.StrongSmith || perk == DefaultPerks.Athletics.Steady)
        {
            yield return new PerkBonus(DefaultCharacterAttributes.Control, null);
        }
        else if (perk == DefaultPerks.Crafting.EnduringSmith || perk == DefaultPerks.Athletics.Durable)
        {
            yield return new PerkBonus(DefaultCharacterAttributes.Endurance, null);
        }
        else if (perk == DefaultPerks.Crafting.WeaponMasterSmith)
        {
            yield return new PerkBonus(null, DefaultSkills.OneHanded);
            yield return new PerkBonus(null, DefaultSkills.TwoHanded);
        }
    }

    private readonly struct PerkBonus
    {
        public PerkBonus(CharacterAttribute? attribute, SkillObject? skill)
        {
            Attribute = attribute;
            Skill = skill;
        }

        public CharacterAttribute? Attribute { get; }
        public SkillObject? Skill { get; }
    }
}