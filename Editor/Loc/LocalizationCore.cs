using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SAWC.Editor.Localization
{
    public struct LanguageData
    {
        public string Guid;
        public string UniqueName;
        public string RawName;
        public string AssetName;
        public bool IsActive;
        public LanguageAsset Asset;
    }

    public class LocSettings
    {
        private string PrefsKey => $"SAWCActiveLang_{Application.dataPath.GetHashCode():X}";
        public string GetActiveGuid() => EditorPrefs.GetString(PrefsKey, string.Empty);
        public void SetActiveGuid(string guid) => EditorPrefs.SetString(PrefsKey, guid);
    }

    public class LocProvider
    {
        public LanguageAsset LoadAsset(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<LanguageAsset>(path);
        }

        public List<LanguageData> GetAllLanguages(string activeGuid)
        {
            var result = new List<LanguageData>();
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(LanguageAsset)}");

            foreach (var guid in guids)
            {
                var asset = LoadAsset(guid);
                if (asset == null) continue;

                result.Add(new LanguageData
                {
                    Guid = guid,
                    UniqueName = $"{asset.LanguageName} ({asset.name})",
                    RawName = asset.LanguageName,
                    AssetName = $"{asset.name}.asset",
                    IsActive = (guid == activeGuid),
                    Asset = asset
                });
            }
            return result;
        }
    }

    public class LocCache
    {
        private readonly Dictionary<string, FieldInfo> _fields = new(StringComparer.Ordinal);
        public bool IsBuilt { get; private set; }

        public void Clear()
        {
            _fields.Clear();
            IsBuilt = false;
        }

        public void Build()
        {
            _fields.Clear();
            var fields = typeof(LanguageAsset).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(TranslationEntry))
                {
                    _fields[field.Name] = field;
                }
            }
            IsBuilt = true;
        }

        public TranslationEntry GetEntry(LanguageAsset asset, string key)
        {
            if (asset == null || string.IsNullOrEmpty(key)) return null;
            if (!IsBuilt) Build();

            if (_fields.TryGetValue(key, out var fieldInfo))
            {
                return fieldInfo.GetValue(asset) as TranslationEntry;
            }
            return null;
        }
    }

    public class LocalizationSystem
    {
        public event Action OnLanguageChanged;

        private readonly LocSettings _settings;
        private readonly LocProvider _provider;
        private readonly LocCache _cache;
        private LanguageAsset _currentAsset;

        private bool _attemptedLoad;

        public LocalizationSystem()
        {
            _settings = new LocSettings();
            _provider = new LocProvider();
            _cache = new LocCache();
        }

        public LanguageAsset CurrentAsset
        {
            get
            {
                if (_currentAsset == null && !_attemptedLoad)
                {
                    LoadActiveLanguage();
                }
                return _currentAsset;
            }
        }

        private void LoadActiveLanguage()
        {
            _attemptedLoad = true;
            _currentAsset = _provider.LoadAsset(_settings.GetActiveGuid());
            _cache.Clear();
            OnLanguageChanged?.Invoke();
        }

        public List<LanguageData> GetProjectLanguages() => _provider.GetAllLanguages(_settings.GetActiveGuid());

        public void ActivateLanguage(string guid)
        {
            _settings.SetActiveGuid(guid);
            _attemptedLoad = false;
            LoadActiveLanguage();
        }

        public void EnsureValidSelection(List<LanguageData> availableLanguages)
        {
            if (availableLanguages.Count == 0) return;
            string activeGuid = _settings.GetActiveGuid();
            if (!availableLanguages.Exists(l => l.Guid == activeGuid)) ActivateLanguage(availableLanguages[0].Guid);
        }

        public void ReloadCurrentAsset()
        {
            _attemptedLoad = false;
            LoadActiveLanguage();
        }

        public TranslationEntry GetEntryByUnityString(string key)
        {
            return _cache.GetEntry(CurrentAsset, key);
        }

        public void NotifyLanguageChanged()
        {
            OnLanguageChanged?.Invoke();
        }
    }
}