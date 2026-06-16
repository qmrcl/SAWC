using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SAWC.Editor.Localization
{
    public class SAWCPanel : EditorWindow, IHasCustomMenu
    {
        private Label _greetingLabel;
        private Label _docLabel;
        private Label _activeLangValueLabel;
        private ScrollView _listContainer;
        private Button _docButton;
        private Label _activeLangTitleLabel;
        private Label _installedPacksLabel;

        private List<LanguageData> _cachedLanguages = new();

        private bool _isRendering;

        [MenuItem("Window/SAWC Central Panel")]
        public static void Open()
        {
            var window = GetWindow<SAWCPanel>("SAWC Hub");
            var fixedSize = new Vector2(440, 500);
            window.minSize = fixedSize;
            window.maxSize = fixedSize;
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("SAWCEditor/Refresh Project Languages"), false, RenderLanguageSection);
            menu.AddItem(new GUIContent("SAWCEditor/Sync Localization Fields"), false, LanguageAssetGenerator.RegenerateFields);
        }

        private void OnEnable()
        {
            SAWCLoc.OnLanguageChanged -= UpdateWindowTexts;
            SAWCLoc.OnLanguageChanged += UpdateWindowTexts;
            SAWCLoc.OnLanguageChanged -= RenderLanguageSection;
            SAWCLoc.OnLanguageChanged += RenderLanguageSection;
        }

        private void OnDisable()
        {
            SAWCLoc.OnLanguageChanged -= UpdateWindowTexts;
            SAWCLoc.OnLanguageChanged -= RenderLanguageSection;
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;

            root.Clear();

            string[] uxmlGuids = AssetDatabase.FindAssets($"{nameof(SAWCPanel)} t:VisualTreeAsset");
            string[] ussGuids = AssetDatabase.FindAssets($"{nameof(SAWCPanel)} t:StyleSheet");

            if (uxmlGuids.Length > 0 && ussGuids.Length > 0)
            {
                var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(uxmlGuids[0]));
                var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(ussGuids[0]));

                uxml.CloneTree(root);
                root.styleSheets.Add(uss);
            }
            else
            {
                root.Add(new HelpBox($"UI Assets (UXML/USS) for {nameof(SAWCPanel)} not found.", HelpBoxMessageType.Error));
                return;
            }

            _greetingLabel = root.Q<Label>("greeting-label");
            _docLabel = root.Q<Label>("doc-label");
            _activeLangTitleLabel = root.Q<Label>("active-lang-title");
            _activeLangValueLabel = root.Q<Label>("active-lang-value");
            _installedPacksLabel = root.Q<Label>("installed-packs-label");
            _listContainer = root.Q<ScrollView>("list-container");

            _docButton = root.Q<Button>("doc-button");
            if (_docButton != null)
            {
                _docButton.clicked += () => Application.OpenURL("https://qmrcl.github.io/SAWC-docs/");
            }

            RenderLanguageSection();
        }

        private void RenderLanguageSection()
        {
            if (_listContainer == null || _activeLangValueLabel == null) return;

            if (_isRendering) return;

            _isRendering = true;

            try
            {
                _listContainer.Clear();
                _cachedLanguages = SAWCLoc.System.GetProjectLanguages();
                SAWCLoc.System.EnsureValidSelection(_cachedLanguages);

                if (_cachedLanguages.Count == 0)
                {
                    _listContainer.Add(new HelpBox("No language assets found in project.", HelpBoxMessageType.Warning));
                    _activeLangValueLabel.text = "NONE";
                    return;
                }

                string activeNameText = "Unknown";

                foreach (var lang in _cachedLanguages)
                {
                    if (lang.IsActive) activeNameText = lang.RawName.ToUpper();
                    _listContainer.Add(CreateLanguageRow(lang));
                }

                _activeLangValueLabel.text = activeNameText;
                UpdateWindowTexts();
            }
            finally
            {
                _isRendering = false;
            }
        }

        private VisualElement CreateLanguageRow(LanguageData lang)
        {
            var row = new VisualElement();
            row.AddToClassList("language-row");

            var nameGroup = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            var icon = new Label(lang.IsActive ? "✅" : "🌐") { style = { fontSize = 11, marginRight = 6 } };

            var nameLabel = new Label(lang.RawName);
            if (lang.IsActive) nameLabel.AddToClassList("lang-active-title");

            var fileNameLabel = new Label($"({lang.AssetName})");
            fileNameLabel.AddToClassList("lang-filename");

            nameGroup.Add(icon);
            nameGroup.Add(nameLabel);
            nameGroup.Add(fileNameLabel);
            row.Add(nameGroup);

            if (lang.IsActive)
            {
                var activeBadge = new Label("ACTIVE");
                activeBadge.AddToClassList("active-badge");
                row.Add(activeBadge);
            }
            else
            {
                var btn = new Button(() => SAWCLoc.System.ActivateLanguage(lang.Guid)) { text = "ACTIVATE" };
                btn.AddToClassList("activate-btn");
                row.Add(btn);
            }

            return row;
        }

        private void UpdateWindowTexts()
        {
            var lang = SAWCLoc.Current;

            if (_greetingLabel != null) _greetingLabel.text = GetLocText(lang?.Editor_Hello, "Welcome to SAW Hub");
            if (_docLabel != null) _docLabel.text = GetLocText(lang?.Editor_DocMessage, "Select your language below to instantly translate the entire inspector interface. Need help? Check the docs.");
            if (_docButton != null) _docButton.text = GetLocText(lang?.Editor_DocButton, "📖 OPEN DOCUMENTATION");
            if (_activeLangTitleLabel != null) _activeLangTitleLabel.text = GetLocText(lang?.Editor_ActiveLangLabel, "ACTIVE INTERFACE LANGUAGE");
            if (_installedPacksLabel != null) _installedPacksLabel.text = GetLocText(lang?.Editor_InstalledPacksLabel, "INSTALLED LANGUAGE PACKS");
        }

        private string GetLocText(TranslationEntry entry, string defaultText)
        {
            return (entry != null && !string.IsNullOrWhiteSpace(entry.DisplayName)) ? entry.DisplayName : defaultText;
        }
    }
}