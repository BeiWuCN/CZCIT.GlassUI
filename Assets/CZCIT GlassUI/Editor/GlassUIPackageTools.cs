// 导出可复用 GlassUI 包并提供面板创建入口，供项目外接入使用。
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CZCIT.GlassUI.Editor
{
    [InitializeOnLoad]
    public static class GlassUIPackageTools
    {
        const string Root = "Assets/CZCIT GlassUI";
        const string Request = "Temp/ExportGlassUI.request";

        static GlassUIPackageTools()
        {
            EditorApplication.update += Poll;
        }

        static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            File.Delete(Request);
            try
            {
                Export();
            }
            catch (Exception e)
            {
                File.WriteAllText("Temp/ExportGlassUI.result", e.ToString());
                Debug.LogException(e);
            }
        }


        [MenuItem("Tools/Glass UI/Export package to Desktop")]
        public static void Export()
        {
            AssetDatabase.SaveAssets();
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var output = Path.Combine(desktop, "CZCIT-GlassUI-26.11.17.unitypackage");
            // Explicit boundary: do not pull exhibit scenes, fonts, or business scripts into the package.
            AssetDatabase.ExportPackage(Root, output, ExportPackageOptions.Recurse);
            File.WriteAllText("Temp/ExportGlassUI.result", "PASS Unity compilation and export completed\n" + output);
            Debug.Log("Glass UI exported: " + output);
        }


        [MenuItem("GameObject/UI/CZCIT Frosted Glass Panel", false, 2100)]
        public static void CreatePanel()
        {
            var root = new GameObject(
                "Frosted Glass Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(root, "Create frosted glass panel");
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            ((RectTransform)root.transform).sizeDelta = new Vector2(900, 500);
            root.transform.localScale = Vector3.one * .001f;
            if (Camera.main)
            {
                root.transform.SetPositionAndRotation(
                    Camera.main.transform.position + Camera.main.transform.forward * 1.5f,
                    Camera.main.transform.rotation);
                if (!Camera.main.GetComponent<CZCIT.GlassUI.GlassSceneBackground>())
                    Undo.AddComponent<CZCIT.GlassUI.GlassSceneBackground>(Camera.main.gameObject);
            }
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/FrostedPanel.mat");
            panel.GetComponent<Image>().raycastTarget = false;
            CZCIT.GlassUI.GlassUiMotion.For(root);
            Selection.activeGameObject = root;
        }
    }
}
