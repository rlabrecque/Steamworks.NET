using UnityEngine;
using UnityEngine.UI;

namespace HeathenEngineering
{
    /// <summary>
    /// <para>Presents log messages to a text object and manages a parent scroll rect simulating the behaviour of the Uniy console panel for use at run time.</para>
    /// <para>For example: calling <see cref="Debug.Log(string message)"/> will cause the message to be appended to the <see cref="Console.text"/> field.</para>
    /// </summary>
    /// <example>
    /// You can find a working example of this componenet in every example scene.
    /// </example>
    public class Console : MonoBehaviour
    {
        /// <summary>
        /// The maximum number of lines to be added to the <see cref="Console.text"/> field.
        /// </summary>
        [Tooltip("The text object log messages will be written to")]
        public int maxLines = 200;

        /// <summary>
        /// The text object log messages will be written to
        /// </summary>
        [Tooltip("The text object log messages will be written to")]
        public Text text;
        /// <summary>
        /// The parent scroll rect of the <see cref="Console.text"/> field.
        /// </summary>
        [Tooltip("The parent scroll rect of the text field")]
        public ScrollRect scrollRect;

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            Color color;

            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    color = Color.red;
                    break;
                case LogType.Warning:
                    color = Color.yellow;
                    break;
                default:
                    color = Color.white;
                    break;
            }

            text.text += "\n<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + logString + "</color>";
            Canvas.ForceUpdateCanvases();
            if (text.cachedTextGenerator.lineCount > maxLines)
            {
                var firstLine = text.cachedTextGenerator.lines[text.cachedTextGenerator.lineCount - maxLines];
                text.text = text.text.Substring(firstLine.startCharIdx);
            }
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}