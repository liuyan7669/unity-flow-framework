using UnityEditor;
using Cowart.InspectorGroups.Editor;
using UnityEngine;

namespace Cowart.FlowFramework.Editor
{
    /// <summary>
    /// 在 FlowManager Inspector 中显示当前步骤的只读实时运行信息。
    /// 调试界面仅存在于 Editor 程序集，不会进入运行时构建。
    /// </summary>
    [CustomEditor(typeof(FlowManager))]
    public class FlowManagerEditor : InspectorGroupsEditor<FlowManager>
    {
        void OnEnable()
        {
            EditorApplication.update += RefreshWhilePlaying;
        }

        protected override void OnDisable()
        {
            EditorApplication.update -= RefreshWhilePlaying;
            base.OnDisable();
        }

        protected override void BuildInspector(InspectorContent content)
        {
            content.Group("FlowState", "当前步骤（实时）", status =>
            {
                if (!EditorApplication.isPlaying)
                {
                    status.Message("进入 Play Mode 后可查看当前步骤的实时运行信息。");
                    return;
                }
                var manager = (FlowManager)target;
                if (!manager.isActiveAndEnabled)
                {
                    status.Message("当前 FlowManager 未启用，无法显示实时运行信息。", MessageType.Warning);
                    return;
                }
                var context = FlowManager.FlowContext;
                if (context != null)
                    foreach (var value in FlowDebugWindow.GetCurrentStateValues(context))
                        status.Value(value.Key, value.Value);
            }, style: Cowart.InspectorGroups.InspectorGroupStyle.Framework);
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
