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
using crecheng.DSPModSave;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace DSPStarMapMemo
{

    [BepInPlugin("Appun.DSP.plugin.StarMapMemo", "DSPStarMapMemo", "0.0.4")]
    [BepInProcess("DSPGAME.exe")]
    [BepInDependency(DSPModSavePlugin.MODGUID)]



    public class Main : BaseUnityPlugin , IModCanSave
    {
        public static ConfigEntry<int> MemoFontSize;

        public void Start()
        {
            LogManager.Logger = Logger;
            MemoFontSize = Config.Bind("General", "MemoFontSize", 0,
                "星图备忘录文字字号。0 = 使用原版字号（建议 20~32） / Font size of the memo text. 0 = vanilla size.");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            UI.Create();
        }


        public void Import(BinaryReader r)
        {
            MemoPool.memoPool.Clear();

            if (r.ReadInt32() == 1)  //セーブデータバージョンチェック
            {
                int num3 = r.ReadInt32();
                for (int j = 0; j < num3; j++)
                {
                    var Key = r.ReadInt32();
                    MemoPool.Memo newMemo = new MemoPool.Memo();
                    newMemo.signalIconId = new int[10];
                    newMemo.id = r.ReadInt32();
                    newMemo.desc = r.ReadString();
                    newMemo.color.r = r.ReadSingle();
                    newMemo.color.g = r.ReadSingle();
                    newMemo.color.b = r.ReadSingle();
                    newMemo.color.a = r.ReadSingle();
                    for (int i = 0; i < 10; i++)
                    {
                        newMemo.signalIconId[i] = r.ReadInt32();
                    }
                    MemoPool.memoPool.Add(Key, newMemo);
                }
            }
            else
            {
                LogManager.Logger.LogInfo("Save data version error");
            }

        }

        public void Export(BinaryWriter w)
        {
            //LogManager.Logger.LogInfo("---------------------------------------------------------Export");
            w.Write(1); //セーブデータバージョン
            w.Write(MemoPool.memoPool.Count);
            for (int j = 0; j < MemoPool.memoPool.Count; j++)
            {
                foreach (KeyValuePair<int, MemoPool.Memo> keyValuePair in MemoPool.memoPool)
                {
                    w.Write(keyValuePair.Key);
                    w.Write(keyValuePair.Value.id);
                    w.Write(keyValuePair.Value.desc);
                    w.Write(keyValuePair.Value.color.r);
                    w.Write(keyValuePair.Value.color.g);
                    w.Write(keyValuePair.Value.color.b);
                    w.Write(keyValuePair.Value.color.a);
                    for (int i = 0; i < 10; i++)
                    {
                        w.Write(keyValuePair.Value.signalIconId[i]);
                    }
                }
            }

        }

        public void IntoOtherSave()
        {
            if (MemoPool.memoPool.Count > 0)
            {
                MemoPool.memoPool.Clear();
            }
        }
    }


    public class LogManager
    {
        public static ManualLogSource Logger;
    }

}