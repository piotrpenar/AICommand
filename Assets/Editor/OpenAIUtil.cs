
using System.Threading;
using AICommand.OpenAI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace AICommand
{
    public enum ModelType
    {
        Gpt4,
        Gpt35Turbo
    }

    //Dictionary for the ModelType to the string used by the API
    internal static class ModelTypeDict
    {
        #region Public Methods

        public static string GetModelTypeString(ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.Gpt4 :
                    return "gpt-4";
                default :
                    return "gpt-3.5-turbo";
            }
        }

        #endregion
    }

    internal static class OpenAIUtil
    {
        #region Public Methods

        public static string InvokeChat(string prompt, ModelType modelType,AICommandSettings settings)
        {

            // POST
            using UnityWebRequest post = UnityWebRequest.Post(Api.Url, CreateChatRequestBody(prompt, modelType), "application/json");

            // Request timeout setting
            post.timeout = settings.timeout;

            // API key authorization
            post.SetRequestHeader("Authorization", "Bearer " + settings.apiKey);

            // Request start
            UnityWebRequestAsyncOperation req = post.SendWebRequest();

            // Progress bar (Totally fake! Don't try this at home.)
            for (float progress = 0.0f; !req.isDone; progress += 0.01f)
            {
                EditorUtility.DisplayProgressBar("AI Command", "Generating...", progress);
                Thread.Sleep(100);
                progress += 0.01f;
            }

            EditorUtility.ClearProgressBar();

            // Response extraction
            string json = post.downloadHandler.text;
            Response data = JsonUtility.FromJson<Response>(json);

            return data.choices[0].message.content;
        }

        #endregion

        #region Private Methods

        private static string CreateChatRequestBody(string prompt, ModelType modelType)
        {
            RequestMessage msg = new RequestMessage { role = "user", content = prompt };

            Request req = new Request { model = ModelTypeDict.GetModelTypeString(modelType), messages = new[] { msg }, temperature = 0.3f };

            return JsonUtility.ToJson(req);
        }

        #endregion
    }
}
// namespace AICommand