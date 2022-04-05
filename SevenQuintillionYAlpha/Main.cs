using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SevenQuintillionYAlpha
{
    class Main : MonoBehaviour
    {
        public void Start()
        {
            UtilizeTheJuice.Instance.GetLocalGuy();
            UtilizeTheJuice.Instance.GetNonLocalGuys();
        }

        public void Update()
        {
            UtilizeTheJuice.Instance.UpdateTheJuicer();
        }

        public void OnGUI()
        {
            JuicyMenu.Instance.CreateWindow();
        }
    }
}
