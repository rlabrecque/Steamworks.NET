using System.IO;
using UnityEditor;
using UnityEngine.UIElements;

public sealed class EditorSteamworksNETSettingsElement : VisualElement
{
    private readonly EditorSteamworksNETSettings _settings;

    private const string UssFilePath =
        "Packages/com.rlabrecque.steamworks.net/Editor/Settings/EditorSteamworksNETSettingsStyleSheet.uss";

    public EditorSteamworksNETSettingsElement()
    {
        _settings = EditorSteamworksNETSettings.Instance;
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssFilePath);

        if (styleSheet)
        {
            styleSheets.Add(styleSheet);
        }
        else
        {
            throw new FileNotFoundException($"File not found: {UssFilePath}");
        }

        var root = new VisualElement();
        root.Add(CreateWindowTitleBar());
        root.Add(CreateDefineSymbolsSection());
        Add(root);
    }

    private static VisualElement CreateWindowTitleBar()
    {
        var titleBar = new VisualElement();
        titleBar.AddToClassList("project-settings-title-bar");

        var title = new Label { text = "Steamworks.NET Settings" };
        title.AddToClassList("project-settings-title-bar__label");

        titleBar.Add(title);
        return titleBar;
    }

    private VisualElement CreateDefineSymbolsSection()
    {
        var toggleField = new Toggle("Can Manage the Define Symbols")
        {
            tooltip = "Set to true to allow Steamworks.NET to add define symbols in your project."
        };

        toggleField.SetValueWithoutNotify(_settings.CanManageDefineSymbols);
        toggleField.RegisterValueChangedCallback(e => { _settings.CanManageDefineSymbols = e.newValue; });

        const string Title = "Define Symbols";

        const string Description =
            "The Steamworks.NET package makes use of define symbols to enable or disable certain features. " +
            "With this setting you can choose to let the package add these define symbols automatically or not.\n" +
            "The default value is true.";

        return CreateSection(title: Title, description: Description, content: toggleField);
    }

    private static VisualElement CreateSection(string title, string description, VisualElement content)
    {
        var section = new VisualElement { name = "Section" };
        section.AddToClassList("steamworks-section");

        var sectionHeader = new Label { text = title };
        sectionHeader.AddToClassList("steamworks-section__header");

        var helpBox = new HelpBox();
        helpBox.AddToClassList("steamworks-section__description");
        var helpBoxIcon = new VisualElement();
        helpBoxIcon.AddToClassList("unity-help-box__icon");
        helpBoxIcon.AddToClassList("unity-help-box__icon--info");
        var helpBoxLabel = new Label { text = description };
        helpBoxLabel.AddToClassList("unity-help-box__label");
        helpBox.Add(helpBoxIcon);
        helpBox.Add(helpBoxLabel);

        content.AddToClassList("steamworks-section__content");

        section.Add(sectionHeader);
        section.Add(helpBox);
        section.Add(content);

        return section;
    }
}