using System.Text;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Extensions
{
    public static class MetalitixDebug
    {
        private static readonly int DEFAULT_SIZE = 12;

        public static bool IsEnabled = true;

        public static int FontSize = 12;
        
        
        /// <summary>
        /// Log information to console
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isError"></param>
        /// <param name="requestCallBack"></param>
        public static void Log(object sender, string value, bool isError = false)
        {
            if (IsEnabled)
            {
                LogColor(sender, value, "white", !isError ? "cyan" : "red");
            }
        }

        private static void Log(object message)
        {
            Debug.Log(ApplyStyle(message));
        }

        private static void LogColor(object obj, object message, string colorForText, string colorForName, object secondValue = null)
        {
            var name = obj.GetType().Name;
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("[");
            stringBuilder.Append("<b>");
            stringBuilder.Append("<color=");
            stringBuilder.Append(colorForName);
            stringBuilder.Append(">");
            stringBuilder.Append(name);
            stringBuilder.Append("</color>");
            stringBuilder.Append("</b>");
            stringBuilder.Append("]: ");

            if (secondValue != null)
            {
                stringBuilder.Append("<color=");
                stringBuilder.Append(colorForText);
                stringBuilder.Append(">");
                stringBuilder.Append(message);
                stringBuilder.Append("</color>");
                stringBuilder.Append(secondValue);
            }
            else
            {
                stringBuilder.Append("<color=");
                stringBuilder.Append(colorForText);
                stringBuilder.Append(">");
                stringBuilder.Append(message);
                stringBuilder.Append("</color>");
            }

            Log(ApplyStyle(stringBuilder.ToString()));
        }

        private static object ApplyStyle(object message)
        {
            object log = message;

            if (DEFAULT_SIZE != FontSize)
            {
                log = "<size=" + FontSize + ">" + message + "</size>";
            }

            log += "\n";

            return log;
        }
    }
}