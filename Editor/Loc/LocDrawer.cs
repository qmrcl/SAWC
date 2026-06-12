using SAWC.Localization;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SAWC.Editor.Localization
{
    [CustomPropertyDrawer(typeof(LocAttribute))]
    public class LocDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (!property.hasVisibleChildren)
            {
                var field = new PropertyField(property);

                string fieldName = property.name;
                if (fieldName.Length > 0 && char.IsLower(fieldName[0]))
                {
                    fieldName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
                }

                string key = fieldInfo != null ? $"{fieldInfo.DeclaringType.Name}{fieldName}" : property.name;
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

                    string childName = childProperty.name;
                    if (childName.Length > 0 && char.IsLower(childName[0]))
                    {
                        childName = char.ToUpper(childName[0]) + childName.Substring(1);
                    }

                    string key = fieldInfo != null ? $"{fieldInfo.FieldType.Name}{childName}" : childProperty.name;
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

                string targetText = defaultName;
                if (entry != null && !string.IsNullOrEmpty(entry.DisplayName))
                {
                    targetText = entry.DisplayName;
                }

                string targetTooltip = string.Empty;
                if (entry != null && !string.IsNullOrEmpty(entry.Tooltip))
                {
                    targetTooltip = entry.Tooltip;
                }

                field.label = targetText;
                field.tooltip = targetTooltip;
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