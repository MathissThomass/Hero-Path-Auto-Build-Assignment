using System;
using System.Linq;
using HeroPathAutoBuildAssignment.Models;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace HeroPathAutoBuildAssignment.ViewModels;

public class TemplateManagerSkillVM : ViewModel
{
    private string _nameText;
    private string _skillId;
    private readonly SkillObject _skillObject;
    private MBBindingList<TemplateManagerPerkVM> _perks;
    private TemplateManagerCharacter _template;
    private bool _isInspected;
    private float _learningRate;
    private int _fullLearningRateLevel;
    private bool _canLearnSkill;
    private int _level;
    private int _maxLevel;
    private float _skillPriority;
    private int _numOfUnopenedPerks;
    Action<TemplateManagerSkillVM> onSkillSelection;


    public TemplateManagerSkillVM(SkillObject skillObject, TemplateManagerCharacter template, float skillPriority,
        Action<TemplateManagerSkillVM> onSkillSelection) 
    {
        _perks = new MBBindingList<TemplateManagerPerkVM>();
        _skillObject = skillObject;
        _skillPriority = TemplateManagerCharacter.ClampSkillPriority(skillPriority);
        SkillId = skillObject.StringId;
        NameText = skillObject.Name.ToString();
        FillHeroData(template);
        this.onSkillSelection = onSkillSelection;
        RefreshNumOfUnopenedPerks();
    }

    public SkillObject Skill
    {
        get { return _skillObject; }
    }

    public void FillHeroData(TemplateManagerCharacter template)
    {
        _template = template;
        LearningRate = 0;
        CanLearnSkill = true;
        _fullLearningRateLevel = 330;
        Level = 330;
        MaxLevel = 330;
        SkillPriority = _skillPriority;

        RefreshPerks();
    }

    [DataSourceProperty]
    public MBBindingList<TemplateManagerPerkVM> Perks
    {
        get { return _perks; }
        set
        {
            if (value != _perks)
            {
                _perks = value;
                OnPropertyChangedWithValue(value, "Perks");
            }
        }
    }

    [DataSourceProperty]
    public int FullLearningRateLevel
    {
        get { return _fullLearningRateLevel; }
        set
        {
            if (value != _fullLearningRateLevel)
            {
                _fullLearningRateLevel = value;
                OnPropertyChangedWithValue(value, "FullLearningRateLevel");
            }
        }
    }


    [DataSourceProperty]
    public bool CanLearnSkill
    {
        get { return _canLearnSkill; }
        set
        {
            if (value != _canLearnSkill)
            {
                _canLearnSkill = value;
                OnPropertyChangedWithValue(value, "CanLearnSkill");
            }
        }
    }

    [DataSourceProperty]
    public float LearningRate
    {
        get { return _learningRate; }
        set
        {
            if (value != _learningRate)
            {
                _learningRate = value;
                OnPropertyChangedWithValue(value, "LearningRate");
            }
        }
    }

    [DataSourceProperty]
    public bool IsInspected
    {
        get { return _isInspected; }
        set
        {
            if (value != _isInspected)
            {
                _isInspected = value;
                OnPropertyChangedWithValue(value, "IsInspected");
            }
        }
    }

    [DataSourceProperty]
    public string NameText
    {
        get { return _nameText; }
        set
        {
            if (value != _nameText)
            {
                _nameText = value;
                OnPropertyChangedWithValue(value, "NameText");
            }
        }
    }

    [DataSourceProperty]
    public string SkillId
    {
        get { return _skillId; }
        set
        {
            if (value != _skillId)
            {
                _skillId = value;
                OnPropertyChangedWithValue(value, "SkillId");
            }
        }
    }

    [DataSourceProperty]
    public int Level
    {
        get { return _level; }
        set
        {
            if (value != _level)
            {
                _level = value;
                OnPropertyChangedWithValue(value, "Level");
            }
        }
    }

    [DataSourceProperty]
    public int MaxLevel
    {
        get { return _maxLevel; }
        set
        {
            if (value != _maxLevel)
            {
                _maxLevel = value;
                OnPropertyChangedWithValue(value, "MaxLevel");
            }
        }
    }

