using Bannerlord.UIExtenderEx.Prefabs2;
using Bannerlord.UIExtenderEx.Attributes;

namespace HeroPathAutoBuildAssignment.PrefabExtensionInsertPatch;

[PrefabExtension("CharacterDeveloper", "/Prefab/Window/Widget/Children")]
public class CharacterDeveloperPatchInsertTemplatesButton : Bannerlord.UIExtenderEx.Prefabs2.PrefabExtensionInsertPatch
{
    public override InsertType Type => InsertType.Child;

    public override int Index => int.MaxValue;

    [PrefabExtensionText]
    public string Content => """
                             <ListPanel 
                              WidthSizePolicy="CoverChildren"  
                              HeightSizePolicy="CoverChildren"
                              HorizontalAlignment="Right"
                              VerticalAlignment="Top"
                              MarginRight="30"
                              MarginTop="30"
                              StackLayout.LayoutMethod="VerticalTopToBottom">
                               <Children>
                                 <ButtonWidget
                                         Id="TemplatesCharacterButton"
                                         DoNotPassEventsToChildren="true"
                                         WidthSizePolicy="Fixed"
                                         HeightSizePolicy="Fixed"
                                         SuggestedWidth="180"
                                         SuggestedHeight="40"
                                         Brush="ButtonBrush1"
                                         Command.Click="ExecuteOpenTemplateManager">
                                   <Children>
                                     <RichTextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="OverlayPopup.ButtonText" Brush.FontSize="20" Text="@TitleTemplateText"/>
                                   </Children>
                                 </ButtonWidget>
                                 
                                 <ButtonWidget
                                         Id="ForceApplyTemplateButton"
                                         DoNotPassEventsToChildren="true"
                                         WidthSizePolicy="Fixed"
                                         HeightSizePolicy="Fixed"
                                         SuggestedWidth="180"
                                         SuggestedHeight="40"
                                         MarginTop="20"
                                         Brush="ButtonBrush1"
                                         IsVisible="@CanApplyTemplate"
                                         Hint="@ApplyTemplateHint"
                                         Command.Click="ExecuteForceApplyTemplate">
                                   <Children>
                                     <HintWidget
                                     DataSource="{ApplyTemplateHint}"
                                     WidthSizePolicy="StretchToParent"
                                     HeightSizePolicy="StretchToParent"
                                     Command.HoverBegin="ExecuteBeginHint"
                                     Command.HoverEnd="ExecuteEndHint"/>
                                     <RichTextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="OverlayPopup.ButtonText" Brush.FontSize="20" Text="@ForceApplyTemplateText"/>
                                   </Children>
                                 </ButtonWidget>
                               </Children>
                             </ListPanel>
                             """;
}