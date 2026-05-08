using UnityEditor;
using UnityEngine;

namespace DoDoEng.Common
{
    [CustomEditor(typeof(TimelineTrackNames))]
    public class TimelineTrackNamesEditor : Editor
    {
        // Fields
        private TimelineTrackNames timelineTrackNames;



        // Overrides
        public override void OnInspectorGUI()
        {
            var serializedObject = this.serializedObject;
            serializedObject.Update();

            DrawDefaultInspector();

            GUILayout.Space(20);

            var prevColor = GUI.color;
            GUI.color = new Color(0.5f, 0.75f, 1, 1);
            if (GUILayout.Button("Refresh"))
            {
                timelineTrackNames.Refresh();
            }

            GUI.color = prevColor;

            //Let's make sure to save here, in case we used any non-SerializedProperty Editor GUI:
            if (serializedObject.ApplyModifiedProperties())
                EditorUtility.SetDirty(timelineTrackNames);
        }



        // Unity Messages
        private void OnEnable()
        {
            var serializedObject = this.serializedObject;

            //Initialize the take number upon opening this inspector
            timelineTrackNames = (TimelineTrackNames)target;
            serializedObject.Update();
        }
    }
}