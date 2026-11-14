using System;
using UnityEditor;
using UnityEngine;

namespace Cowart.FlowFramework.Editor
{
    /// <summary>
    /// 在 Unity Editor 中只读查看 Flow Framework 当前状态和运行历史。
    /// 此窗口位于 Editor 程序集，不会被打入运行时构建。
    /// </summary>
    public class FlowDebugWindow : EditorWindow
    {
        private Vector2 scrollPosition;

        /// <summary>
        /// 打开 Flow Framework 运行调试窗口。
        /// </summary>
        [MenuItem("Cowart/调试/Flow 系统调试")]
        public static void Open()
        {
            GetWindow<FlowDebugWindow>("Flow Debugger");
        }

        void OnEnable()
        {
            EditorApplication.update += RefreshWhilePlaying;
        }

        void OnDisable()
        {
            EditorApplication.update -= RefreshWhilePlaying;
        }

        private void RefreshWhilePlaying()
        {
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                Repaint();
            }
        }

        void OnGUI()
        {
            // 不在未初始化时访问静态 FlowContext，避免调试窗口产生无意义报错。
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "进入 Play Mode 后可查看 Flow Framework 的实时运行信息。",
                    MessageType.Info);
                return;
            }

            FlowManager manager = FindObjectOfType<FlowManager>();
            if (manager == null)
            {
                EditorGUILayout.HelpBox(
                    "当前场景中没有已初始化的 FlowManager。",
                    MessageType.Warning);
                return;
            }

            FlowContext context = FlowManager.FlowContext;
            if (context == null)
            {
                return;
            }

            DrawCurrentState(context);
            EditorGUILayout.Space();
            DrawHistory(context);
        }

        /// <summary>
        /// 绘制当前步骤的实时运行信息。
        /// 此方法仅用于调试窗口；Inspector 通过 GetCurrentStateValues 复用数据。
        /// </summary>
        internal static void DrawCurrentState(FlowContext context)
        {
            EditorGUILayout.LabelField("当前步骤（实时）", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                DrawCurrentStateValues(context);
        }

        /// <summary>为窗口与 Inspector 提供相同的状态数据。</summary>
        internal static System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>> GetCurrentStateValues(FlowContext context)
        {
            return new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>("当前状态", GetStateName(context.CurrentState)),
                new System.Collections.Generic.KeyValuePair<string, string>("状态类", GetStateTypeName(context.CurrentState)),
                new System.Collections.Generic.KeyValuePair<string, string>("上一状态", GetStateName(context.PreviousState)),
                new System.Collections.Generic.KeyValuePair<string, string>("计时状态", context.IsTimingPaused ? "已暂停" : "运行中"),
                new System.Collections.Generic.KeyValuePair<string, string>("开始时间", FormatTime(context.CurrentStateStartTime)),
                new System.Collections.Generic.KeyValuePair<string, string>("实际用时", FormatDuration(context.CurrentStateElapsedTime)),
                new System.Collections.Generic.KeyValuePair<string, string>("有效用时", FormatDuration(context.CurrentStateActiveElapsedTime))
            };
        }

        private static void DrawCurrentStateValues(FlowContext context)
        {
            foreach (var value in GetCurrentStateValues(context)) DrawValue(value.Key, value.Value);
        }

        private void DrawHistory(FlowContext context)
        {
            int completedCount = 0;
            foreach (StateRunRecord record in context.StateRunHistory)
            {
                if (record.IsCompleted)
                {
                    completedCount++;
                }
            }

            EditorGUILayout.LabelField($"已完成记录 ({completedCount})", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = context.StateRunHistory.Count - 1; i >= 0; i--)
            {
                StateRunRecord record = context.StateRunHistory[i];
                if (record.IsCompleted)
                {
                    DrawRecord(i, record);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private static void DrawRecord(int index, StateRunRecord record)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string title = string.IsNullOrEmpty(record.DisplayName)
                    ? record.StateType.Name
                    : record.DisplayName;
                EditorGUILayout.LabelField($"#{index + 1}  {title}", EditorStyles.boldLabel);

                DrawValue("状态类型", record.StateType.FullName);
                DrawValue("状态标识", record.StateId);
                DrawValue("运行结果", record.Result.ToString());
                DrawValue("开始时间", FormatTime(record.StartTime));
                DrawValue("结束时间", FormatTime(record.EndTime));
                DrawValue("实际用时", FormatDuration(record.Duration));
                DrawValue("有效用时", FormatDuration(record.ActiveDuration));

                if (!string.IsNullOrEmpty(record.Note))
                {
                    DrawValue("备注", record.Note);
                }
            }
        }

        private static string GetStateName(IState state)
        {
            if (state == null)
            {
                return "无";
            }

            IStateInfo stateInfo = state as IStateInfo;
            return stateInfo?.DisplayName ?? state.GetType().Name;
        }

        private static string GetStateTypeName(IState state)
        {
            if (state == null || state is NullState)
            {
                return "-";
            }

            return state.GetType().FullName;
        }

        private static string FormatTime(DateTime? time)
        {
            return time?.ToString("yyyy-MM-dd HH:mm:ss.fff") ?? "-";
        }

        private static string FormatDuration(TimeSpan duration)
        {
            return $"{duration.TotalSeconds:F3} 秒";
        }

        private static void DrawValue(string label, string value)
        {
            EditorGUILayout.LabelField(label, value ?? "-");
        }
    }
}