    [DataSourceProperty]
    public float SkillPriority
    {
        get { return _skillPriority; }
        set
        {
            var roundedValue = TemplateManagerCharacter.ClampSkillPriority(value);
            if (roundedValue != _skillPriority)
            {
                _skillPriority = roundedValue;
                OnPropertyChangedWithValue(roundedValue, "SkillPriority");
                OnPropertyChanged("SkillPriorityText");
                OnPropertyChanged("HasSkillPriority");
            }
        }
    }

    [DataSourceProperty]
    public string SkillPriorityText
    {
        get { return _skillPriority.ToString("0.##"); }
    }

    [DataSourceProperty]
    public bool HasSkillPriority
    {
        get { return _skillPriority != 0f; }
    }
    
    [DataSourceProperty]
    public int NumOfUnopenedPerks
    {
        get
        {
            return _numOfUnopenedPerks;
        }
        set
        {
            if (value != _numOfUnopenedPerks)
            {
                _numOfUnopenedPerks = value;
                OnPropertyChangedWithValue(value, "NumOfUnopenedPerks");
            }
        }
    }

    private void ExecuteInspect()
    {
        IsInspected = true;
        onSkillSelection(this);
    }


    private void RefreshPerks()
    {
        Perks.Clear();
        PerkObject? perkAlternative = null;

        var listPerks = PerkObject.All
            .Where(p => p.Skill == Skill)
            .OrderBy(p => p.RequiredSkillValue);

        foreach (var perk in listPerks)
        {
            if (perk.AlternativePerk != null)
            {
                var altType = (perkAlternative == null)
                    ? TemplateManagerPerkVM.PerkAlternativeType.FirstAlternative
                    : TemplateManagerPerkVM.PerkAlternativeType.SecondAlternative;

                var vm = new TemplateManagerPerkVM(perk, altType, IsPerkSelected, OnPerkSelectedChange);
                Perks.Add(vm);

                if (altType == TemplateManagerPerkVM.PerkAlternativeType.SecondAlternative)
                    perkAlternative = null;
                else
                    perkAlternative = perk.AlternativePerk;
            }
            else
            {
                var vm = new TemplateManagerPerkVM(perk,
                    TemplateManagerPerkVM.PerkAlternativeType.NoAlternative, IsPerkSelected, OnPerkSelectedChange);
                Perks.Add(vm);
                perkAlternative = null;
            }
        }
    }
    
    private void RefreshNumOfUnopenedPerks()
    {
        int num = 0;
        foreach (var perkVM in Perks)
        {
            if (perkVM.CurrentState == TemplateManagerPerkVM.PerkStates.EarnedButNotSelected && (perkVM.AlternativeType == 1 || perkVM.AlternativeType == 0))
            {
                num++;
            }
        }
        NumOfUnopenedPerks = num;
    }

    private void OnPerkSelectedChange(PerkObject perk, bool selected, TemplateManagerPerkVM perkVM)
    {
        _template.SetPerkValue(perk, selected);
        if (perk.AlternativePerk != null)
        {
            RefreshAlternativePerkState(perk.StringId);
        }
        perkVM.RefreshState();
        RefreshNumOfUnopenedPerks();
    }

    private void RefreshAlternativePerkState(string perkStringId)
    {
        if (string.IsNullOrEmpty(perkStringId)) return;
        
        foreach (var perkVM in Perks)
        {
            if (perkVM?.Perk.AlternativePerk == null) continue;
            if (string.IsNullOrEmpty(perkVM.Perk.AlternativePerk.StringId)) continue;
            if (perkVM.Perk.AlternativePerk.StringId != perkStringId) continue;
            perkVM.RefreshState();
            return;
        }
    }

    public void RefreshSkillPerksState()
    {
        foreach (var perkVM in Perks)
        {
            perkVM.RefreshState();
        }
        RefreshNumOfUnopenedPerks();
    }

    private bool IsPerkSelected(PerkObject perk)
    {
        return _template.GetPerkValue(perk);
    }
    
}