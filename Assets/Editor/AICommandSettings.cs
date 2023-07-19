using UnityEditor;

namespace AICommand
{
    [FilePath("UserSettings/AICommandSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class AICommandSettings : ScriptableSingleton<AICommandSettings>
    {
        public string apiKey;
        public int timeout;

        #region Unity Callbacks

        private void OnDisable() => Save();

        #endregion

        #region Public Methods

        public void Save() => Save(true);

        #endregion
    }

    internal sealed class AICommandSettingsProvider : SettingsProvider
    {
        #region Constructors

        public AICommandSettingsProvider() : base("Project/AI Command", SettingsScope.Project)
        {
        }

        #endregion

        #region Unity Callbacks

        public override void OnGUI(string search)
        {
            AICommandSettings settings = AICommandSettings.instance;

            string key = settings.apiKey;
            int timeout = settings.timeout;

            EditorGUI.BeginChangeCheck();

            key = EditorGUILayout.TextField("API Key", key);
            timeout = EditorGUILayout.IntField("Timeout", timeout);

            if (EditorGUI.EndChangeCheck())
            {
                settings.apiKey = key;
                settings.timeout = timeout;
                settings.Save();
            }
        }

        #endregion

        #region Public Methods

        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider() => new AICommandSettingsProvider();

        #endregion
    }
}