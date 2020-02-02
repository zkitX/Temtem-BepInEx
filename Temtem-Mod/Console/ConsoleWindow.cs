using System;
using System.Text;
using BepInEx;
using UnityEngine;

namespace Temtem_Mod.Console
{
    public class ConsoleWindow : MonoBehaviour
    {
        public static ConsoleWindow instance { get; private set; }
        private Console.AutoComplete autoComplete;
        private bool preventAutoCompleteUpdate;
        private bool preventHistoryReset;
        private int historyIndex = -1;
        private readonly StringBuilder stringBuilder = new StringBuilder();
        private const string consoleEnabledDefaultVaule = "0";
        private static BoolConVar cvConsoleEnabled = new BoolConVar("console_enabled", ConVarFlags.None, "0", "Enables/Disables the console.");
        private enum InputFiledState
        {
            Neutral,
            History,
            AutoComplete
        }

    }
}
