using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SevenQuintillionYAlpha
{
    public class Juicer
    {
        public static void StartTheJuicer()
        {
            GetTheJuice();
        }

        private static void GetTheJuice()
        {
            _Juice = new GameObject();
            _Juice.AddComponent<Main>();
            GameObject.DontDestroyOnLoad(_Juice);
        }

        public static void ExtractTheJuicer()
        {
            UnJuice();
        }

        private static void UnJuice()
        {
            GameObject.Destroy(_Juice);
        }

        static private GameObject _Juice;
    }
}
