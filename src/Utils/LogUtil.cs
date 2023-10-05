using System;

namespace mmd2timeline
{
    class LogUtil
    {
        public static void Debug(string log)
        {
#if DEBUG
            Log(log);
#endif
        }
        public static void Debug(Exception ex, string source = null)
        {
            if (source == null)
            {
                source = "Error";
            }

            Debug($"\n{source}\n---\n" + ex.Message);
            //#if DEBUG
            Debug(
               "\nStackTrace ---\n" + ex.StackTrace);
            //#endif
        }
        public static void Log(string log)
        {
            SuperController.LogMessage(DateTime.Now.ToString("HH:mm:ss") + "【mmd2timeline】" + log);
        }
        public static void LogError(string log)
        {
            SuperController.LogError(DateTime.Now.ToString("HH:mm:ss") + "【mmd2timeline】" + log);
        }
        public static void LogError(Exception ex, string source = null)
        {
            if (source == null)
            {
                source = "Error";
            }

            LogError($"\n{source}\n---\n" + ex.Message);
            //#if DEBUG
            LogError(
               "\nStackTrace ---\n" + ex.StackTrace);
            //#endif
        }
        public static void LogWarning(string log)
        {
            SuperController.LogMessage(DateTime.Now.ToString("HH:mm:ss") + "【mmd2timeline】" + log);
        }
    }
}
