using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using SAWC.Core;
using SAWC.Core.Data;
using SAWC.Editor.Localization;

namespace SAWC.Editor
{
    [CustomEditor(typeof(SAWController))]
    public class SAWCDashboard : UnityEditor.Editor
    {
        private const string SESSION_TAB_KEY = "SAWC_Dashboard_SelectedTabIndex";

        private VisualElement _root;
        private VisualElement _warnings;
        private VisualElement _dashboard;
        private VisualElement _tabsHeader;
        private VisualElement _tabsContent;
        private Label _dashboardTitle;

        private SerializedObject _settingsSerialized;
        private SerializedProperty _settingsProp;
        private PropertyField _assetField;

        private HelpBox _sprintWarningBox;
        private HelpBox _crouchSpeedWarningBox;
        private HelpBox _crouchHeightErrorBox;
        private HelpBox _gravityErrorBox;

        private readonly List<Action> _locUpdates = new();

        private void OnEnable()
        {
            if (target == null) return;

            var fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(CharacterSettings))
                {
                    _settingsProp = serializedObject.FindProperty(field.Name);
                    break;
                }
            }

            SAWCLoc.OnLanguageChanged -= UpdateAllTexts;
            SAWCLoc.OnLanguageChanged += UpdateAllTexts;
        }

        private void OnDisable()
        {
            SAWCLoc.OnLanguageChanged -= UpdateAllTexts;
        }

        private string GetLocText(TranslationEntry entry, string defaultText)
        {
            return (entry != null && !string.IsNullOrWhiteSpace(entry.DisplayName)) ? entry.DisplayName : defaultText;
        }

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();

            string[] uxmlGuids = AssetDatabase.FindAssets($"{nameof(SAWCDashboard)} t:VisualTreeAsset");
            string[] ussGuids = AssetDatabase.FindAssets($"{nameof(SAWCDashboard)} t:StyleSheet");

            if (uxmlGuids.Length > 0 && ussGuids.Length > 0)
            {
                var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(uxmlGuids[0]));
                var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(ussGuids[0]));

                uxml.CloneTree(_root);
                _root.styleSheets.Add(uss);
            }
            else
            {
                _root.Add(new HelpBox($"UI Assets (UXML/USS) for {nameof(SAWCDashboard)} not found.", HelpBoxMessageType.Error));
                return _root;
            }

            _warnings = _root.Q<VisualElement>("warnings-container");
            _dashboard = _root.Q<VisualElement>("dashboard-main");
            _tabsHeader = _root.Q<VisualElement>("tabs-header");
            _tabsContent = _root.Q<VisualElement>("tabs-content");
            _dashboardTitle = _root.Q<Label>("dashboard-title");

            if (_settingsProp == null)
            {
                var errorBox = new HelpBox(string.Empty, HelpBoxMessageType.Error);
                _locUpdates.Add(() => errorBox.text = GetLocText(SAWCLoc.Current?.DashboardErrorNoSettingsField, "Error: CharacterSettings field not found in SAWController."));
                _warnings.Add(errorBox);
                UpdateAllTexts();
                return _root;
            }

            _assetField = new PropertyField(_settingsProp);
            _assetField.RegisterCallback<AttachToPanelEvent>(evt =>
            {
                _assetField.label = GetLocText(SAWCLoc.Current?.DashboardCharacterSettingsLabel, "Character Settings");
            });

            _root.Q<VisualElement>("asset-field-container").Add(_assetField);
            _assetField.RegisterValueChangeCallback(evt => RebuildUIStructure());

            _root.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                if (_settingsProp != null && _settingsProp.objectReferenceValue is CharacterSettings settings)
                {
                    RunLiveValidation(settings);
                }
            });

            RebuildUIStructure();
            return _root;
        }

        private void RebuildUIStructure()
        {
            if (_dashboard == null) return;

            _tabsHeader.Clear();
            _tabsContent.Clear();
            _warnings.Clear();
            _locUpdates.Clear();

            _sprintWarningBox = new HelpBox(string.Empty, HelpBoxMessageType.Warning) { style = { display = DisplayStyle.None } };
            _crouchSpeedWarningBox = new HelpBox(string.Empty, HelpBoxMessageType.Warning) { style = { display = DisplayStyle.None } };
            _crouchHeightErrorBox = new HelpBox(string.Empty, HelpBoxMessageType.Error) { style = { display = DisplayStyle.None } };
            _gravityErrorBox = new HelpBox(string.Empty, HelpBoxMessageType.Error) { style = { display = DisplayStyle.None } };

            _warnings.Add(_sprintWarningBox);
            _warnings.Add(_crouchSpeedWarningBox);
            _warnings.Add(_crouchHeightErrorBox);
            _warnings.Add(_gravityErrorBox);

            _locUpdates.Add(() =>
            {
                if (_dashboardTitle == null) return;
                _dashboardTitle.text = GetLocText(SAWCLoc.Current?.Configuration, "CONFIGURATION").ToUpper();
            });

            if (_settingsProp == null) return;
            var settings = _settingsProp.objectReferenceValue as CharacterSettings;

            if (settings == null)
            {
                var infoBox = new HelpBox(string.Empty, HelpBoxMessageType.Info);
                _locUpdates.Add(() => infoBox.text = GetLocText(SAWCLoc.Current?.DashboardAssignConfig, "Assign a configuration ScriptableObject to deploy the dashboard."));
                _tabsContent.Add(infoBox);
                UpdateAllTexts();
                return;
            }

            if (_settingsSerialized != null) _settingsSerialized.Dispose();
            _settingsSerialized = new SerializedObject(settings);

            var dataProp = _settingsSerialized.FindProperty(nameof(CharacterSettings.Data));
            if (dataProp == null) return;

            CreateTab(_tabsHeader, _tabsContent, "Movement", dataProp.FindPropertyRelative("Movement"), "MovementSettings");
            CreateTab(_tabsHeader, _tabsContent, "Jump", dataProp.FindPropertyRelative("Jump"), "JumpSettings");
            CreateTab(_tabsHeader, _tabsContent, "Crouch", dataProp.FindPropertyRelative("Crouch"), "CrouchSettings");
            CreateTab(_tabsHeader, _tabsContent, "Physics", dataProp.FindPropertyRelative("Physics"), "PhysicsSettings");
            CreateTab(_tabsHeader, _tabsContent, "Rotation", dataProp.FindPropertyRelative("Rotation"), "RotationSettings");
            CreateTab(_tabsHeader, _tabsContent, "Thresholds", dataProp.FindPropertyRelative("Thresholds"), "ThresholdSettings");

            if (_tabsHeader.childCount > 0 && _tabsContent.childCount > 0)
            {
                int savedIndex = SessionState.GetInt(SESSION_TAB_KEY, 0);
                int clampedIndex = Mathf.Clamp(savedIndex, 0, _tabsHeader.childCount - 1);

                SelectTab(_tabsHeader.ElementAt(clampedIndex) as Button, _tabsContent.ElementAt(clampedIndex));
            }

            UpdateAllTexts();
        }

        private void CreateTab(VisualElement header, VisualElement content, string tabName, SerializedProperty subProperty, string structName)
        {
            var tabView = new VisualElement { style = { display = DisplayStyle.None } };
            var prop = subProperty.Copy();
            var endProp = prop.GetEndProperty();

            prop.NextVisible(true);
            while (!SerializedProperty.EqualContents(prop, endProp))
            {
                var childProp = prop.Copy();
                var field = new PropertyField(childProp);
                field.Bind(_settingsSerialized);

                string fieldKey = $"{structName}{childProp.name}";
                string defaultName = childProp.displayName;

                _locUpdates.Add(() =>
                {
                    if (field == null) return;
                    var entry = SAWCLoc.System.GetEntryByUnityString(fieldKey);
                    field.label = GetLocText(entry, defaultName);
                    field.tooltip = (entry != null && !string.IsNullOrWhiteSpace(entry.Tooltip)) ? entry.Tooltip : string.Empty;
                });

                tabView.Add(field);
                prop.NextVisible(false);
            }
            content.Add(tabView);

            Button btn = new Button();
            btn.AddToClassList("tab-button");
            btn.clicked += () => SelectTab(btn, tabView);
            header.Add(btn);

            _locUpdates.Add(() =>
            {
                if (btn == null) return;
                var entryTab = SAWCLoc.System.GetEntryByUnityString(tabName);
                btn.text = GetLocText(entryTab, tabName).ToUpper();
            });
        }

        private void UpdateAllTexts()
        {
            if (_assetField != null)
            {
                _assetField.label = GetLocText(SAWCLoc.Current?.DashboardCharacterSettingsLabel, "Character Settings");
            }

            foreach (var updateAction in _locUpdates)
            {
                updateAction?.Invoke();
            }

            if (_warnings != null && _settingsProp != null && _settingsProp.objectReferenceValue is CharacterSettings settings)
            {
                RunLiveValidation(settings);
            }
        }

        private void RunLiveValidation(CharacterSettings asset)
        {
            if (_warnings == null || asset == null || _sprintWarningBox == null) return;

            var data = asset.Data;
            var lang = SAWCLoc.Current;

            if (data.Movement.SprintSpeed < data.Movement.MoveSpeed)
            {
                _sprintWarningBox.text = GetLocText(lang?.DashboardValidationSprint, "Warning: Sprint speed is slower than normal walking speed.");
                _sprintWarningBox.style.display = DisplayStyle.Flex;
            }
            else _sprintWarningBox.style.display = DisplayStyle.None;

            if (data.Crouch.CrouchSpeed > data.Movement.MoveSpeed)
            {
                _crouchSpeedWarningBox.text = GetLocText(lang?.DashboardValidationCrouchSpeed, "Warning: Crouch speed is higher than normal walking speed.");
                _crouchSpeedWarningBox.style.display = DisplayStyle.Flex;
            }
            else _crouchSpeedWarningBox.style.display = DisplayStyle.None;

            if (data.Crouch.CrouchHeight > data.Crouch.StandingHeight)
            {
                _crouchHeightErrorBox.text = GetLocText(lang?.DashboardValidationCrouchHeight, "Logic Error: Crouch height cannot be higher than standing height.");
                _crouchHeightErrorBox.style.display = DisplayStyle.Flex;
            }
            else _crouchHeightErrorBox.style.display = DisplayStyle.None;

            if (data.Physics.GroundedGravity > 0f || data.Physics.TerminalVelocity > 0f)
            {
                _gravityErrorBox.text = GetLocText(lang?.DashboardValidationGravity, "Critical: Gravity parameters have positive values. The character will fly away upward.");
                _gravityErrorBox.style.display = DisplayStyle.Flex;
            }
            else _gravityErrorBox.style.display = DisplayStyle.None;
        }

        private void SelectTab(Button targetButton, VisualElement targetView)
        {
            var header = targetButton.parent;
            var content = targetView.parent;

            int selectedIndex = 0;

            foreach (var child in header.Children())
            {
                if (child is Button b)
                {
                    b.RemoveFromClassList("tab-button--active");
                    if (b == targetButton)
                    {
                        selectedIndex = header.IndexOf(b);
                    }
                }
            }

            SessionState.SetInt(SESSION_TAB_KEY, selectedIndex);

            foreach (var child in content.Children())
            {
                child.style.display = DisplayStyle.None;
            }

            targetButton.AddToClassList("tab-button--active");
            targetView.style.display = DisplayStyle.Flex;
        }
    }
}