using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using BepInEx;
using UnityEngine;

namespace Temtem_Mod.Console
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ConVarProviderAttribute : Attribute
    {
    }
    public class Console : MonoBehaviour
    {
        public delegate void LogReceivedDelegate(Console.Log log);
        public static Console instance { get; private set; }
        public struct Log
        {
            public string message;
            public string stackTrace;
            public LogType logType;
        }
        public static List<Console.Log> logs = new List<Console.Log>();
        public static List<string> usercmdHistory = new List<string>();
        private Dictionary<string, Console.ConCommand> concommandCatalog = new Dictionary<string, Console.ConCommand>();
        private Dictionary<string, string> vstrs = new Dictionary<string, string>();
        private Dictionary<string, BaseConVar> allConVars;
        private List<BaseConVar> archiveConVars;
        private static Queue<string> stdInQueue = new Queue<string>();
        private const int VK_RETURN = 13;
        private const int WM_KEYDOWN = 256;
        private static byte[] inputStreamBuffer = new byte[256];
        public static event Console.LogReceivedDelegate onLogReceived;
        private static Console.SystemConsoleType systemConsoleType = Console.SystemConsoleType.None;
        private static Thread stdInReaderThread = null;


        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int processId);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private static string ReadInputStream()
        {
            if (Console.stdInQueue.Count > 0)
            {
                return Console.stdInQueue.Dequeue();
            }
            return null;
        }

        public BaseConVar FindConVar(string name)
        {
            BaseConVar result;
            if (this.allConVars.TryGetValue(name, out result))
            {
                return result;
            }
            return null;
        }

        private string GetVstrValue(string identifier)
        {
            string result;
            if (this.vstrs.TryGetValue(identifier, out result))
            {
                return result;
            }
            return "";
        }

        public void SubmitCmd(string cmd, bool recordSubmit = false)
        {
            if (recordSubmit)
            {
                Console.Log log = new Console.Log
                {
                    message = string.Format(CultureInfo.InvariantCulture, "<color=#C0C0C0>] {0}</color>", cmd),
                    stackTrace = "",
                    logType = LogType.Log
                };
                Console.logs.Add(log);
                if (Console.onLogReceived != null)
                {
                    Console.onLogReceived(log);
                }
                Console.usercmdHistory.Add(cmd);
            }
            Queue<string> tokens = new Console.Lexer(cmd).GetTokens();
            List<string> list = new List<string>();
            bool flag = false;
            while (tokens.Count != 0)
            {
                string text = tokens.Dequeue();
                if (text == ";")
                {
                    flag = false;
                    if (list.Count > 0)
                    {
                        string concommandName = list[0].ToLower(CultureInfo.InvariantCulture);
                        list.RemoveAt(0);
                        this.RunCmd(concommandName, list);
                        list.Clear();
                    }
                }
                else
                {
                    if (flag)
                    {
                        text = this.GetVstrValue(text);
                        flag = false;
                    }
                    if (text == "vstr")
                    {
                        flag = true;
                    }
                    else
                    {
                        list.Add(text);
                    }
                }
            }
        }

        private void RunCmd(string concommandName, List<string> userArgs)
        {
            Console.ConCommand conCommand = null;
            BaseConVar baseConVar = null;
            ConVarFlags flags;
            if (this.concommandCatalog.TryGetValue(concommandName, out conCommand))
            {
                flags = conCommand.flags;
            }
            else
            {
                baseConVar = this.FindConVar(concommandName);
                if (baseConVar == null)
                {
                    Debug.LogFormat("\"{0}\" is not a recognized ConCommand or ConVar.", new object[]
                    {
                        concommandName
                    });
                    return;
                }
                flags = baseConVar.flags;
            }
            if (conCommand != null)
            {
                try
                {
                    conCommand.action(new ConCommandArgs
                    {
                        commandName = concommandName,
                        userArgs = userArgs
                    });
                }
                catch (ConCommandException ex)
                {
                    Debug.LogFormat("Command \"{0}\" failed: {1}", new object[]
                    {
                        concommandName,
                        ex.Message
                    });
                }
                return;
            }
            if (baseConVar == null)
            {
                return;
            }
            if (userArgs.Count > 0)
            {
                baseConVar.AttemptSetString(userArgs[0]);
                return;
            }
            Debug.LogFormat("\"{0}\" = \"{1}\"\n{2}", new object[]
            {
                concommandName,
                baseConVar.GetString(),
                baseConVar.helpText
            });
        }
        private static void ThreadedInputQueue()
        {
            string item;
            while (Console.systemConsoleType != Console.SystemConsoleType.None && (item = System.Console.ReadLine()) != null)
            {
                Console.stdInQueue.Enqueue(item);
            }
        }
        private void RegisterConVarInternal(BaseConVar conVar)
        {
            if (conVar == null)
            {
                Debug.LogWarning("Attempted to register null ConVar");
                return;
            }
            this.allConVars[conVar.name] = conVar;
            if ((conVar.flags & ConVarFlags.Archive) != ConVarFlags.None)
            {
                this.archiveConVars.Add(conVar);
            }
        }
        private void InitConVars()
        {
            this.allConVars = new Dictionary<string, BaseConVar>();
            this.archiveConVars = new List<BaseConVar>();
            foreach (Type type in typeof(BaseConVar).Assembly.GetTypes())
            {
                foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (fieldInfo.FieldType.IsSubclassOf(typeof(BaseConVar)))
                    {
                        if (fieldInfo.IsStatic)
                        {
                            BaseConVar conVar = (BaseConVar)fieldInfo.GetValue(null);
                            this.RegisterConVarInternal(conVar);
                        }
                        else if (type.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
                        {
                            Debug.LogErrorFormat("ConVar defined as {0}.{1} could not be registered. ConVars must be static fields.", new object[]
                            {
                                type.Name,
                                fieldInfo.Name
                            });
                        }
                    }
                }
                foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (methodInfo.GetCustomAttribute<ConVarProviderAttribute>() != null)
                    {
                        if (methodInfo.ReturnType != typeof(IEnumerable<BaseConVar>) || methodInfo.GetParameters().Length != 0)
                        {
                            Debug.LogErrorFormat("ConVar provider {0}.{1} does not match the signature \"static IEnumerable<ConVar.BaseConVar>()\".", new object[]
                            {
                                type.Name,
                                methodInfo.Name
                            });
                        }
                        else if (!methodInfo.IsStatic)
                        {
                            Debug.LogErrorFormat("ConVar provider {0}.{1} could not be invoked. Methods marked with the ConVarProvider attribute must be static.", new object[]
                            {
                                type.Name,
                                methodInfo.Name
                            });
                        }
                        else
                        {
                            foreach (BaseConVar conVar2 in ((IEnumerable<BaseConVar>)methodInfo.Invoke(null, Array.Empty<object>())))
                            {
                                this.RegisterConVarInternal(conVar2);
                            }
                        }
                    }
                }
            }
            foreach (KeyValuePair<string, BaseConVar> keyValuePair in this.allConVars)
            {
                BaseConVar value = keyValuePair.Value;
                if ((value.flags & ConVarFlags.Engine) != ConVarFlags.None)
                {
                    value.defaultValue = value.GetString();
                }
                else if (value.defaultValue != null)
                {
                    value.AttemptSetString(value.defaultValue);
                }
            }
        }
        private static void SetupSystemConsole()
        {
            bool flag = false;
            bool flag2 = false;
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                if (commandLineArgs[i] == "-console")
                {
                    flag = true;
                }
                if (commandLineArgs[i] == "-console_detach")
                {
                    flag2 = true;
                }
            }
            if (flag)
            {
                Console.systemConsoleType = Console.SystemConsoleType.Attach;
                if (flag2)
                {
                    Console.systemConsoleType = Console.SystemConsoleType.Alloc;
                }
            }
            switch (Console.systemConsoleType)
            {
                case Console.SystemConsoleType.Attach:
                    Console.AttachConsole(-1);
                    break;
                case Console.SystemConsoleType.Alloc:
                    Console.AllocConsole();
                    break;
            }
            if (Console.systemConsoleType != Console.SystemConsoleType.None)
            {
                System.Console.SetIn(new StreamReader(System.Console.OpenStandardInput()));
                Console.stdInReaderThread = new Thread(new ThreadStart(Console.ThreadedInputQueue));
                Console.stdInReaderThread.Start();
            }
        }
        private void Awake()
        {
            Console.instance = this;
            Console.SetupSystemConsole();
            this.InitConVars();
        }
        private void Update()
        {
            string cmd;
            while ((cmd = Console.ReadInputStream()) != null)
            {
                this.SubmitCmd(cmd, true);
            }
        }
        private class Lexer
        {
            private string srcString;
            private int readIndex;
            private StringBuilder stringBuilder = new StringBuilder();
            private enum TokenType
            {
                Identifier,
                NestedString
            }

            public Lexer(string srcString)
            {
                this.srcString = srcString;
                this.readIndex = 0;
            }

            private static bool IsIgnorableCharacter(char character)
            {
                return !Console.Lexer.IsSeparatorCharacter(character) && !Console.Lexer.IsQuoteCharacter(character) && !Console.Lexer.IsIdentifierCharacter(character) && character != '/';
            }
            private static bool IsSeparatorCharacter(char character)
            {
                return character == ';' || character == '\n';
            }
            private static bool IsQuoteCharacter(char character)
            {
                return character == '\'' || character == '"';
            }
            private static bool IsIdentifierCharacter(char character)
            {
                return char.IsLetterOrDigit(character) || character == '_' || character == '.' || character == '-' || character == ':';
            }
            private bool TrimComment()
            {
                if (this.readIndex >= this.srcString.Length)
                {
                    return false;
                }
                if (this.srcString[this.readIndex] == '/')
                {
                    if (this.readIndex + 1 < this.srcString.Length)
                    {
                        char c = this.srcString[this.readIndex + 1];
                        if (c == '/')
                        {
                            while (this.readIndex < this.srcString.Length)
                            {
                                if (this.srcString[this.readIndex] == '\n')
                                {
                                    this.readIndex++;
                                    return true;
                                }
                                this.readIndex++;
                            }
                            return true;
                        }
                        if (c == '*')
                        {
                            while (this.readIndex < this.srcString.Length - 1)
                            {
                                if (this.srcString[this.readIndex] == '*' && this.srcString[this.readIndex + 1] == '/')
                                {
                                    this.readIndex += 2;
                                    return true;
                                }
                                this.readIndex++;
                            }
                            return true;
                        }
                    }
                    this.readIndex++;
                }
                return false;
            }
            private void TrimWhitespace()
            {
                while (this.readIndex < this.srcString.Length && Console.Lexer.IsIgnorableCharacter(this.srcString[this.readIndex]))
                {
                    this.readIndex++;
                }
            }
            private void TrimUnused()
            {
                do
                {
                    this.TrimWhitespace();
                }
                while (this.TrimComment());
            }
            private static int UnescapeNext(string srcString, int startPos, out char result)
            {
                result = '\\';
                int num = startPos + 1;
                if (num < srcString.Length)
                {
                    char c = srcString[num];
                    if (c <= '\'')
                    {
                        if (c != '"' && c != '\'')
                        {
                            return 1;
                        }
                    }
                    else if (c != '\\')
                    {
                        if (c != 'n')
                        {
                            return 1;
                        }
                        result = '\n';
                        return 2;
                    }
                    result = c;
                    return 2;
                }
                return 1;
            }
            public string NextToken()
            {
                this.TrimUnused();
                if (this.readIndex == this.srcString.Length)
                {
                    return null;
                }
                Console.Lexer.TokenType tokenType = Console.Lexer.TokenType.Identifier;
                char c = this.srcString[this.readIndex];
                char c2 = '\0';
                if (Console.Lexer.IsQuoteCharacter(c))
                {
                    tokenType = Console.Lexer.TokenType.NestedString;
                    c2 = c;
                    this.readIndex++;
                }
                else if (Console.Lexer.IsSeparatorCharacter(c))
                {
                    this.readIndex++;
                    return ";";
                }
                while (this.readIndex < this.srcString.Length)
                {
                    char c3 = this.srcString[this.readIndex];
                    if (tokenType == Console.Lexer.TokenType.Identifier)
                    {
                        if (!Console.Lexer.IsIdentifierCharacter(c3))
                        {
                            break;
                        }
                    }
                    else if (tokenType == Console.Lexer.TokenType.NestedString)
                    {
                        if (c3 == '\\')
                        {
                            this.readIndex += Console.Lexer.UnescapeNext(this.srcString, this.readIndex, out c3) - 1;
                        }
                        else if (c3 == c2)
                        {
                            this.readIndex++;
                            break;
                        }
                    }
                    this.stringBuilder.Append(c3);
                    this.readIndex++;
                }
                string result = this.stringBuilder.ToString();
                this.stringBuilder.Length = 0;
                return result;
            }
            public Queue<string> GetTokens()
            {
                Queue<string> queue = new Queue<string>();
                for (string item = this.NextToken(); item != null; item = this.NextToken())
                {
                    queue.Enqueue(item);
                }
                queue.Enqueue(";");
                return queue;
            }
        }
        public delegate void ConCommandDelegate(ConCommandArgs args);
        private class ConCommand
        {
            // Token: 0x040009E3 RID: 2531
            public ConVarFlags flags;

            // Token: 0x040009E4 RID: 2532
            public Console.ConCommandDelegate action;

            // Token: 0x040009E5 RID: 2533
            public string helpText;
        }
        private enum SystemConsoleType
        {
            // Token: 0x040009E7 RID: 2535
            None,
            // Token: 0x040009E8 RID: 2536
            Attach,
            // Token: 0x040009E9 RID: 2537
            Alloc
        }

        public class AutoComplete
        {
            public AutoComplete(Console console)
            {
                HashSet<string> hashSet = new HashSet<string>();
                for (int i = 0; i < Console.usercmdHistory.Count; i++)
                {
                    hashSet.Add(Console.usercmdHistory[i]);
                }
                foreach (KeyValuePair<string, BaseConVar> keyValuePair in console.allConVars)
                {
                    hashSet.Add(keyValuePair.Key);
                }
                foreach (KeyValuePair<string, Console.ConCommand> keyValuePair2 in console.concommandCatalog)
                {
                    hashSet.Add(keyValuePair2.Key);
                }
                foreach (string item in hashSet)
                {
                    this.searchableStrings.Add(item);
                }
                this.searchableStrings.Sort();
            }

            public bool SetSearchString(string newSearchString)
            {
                newSearchString = newSearchString.ToLower(CultureInfo.InvariantCulture);
                if (newSearchString == this.searchString)
                {
                    return false;
                }
                this.searchString = newSearchString;
                List<Console.AutoComplete.MatchInfo> list = new List<Console.AutoComplete.MatchInfo>();
                for (int i = 0; i < this.searchableStrings.Count; i++)
                {
                    string text = this.searchableStrings[i];
                    int num = Math.Min(text.Length, this.searchString.Length);
                    int num2 = 0;
                    while (num2 < num && char.ToLower(text[num2]) == this.searchString[num2])
                    {
                        num2++;
                    }
                    if (num2 > 1)
                    {
                        list.Add(new Console.AutoComplete.MatchInfo
                        {
                            str = text,
                            similarity = num2
                        });
                    }
                }
                list.Sort(delegate (Console.AutoComplete.MatchInfo a, Console.AutoComplete.MatchInfo b)
                {
                    if (a.similarity == b.similarity)
                    {
                        return string.CompareOrdinal(a.str, b.str);
                    }
                    if (a.similarity <= b.similarity)
                    {
                        return 1;
                    }
                    return -1;
                });
                this.resultsList = new List<string>();
                for (int j = 0; j < list.Count; j++)
                {
                    this.resultsList.Add(list[j].str);
                }
                return true;
            }

            private List<string> searchableStrings = new List<string>();

            private string searchString;

            public List<string> resultsList = new List<string>();

            private struct MatchInfo
            {
                public string str;

                public int similarity;
            }
        }
    }
}
