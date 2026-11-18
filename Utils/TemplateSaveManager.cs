using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using HeroPathAutoBuildAssignment.Data;
using HeroPathAutoBuildAssignment.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using Path = System.IO.Path;

namespace HeroPathAutoBuildAssignment.Utils;

public class TemplateSaveManager
{
    private const float LegacyImportantSkillPriority = 0.5f;
    private const string LegacyImportantSkillListProperty = "ImportantSkillIdList";
    private const string SkillPriorityProperty = "SkillPriorityById";

    private static string SanitizeFileName(string name)
    {
        var invalid = new string(Path.GetInvalidFileNameChars());
        var regex = new Regex($"[{Regex.Escape(invalid)}]");
        var cleaned = regex.Replace(name, "_").Trim();
        if (string.IsNullOrEmpty(cleaned)) cleaned = "template";
        return cleaned;
    }

    private static string GetSavesFolder()
    {
        var savesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Mount and Blade II Bannerlord", "Configs", "Hero's Path", "SavedTemplates");
        if (!Directory.Exists(savesFolder)) Directory.CreateDirectory(savesFolder);
        return savesFolder;
    }

    public static void SaveTemplate(TemplateCharacterDto data)
    {
        try
        {
            var saveFolder = GetSavesFolder();
            string fileName = SanitizeFileName(data.Name) + ".json";
            string path = Path.Combine(saveFolder, fileName);
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }
        catch (JsonException e)
        {
            TM_Log.Error(e.Message);
            throw;
        }
    }

    public static TemplateCharacterDto LoadTemplateFromFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<TemplateCharacterDto>(json);
    }

    public static string[]? ListSavedTemplates()
    {
        var folder = GetSavesFolder();
        if (!Directory.Exists(folder)) return null;
        return Directory.GetFiles(folder, "*.json");
    }

    public static void ConvertLegacySkillPriorities()
    {
        if (Game.Current?.ObjectManager == null)
        {
            return;
        }

        var files = ListSavedTemplates();
        if (files == null)
        {
            return;
        }

        foreach (var file in files)
        {
            try
            {
                ConvertLegacySkillPriorityFile(file);
            }
            catch (Exception e)
            {
                TM_Log.Error($"Erreur lors de la conversion de {file} : {e.Message}");
            }
        }
    }

    private static void ConvertLegacySkillPriorityFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var data = JObject.Parse(json);
        if (data[SkillPriorityProperty] != null)
        {
            return;
        }

        var importantSkillIds = new HashSet<string>(StringComparer.Ordinal);
        if (data[LegacyImportantSkillListProperty] is JArray importantSkills)
        {
            foreach (var skillId in importantSkills)
            {
                var id = skillId?.ToString();
                if (!string.IsNullOrEmpty(id))
                {
                    importantSkillIds.Add(id);
                }
            }
        }

        var priorities = new JObject();
        foreach (var skill in CharacterUtils.GetSkillsWithWarSails())
        {
            if (skill == null || string.IsNullOrEmpty(skill.StringId))
            {
                continue;
            }

            var priority = importantSkillIds.Contains(skill.StringId) ? LegacyImportantSkillPriority : 0f;
            priorities[skill.StringId] = priority;
        }

        foreach (var skillId in importantSkillIds)
        {
            if (priorities[skillId] == null)
            {
                priorities[skillId] = LegacyImportantSkillPriority;
            }
        }

        data.Remove(LegacyImportantSkillListProperty);
        data[SkillPriorityProperty] = priorities;
        File.WriteAllText(filePath, data.ToString(Formatting.Indented));
    }

    public static List<TemplateManagerCharacter> LoadSavedTemplatesList()
    {
        var templateManagerCharacterList = new List<TemplateManagerCharacter>();
        var files = ListSavedTemplates();
        if (files == null) return templateManagerCharacterList;
        foreach (var file in files)
        {
            var dto = LoadTemplateFromFile(file);
            TemplateManagerCharacter template = new TemplateManagerCharacter();
            template.CreateNewTemplate();
            templateManagerCharacterList.Add(template);
            dto.ApplyToModel(template);
            template.UpdateSavedStateSnapshot();
        }

        return templateManagerCharacterList;
    }

    public static void DeleteTemplate(string fileName)
    {
        try
        {
            var folder = GetSavesFolder();
            var path = Path.Combine(folder, SanitizeFileName(fileName) + ".json");

            if (!File.Exists(path))
            {
                return;
            }

            File.Delete(path);
        }
        catch (IOException e)
        {
            TM_Log.Error($"IO error while deleting template: {e.Message}");
        }
    }

    public static Tuple<bool, string> CheckIfNameExists(string fileName)
    {
        var folder = GetSavesFolder();
        var path = Path.Combine(folder, fileName + ".json");
        if (!File.Exists(path))
        {
            return new Tuple<bool, string>(true, "");
        }
        return new Tuple<bool, string>(false, "Name already exists");
    }

    public static void RenameSaveFile(string newFileName, string oldFileName)
    {
        var folder = GetSavesFolder();

        string oldPath = Path.Combine(folder, SanitizeFileName(oldFileName) + ".json");
        string newPath = Path.Combine(folder, SanitizeFileName(newFileName) + ".json");

        if (!File.Exists(oldPath))
        {
            TM_Log.Error($"Le fichier à renommer n'existe pas : {oldPath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(oldPath);

            var data = JsonConvert.DeserializeObject<TemplateCharacterDto>(json);

            data.Name = newFileName;

            string newJson = JsonConvert.SerializeObject(data, Formatting.Indented);

            File.WriteAllText(newPath, newJson);

            File.Delete(oldPath);

        }
        catch (Exception e)
        {
            TM_Log.Error($"Erreur lors du renommage : {e.Message}");
        }
    }

    public static void RemoveHeroFromTemplate(Hero hero)
    {
        if (string.IsNullOrEmpty(hero.StringId))
        {
            return;
        }

        var files = ListSavedTemplates();
        if (files == null)
        {
            return;
        }

        foreach (var file in files)
        {
            var dto = LoadTemplateFromFile(file);
            if (dto.HeroIdList.Contains(hero.StringId))
            {
                continue;
            }

            dto.HeroIdList.Remove(hero.StringId);
            SaveTemplate(dto);
            RemoveHeroFromLoadedStore(hero);
            return;
        }

    }

    private static void RemoveHeroFromLoadedStore(Hero hero)
    {
        foreach (var template in TemplateStore.Instance.TemplateList)
        {
            int removed = template.HeroList.RemoveAll(assigned =>
                assigned != null && assigned.StringId == hero.StringId);
            if (removed <= 0)
            {
                continue;
            }

            template.UpdateSavedStateSnapshot();
            return;
        }
    }

}