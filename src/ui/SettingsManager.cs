using Godot;
using System;
using System.Collections.Generic;

public static class SettingsManager
{
    const String USER_SETTINGS_FILE_NAME = "UserSettings.cfg";
    const String DEFAULT_SETTINGS_FILE_NAME = "DefaultSettings.cfg";

    private static Dictionary<String, Dictionary<String, Variant>> currentSettings = new Dictionary<String, Dictionary<String, Variant>>();

    public static void ApplySettings(Settings settings, Interface ui)
    {
        String currentPanelName = settings.GetCurrentPanelName();
        Dictionary<String, Variant> currentPanelSettings = settings.GetCurrentPanelSettings();

        currentSettings[currentPanelName] = currentPanelSettings;
    
        SaveSettings(ui);

        ApplySettings(currentPanelName, currentPanelSettings);
    }

    public static void ApplyAllSettings(Settings settings, Interface ui)
    {
        foreach (String panelName in currentSettings.Keys)
            ApplySettings(panelName, currentSettings[panelName]);
    }

    private static void ApplySettings(String panelName, Dictionary<String, Variant> panelSettings)
    {
        switch (panelName)
        {
            case "Testing":
                ApplyTestingSettings(panelSettings);
                break;
            default:
                throw new Exception("Unkown panel name");
        }
    }

    private static void ApplyTestingSettings(Dictionary<String, Variant> panelSettings)
    {
        GD.Print(panelSettings["Text"]);
        GD.Print(panelSettings["Dropdown"]);
        GD.Print(panelSettings["Checkbox"]);
    }

    private static void SaveSettings(Interface ui)
    {
        ConfigFile configFile = new ConfigFile();

        foreach (KeyValuePair<String, Dictionary<String, Variant>> sectionPair in currentSettings)
        {
            String section = sectionPair.Key;
            foreach (KeyValuePair<String, Variant> settingPair in sectionPair.Value)
                configFile.SetValue(section, settingPair.Key, settingPair.Value);
        }

        Error saveError = configFile.Save(GetSavePath());

        if (saveError != Error.Ok)
            ui.Error(saveError.ToString());
    }

    public static void LoadSettings(Interface ui)
    {
        String filePath = GetSavePath();
        if (!FileAccess.FileExists(filePath))
        {
            filePath = GetSavePath(DEFAULT_SETTINGS_FILE_NAME);
            if (!FileAccess.FileExists(filePath))
            {
                ui.Error("No user or default settings files were found");
                return;
            }
        }

        LoadSettings(ui, filePath);
    }

    private static void LoadSettings(Interface ui, String filePath)
    {
        ConfigFile configFile = new ConfigFile();

        Error loadError = configFile.Load(filePath);
        if (loadError != Error.Ok)
        {
            ui.Error(loadError.ToString());
            return;
        }

        foreach (String section in configFile.GetSections())
        {
            String[] settingKeys = configFile.GetSectionKeys(section);
            Dictionary<String, Variant> sectionSettings = new Dictionary<String, Variant>();
            foreach (String key in settingKeys)
            {
                Variant value = configFile.GetValue(section, key);
                sectionSettings[key] = value;
            }

            currentSettings[section] = sectionSettings;
        }
    }

    public static Dictionary<String, Variant> GetPanelSettings(String panelName)
    {
        return currentSettings[panelName];
    }    

    private static String GetExecutablePath()
    {
        if (!OS.HasFeature("editor"))
            return OS.GetExecutablePath().Remove(OS.GetExecutablePath().LastIndexOf("/") + 1);
        else
            return "res://";
    }

    public static String GetSavePath(String fileName = USER_SETTINGS_FILE_NAME)
    {
        return GetExecutablePath() + fileName;
    }
}
