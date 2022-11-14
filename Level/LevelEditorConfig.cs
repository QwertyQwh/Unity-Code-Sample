using System.IO;
using UnityEditor;
using ZhFramework.Engine.Utilities;

namespace Battle.Level.Editor
{
    internal class LevelEditorConfig
    {
        public string kWorkingPath;
        private const string kEidtorConfigPath = "Assets/_Scripts/Battle/Editor/Level/LevelEditor.json";

        public static LevelEditorConfig Load()
        {
            if (!File.Exists(kEidtorConfigPath))
                return new LevelEditorConfig();

            var data = File.ReadAllText(kEidtorConfigPath);
            var config = JsonUtils.ToObject<LevelEditorConfig>(data);
            return config;
        }

        public void Save()
        {
            FileUtil.DeleteFileOrDirectory(kEidtorConfigPath);
            File.WriteAllText(kEidtorConfigPath, JsonUtils.ToString(this));
        }
    }
}
