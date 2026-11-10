using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using SAWC.Core;
using SAWC.Core.Data;

namespace SAWC.Editor
{
    [CustomEditor(typeof(SAWController))]
    public class SAWCDashboard : UnityEditor.Editor
    {
        private const string SessionTabKey = "SAWC_Dashboard_SelectedTabIndex";

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
        }

        private void OnDisable()
        {
            if (_settingsSerialized != null)
            {
                _settingsSerialized.Dispose();
                _settingsSerialized = null;
            }
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
                var errorBox = new HelpBox("Error: CharacterSettings field not found in SAWController.", HelpBoxMessageType.Error);
                _warnings.Add(errorBox);
                return _root;
            }

            _assetField = new PropertyField(_settingsProp, "Character Settings");
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

            _sprintWarningBox = new HelpBox(string.Empty, HelpBoxMessageType.Warning) { style = { display = DisplayStyle.None } };
            _crouchSpeedWarningBox = new HelpBox(string.Empty, HelpBoxMessageType.Warning) { style = { display = DisplayStyle.None } };
            _crouchHeightErrorBox = new HelpBox(string.Empty, HelpBoxMessageType.Error) { style = { display = DisplayStyle.None } };
            _gravityErrorBox = new HelpBox(string.Empty, HelpBoxMessageType.Error) { style = { display = DisplayStyle.None } };

            _warnings.Add(_sprintWarningBox);
            _warnings.Add(_crouchSpeedWarningBox);
            _warnings.Add(_crouchHeightErrorBox);
            _warnings.Add(_gravityErrorBox);

            if (_dashboardTitle != null)
            {
                _dashboardTitle.text = "CONFIGURATION";
            }

            if (_settingsSerialized != null)
            {
                _settingsSerialized.Dispose();
                _settingsSerialized = null;
            }

            if (_settingsProp == null) return;
            var settings = _settingsProp.objectReferenceValue as CharacterSettings;

            if (settings == null)
            {
                var infoBox = new HelpBox("Assign a configuration ScriptableObject to deploy the dashboard.", HelpBoxMessageType.Info);
                _tabsContent.Add(infoBox);
                return;
            }

            _settingsSerialized = new SerializedObject(settings);

            var dataProp = _settingsSerialized.FindProperty(nameof(CharacterSettings.Data));
            if (dataProp == null) return;

            CreateTab(_tabsHeader, _tabsContent, "Movement", dataProp.FindPropertyRelative("Movement"));
            CreateTab(_tabsHeader, _tabsContent, "Jump", dataProp.FindPropertyRelative("Jump"));
            CreateTab(_tabsHeader, _tabsContent, "Crouch", dataProp.FindPropertyRelative("Crouch"));
            CreateTab(_tabsHeader, _tabsContent, "Physics", dataProp.FindPropertyRelative("Physics"));
            CreateTab(_tabsHeader, _tabsContent, "Rotation", dataProp.FindPropertyRelative("Rotation"));
            CreateTab(_tabsHeader, _tabsContent, "Thresholds", dataProp.FindPropertyRelative("Thresholds"));

            if (_tabsHeader.childCount > 0 && _tabsContent.childCount > 0)
            {
                int savedIndex = SessionState.GetInt(SessionTabKey, 0);
                int clampedIndex = Mathf.Clamp(savedIndex, 0, _tabsHeader.childCount - 1);

                SelectTab(_tabsHeader.ElementAt(clampedIndex) as Button, _tabsContent.ElementAt(clampedIndex));
            }

            RunLiveValidation(settings);
        }

        private void CreateTab(VisualElement header, VisualElement content, string tabName, SerializedProperty subProperty)
        {
            if (subProperty == null) return;

            var tabView = new VisualElement { style = { display = DisplayStyle.None } };
            var prop = subProperty.Copy();
            var endProp = prop.GetEndProperty();

            prop.NextVisible(true);
            while (!SerializedProperty.EqualContents(prop, endProp))
            {
                var childProp = prop.Copy();
                var field = new PropertyField(childProp);
                field.Bind(_settingsSerialized);

                tabView.Add(field);
                prop.NextVisible(false);
            }
            content.Add(tabView);

            Button btn = new Button { text = tabName.ToUpper() };
            btn.AddToClassList("tab-button");
            btn.clicked += () => SelectTab(btn, tabView);
            header.Add(btn);
        }

        private void RunLiveValidation(CharacterSettings asset)
        {
            if (_warnings == null || asset == null || _sprintWarningBox == null) return;

            var data = asset.Data;

            if (data.Movement.SprintSpeed < data.Movement.MoveSpeed)
            {
                _sprintWarningBox.text = "Warning: Sprint speed is slower than normal walking speed.";
                _sprintWarningBox.style.display = DisplayStyle.Flex;
            }
            else _sprintWarningBox.style.display = DisplayStyle.None;

            if (data.Crouch.CrouchSpeed > data.Movement.MoveSpeed)
            {
                _crouchSpeedWarningBox.text = "Warning: Crouch speed is higher than normal walking speed.";
                _crouchSpeedWarningBox.style.display = DisplayStyle.Flex;
            }
            else _crouchSpeedWarningBox.style.display = DisplayStyle.None;

            if (data.Crouch.CrouchHeight > data.Crouch.StandingHeight)
            {
                _crouchHeightErrorBox.text = "Logic Error: Crouch height cannot be higher than standing height.";
                _crouchHeightErrorBox.style.display = DisplayStyle.Flex;
            }
            else _crouchHeightErrorBox.style.display = DisplayStyle.None;

            if (data.Physics.GroundedGravity > 0f || data.Physics.TerminalVelocity > 0f)
            {
                _gravityErrorBox.text = "Critical: Gravity parameters have positive values. The character will fly away upward.";
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

            SessionState.SetInt(SessionTabKey, selectedIndex);

            foreach (var child in content.Children())
            {
                child.style.display = DisplayStyle.None;
            }

            targetButton.AddToClassList("tab-button--active");
            targetView.style.display = DisplayStyle.Flex;
        }
    }
}