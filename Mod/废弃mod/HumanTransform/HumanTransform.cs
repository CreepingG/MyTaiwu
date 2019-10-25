using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.IO;

namespace HumanTransform
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static readonly string Separator = "|";
        public static string resBasePath;
        public static string folderName = "Texture";

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            
            resBasePath = modEntry.Path;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {

            GUILayout.BeginHorizontal();

            GUILayout.EndHorizontal();

        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        
    }
    public class Face
    {
        public static class ImageName
        {
            public static string Lion = "Lion";
        }
        public static Sprite Lion_Male;
        public static Sprite Lion_Female_Front;
        public static Sprite Lion_Female_Back;
        public static bool SpriteAdded = false;

        //加载图片
        [HarmonyPatch(typeof(Loading), "LoadScene")]
        [HarmonyPriority(Priority.Last)]
        public static class HumanTransform_LoadScene_Patch
        {
            static readonly string Separator = "_";
            static readonly string Suffix = ".png";
            static void Postfix()
            {
                LoadSprite(Face.ImageName.Lion + Separator + "1" + Suffix, ref Face.Lion_Male);
                LoadSprite(Face.ImageName.Lion + "_0_0" + Suffix, ref Face.Lion_Female_Back);
                LoadSprite(Face.ImageName.Lion + "_0_1" + Suffix, ref Face.Lion_Female_Front);
                SpriteAdded = true;
            }
            static bool LoadSprite(string imageName, ref Sprite sprite)
            {
                if (sprite != null)
                {
                    return true;
                }
                string path = Path.Combine(Path.Combine(Main.resBasePath, Main.folderName), imageName);
                if (!File.Exists(path))
                {
                    Main.Logger.Log($"图片路径[{path}]不存在");
                    return false;
                }
                var fileData = File.ReadAllBytes(path);
                var texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100);
                Main.Logger.Log($"成功加载图片[{path}]\n大小为{texture.width}*{texture.height}");
                return true;
            }
        }
        //显示头饰
        [HarmonyPatch(typeof(ActorFace), "UpdateFace")]
        [HarmonyPatch(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
        public static class GuiltyNature_UpdateFace_Patch
        {
            static void Postfix(ActorFace __instance, int age, int gender, int actorGenderChange, int actorId, int[] faceDate, bool life)
            {
                if (!Main.enabled) return;
                Main.Logger.Log(DateFile.instance.GetActorName(actorId));
                bool showMale = gender == 1;
                if (actorGenderChange == 1) showMale = !showMale;
                bool alive = DateFile.instance.GetActorDate(actorId, 26, false) == "0" || life;
                bool invalid = !alive || age <= 14 || faceDate.Length <= 1 || actorId <= 0;
                if (!ImageDict.ContainsKey(__instance))
                {
                    AddImage(__instance);
                }
                bool flag = !invalid && SpriteAdded;
                Image ear = ImageDict[__instance].ear;
                Image back = ImageDict[__instance].back;
                ear.gameObject.SetActive(false);
                back.gameObject.SetActive(false);
                if (flag)
                {
                    if(showMale)
                    {
                        ear.sprite = Lion_Male;
                        ear.gameObject.SetActive(true);
                    }
                    else
                    {
                        ear.sprite = Lion_Female_Front;
                        ear.gameObject.SetActive(true);
                        /*back.sprite = Lion_Female_Back;
                        back.gameObject.SetActive(true);*/
                    }
                    Main.Logger.Log($"showMale={showMale}");
                }
                else Main.Logger.Log($"invalid={invalid}, SpriteAdded={SpriteAdded}");
            }
            static void AddImage(ActorFace instance)
            {
                Transform parent = instance.transform;
                Color noColor = DateFile.instance.faceColor[7][0];

                Image ear = Image.Instantiate(instance.body, parent);
                ear.color = noColor;
                ear.transform.SetAsLastSibling();//显示在最前
                ear.gameObject.SetActive(false);

                Image back = Image.Instantiate(instance.body, parent);
                back.color = noColor;
                back.transform.SetAsFirstSibling();//显示在最后
                back.gameObject.SetActive(false);

                instance.beard.transform.SetAsLastSibling();//防止狮子头挡住胡子

                ImageDict.Add(instance, new Images(ear, back));
                Main.Logger.Log($"add Image");
            }
        }
        static Dictionary<ActorFace, Images> ImageDict = new Dictionary<ActorFace, Images>();
        struct Images
        {
            public Image ear;
            public Image back;
            public Images(Image ear, Image back)
            {
                this.ear = ear;
                this.back = back;
            }
        }
    }
    
}