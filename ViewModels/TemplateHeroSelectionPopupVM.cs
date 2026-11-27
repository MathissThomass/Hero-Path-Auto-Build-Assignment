using System;
using System.Collections.Generic;
using HeroPathAutoBuildAssignment.Models;
using HeroPathAutoBuildAssignment.Utils;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace HeroPathAutoBuildAssignment.ViewModels;

public enum HeroSelectionMode
{
    Multiple,
    Single,
    Close
}

public class TemplateHeroSelectionPopupVM : ViewModel
{
    private MBBindingList<HeroSelectionItemVM> _heroItemList;
    private TemplateManagerCharacter? _currentTemplate;

    private HeroSelectionMode _currentMode = HeroSelectionMode.Close;
    public HeroSelectionMode CurrentMode => _currentMode;
    private Hero? _selectedHero;
    private Action<Hero>? _onHeroConfirmed;
    public event Action? ModeChanged;
    
    private bool _isConfirmEnabled;
    private bool _isConfirmButtonVisible;
    private bool _isHeroListOpen;

    private string _heroSelectionTitle;
    private string _AssignToHeroTitle;
    private string _ImportPerksTitle;
    

    public TemplateHeroSelectionPopupVM()
    {
        _heroItemList = new MBBindingList<HeroSelectionItemVM>();
        _AssignToHeroTitle = new TextObject("{=assign_to_hero_text}Assign to hero").ToString();
        _ImportPerksTitle = new TextObject("{=import_perk_text}Import perks").ToString();
        _currentMode = HeroSelectionMode.Close;
    }

    public void OpenForTemplate(TemplateManagerCharacter template, List<TemplateManagerCharacter> templateList)
    {
        _currentTemplate = template;
        SetMode(HeroSelectionMode.Multiple);
        _selectedHero = null;
        _onHeroConfirmed = null;
        IsConfirmEnabled = false;
        IsConfirmButtonVisible = false;
        HeroSelectionTitle = _AssignToHeroTitle;
        RefreshItemListForTemplate(templateList);
    }

    public void OpenForSingleHeroSelection(
        Action<Hero> onHeroConfirmed)
    {
        SetMode(HeroSelectionMode.Single);
        _selectedHero = null;
        _onHeroConfirmed = onHeroConfirmed;
        IsConfirmEnabled = false;
        IsConfirmButtonVisible = true;
        HeroSelectionTitle = _ImportPerksTitle;
        RefreshItemList();
    }

    public void Close()
    {
        SetMode(HeroSelectionMode.Close);
        _currentTemplate = null;
        _selectedHero = null;
        _onHeroConfirmed = null;
        IsConfirmEnabled = false;
        IsConfirmButtonVisible = false;
        HeroItemList.Clear();
        OnPropertyChanged("HeroItemList");
    }

    private void SetMode(HeroSelectionMode mode)
    {
        if (_currentMode == mode)
            return;

        _currentMode = mode;
        IsHeroListOpen = mode != HeroSelectionMode.Close;

        OnPropertyChanged(nameof(CurrentMode));
        ModeChanged?.Invoke();
    }
    
    public void RefreshItemListForTemplate(List<TemplateManagerCharacter> templateList)
    {
        HeroItemList.Clear();
        var list = TemplateHeroUtils.GetHeroSelectionEntries(templateList, _currentTemplate);
        foreach (var heroSelectionItem in list)
        {
            HeroItemList.Add(new HeroSelectionItemVM(heroSelectionItem.Hero, heroSelectionItem.IsEnabled, heroSelectionItem.IsSelected, OnHeroSelected));
        }
        OnPropertyChanged("HeroItemList");
    }

    private void RefreshItemList()
    {
        HeroItemList.Clear();
        var list = TemplateHeroUtils.GetHeroSelectionEntries();
        foreach (var heroSelectionItem in list)
        {
            HeroItemList.Add(new HeroSelectionItemVM(heroSelectionItem.Hero, heroSelectionItem.IsEnabled, heroSelectionItem.IsSelected, OnHeroSelected));

        }
        OnPropertyChanged("HeroItemList");
    }

    private void OnHeroSelected(bool isSelected, Hero hero)
    {
        if (CurrentMode == HeroSelectionMode.Multiple)
        {
            if (isSelected)
            {
                _currentTemplate.HeroList.Add(hero);
            }
            else
            {
                _currentTemplate.HeroList.Remove(hero);
            }

            return;
        }

        if (isSelected)
        {
            _selectedHero = hero;

            foreach (var heroItem in HeroItemList)
            {
                heroItem.IsSelected = heroItem.Hero == hero;
            }
        }
        else
        {
            _selectedHero = null;
        }

        IsConfirmEnabled = _selectedHero != null;
    }

    private void ExecuteConfirmSelection()
    {
        if (!IsConfirmEnabled || _selectedHero == null)
            return;

        _onHeroConfirmed?.Invoke(_selectedHero);

        Close();
    }

    [DataSourceProperty]
    public MBBindingList<HeroSelectionItemVM> HeroItemList
    {
        get => _heroItemList;
        set
        {
            if (value != _heroItemList)
            {
                _heroItemList = value;
                OnPropertyChanged("HeroItemList");
            }
        }
    }

    [DataSourceProperty]
    public bool IsConfirmEnabled
    {
        get => _isConfirmEnabled;
        set
        {
            if (value != _isConfirmEnabled)
            {
                _isConfirmEnabled = value;
                OnPropertyChanged("IsConfirmEnabled");
            }
        }
    }

    [DataSourceProperty]
    public bool IsConfirmButtonVisible
    {
        get => _isConfirmButtonVisible;

        set
        {
            if (value != _isConfirmButtonVisible)
            {
                _isConfirmButtonVisible = value;
                OnPropertyChanged("IsConfirmButtonVisible");
            }
        }
    }

    [DataSourceProperty]
    public string ConfirmButtonBrushStringId
    {
        get { return _isConfirmEnabled ? "ButtonBrush1" : "ButtonBrush2"; }
    }

    [DataSourceProperty]
    public string HeroSelectionTitle
    {
        get => _heroSelectionTitle;

        set
        {
            if (value != _heroSelectionTitle)
            {
                _heroSelectionTitle = value;
                OnPropertyChanged("HeroSelectionTitle");
            }
        }
    }

    [DataSourceProperty]
    public string ConfirmSelectionText { get; set; } =
        new TextObject("{=import_perk_text}Import perk").ToString();
    
    [DataSourceProperty]
    public bool IsHeroListOpen
    {
        get => _isHeroListOpen;
        set
        {
            if (_isHeroListOpen != value)
            {
                _isHeroListOpen = value;
                OnPropertyChanged("IsHeroListOpen");
            }
        }
    }
}