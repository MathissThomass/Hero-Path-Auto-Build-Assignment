using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace HeroPathAutoBuildAssignment.Settings;

internal sealed class  HeroPathSettings : AttributeGlobalSettings<HeroPathSettings>
{
    public override string Id => "HeroPathSettings";
    public override string DisplayName => "Hero's Path - Auto Build Assignment";
    public override string FolderName => "Hero's Path";
    public override string FormatType => "json";

    private bool _attributAutomaticAssignment = true;
    private bool _focusAutomaticAssignment = true;
    private bool _unSelectedPerkRandomAssignment = true;


    [SettingPropertyBool("{=attribut_automatic_assignment_option_title}Attributes automatic assignment",
        Order = 0,
        RequireRestart = false,
        HintText = "{=attribut_automatic_assignment_option_hint}Should Hero's Path automatically assign the Attributes point for the hero you assigned a template?")]
    [SettingPropertyGroup("General")]
    public bool AttributAutomaticAssignment
    {
        get => _attributAutomaticAssignment;
        set
        {
            if (value != _attributAutomaticAssignment)
            {
                _attributAutomaticAssignment = value;
                OnPropertyChanged("AttributAutomaticAssignment");
            }
        }
    }
    
    [SettingPropertyBool("{=focus_automatic_assignment_option_title}Focus automatic assignment",
        Order = 1,
        RequireRestart = false,
        HintText = "{=focus_automatic_assignment_option_hint}Should Hero's Path automatically assign the Focus point for the hero you assigned a template?")]
    [SettingPropertyGroup("General")]
    public bool FocusAutomaticAssignment
    {
        get => _focusAutomaticAssignment;
        set
        {
            if (value != _focusAutomaticAssignment)
            {
                _focusAutomaticAssignment = value;
                OnPropertyChanged("FocusAutomaticAssignment");
            }
        }
    }
    
    [SettingPropertyBool("{=unselected_perk_random_assignment_option_title}Unselected Perk random assignment",
        Order = 2,
        RequireRestart = false,
        HintText = "{=unassigned_perk_random_assignment_option_hint}Should Hero's Path choose a random assignment for the unselected perk in the template?")]
    [SettingPropertyGroup("General")]
    public bool UnSelectedPerkRandomAssignment
    {
        get => _unSelectedPerkRandomAssignment;
        set
        {
            if (value != _unSelectedPerkRandomAssignment)
            {
                _unSelectedPerkRandomAssignment = value;
                OnPropertyChanged("UnSelectedPerkRandomAssignment");
            }
        }
    }
}