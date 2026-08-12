using Cowart.FlowFramework;
using UnityEngine;

namespace Cowart.FlowFramework.Samples
{
    /// <summary>
    /// 五步仿真实验流程示例。
    /// 运行示例场景后，可通过屏幕面板逐步推进流程，并在 Flow Debugger 中观察运行记录。
    /// </summary>
    public class BasicFlowExample : MonoBehaviour
    {
        private const int StepCount = 5;

        [SerializeField] private bool startAutomatically = true;
        [SerializeField] private string participantId = "P-001";
        [SerializeField] private int trialCount = 3;

        private readonly string[] stepNames =
        {
            "设备检查",
            "受试者登记",
            "基线校准",
            "实验执行",
            "数据保存"
        };

        private readonly string[] completionNotes =
        {
            "设备连接和传感器状态正常",
            "受试者信息登记完成",
            "基线数据采集完成",
            "全部实验轮次执行完成",
            "实验数据校验并保存完成"
        };

        private int currentStepIndex = -1;
        private bool flowFinished;
        private bool flowCompletedSuccessfully;
        private string statusMessage = "等待开始";
        private Vector2 scrollPosition;
        private GUIStyle titleStyle;
        private GUIStyle stepStyle;

        /// <summary>
        /// 当前 Flow Framework 共享上下文。
        /// </summary>
        public FlowContext Context => FlowManager.FlowContext;

        void Start()
        {
            if (startAutomatically)
            {
                RestartFlow();
            }
        }

