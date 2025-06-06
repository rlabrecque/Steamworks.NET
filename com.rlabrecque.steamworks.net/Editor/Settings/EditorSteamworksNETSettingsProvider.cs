using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

public sealed class EditorSteamworksNETSettingsProvider : SettingsProvider
{
    private const string SettingsPath = "Project/Steamworks.NET";

    private EditorSteamworksNETSettingsProvider(SettingsScope scopes, IEnumerable<string> keywords = null)
        : base(SettingsPath, scopes, keywords)
    {
    }

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        base.OnActivate(searchContext, rootElement);
        rootElement.Add(new EditorSteamworksNETSettingsElement());
    }

    /// <summary>
    /// Method which adds your settings provider to ProjectSettings
    /// </summary>
    /// <returns>A <see cref="EditorSteamworksNETSettingsProvider"/>.</returns>
    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        return new EditorSteamworksNETSettingsProvider(SettingsScope.Project);
    }
}