using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// PlayerPrefs-backed implementation of <see cref="IMetaSaveService"/>.
    /// Serializes <see cref="MetaSaveData"/> to JSON via <c>JsonUtility</c>
    /// and stores it as a single PlayerPrefs string entry.
    /// </summary>
    public class PlayerPrefsMetaSaveService : IMetaSaveService
    {
        private const string PrefsKey = "PuzzleTap_MetaSave";

        public void Save(MetaSaveData data)
        {
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(PrefsKey, json);
            PlayerPrefs.Save();
        }

        public MetaSaveData Load()
        {
            if (!PlayerPrefs.HasKey(PrefsKey))
                return new MetaSaveData();

            var json = PlayerPrefs.GetString(PrefsKey);
            if (string.IsNullOrEmpty(json))
                return new MetaSaveData();

            var data = JsonUtility.FromJson<MetaSaveData>(json);
            return data ?? new MetaSaveData();
        }

        public void Delete()
        {
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();
        }
    }
}
