
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AICommand
{
    public sealed class AIGenerateScriptWindow : EditorWindow
    {
        #region Private Fields

        private string prompt = "Create a script that enables a player to move a gameobject with physics based movement \n" + "The player should be able to walk, run and jump \n" +
                                 "The player should be have a camera gameobject following them that moves smoothly and can be rotated with the mouse \n" +
                                 "The camera should be able to orbit the player and the player should move in the direction the camera is facing";

        #endregion

        #region Constants

        private const string TempFilePath = "Assets/Scripts/";

        private const string ApiKeyErrorText = "API Key hasn't been set. Please check the project settings " + "(Edit > Project Settings > AI Command > API Key).";

        #endregion

        private bool TempFileExists => File.Exists(TempFilePath);

        private bool IsApiKeyOk => !string.IsNullOrEmpty(AICommandSettings.instance.apiKey);

        #region Unity Callbacks

        private void OnGUI()
        {
            if (IsApiKeyOk)
            {
                prompt = EditorGUILayout.TextArea(prompt, GUILayout.ExpandHeight(true));

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

        #endregion

        #region Private Methods

        private void CreateScriptAsset(string code, string className)
        {
            // UnityEditor internal method: ProjectWindowUtil.CreateScriptAssetWithContent
            BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            MethodInfo method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
            method.Invoke(null, new object[] { TempFilePath + className + ".cs", code });
        }

        private static string WrapPrompt(string input) =>
            "Write a full Unity Engine script.\n" + 
            " - Include using UnityEngine;\n" + 
            " - Do not use GameObject.FindGameObjectsWithTag.\n" +
            " - There is no selected object. Find game objects manually.\n" + 
            " - I only need the script body. Don’t add any explanation.\n" + 
            " - The script should be able to be attached to a game object and run.\n" +
            " - If the script uses Lists, remember to include System.Collections.Generic.\n" +
            " - Return only script body. Don't add anything else to the answer. Don't respond in markdown - give me raw text without any additional characters - including any ``` and similar.\n" +
            "The task is described as follows:\n" + input;

        private void RunGenerator()
        {
            ModelType modelType = AICommandWindow.modelType;
            AICommandSettings settings = AICommandSettings.instance;
            string code = OpenAIUtil.InvokeChat(WrapPrompt(prompt), modelType,settings);
            Debug.Log("AI command script:" + code);

            string pattern = @"class\s+(\w+)\s*:";
            string className = "";
            Match match = Regex.Match(code, pattern);

            if (match.Success)
            {
                className = match.Groups[1].Value;
                Debug.Log(className); // Output: AIGenerateScriptWindow
            }

            CreateScriptAsset(code, className);
        }

        [MenuItem("Window/AI Command/Generate Script")]
        private static void Init() => GetWindow<AIGenerateScriptWindow>(true, "AI Generate Script");

        #endregion
    }
} 