        void OnGUI()
        {
            EnsureStyles();

            float width = Mathf.Min(620f, Screen.width - 32f);
            float height = Mathf.Min(660f, Screen.height - 32f);
            Rect panelRect = new Rect(16f, 16f, width, height);

            GUILayout.BeginArea(panelRect, GUI.skin.window);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Flow Framework 五步实验流程", titleStyle);
            GUILayout.Space(6f);
            DrawStepProgress();
            GUILayout.Space(10f);
            DrawCurrentState();
            GUILayout.Space(10f);
            DrawControls();
            GUILayout.Space(10f);
            DrawCompletedRecords();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// 完成当前步骤并立即进入下一步；第五步完成后结束整个流程。
        /// </summary>
        public void CompleteCurrentStep()
        {
            if (!IsBusinessStateRunning())
            {
                return;
            }

            string completedStepName = stepNames[currentStepIndex];
            string completionNote = completionNotes[currentStepIndex];

            if (currentStepIndex == StepCount - 1)
            {
                FlowManager.EndCurrentState(StateRunResult.Succeeded, completionNote);
                flowFinished = true;
                flowCompletedSuccessfully = true;
                statusMessage = "五步流程已全部完成";
                return;
            }

            currentStepIndex++;
            FlowManager.ChangeState(
                CreateStepState(currentStepIndex),
                StateRunResult.Succeeded,
                completionNote);
            statusMessage = $"{completedStepName}已完成，进入{stepNames[currentStepIndex]}";
        }

        /// <summary>
        /// 将当前步骤记录为失败并结束流程。
        /// </summary>
        public void FailCurrentStep()
        {
            if (!IsBusinessStateRunning())
            {
                return;
            }

            string failedStepName = stepNames[currentStepIndex];
            FlowManager.EndCurrentState(
                StateRunResult.Failed,
                $"{failedStepName}未通过，流程提前结束");
            flowFinished = true;
            flowCompletedSuccessfully = false;
            statusMessage = $"{failedStepName}失败，可重新开始流程";
        }

        /// <summary>
        /// 暂停当前步骤的有效用时累计。
        /// </summary>
        public void PauseTiming()
        {
            if (!IsBusinessStateRunning())
            {
                return;
            }

            FlowManager.PauseTiming();
            statusMessage = "有效用时已暂停，实际用时仍会继续累计";
        }

        /// <summary>
        /// 恢复当前步骤的有效用时累计。
        /// </summary>
        public void ResumeTiming()
        {
            FlowManager.ResumeTiming();
            statusMessage = "有效用时已恢复";
        }

        /// <summary>
        /// 清除已有记录并从第一步重新开始。
        /// </summary>
        public void RestartFlow()
        {
            FlowContext context = Context;
            if (context == null)
            {
                return;
            }

            if (context.CurrentStateStartTime.HasValue)
            {
                FlowManager.EndCurrentState(
                    StateRunResult.Cancelled,
                    "重新开始示例流程");
            }

            FlowManager.ResumeTiming();
            context.ClearHistory();
            currentStepIndex = 0;
            flowFinished = false;
            flowCompletedSuccessfully = false;
            statusMessage = "流程开始，请完成设备检查";
            FlowManager.ChangeState<DeviceCheckState>();
        }

        /// <summary>
        /// 创建当前运行信息的 JSON 快照并输出到 Console。
        /// </summary>
        public void LogSnapshotJson()
        {
            string json = Context?.CreateSnapshot().ToJson() ?? string.Empty;
            Debug.Log($"Flow Framework Snapshot:\n{json}");
            statusMessage = "快照 JSON 已输出到 Console";
        }

        private StateBase CreateStepState(int stepIndex)
        {
            switch (stepIndex)
            {
                case 1:
                    return new ParticipantRegistrationState(participantId);
                case 2:
                    return new BaselineCalibrationState();
                case 3:
                    return new ExperimentExecutionState(trialCount);
                case 4:
                    return new DataSaveState();
                default:
                    return new DeviceCheckState();
            }
        }

        private bool IsBusinessStateRunning()
        {
            return !flowFinished &&
                currentStepIndex >= 0 &&
                currentStepIndex < StepCount &&
                Context != null &&
                Context.CurrentStateStartTime.HasValue;
        }

        private void DrawStepProgress()
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < StepCount; i++)
            {
                string marker;
                if (flowCompletedSuccessfully || i < currentStepIndex)
                {
                    marker = "[完成]";
                }
                else if (!flowFinished && i == currentStepIndex)
                {
                    marker = "[进行中]";
                }
                else if (flowFinished && i == currentStepIndex)
                {
                    marker = "[失败]";
                }
                else
                {
                    marker = "[等待]";
                }

                GUILayout.Label($"{i + 1}. {stepNames[i]}\n{marker}", stepStyle, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawCurrentState()
        {
            FlowContext context = Context;
            string currentName = "无";
            if (context?.CurrentState is IStateInfo stateInfo && !(context.CurrentState is NullState))
            {
                currentName = stateInfo.DisplayName;
            }

            GUILayout.Label("当前步骤", GUI.skin.box);
            GUILayout.Label($"状态：{currentName}");
            GUILayout.Label($"实际用时：{context?.CurrentStateElapsedTime.TotalSeconds ?? 0:F3} 秒");
            GUILayout.Label($"有效用时：{context?.CurrentStateActiveElapsedTime.TotalSeconds ?? 0:F3} 秒");
            GUILayout.Label($"计时：{(context?.IsTimingPaused == true ? "已暂停" : "运行中")}");
            GUILayout.Label($"提示：{statusMessage}");
        }

        private void DrawControls()
        {
            bool running = IsBusinessStateRunning();

            GUI.enabled = running;
            string completeButtonText = currentStepIndex == StepCount - 1
                ? "完成最后一步并结束流程"
                : "完成当前步骤，进入下一步";
            if (GUILayout.Button(completeButtonText, GUILayout.Height(34f)))
            {
                CompleteCurrentStep();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("暂停有效计时", GUILayout.Height(28f)))
            {
                PauseTiming();
            }
            if (GUILayout.Button("恢复有效计时", GUILayout.Height(28f)))
            {
                ResumeTiming();
            }
            if (GUILayout.Button("当前步骤失败", GUILayout.Height(28f)))
            {
                FailCurrentStep();
            }
            GUILayout.EndHorizontal();

            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("重新开始", GUILayout.Height(28f)))
            {
                RestartFlow();
            }
            if (GUILayout.Button("输出 JSON 快照", GUILayout.Height(28f)))
            {
                LogSnapshotJson();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawCompletedRecords()
        {
            FlowContext context = Context;
            GUILayout.Label("已完成记录", GUI.skin.box);

            if (context == null)
            {
                return;
            }

            int completedCount = 0;
            foreach (StateRunRecord record in context.StateRunHistory)
            {
                if (!record.IsCompleted)
                {
                    continue;
                }

                completedCount++;
                GUILayout.Label(
                    $"#{completedCount} {record.DisplayName} | {record.Result} | " +
                    $"实际 {record.Duration.TotalSeconds:F3}s | 有效 {record.ActiveDuration.TotalSeconds:F3}s");
            }

            if (completedCount == 0)
            {
                GUILayout.Label("完成第一步后，这里会出现冻结的运行记录。");
            }
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            stepStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                fixedHeight = 52f
            };
        }
    }

    /// <summary>
    /// 示例步骤的公共基类，统一输出 Enter 和 Exit 生命周期日志。
    /// </summary>
    public abstract class ExperimentStepState : StateBase
    {
        public override void Enter()
        {
            Debug.Log($"[Flow] Enter: {DisplayName}");
        }

        public override void Exit()
        {
            Debug.Log($"[Flow] Exit: {DisplayName}");
        }
    }

    /// <summary>第一步：检查实验设备。</summary>
    public class DeviceCheckState : ExperimentStepState
    {
        public override string StateId => "device-check";
        public override string DisplayName => "1. 设备检查";
    }

    /// <summary>第二步：登记受试者信息，演示通过构造函数传递状态参数。</summary>
    public class ParticipantRegistrationState : ExperimentStepState
    {
        private readonly string participantId;

        public ParticipantRegistrationState(string participantId)
        {
            this.participantId = string.IsNullOrWhiteSpace(participantId)
                ? "未编号"
                : participantId;
        }

        public override string StateId => "participant-registration";
        public override string DisplayName => $"2. 受试者登记（{participantId}）";
    }

    /// <summary>第三步：采集实验基线。</summary>
    public class BaselineCalibrationState : ExperimentStepState
    {
        public override string StateId => "baseline-calibration";
        public override string DisplayName => "3. 基线校准";
    }

    /// <summary>第四步：执行指定轮数的正式实验。</summary>
    public class ExperimentExecutionState : ExperimentStepState
    {
        private readonly int trialCount;

        public ExperimentExecutionState(int trialCount)
        {
            this.trialCount = Mathf.Max(1, trialCount);
        }

        public override string StateId => "experiment-execution";
        public override string DisplayName => $"4. 实验执行（{trialCount} 轮）";
    }

    /// <summary>第五步：校验并保存实验数据。</summary>
    public class DataSaveState : ExperimentStepState
    {
        public override string StateId => "data-save";
        public override string DisplayName => "5. 数据保存";
    }
}
