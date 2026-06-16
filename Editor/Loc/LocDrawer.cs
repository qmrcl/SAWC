using SAWC.Localization;
using SAWC.Core.Data;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SAWC.Editor.Localization
{
    [CustomPropertyDrawer(typeof(LocAttribute))]
    public class LocDrawer : PropertyDrawer
    {
        private static readonly HashSet<Type> ValidStructures = new()
        {
            typeof(CharacterSettingsData),
            typeof(MovementSettings),
            typeof(JumpSettings),
            typeof(CrouchSettings),
            typeof(PhysicsSettings),
            typeof(RotationSettings),
            typeof(ThresholdSettings)
        };

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Type fieldType = fieldInfo?.FieldType;

            bool shouldExpand = fieldType != null && ValidStructures.Contains(fieldType);

            if (!property.hasVisibleChildren || !shouldExpand)
            {
                var field = new PropertyField(property);
                string key = fieldInfo != null ? $"{fieldInfo.DeclaringType.Name}{property.name}" : property.name;
                SetupFieldLocalization(field, key, property.displayName);
                return field;
            }

            var container = new VisualElement();
            var foldout = new Foldout { text = property.displayName };

            foldout.viewDataKey = property.propertyPath;
            foldout.style.marginTop = 2;
            foldout.style.marginBottom = 2;

            int parentDepth = property.depth;
            var iterator = property.Copy();
            var nextElement = property.Copy();
            bool hasNext = nextElement.NextVisible(false);

            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.depth <= parentDepth) break;
                    if (hasNext && SerializedProperty.EqualContents(iterator, nextElement)) break;

                    var childProperty = iterator.Copy();
                    var childField = new PropertyField(childProperty);

                    string key = $"{fieldType.Name}{childProperty.name}";
                    SetupFieldLocalization(childField, key, childProperty.displayName);
                    foldout.Add(childField);

                } while (iterator.NextVisible(false));
            }

            container.Add(foldout);
            return container;
        }

        private void SetupFieldLocalization(PropertyField field, string key, string defaultName)
        {
            Action updateLabels = () =>
            {
                if (field == null) return;
                var entry = SAWCLoc.System.GetEntryByUnityString(key);

                field.label = (entry != null && !string.IsNullOrEmpty(entry.DisplayName)) ? entry.DisplayName : defaultName;
                field.tooltip = (entry != null && !string.IsNullOrEmpty(entry.Tooltip)) ? entry.Tooltip : string.Empty;
            };

            field.RegisterCallback<AttachToPanelEvent>(evt =>
            {
                SAWCLoc.OnLanguageChanged -= updateLabels;
                SAWCLoc.OnLanguageChanged += updateLabels;
                updateLabels();
            });

            field.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                SAWCLoc.OnLanguageChanged -= updateLabels;
            });
        }
    }
}