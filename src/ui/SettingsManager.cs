using Godot;
using System;
using System.Collections.Generic;

public static class SettingsManager
{
    const String USER_SETTINGS_FILE_NAME = "UserSettings.json";
    const String DEFAULT_SETTINGS_FILE_NAME = "DefaultSettings.json";

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
        GD.Print(panelSettings["Test Edit"]);
    }

    private static void SaveSettings(Interface ui)
    {
        using FileAccess file = FileAccess.Open(GetSavePath(), FileAccess.ModeFlags.Write);

        if (file == null)
        {
            Error error = FileAccess.GetOpenError();
            ui.Error(error.ToString());
            return;
        }

        Godot.Collections.Dictionary<String, Godot.Collections.Dictionary<String, Variant>> godotSettingsDict = new Godot.Collections.Dictionary<String, Godot.Collections.Dictionary<String, Variant>>();
        foreach (KeyValuePair<String, Dictionary<String, Variant>> pair in currentSettings)
            godotSettingsDict[pair.Key] = new Godot.Collections.Dictionary<String, Variant>(pair.Value);
        
        String settingsString = Json.Stringify(godotSettingsDict);
        file.StoreLine(settingsString);

        file.Close();
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
        using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);

        if (file == null)
        {
            Error error = FileAccess.GetOpenError();
            ui.Error(error.ToString());
            return;
        }

        String jsonString = file.GetLine();
        Json json = new Json();

        Error parseResult = json.Parse(jsonString);
        if (parseResult != Error.Ok)
        {
            ui.Error(parseResult.ToString());
            return;
        }

        Godot.Collections.Dictionary jsonDict = (Godot.Collections.Dictionary)json.Data;
        Godot.Collections.Dictionary<String, Godot.Collections.Dictionary<String, Variant>> godotDict = new Godot.Collections.Dictionary<String, Godot.Collections.Dictionary<String, Variant>>(jsonDict);
        Dictionary<String, Dictionary<String, Variant>> loadedSettings = new Dictionary<String, Dictionary<String, Variant>>();
        foreach (KeyValuePair<String, Godot.Collections.Dictionary<String, Variant>> pair in godotDict)
            loadedSettings[pair.Key] = new Dictionary<String, Variant>(pair.Value);

        currentSettings = loadedSettings;
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
