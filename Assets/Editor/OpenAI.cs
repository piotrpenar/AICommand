
using System;

namespace AICommand.OpenAI
{
    public static class Api
    {
        #region Constants

        public const string Url = "https://api.openai.com/v1/chat/completions";

        #endregion
    }

    [Serializable]
    public struct ResponseMessage
    {
        public string content;
        public string role;
    }

    [Serializable]
    public struct ResponseChoice
    {
        public int index;
        public ResponseMessage message;
    }

    [Serializable]
    public struct Response
    {
        public ResponseChoice[] choices;
        public string id;
    }

    [Serializable]
    public struct RequestMessage
    {
        public string content;
        public string role;
    }

    [Serializable]
    public struct Request
    {
        public RequestMessage[] messages;
        public string model;
        public float temperature;
    }
}