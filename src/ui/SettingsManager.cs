using Godot;
using System;
using System.Collections.Generic;

public static class SettingsManager
{
    private static List<String> SHADOW_QUALITY_VALUES = new List<String> { "Hard", "Soft Very Low", "Soft Low", "Soft Medium", "Soft High", "Soft Ultra" };
    private static List<String> SSAO_QUALITY_VALUES = new List<String> { "Very Low", "Low", "Medium", "High" };
    private static List<String> SSR_QUALITY_VALUES = new List<String> { "Low", "Medium", "High" };
    private static List<String> SCALING_MODE_VALUES = new List<String> { "Bilinear", "FSR" };
    private static List<String> TONEMAP_VALUES = new List<String> { "Linear", "Reinhard", "Filmic", "ACES" };

    const String USER_SETTINGS_FILE_NAME = "UserSettings.cfg";
    const String DEFAULT_SETTINGS_FILE_NAME = "DefaultSettings.cfg";

    private static Dictionary<String, Dictionary<String, Variant>> currentSettings = new Dictionary<String, Dictionary<String, Variant>>();

    public static void ApplySettings(Settings settings, Interface ui, World world)
    {
        String currentPanelName = settings.GetCurrentPanelName();
        Dictionary<String, Variant> currentPanelSettings = settings.GetCurrentPanelSettings();

        currentSettings[currentPanelName] = currentPanelSettings;

        SaveSettings(ui);

        ApplySettings(currentPanelName, currentPanelSettings, world);
    }

    public static void ApplyAllSettings(Settings settings, Interface ui, World world)
    {
        foreach (String panelName in currentSettings.Keys)
            ApplySettings(panelName, currentSettings[panelName], world);
    }

    private static void ApplySettings(String panelName, Dictionary<String, Variant> panelSettings, World world)
    {
        switch (panelName)
        {
            case "General":
                ApplyGeneralSettings(panelSettings, world);
                break;
            case "Graphics":
                ApplyGraphicsSettings(panelSettings, world);
                break;
            case "Environment":
                ApplyEnvironmentSettings(panelSettings, world);
                break;
            case "Keybinds":
                ApplyKeybindsSettings(panelSettings, world);
                break;
            case "Hotbox":
                ApplyHotboxSettings(panelSettings, world);
                break;
            default:
                throw new Exception("Unkown panel name");
        }
    }

    private static void ApplyGeneralSettings(Dictionary<String, Variant> panelSettings, World world)
    {
        Pivot pivot = world.GetPivotNode();
        Interface ui = world.GetInterfaceNode();

        ui.SetTheme((String)panelSettings["Theme"]);

        pivot.SetFieldOfView((float)panelSettings["Field of View"]);
        pivot.SetZoomSpeed((float)panelSettings["Zoom Speed"]);
        pivot.SetXOrbitSpeed((float)panelSettings["X Orbit Speed"]);
        pivot.SetYOrbitSpeed((float)panelSettings["Y Orbit Speed"]);
        pivot.SetPanSpeed((float)panelSettings["Pan Speed"]);
        pivot.SetXOrbitInverted((bool)panelSettings["Invert X Orbit"]);
        pivot.SetYOrbitInverted((bool)panelSettings["Invert Y Orbit"]);
        pivot.SetXPanInverted((bool)panelSettings["Invert X Pan"]);
        pivot.SetYPanInverted((bool)panelSettings["Invert Y Pan"]);
    }

