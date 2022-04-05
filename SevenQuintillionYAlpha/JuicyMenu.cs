using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

enum ApplyElements
{
    ApplySwitch,
    ApplyKills,
    ApplyItem,
    ApplyEXP,
    ApplyGetCoordinates,
    ApplyTeleport,
    ApplyDeNerf,
    ApplySP,
    Apply0D,
    ApplyUnload,
    MAX_APPLY
}

namespace SevenQuintillionYAlpha
{
    class JuicyMenu
    {
        private static JuicyMenu singletonInstance;
        public static JuicyMenu Instance
        {
            get
            {
                singletonInstance = singletonInstance ?? new JuicyMenu();
                return singletonInstance;
            }
        }

        private Rect RectWindow = new Rect(20, 20, 200, 560);

        private bool bEnabled = false;

        private int iRectY;
        public bool bLocalOnly { get; private set; } = true;

        public int iSelected { get; private set; } = 0;
        public string szAddKills { get; private set; } = "";
        public string szIName { get; private set; } = "";
        public string szAmmount { get; private set; } = "1";
        public string szDreamEXP { get; private set; } = "10";
        public string szDreamSP { get; private set; } = "1";
        public bool bAimer { get; private set; } = true;
        public bool bAimPlayers { get; private set; } = false;
        public float flAimOffset { get; set; } = 0.0751952f;

        public bool[] bApply = new bool[(int)ApplyElements.MAX_APPLY];

        public string[] szTargetCoordinates = { "", "", "" };

        private Rect RectTelWindow = new Rect(40, 40, 200, 330);
        private bool bTelEnabled = false;
        private string szLocationName = "";
        private int iSelectButton = 0;
        private Dictionary<int, string> arrLocationNames = new Dictionary<int, string>();
        private Dictionary<int, Vector3?> arrCoordinates = new Dictionary<int, Vector3?>();
        private bool bFirstTime = true;
        string szFileName = "savedlocations.txt";
        
        public void ToggleMenuVisibilty()
        {
            bEnabled = !bEnabled;
        }
        public void CreateWindow()
        {
            if (bEnabled)
            {
                RectWindow = GUI.Window(0, RectWindow, DoMyWindow, "Juicy Menu");
            }
            if (bTelEnabled)
            {
                RectTelWindow = GUI.Window(1, RectTelWindow, DoMyTelWindow, "Tel Menu");
            }
        }
        private void GUINext()
        {
            iRectY += 30;
        }
        private void DoMyWindow(int WindowId)
        {
            GUI.BeginGroup(new Rect(0, 0, 200, 560));

            iRectY = 20;

            if(GUI.Button(new Rect(10, iRectY, 150, 30), bLocalOnly ? "Switch to Others" : "Switch To Me"))
            {
                bLocalOnly = !bLocalOnly;
                bApply[(int)ApplyElements.ApplySwitch] = true;
            }
            GUINext();

            if(!bLocalOnly)
            {
                iSelected = GUI.SelectionGrid(new Rect(10, iRectY, 150, 30), iSelected, UtilizeTheJuice.Instance.GetNonLocalGuysNames(), 2);
                GUINext();
            }

            szAddKills = GUI.TextField(new Rect(10, iRectY, 150, 30), szAddKills);
            if (GUI.Button(new Rect(160, iRectY, 30, 30), "K-Add"))
                bApply[(int)ApplyElements.ApplyKills] = true;
            GUINext();

            bAimer = GUI.Toggle(new Rect(10, iRectY, 70, 30), bAimer, "forsenCD");
            bAimPlayers = GUI.Toggle(new Rect(80, iRectY, 80, 30), bAimPlayers, "Players too");
            GUINext();


            flAimOffset = GUI.HorizontalSlider(new Rect(10, iRectY, 130, 30), flAimOffset, -1.00f, 1.00f);
            if (float.TryParse(GUI.TextField(new Rect(140, iRectY - 10, 50, 30), flAimOffset.ToString()), out float tempAimOffset))
            {
                if (flAimOffset >= -1.00f && flAimOffset <= 1.00f)
                    flAimOffset = tempAimOffset;
            }
            GUINext();

            GUI.Label(new Rect(10, iRectY, 30, 30), "ID");
            szIName = GUI.TextField(new Rect(40, iRectY, 150, 30), szIName);
            GUINext();

            GUI.Label(new Rect(10, iRectY, 30, 30), "#%");
            szAmmount = GUI.TextField(new Rect(40, iRectY, 150, 30), szAmmount);
            GUINext();

            if (GUI.Button(new Rect(40, iRectY, 150, 30), "Apply (F5)"))
                bApply[(int)ApplyElements.ApplyItem] = true;
            GUINext();

            GUI.Label(new Rect(10, iRectY, 30, 30), "EXP");
            szDreamEXP = GUI.TextField(new Rect(40, iRectY, 150, 30), szDreamEXP);
            GUINext();

            if (GUI.Button(new Rect(40, iRectY, 150, 30), "Apply (F6)"))
                bApply[(int)ApplyElements.ApplyEXP] = true;
            GUINext();

            GUI.Label(new Rect(10, iRectY, 150, 30), "Coordinates x y z:");
            GUINext();

            szTargetCoordinates[0] = GUI.TextField(new Rect(10, iRectY, 40, 30), szTargetCoordinates[0]);
            szTargetCoordinates[1] = GUI.TextField(new Rect(60, iRectY, 40, 30), szTargetCoordinates[1]);
            szTargetCoordinates[2] = GUI.TextField(new Rect(110, iRectY, 40, 30), szTargetCoordinates[2]);
            GUINext();

            if (GUI.Button(new Rect(10, iRectY, 150, 30), "Get Coordinates"))
                bApply[(int)ApplyElements.ApplyGetCoordinates] = true;
            GUINext();

            if (GUI.Button(new Rect(10, iRectY, 150, 30), "Teleport (F7)"))
                bApply[(int)ApplyElements.ApplyTeleport] = true;
            GUINext();

            if (GUI.Button(new Rect(10, iRectY, 150, 30), bTelEnabled ? "UnPop TelMenu" : "Pop TelMenu"))
                bTelEnabled = !bTelEnabled;
            GUINext();

            if (GUI.Button(new Rect(10, iRectY, 150, 30), "DeNerfMe (F8)"))
                bApply[(int)ApplyElements.ApplyDeNerf] = true;
            GUINext();

            GUI.Label(new Rect(10, iRectY, 30, 30), "SP");
            szDreamSP = GUI.TextField(new Rect(40, iRectY, 80, 30), szDreamSP);
            if (GUI.Button(new Rect(120, iRectY, 30, 30), "0D"))
                bApply[(int)ApplyElements.Apply0D] = true;
            GUINext();

            if (GUI.Button(new Rect(40, iRectY, 150, 30), "Apply"))
                bApply[(int)ApplyElements.ApplySP] = true;
            GUINext();

            if (GUI.Button(new Rect(10, iRectY, 150, 30), "Extract The Juice"))
                bApply[(int)ApplyElements.ApplyUnload] = true;
            GUINext();

            GUI.EndGroup();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
        private static void GetFields(string szLine, out string[] szFields)
        {
            szFields = new string[5];
            int iC = 0, iCheckpoint = 0;
            for (int i = 0; i < szLine.Length; i++)
            {
                if (szLine[i] == '"')
                {
                    if (iC % 2 == 1)
                        szFields[iC / 2] = szLine.Substring(iCheckpoint + 1, i - iCheckpoint - 1);
                    else
                        iCheckpoint = i;

                    iC++;
                }
            }
        }
        private void ReadLocations()
        {
            string szPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), szFileName);

