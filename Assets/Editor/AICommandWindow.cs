using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AICommand
{
    public sealed class AICommandWindow : EditorWindow
    {
        #region Private Fields

        private static string prompt = "Create 100 cubes at random points.";

        private string code;

        #endregion

        #region Constants

        private const string TempFilePath = "Assets/AICommandTemp.cs";

        private const string ApiKeyErrorText = "API Key hasn't been set. Please check the project settings " + "(Edit > Project Settings > AI Command > API Key).";

        public static ModelType modelType = ModelType.Gpt4;

        #endregion

        private bool TempFileExists => File.Exists(TempFilePath);

        private bool IsApiKeyOk => !string.IsNullOrEmpty(AICommandSettings.instance.apiKey);

        #region Unity Callbacks

        private void OnGUI()
        {
            if (IsApiKeyOk)
            {
                prompt = EditorGUILayout.TextArea(prompt, GUILayout.ExpandHeight(true));
                EditorGUILayout.EnumPopup("Model To Use", modelType);

                if (modelType == ModelType.Gpt4)
                {
                    EditorGUILayout.HelpBox("Ensure you have approved API access to GPT-4, or the command will fail!", MessageType.Info);
                }

                if (GUILayout.Button("Run"))
                {
                    RunGenerator();
                }
            }
            else
            {
                EditorGUILayout.HelpBox(ApiKeyErrorText, MessageType.Error);
            }
        }

        private void OnEnable() => AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

        private void OnDisable() => AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

        private void OnDestroy()
        {
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }

        #endregion

        #region Private Methods

        private void CreateScriptAsset(string targetCode)
        {
            // UnityEditor internal method: ProjectWindowUtil.CreateScriptAssetWithContent
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            MethodInfo method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);

            if (method != null)
            {
                method.Invoke(null, new object[] { TempFilePath, targetCode });
            }
        }

        private static string WrapPrompt(string input) =>
            "Write a Unity Editor script with the following specifications:\n" +
            " - The script should provide its functionality as a menu item placed in the \"Edit\" menu, with the label \"Do Task\".\n" +
            " - The script should not create or utilize any editor windows. Instead, the designated task should be executed immediately when the menu item is invoked.\n" +
            " - Do not use GameObject.FindGameObjectsWithTag in the script.\n" +
            " - Assume all objects are not tagged.\n" +
            " - The script should not rely on a currently selected object. Instead, find the relevant game objects manually within the script.\n" +
            " - Provide only the body of the script without any additional explanations or comments. .\n" +
            " - Do not use any type of markdown or code formating - return just the C# code in plain text. Do not write 'csharp'. \n" + 
            "The task for the script to perform is described as follows:\n" + 
            input;

        private void RunGenerator()
        {
            
            AICommandSettings settings = AICommandSettings.instance;
            string code = OpenAIUtil.InvokeChat(WrapPrompt(prompt), modelType,settings);
            code = PostProcessGeneratedCode(code); // Add this line
            Debug.Log("AI command script:" + code);
            CreateScriptAsset(code);
        }

        private string PostProcessGeneratedCode(string code)
        {
            // Remove unexpected characters
            code = code.Replace("`", "");

            // Make sure the script starts with 'using' statements or a namespace declaration
            if (!code.TrimStart().StartsWith("using") && !code.TrimStart().StartsWith("namespace"))
            {
                code = "using UnityEngine;\nusing UnityEditor;\n\n" + code;
            }

            return code;
        }

        [MenuItem("Window/AI Command/Editor Command")]
        private static void Init() => GetWindow<AICommandWindow>(true, "AI Command");

        private void OnAfterAssemblyReload()
        {
            if (!TempFileExists)
            {
                return;
            }

            EditorApplication.ExecuteMenuItem("Edit/Do Task");
            AssetDatabase.DeleteAsset(TempFilePath);
        }

        #endregion
    }
} // namespace AICommand