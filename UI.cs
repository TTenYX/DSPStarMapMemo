using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Reflection.Emit;
using System.Collections.Generic;
//using System.ComponentModel;
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

    internal class UI : MonoBehaviour
    {
        public static GameObject memoBase = new GameObject();
        public static GameObject memoText = new GameObject();
        public static GameObject iconPrefab = new GameObject();
        public static GameObject[] icon = new GameObject[10];
        public static GameObject iconBoxPrefab = new GameObject();
        public static GameObject[] iconBox = new GameObject[10];
        public static GameObject memoWindow;
        public static GameObject descBox;
        public static GameObject explainText;

        //public static GameObject iconPicker = new GameObject();
        public static GameObject emptyButton = new GameObject();
        //public static bool IconSelectMode;

        public static Sprite emptySprite;
        public static int selectedIconNo;
        public static MemoPool.Memo memo;

        public static void Create()

        {
            memo.id = 0;
            memo.color = Color.white;
            memo.desc = "";
            memo.signalIconId = new int[10];
            for (int i = 0; i < 10; i++)
            {
                memo.signalIconId[i] = 0;
            }

            //アイコンとメモテキスト作成 starmap-star
            GameObject nameText = GameObject.Find("UI Root/Overlay Canvas/In Game/Starmap UIs/Starmap Screen/starmap-star-ui/name-text");

            //GameObject starmapStarUI = UIRoot.instance.uiGame.starmap.focusStar.gameObject;
            //GameObject nameText = starmapStarUI.transform.Find("name-text").gameObject;

//            memoBase.transform.parent = starmapStarUI.transform;
            memoBase.transform.parent = nameText.transform.parent;
            memoBase.name = "memoBase";
            memoBase.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            memoText = Instantiate(nameText, memoBase.transform);
            memoText.name = "memoText";
            memoText.transform.localPosition = new Vector3(0, -130, 0);
            memoText.transform.localScale = new Vector3(1, 1, 1);
            memoText.GetComponent<RectTransform>().sizeDelta = new Vector3(300, 150, 0);
            memoText.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
            memoText.GetComponent<Text>().lineSpacing = 0.8f;
            memoText.SetActive(false);

            iconPrefab.AddComponent<RectTransform>().sizeDelta = new Vector3(28, 28, 0);
            iconPrefab.AddComponent<Image>();

            for (int i = 0; i < 10; i++)
            {
                icon[i] = Instantiate(iconPrefab.gameObject, memoBase.transform);
                icon[i].name = "icon" + i;

                icon[i].transform.localPosition = new Vector3(30f * i + 15f, -38, 0);
            }

            //アイコンとメモテキスト作成 starmap-planet
            //GameObject memoBase2 = Instantiate(memoBase, UIRoot.instance.uiGame.planetDetail.transform);
            GameObject memoBase2 = Instantiate(memoBase, GameObject.Find("UI Root/Overlay Canvas/In Game/Starmap UIs/Starmap Screen/starmap-planet-ui").transform);
            memoBase2.name = "memoBase";
            for (int i = 0; i < 10; i++)
            {
                GameObject icon = memoBase2.transform.Find("icon" + i).gameObject;
                icon.transform.localPosition = new Vector3(30f * i + 15f, -38, 0);
            }

            //メモ編集ウインドウ作成
            GameObject planetdetailwindow = UIRoot.instance.uiGame.planetDetail.gameObject;
            memoWindow = Instantiate(planetdetailwindow, planetdetailwindow.transform.parent);
            //0.10.x UI 适配（移植自 0.0.7）：black-bg 尺寸/位置 + 移除 detail-tabs
            var blackBg = memoWindow.transform.Find("black-bg");
            if (blackBg != null)
            {
                blackBg.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(-30f, 40f);
                blackBg.localPosition = new Vector3(-105f, -105f, 0f);
                var detailTabs = blackBg.Find("detail-tabs");
                if (detailTabs != null) Destroy(detailTabs.gameObject);
            }
            //不要なオブジェクト等の削除
            var keepTransformList = new List<string> { "black-bg" , "type-text" };
            var keepComponentList = new List<Type>() { typeof(RectTransform) };
            DestroyChildren(memoWindow.transform, keepTransformList, keepComponentList);

            memoWindow.name = "memoWindow";
            memoWindow.transform.localPosition = new Vector3(0, -300, 0);
            GameObject typeText = memoWindow.transform.Find("type-text").gameObject;
            typeText.transform.localPosition = new Vector3(-240, -3, 0);
            typeText.name = "typeText";
            typeText.GetComponent<Text>().text = "Memo".Translate();
            typeText.GetComponent<Text>().resizeTextMaxSize = 20;

            explainText = Instantiate(typeText.gameObject, memoWindow.transform);
            explainText.name = "explainText";
            explainText.transform.localPosition = new Vector3(-240, -190, 0);
            explainText.GetComponent<Text>().text = "Press [Enter] to insert a line break.".Translate();
            explainText.GetComponent<Text>().resizeTextMaxSize = 20;
            explainText.SetActive(false);

            //テキスト編集ボックスの作成
            descBox = Instantiate(UIRoot.instance.uiGame.blueprintBrowser.inspector.descTextInput.gameObject, memoWindow.transform);
            //不要なオブジェクト等の削除
            keepTransformList = new List<string> { "value-text" };
            keepComponentList = new List<Type>() { typeof(RectTransform), typeof(InputField), typeof(Image) };
            DestroyChildren(descBox.transform, keepTransformList, keepComponentList);

            descBox.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            descBox.name = "descBox";

            //GameObject nameInput = memoWindow.transform.Find("name-input").gameObject;
            descBox.transform.localPosition = new Vector3(-250, -80, 0);
            descBox.transform.localScale = new Vector3(0.87f, 0.87f, 0.87f);
            descBox.GetComponent<RectTransform>().sizeDelta = new Vector3(300, 120, 0);
            //Destroy(descBox.GetComponent<UIButton>());

            Text valueText = descBox.transform.Find("value-text").GetComponent<Text>();
            valueText.alignment = TextAnchor.UpperLeft;
            valueText.lineSpacing = 0.8f;
            valueText.fontSize = 20;

            memoWindow.SetActive(false);

            //アイコン編集ボックス作成
            iconBoxPrefab = Instantiate(UIRoot.instance.uiGame.blueprintBrowser.inspector.thumbIconImage1.gameObject, memoWindow.transform);
            iconBoxPrefab.name = "iconBoxPrefab";
            iconBoxPrefab.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            iconBoxPrefab.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            iconBoxPrefab.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            iconBoxPrefab.transform.localPosition = new Vector3(45, -120, 0);
            iconBoxPrefab.SetActive(false);

            emptySprite = iconBoxPrefab.GetComponent<Image>().sprite;

            for (int i = 0; i < 10; i++)
            {
                iconBox[i] = Instantiate(iconPrefab.gameObject, memoWindow.transform);
                iconBox[i].name = "iconBox" + i;
                iconBox[i].transform.localPosition = new Vector3(26f * i - 235f, -55, 0);
                iconBox[i].GetComponent<RectTransform>().sizeDelta = new Vector3(24, 24, 0);
                iconBox[i].GetComponent<Image>().sprite = emptySprite;
                iconBox[i].AddComponent<Button>();
                iconBox[i].SetActive(true);
                var iconNo = i;
                iconBox[i].GetComponent<Button>().onClick.AddListener(() => OnClickIconBox(iconNo));
            }
            descBox.GetComponent<InputField>().onEndEdit.AddListener(new UnityAction<string>(onEndEditDescBox));


            //iconPicker = Instantiate(UIRoot.instance.uiGame.signalPicker.gameObject, UIRoot.instance.uiGame.signalPicker.transform.parent);

            //next&previousボタンの追加
            emptyButton = Instantiate(UIRoot.instance.uiGame.assemblerWindow.copyButton.gameObject, UIRoot.instance.uiGame.signalPicker.transform);
            emptyButton.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
            //emptyButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            emptyButton.transform.localPosition = new Vector3(575, -60, 0);

            emptyButton.transform.Find("Text").GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            emptyButton.transform.Find("Text").GetComponent<Text>().lineSpacing = 0.8f;
            emptyButton.transform.Find("Text").GetComponent<Text>().fontSize = 12;
            emptyButton.transform.Find("Text").GetComponent<Text>().text = "No\nIcon".Translate();
            Destroy(emptyButton.transform.Find("Text").GetComponent<Localizer>());
            Destroy(emptyButton.GetComponent<UIButton>());
            emptyButton.name = "emptyButton";
            emptyButton.SetActive(false);
            emptyButton.GetComponent<Button>().onClick.AddListener(onEmptyButton);


        }

        //説明文が更新されたら
        public static void onEndEditDescBox(string str)
        {
            memo.desc = str;
            MemoPool.AddOrUpdate();

        }

        //アイコンをクリックしたらピッカーを開く
        public static void OnClickIconBox(int iconNo)
        {
            if (UISignalPicker.isOpened)
            {
                return;
            }
            emptyButton.SetActive(true);//IconSelectMode = true;
            selectedIconNo = iconNo;
            UISignalPicker.Popup(new Vector2(50f, 350f), new Action<int>(onIconBoxChanged));
        }

        //アイコンが指定されたら保存
        public static void onIconBoxChanged(int signalId)
        {
            iconBox[selectedIconNo].GetComponent<Image>().sprite = LDB.signals.IconSprite(signalId);
            memo.signalIconId[selectedIconNo] = signalId;
            MemoPool.AddOrUpdate();
            emptyButton.SetActive(false);// IconSelectMode = false;
        }

        //EmptyButtonを押したら
        public static void onEmptyButton()
        {
            UISignalPicker.Close();
            iconBox[selectedIconNo].GetComponent<Image>().sprite = emptySprite;
            memo.signalIconId[selectedIconNo] = 0;
            MemoPool.AddOrUpdate();
        }


        public static void MemoWindowOpen(int key)
        {
            if (MemoPool.memoPool.ContainsKey(key))
            {
                UI.memo.id = MemoPool.memoPool[key].id;
                UI.memo.desc = MemoPool.memoPool[key].desc;
                UI.memo.color = MemoPool.memoPool[key].color;

                InputField valueText = descBox.GetComponent<InputField>();
                valueText.text = UI.memo.desc;
                for (int i = 0; i < 10; i++)
                {
                    UI.memo.signalIconId[i] = MemoPool.memoPool[key].signalIconId[i];
                    if (UI.memo.signalIconId[i] != 0)
                    {
                        iconBox[i].GetComponent<Image>().sprite = LDB.signals.IconSprite(UI.memo.signalIconId[i]);
                    } else
                    {
                        iconBox[i].GetComponent<Image>().sprite = emptySprite;
                    }
                }
            }
            else
            {
                UI.memo.id = 0;
                UI.memo.desc = "";
                UI.memo.color = Color.white;

                InputField valueText = descBox.GetComponent<InputField>();
                valueText.text = "";
                for (int i = 0; i < 10; i++)
                {
                    iconBox[i].GetComponent<Image>().sprite = emptySprite;
                    UI.memo.signalIconId[i] = 0;
                }
            }
            memoWindow.SetActive(true);
        }

        public static void DestroyChildren(Transform transform, List<string> keepTransformList, List<Type> keepComponentList)
        {
            foreach (Transform child in transform)
            {
                LogManager.Logger.LogInfo("Transform: " + child.name);
                if (keepTransformList.Contains(child.name))
                {
                    continue;
                }
                GameObject.Destroy(child.gameObject);
                LogManager.Logger.LogInfo("destroy: " + child.name);
            }
            foreach (Component child in transform.GetComponents<Component>())
            {
                LogManager.Logger.LogInfo("Component: " + child);
                if (keepComponentList.Contains(child.GetType()))
                {
                    continue;
                }
                GameObject.Destroy(child);
                LogManager.Logger.LogInfo("destory: " + child.GetType());
            }
        }

    }

}
