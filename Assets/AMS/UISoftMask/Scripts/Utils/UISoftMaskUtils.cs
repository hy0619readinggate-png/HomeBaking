using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AMS.UI.SoftMask
{
    public class UISoftMaskUtils
    {
        //UISoftMaskShader
        public const string k_DefaultSoftMaskShader = "AMS/UISoftMask";
        public static int s_SoftMaskID = Shader.PropertyToID("_SoftMask");
        public const string k_USE_SOFTMASK = "_USE_SOFTMASK";
        public static Shader s_SoftMaskShader = Shader.Find(k_DefaultSoftMaskShader);

        //UISoftMaskSceneView
        public const string k_SceneViewSoftMaskShader = "Hidden/AMS/UISoftMaskSceneView";
        public static Shader s_SceneViewSoftMaskShader = Shader.Find(k_SceneViewSoftMaskShader);

        //UISoftMaskBlitShader
        public const string k_SoftMaskBlitShader = "Hidden/AMS/UISoftMaskBlit";
        public static Shader s_SoftMaskBlitShader = Shader.Find(k_SoftMaskBlitShader);
        public static int s_OverridingMaskID = Shader.PropertyToID("_OverridingMask");
        public static int s_ScaleID = Shader.PropertyToID("_Scale");
        public static int s_OffsetID = Shader.PropertyToID("_Offset");
        public static int s_FalloffID = Shader.PropertyToID("_FallOff");
        public static int s_OpacityID = Shader.PropertyToID("_Opacity");
        public static int s_AngleID = Shader.PropertyToID("_Angle");
        public static int s_SpriteAtlasOffsetID = Shader.PropertyToID("_SpriteAtlasOffset");

        public const string k_SoftMaskMatTag = "SoftMaskMat[";

#if UNITY_EDITOR
        [MenuItem("Window/AMS/UISoftMask/Force Include Shaders (ProjectSettings)", priority = 0)]
        private static void ForceIncludeShaders()
        {
            var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            var serializedObject = new SerializedObject(graphicsSettingsObj);
            var inludedShaderProperty = serializedObject.FindProperty("m_AlwaysIncludedShaders");
            List<Shader> shaders = new List<Shader>();
            for (int i = 0; i < inludedShaderProperty.arraySize; i++)
            {
                if (inludedShaderProperty.GetArrayElementAtIndex(i).objectReferenceValue is Shader shader)
                    shaders.Add(shader);
                else
                {
                    inludedShaderProperty.DeleteArrayElementAtIndex(i);
                    i--;
                }
            }

            if (!shaders.Contains(s_SoftMaskShader))
            {
                var index = inludedShaderProperty.arraySize;
                inludedShaderProperty.InsertArrayElementAtIndex(index);
                var arrayElem = inludedShaderProperty.GetArrayElementAtIndex(index);
                arrayElem.objectReferenceValue = s_SoftMaskShader;
            }

            if (!shaders.Contains(s_SoftMaskBlitShader))
            {
                var index = inludedShaderProperty.arraySize;
                inludedShaderProperty.InsertArrayElementAtIndex(index);
                var arrayElem = inludedShaderProperty.GetArrayElementAtIndex(index);
                arrayElem.objectReferenceValue = s_SoftMaskBlitShader;
            }

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Window/AMS/UISoftMask/Force Scene View Toggle (Built-In RP)", priority = 0)]
        public static void ForceSceneView()
        {
            if (SceneView.lastActiveSceneView.camera.GetComponent<SceneViewHandle>() is SceneViewHandle sceneView)
                sceneView.ForceWorldCanvasToggle();

            EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")).Repaint();
            SceneView.RepaintAll();
        }

        [ExecuteInEditMode, DisallowMultipleComponent, RequireComponent(typeof(Camera))]
        public sealed class SceneViewHandle : MonoBehaviour
        {
            private List<RectUV> m_SoftMaskRectList = new List<RectUV>();
            public List<RectUV> softMaskRectList => m_SoftMaskRectList;

            [SerializeField]
            private bool m_ForceWorldCanvas = false;

            internal void ForceWorldCanvasToggle()
            {
                m_ForceWorldCanvas = !m_ForceWorldCanvas;
            }

            private void OnPreCull()
            {
                if (GraphicsSettings.defaultRenderPipeline != null)
                    DestroyImmediate(this);

                if (m_ForceWorldCanvas)
                    m_SoftMaskRectList.ForEach(m => (m as UISoftMask).SetWorldCanvasMaterials());
            }
        }
#endif
    }
}