using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace HeroPathAutoBuildAssignment.Models;

public class TemplateCharacterDto
{
    public string Name { get; set; }
    public Dictionary<string, float> SkillPriorityById { get; set; }
    public List<string> EnabledPerkIdList { get; set; }
    public List<string> HeroIdList { get; set; }

    public static TemplateCharacterDto FromModel(TemplateManagerCharacter model)
    {
        return new TemplateCharacterDto
        {
            Name = model.Name,
            SkillPriorityById = GetSkillPrioritiesFromList(model.SkillList),
            EnabledPerkIdList = GetEnabledPerkIdsFromList(model.PerkList),
            HeroIdList = GetHeroIdsFromList(model.HeroList),
        };
    }

    private static Dictionary<string, float> GetSkillPrioritiesFromList(List<TemplateManagerCharacterSkill> skillList)
    {
        var result = new Dictionary<string, float>();
        foreach (var skill in skillList)
        {
            result[skill.StringId] = skill.SkillPriority;
        }
        return result;
    }
    
    private static List<string> GetEnabledPerkIdsFromList(List<TemplateManagerCharacterPerk> perkList)
    {
        var result = new List<string>();
        foreach (var perk in perkList)
        {
            if (perk.Enable)
            {
                result.Add(perk.StringId);
            }
        }
        return result;
    }
    
    private static List<string> GetHeroIdsFromList(List<Hero> heroList)
    {
        var result = new List<string>();
        foreach (var hero in heroList)
        {
            result.Add(hero.StringId);
        }
        return result;
    }

    public void ApplyToModel(TemplateManagerCharacter model)
    {
        model.Name = Name;
        if (SkillPriorityById != null)
        {
            foreach (var skillPriority in SkillPriorityById)
            {
                model.SetSkillPriority(skillPriority.Key, skillPriority.Value);
            }
        }

        foreach (var perkId in EnabledPerkIdList)
        {
            model.SetPerkValue(perkId, true);
        }

        foreach (var heroId in HeroIdList)
        {
            Hero? result = Hero.MainHero.Clan.Heroes.Find(x => x.StringId == heroId);
            if (result != null)
            {
                model.HeroList.Add(result);
            }
        }
    }
}