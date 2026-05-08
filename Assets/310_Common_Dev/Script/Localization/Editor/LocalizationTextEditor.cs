using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace DoDoEng.Common
{
    [CustomEditor(typeof(LocalizationText))]
    [CanEditMultipleObjects]
    public class LocalizationTextEditor : Editor
    {
        SerializedProperty ID;

        private void OnEnable()
        {
            ID = serializedObject.FindProperty(nameof(LocalizationText.ID));
        }
        public override void OnInspectorGUI()
        {
            //EditorGUI.BeginChangeCheck();
            //if (EditorGUI.EndChangeCheck()) {}
            serializedObject.Update();
            EditorGUILayout.PropertyField(ID);
            serializedObject.ApplyModifiedProperties();
            var text = LocalizationMGR.One.GetText(ID.stringValue);
            if (text != null)
                ((LocalizationText)target).UpdateText(text);
            else
            {
                GUI.color = Color.red;
                EditorGUILayout.LabelField($"there is no key : \"{ID.stringValue}\"");
            }
        }
    }
}