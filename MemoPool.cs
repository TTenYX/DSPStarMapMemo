using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using static UnityEngine.GUILayout;
using UnityEngine.Rendering;
using xiaoye97;

namespace DSPStarMapMemo
{
    public class MemoPool : MonoBehaviour
    {
        public static Dictionary<int, Memo> memoPool = new Dictionary<int, Memo>();

        public struct Memo
        {
            public int id;
            public int[] signalIconId; 
            public Color color;
            public string desc;
        }

        public static void AddOrUpdate()
        {
            int key = 0;
            if (UIRoot.instance.uiGame.starDetail.active)
            {
                key = UIRoot.instance.uiGame.starDetail.star.id;
            }
            else if (UIRoot.instance.uiGame.planetDetail.active)
            {
                key = UIRoot.instance.uiGame.planetDetail.planet.id;
            }
            UI.memo.id = key;
            Memo tempMemo = new Memo();
            tempMemo.id = UI.memo.id;
            tempMemo.color = UI.memo.color;
            tempMemo.desc = UI.memo.desc;
            tempMemo.signalIconId = new int[10];
            for (int i = 0; i < 10; i++)
            {
                tempMemo.signalIconId[i] = UI.memo.signalIconId[i];
            }

            if (memoPool.ContainsKey(key))
            {
                memoPool[key] = tempMemo;
            }
            else
            {
                memoPool.Add(key, tempMemo);
            }
        }
    }
}
