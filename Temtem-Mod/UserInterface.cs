using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace TemTemMod
{
    public class UserInterface : MonoBehaviour
    {
        public static GUIStyle button = null;

        private GUIStyle window = null;
        private GUIStyle h1 = null;
        private GUIStyle h2 = null;
        private GUIStyle richtext = null;
        private GUIStyle curVersion = null;

        private GUIStyle nodisable = null;
        private GUIStyle red = null;

        private bool hasLaunched;
        private bool hasInit;

        private static bool toggleTP;
        private static bool toggleEncounter;
        private static bool toggleShinyHunter;
        private static bool toggleMonsterInfo;

        private Rect windowRect = new Rect(0, 0, 0, 0);
        private Vector2 windowSize = Vector2.zero;
        private Resolution lastResolution;

        private float pendingScale = 1f;
        private bool isOpen;
        private int tabId = 0;
        private Vector2[] scrollPositions = null;
        private List<Column> columns = new List<Column>();
        private bool GameCursorLocked { get; set; }

        internal UISettings uiSettings = new UISettings();
        internal static UserInterface instance;

        void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
            windowSize = new Vector2(960, 720);
            scrollPositions = new Vector2[3];
            LoadSettings();
        }

        internal void SaveSettings()
        {
            // TODO
        }

        private void LoadSettings()
        {
            // TODO
        }

        void Start()
        {
            CalculateWindowPos();
            if (uiSettings.showOnStart)
            {
                ToggleWindow(true);
            }
        }

        void Update()
        {
            if (isOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            bool toggle = false;
            if (Input.GetKeyUp(KeyCode.F10))
            {
                toggle = true;
            }
            if (toggle)
            {
                ToggleWindow();
            }
            else if (isOpen && Input.GetKey(KeyCode.Escape))
            {
                ToggleWindow();
            }
        }
        private void PrepareGUI()
        {
            window = new GUIStyle();

            h1 = new GUIStyle();
            h1.normal.textColor = Color.white;
            h1.fontStyle = FontStyle.Bold;
            h1.alignment = TextAnchor.MiddleCenter;

            h2 = new GUIStyle();
            h2.normal.textColor = new Color(0.6f, 0.91f, 1f);
            h2.padding = new RectOffset(0, 0, 7, 0);
            h2.fontStyle = FontStyle.Bold;

            curVersion = new GUIStyle();
            curVersion.normal.textColor = Color.white;
            curVersion.padding = new RectOffset(0, 0, 10, 0);
            curVersion.fontStyle = FontStyle.Bold;

            button = new GUIStyle(GUI.skin.button);

            richtext = new GUIStyle();
            richtext.richText = true;
            richtext.normal.textColor = Color.white;

            red = new GUIStyle();
            red.normal.textColor = Color.red;

            nodisable = new GUIStyle();
            nodisable.padding = new RectOffset(9, 0, 10, 0);
            nodisable.normal.textColor = Color.white;

            red = new GUIStyle();
            red.padding = RectOffset(6);
            red.normal.textColor = Color.red;

            columns.Add(new Column { name = "", width = 50 }); // Group by author
            columns.Add(new Column { name = "Author", width = 130 });
            columns.Add(new Column { name = "Name", width = 200, expand = true });
            columns.Add(new Column { name = "Version", width = 100 });
            columns.Add(new Column { name = "New Version", width = 100 });
            columns.Add(new Column { name = "On/Off", width = 50 });
            columns.Add(new Column { name = "Status", width = 50 });
        }

        private void ScaleGUI()
        {
            GUI.skin.button.padding = new RectOffset(10, 10, 3, 3);
            GUI.skin.button.margin = RectOffset(4, 2);

            GUI.skin.horizontalSlider.fixedHeight = 12;
            GUI.skin.horizontalSlider.border = RectOffset(3, 0);
            GUI.skin.horizontalSlider.padding = RectOffset(0, 0);
            GUI.skin.horizontalSlider.margin = RectOffset(4, 8);

            GUI.skin.horizontalSliderThumb.fixedHeight = 12;
            GUI.skin.horizontalSliderThumb.border = RectOffset(4, 0);
            GUI.skin.horizontalSliderThumb.padding = RectOffset(7, 0);
            GUI.skin.horizontalSliderThumb.margin = RectOffset(0);

            GUI.skin.toggle.margin.left = 10;

            window.padding = RectOffset(5);
            h1.fontSize = 16;
            h1.margin = RectOffset(0, 5);
            h2.fontSize = 13;
            h2.margin = RectOffset(0, 3);
            button.fontSize = 13;
            button.padding = RectOffset(30, 5);
        }

        public bool isToggleTP
        {
            get
            {
                return toggleTP;
            }
        }

        public bool isToggleEncounter
        {
            get
            {
                return toggleEncounter;
            }
        }

        public bool isToggleShinyHunter
        {
            get
            {
                return toggleShinyHunter;
            }
        }

        public bool istoggleMonsterInfo
        {
            get
            {
                return toggleMonsterInfo;
            }
        }

        private void OnGUI()
        {
            if (!hasInit)
            {
                hasInit = true;
                PrepareGUI();
            }

            if (isOpen)
            {
                if (lastResolution.width != Screen.currentResolution.width || lastResolution.height != Screen.currentResolution.height)
                {
                    lastResolution = Screen.currentResolution;
                    CalculateWindowPos();
                }
                ScaleGUI();
                var backgroundColor = GUI.backgroundColor;
                var color = GUI.color;
                GUI.backgroundColor = Color.black;
                GUI.color = Color.black;
                windowRect = GUILayout.Window(0, windowRect, WindowFunction, "", window, GUILayout.Height(windowSize.y));
                GUI.backgroundColor = backgroundColor;
                GUI.color = color;
            }
        }

        string[] tabs = { "Mods", "Settings", "Log" };
        private void WindowFunction(int windowId)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                GUI.DragWindow(windowRect);
            }

            UnityAction buttons = () => { };

            GUILayout.BeginVertical("box");
            GUILayout.Label("zkitX Temtem Mods 1.0.0", h1);
            GUILayout.BeginVertical("box");

            GUILayout.Space(3);
            int tab = tabId;
            
            tab = GUILayout.Toolbar(tab, tabs, button, GUILayout.ExpandWidth(false));
            if (tab != tabId)
            {
                tabId = tab;
            }

            GUILayout.Space(5);

            if(tabId == 0)
            {
                DrawModTab(ref buttons);
            }
            if (tabId == 1)
            {
                //TODO DrawSettingsTab(ref buttons);
            }
            if (tabId == 2)
            {
                DrawLogTab(ref buttons);
            }

            GUILayout.FlexibleSpace();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Close", "Close the overlay."), button, GUILayout.ExpandWidth(false)))
            {
                ToggleWindow();
            }
            buttons();

            GUILayout.Label(GUI.tooltip);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        private void DrawModTab(ref UnityAction buttons)
        {
            if (GUILayout.Button(new GUIContent("Heal Team", "Heals your Temtem team without going to a recovery machine"), button, GUILayout.ExpandWidth(false)))
            {
                Hacks.HealTemtem();
            }
            if (GUILayout.Button(new GUIContent("Open Bank", "Opens your Temtem bank"), button, GUILayout.ExpandWidth(false)))
            {
                Hacks.OpenBank();
            }
            if (GUILayout.Button(new GUIContent("Open Shop", "Opens the shop"), button, GUILayout.ExpandWidth(false)))
            {
                Hacks.OpenShop();
            }
            toggleTP = GUILayout.Toggle(toggleTP, "Enable TP Hack");
            toggleEncounter = GUILayout.Toggle(toggleEncounter, "Enable No Encounter Hack");
            toggleShinyHunter = GUILayout.Toggle(toggleShinyHunter, "Enable Shiny Hunter");
            toggleMonsterInfo = GUILayout.Toggle(toggleMonsterInfo, "Enable Monster Info");
        }

        private void DrawLogTab(ref UnityAction buttons)
        {
            var minWidth = GUILayout.MinWidth(windowSize.x);
            scrollPositions[2] = GUILayout.BeginScrollView(scrollPositions[2], minWidth, GUILayout.ExpandHeight(false));
            var amountWidth = columns.Where(x => !x.skip).Sum(x => x.width);
            var expandWidth = columns.Where(x => x.expand && !x.skip).Sum(x => x.width);

            var colWidth = columns.Select(x =>
                x.expand
                    ? GUILayout.Width(x.width / expandWidth * (windowSize.x - 60 + expandWidth - amountWidth))
                    : GUILayout.Width(x.width)).ToArray();

            GUILayout.BeginVertical("box");
            foreach (string s in TemTemMod.log)
            {
                GUILayout.Label(s, richtext);
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        public void ToggleWindow()
        {
            ToggleWindow(!isOpen);
        }

        public void ToggleWindow(bool newIsOpen)
        {
            if (newIsOpen == isOpen)
            {
                return;
            }
            if (newIsOpen)
            {
                hasLaunched = true;
            }
            try
            {
                isOpen = newIsOpen;
                if (newIsOpen)
                {
                    GameCursorLocked = Cursor.lockState == CursorLockMode.Locked || !Cursor.visible;
                    if (GameCursorLocked)
                    {
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    }
                }
                else
                {
                    if (GameCursorLocked)
                    {
                        Cursor.visible = false;
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
        private static RectOffset RectOffset(int value)
        {
            return new RectOffset(value, value, value, value);
        }

        private static RectOffset RectOffset(int x, int y)
        {
            return new RectOffset(x, x, y, y);
        }

        private void CalculateWindowPos()
        {
            windowSize = new Vector2(960, 720);
            windowRect = new Rect((Screen.width - windowSize.x) / 2f, (Screen.height - windowSize.y) / 2f + 100f, 0, 0);
        }

        class Column
        {
            public string name;
            public float width;
            public bool expand = false;
            public bool skip = false;
        }

        internal class UISettings
        {
            public bool showOnStart;
            public float scale;
        }
    }
}