    private static void ApplyGraphicsSettings(Dictionary<String, Variant> panelSettings, World world)
    {
        Godot.Environment environment = world.GetEnvironment();
        DirectionalLight3D light = world.GetLight();
        Viewport viewport = (Viewport)world.GetNode("Pivot").GetNode("MainViewportContainer").GetNode("MainViewport");
        Pivot pivot = world.GetPivotNode();

        DisplayServer.WindowSetVsyncMode((bool)panelSettings["VSync"] ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);

        environment.GlowEnabled = (bool)panelSettings["Glow"];
        environment.SsrEnabled = (bool)panelSettings["Screen-Space Reflections"];
        environment.SsaoEnabled = (bool)panelSettings["Screen-Space Ambient Occlusion"];
        environment.SsilEnabled = (bool)panelSettings["Screen-Space Indirect Lighting"];

        light.ShadowEnabled = (bool)panelSettings["Shadows"];

        world.SetGridEnabled((bool)panelSettings["Grid"]);
        pivot.SetOutlinesEnabled((bool)panelSettings["Outlines"]);

        switch ((String)panelSettings["Anti-Aliasing"])
        {
            case "Disabled":
                viewport.Msaa3D = Viewport.Msaa.Disabled;
                break;
            case "2x":
                viewport.Msaa3D = Viewport.Msaa.Msaa2X;
                break;
            case "4x":
                viewport.Msaa3D = Viewport.Msaa.Msaa4X;
                break;
            case "8x":
                viewport.Msaa3D = Viewport.Msaa.Msaa8X;
                break;
        }

        String shadowQuality = (String)panelSettings["Shadow Quality"];
        String ssaoQuality = (String)panelSettings["Screen-Space Ambient Occlusion Quality"];
        String ssilQuality = (String)panelSettings["Screen-Space Indirect Lighting Quality"];
        String ssrQuality = (String)panelSettings["Screen-Space Reflections Quality"];
        String scalingMode = (String)panelSettings["Scaling Mode"];

        int shadowQualityIdx = SHADOW_QUALITY_VALUES.IndexOf(shadowQuality);
        int ssaoQualityIdx = SSAO_QUALITY_VALUES.IndexOf(ssaoQuality);
        int ssilQualityIdx = SSAO_QUALITY_VALUES.IndexOf(ssilQuality);
        int ssrQualityIdx = SSR_QUALITY_VALUES.IndexOf(ssrQuality);
        int scalingModeIdx = SCALING_MODE_VALUES.IndexOf(scalingMode);

        ProjectSettings.SetSetting("rendering/lights_and_shadows/directional_shadow/soft_shadow_filter_quality", shadowQualityIdx);
        ProjectSettings.SetSetting("rendering/lights_and_shadows/positional_shadow/soft_shadow_filter_quality", shadowQualityIdx);
        ProjectSettings.SetSetting("rendering/environment/screen_space_reflection/roughness_quality", ssrQualityIdx + 1);
        ProjectSettings.SetSetting("rendering/environment/ssil/quality", ssilQualityIdx);
        ProjectSettings.SetSetting("rendering/environment/ssao/quality", ssaoQualityIdx);
        ProjectSettings.SetSetting("rendering/scaling_3d/mode", scalingModeIdx);
        ProjectSettings.SetSetting("rendering/scaling_3d/scale", (float)panelSettings["Scale"]);
        ProjectSettings.SetSetting("anti_aliasing/quality/use_debanding", (bool)panelSettings["Use Debanding"]);
        ProjectSettings.SetSetting("application/run/max_fps", (int)panelSettings["Max FPS"]);

        ProjectSettings.SaveCustom("override.cfg");
    }
    
    private static void ApplyEnvironmentSettings(Dictionary<String, Variant> panelSettings, World world)
    {
        Godot.Environment environment = world.GetEnvironment();
        DirectionalLight3D light = world.GetLight();
        ProceduralSkyMaterial skyMaterial = (ProceduralSkyMaterial)environment.Sky.SkyMaterial;
        
        skyMaterial.SkyEnergyMultiplier = (float)panelSettings["Sky Energy"];
        skyMaterial.GroundEnergyMultiplier = (float)panelSettings["Ground Energy"];
        light.LightEnergy = (float)panelSettings["Sun Energy"];
        environment.TonemapMode = (Godot.Environment.ToneMapper)TONEMAP_VALUES.IndexOf((String)panelSettings["Tonemap"]);
    }

    private static void ApplyKeybindsSettings(Dictionary<String, Variant> panelSettings, World world)
    {
        foreach (KeyValuePair<String, Variant> mapping in panelSettings)
        {
            InputMap.ActionEraseEvents(mapping.Key);
            InputMap.ActionAddEvent(mapping.Key, (InputEvent)mapping.Value);
        }

        world.GetInterfaceNode().UpdateMenuButtons();
    }

    private static void ApplyHotboxSettings(Dictionary<String, Variant> panelSettings, World world)
    {
        HotBox hotBox = world.GetInterfaceNode().GetHotBoxNode();
        Godot.Collections.Array<String> hotBoxItems = (Godot.Collections.Array<String>)panelSettings["Items"];

        hotBox.SetItems(hotBoxItems);
        hotBox.SetDist((float)panelSettings["Distance"]);
        hotBox.SetCancelRadius((float)panelSettings["Cancel Radius"]);
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
            filePath = "res://" + DEFAULT_SETTINGS_FILE_NAME;
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
