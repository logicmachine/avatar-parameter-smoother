using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using dev.logilabo.parameter_smoother.runtime;

// ReSharper disable once CheckNamespace
namespace dev.logilabo.parameter_smoother.editor
{
    [CustomEditor(typeof(ParameterSmoother))]
    public class SettingsEditor : Editor
    {
        private SerializedProperty _controllerProp;
        private SerializedProperty _layerTypeProp;
        private SerializedProperty _smoothedSuffixProp;
        private SerializedProperty _configsProp;
        private ReorderableList _configsList;
        
        private void OnEnable()
        {
            var so = serializedObject;
            _layerTypeProp = so.FindProperty("layerType");
            _smoothedSuffixProp = so.FindProperty("smoothedSuffix");
            _configsProp = so.FindProperty("configs");
            _configsList = new ReorderableList(so, _configsProp);
            _configsList.drawHeaderCallback += rect =>
            {
                EditorGUI.LabelField(rect, "Target Parameters");
            };
            _configsList.elementHeight = (EditorGUIUtility.singleLineHeight + 2) * 3;
            _configsList.drawElementCallback += (rect, index, selected, focused) =>
            {
                var prop = _configsList.serializedProperty.GetArrayElementAtIndex(index);
                var h = EditorGUIUtility.singleLineHeight;
                var lw = EditorGUIUtility.labelWidth;
                var sourceRect = new Rect(rect.x, rect.y + (h + 2) * 0 + 1, rect.width, h);
                EditorGUI.PropertyField(sourceRect, prop.FindPropertyRelative("parameterName"));
                var localSmoothnessRect = new Rect(rect.x, rect.y + (h + 2) * 1 + 1, rect.width, h);
                EditorGUI.PropertyField(localSmoothnessRect, prop.FindPropertyRelative("localSmoothness"));
                var remoteSmoothnessRect = new Rect(rect.x, rect.y + (h + 2) * 2 + 1, rect.width, h);
                EditorGUI.PropertyField(remoteSmoothnessRect, prop.FindPropertyRelative("remoteSmoothness"));
            };
        }

        public override void OnInspectorGUI()
        {
            var so = serializedObject;
            EditorGUILayout.PropertyField(_layerTypeProp, new GUIContent("Layer Type"));
            EditorGUILayout.PropertyField(_smoothedSuffixProp, new GUIContent("Smoothed Parameter Suffix"));
            _configsList.DoLayoutList();
            so.ApplyModifiedProperties();
        }
    }
}
