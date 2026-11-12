using UnityEditor;
using UnityEngine;

namespace Cowart.FlowFramework.Editor
{
    /// <summary>
    /// 在 FlowManager Inspector 中显示当前步骤的只读实时运行信息。
    /// 调试界面仅存在于 Editor 程序集，不会进入运行时构建。
    /// </summary>
    [CustomEditor(typeof(FlowManager))]
    public class FlowManagerEditor : UnityEditor.Editor
    {
        void OnEnable()
        {
            EditorApplication.update += RefreshWhilePlaying;
        }

        void OnDisable()
        {
            EditorApplication.update -= RefreshWhilePlaying;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "进入 Play Mode 后可查看当前步骤的实时运行信息。",
                    MessageType.Info);
                return;
            }

            FlowManager manager = (FlowManager)target;
            if (!manager.isActiveAndEnabled)
            {
                EditorGUILayout.HelpBox(
                    "当前 FlowManager 未启用，无法显示实时运行信息。",
                    MessageType.Warning);
                return;
            }

            FlowContext context = FlowManager.FlowContext;
            if (context != null)
            {
                FlowDebugWindow.DrawCurrentState(context);
            }
        }

        private void RefreshWhilePlaying()
        {
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                Repaint();
            }
        }
    }
}