            if (!File.Exists(szPath))
                return;

            var F = new StreamReader(szPath);
            string szLine;
            while ((szLine = F.ReadLine()) != null)
            {
                GetFields(szLine, out string[] fields);
                
                if (!int.TryParse(fields[0], out int iCurrent))
                    continue;

                arrLocationNames.Add(iCurrent, fields[1]);
                if (float.TryParse(fields[2], out float x) && float.TryParse(fields[3], out float y) && float.TryParse(fields[4], out float z))
                    arrCoordinates.Add(iCurrent, new Vector3(x, y, z));
                else
                    arrCoordinates.Add(iCurrent, null);
            }
        }

        private void SaveLocations()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (int i in arrLocationNames.Keys)
            {
                if (arrLocationNames[i] == "")
                    continue;

                if (arrCoordinates[i] == null)
                    stringBuilder.Append($"\"{i}\",\"{arrLocationNames[i]}\",\"\",\"\",\"\"\n");
                else
                    stringBuilder.Append($"\"{i}\",\"{arrLocationNames[i]}\",\"{arrCoordinates[i].Value.x}\",\"{arrCoordinates[i].Value.y}\",\"{arrCoordinates[i].Value.z}\"\n");
            }

            string szPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), szFileName);
            File.WriteAllText(szPath, stringBuilder.ToString());
            stringBuilder.Clear();
        }

        private void DoMyTelWindow(int iWindowId)
        {
            GUI.BeginGroup(new Rect(0, 0, 200, 330));

            if (bFirstTime)
            {
                ReadLocations();
                bFirstTime = false;
            }

            
            szFileName = GUI.TextField(new Rect(10, 20, 150, 30), szFileName);
            
            if (GUI.Button(new Rect(160, 20, 30, 30), "Load File"))
                ReadLocations();

            szLocationName = GUI.TextField(new Rect(10, 50, 150, 30), szLocationName);

            if (GUI.Button(new Rect(10, 80, 150, 30), "Save"))
            {
                arrLocationNames[iSelectButton] = szLocationName;
                if (szLocationName != ""
                && float.TryParse(szTargetCoordinates[0], out float x)
                && float.TryParse(szTargetCoordinates[1], out float y)
                && float.TryParse(szTargetCoordinates[2], out float z))
                {
                    arrLocationNames[iSelectButton] = szLocationName;
                    arrCoordinates[iSelectButton] = new Vector3(x, y, z);
                }
                SaveLocations();
            }
            if (GUI.Button(new Rect(10, 110, 150, 30), "Load"))
            {
                if (arrCoordinates[iSelectButton] != null)
                {
                    szTargetCoordinates[0] = arrCoordinates[iSelectButton].Value.x.ToString();
                    szTargetCoordinates[1] = arrCoordinates[iSelectButton].Value.y.ToString();
                    szTargetCoordinates[2] = arrCoordinates[iSelectButton].Value.z.ToString();
                }
            }

            int iTRectY = 140;
            int iC = 0;
            var oldBG = GUI.backgroundColor;
            while (iTRectY < 300)
            {
                if (!arrLocationNames.ContainsKey(iC))
                {
                    arrLocationNames.Add(iC, "Location #" + iC);
                    arrCoordinates.Add(iC, null);
                }

                if(iC == iSelectButton)
                    GUI.backgroundColor = Color.black;

                if (GUI.Button(new Rect(10, iTRectY, 150, 30), arrLocationNames[iC]))
                    iSelectButton = iC;

                GUI.backgroundColor = oldBG;
                iTRectY += 30;
                iC++;
            }

            GUI.EndGroup();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}
