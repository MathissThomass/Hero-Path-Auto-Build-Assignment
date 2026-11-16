using System.ComponentModel;
using System.Runtime.CompilerServices;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using HeroPathAutoBuildAssignment.Behaviors;
using HeroPathAutoBuildAssignment.Data;
using HeroPathAutoBuildAssignment.State;
using HeroPathAutoBuildAssignment.Utils;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace HeroPathAutoBuildAssignment.ViewModels;

[ViewModelMixin("RefreshValues")]
internal class CharacterDeveloperVmMixin : BaseViewModelMixin<CharacterDeveloperVM>
{
    private readonly CharacterDeveloperVM _vm;
    private bool _canApplyTemplate = false;
    private static readonly ConditionalWeakTable<CharacterDeveloperVM, CharacterDeveloperVmMixin>
        Mixins = new();


    public CharacterDeveloperVmMixin(CharacterDeveloperVM vm) : base(vm)
    {
        _vm = vm;
    }
    
    protected override void OnViewModelPropertyChanged(string propertyName)
    {
        base.OnViewModelPropertyChanged(propertyName);

        if (propertyName == nameof(CharacterDeveloperVM.CurrentCharacter))
        {
            UpdateCanApplyTemplate();
        }
    }
    private void UpdateCanApplyTemplate()
    {
        var currentHero = _vm.CurrentCharacter?.Hero;
        bool canApply = false;

        if (currentHero != null)
        {
            var templateList = TemplateStore.Instance.TemplateList;
            var heroTemplate = CharacterUtils.FindHeroAssignedTemplate(currentHero, templateList);
            canApply = heroTemplate != null;
        }
            
        if (_canApplyTemplate != canApply)
        {
            _canApplyTemplate = canApply;
            OnPropertyChangedWithValue(canApply, nameof(CanApplyTemplate));
        }
    }

    public override void OnRefresh()
    {
        base.OnRefresh();
        UpdateCanApplyTemplate();
    }

    [DataSourceMethod]
    public void ExecuteOpenTemplateManager() => OpenTemplate();

    [DataSourceMethod]
    public void ExecuteForceApplyTemplate() => ForceApplyTemplate();

    private void OpenTemplate()
    {
        var gameState = GameStateManager.Current.CreateState<TemplateManagerState>();
        gameState.SelectedHero = _vm.CurrentCharacter?.Hero;
        GameStateManager.Current.PushState(gameState, 0);
    }

    private void ForceApplyTemplate()
    {
        var currentHero = _vm.CurrentCharacter?.Hero;
        var templateList = TemplateStore.Instance.TemplateList;
        
        if (currentHero == null)
        {
            return;
        }
        
        var heroTemplate = CharacterUtils.FindHeroAssignedTemplate(currentHero, templateList);

        if (heroTemplate == null)
        {
            return;
        }
        AutoAssign.CatchUpAssignedHero(currentHero, heroTemplate);

        _vm.RefreshValues();
    }
    
    [DataSourceProperty]
    public bool CanApplyTemplate {
        get { return _canApplyTemplate; }
        set
        {
            if (value != _canApplyTemplate)
            {
                _canApplyTemplate = value;
                OnPropertyChangedWithValue(value, "CanApplyTemplate");
            }
        }
    }
    
    [DataSourceProperty]
    public string TitleTemplateText { get; set; } = new TextObject("{=templates_title}Templates").ToString();
    
    [DataSourceProperty]
    public string ForceApplyTemplateText { get; set; } = new TextObject("{=templates_force_apply}Apply Template").ToString();
 
    [DataSourceProperty]
    public HintViewModel ApplyTemplateHint  {get;set;} = new HintViewModel(new TextObject("{=templates_force_apply_hint}Applies the hero assigned template by automatically assigning available attribute, focus points and perks. Existing assignments will not be changed.{newline}Use This button if automatic assignment stops working or after resetting attributes, focus points or perks with another mod."));
}