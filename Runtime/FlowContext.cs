using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Cowart.FlowFramework
{
    /// <summary>
    /// 状态结束时的运行结果。
    /// </summary>
    public enum StateRunResult
    {
        /// <summary>状态仍在运行。</summary>
        Running,
        /// <summary>状态正常完成，但未区分业务成功或失败。</summary>
        Completed,
        /// <summary>状态执行成功。</summary>
        Succeeded,
        /// <summary>状态执行失败。</summary>
        Failed,
        /// <summary>状态被跳过。</summary>
        Skipped,
        /// <summary>状态被用户或业务取消。</summary>
        Cancelled,
        /// <summary>状态因外部原因中断。</summary>
        Interrupted
    }

    /// <summary>
    /// 记录一次业务状态的运行情况。
    /// 同一类型的状态每进入一次，都会生成一条独立记录。
    /// </summary>
    [Serializable]
    public class StateRunRecord
    {
        private readonly long startTimestamp;
        private long endTimestamp;
        private long pauseStartTimestamp;
        private long pausedTimestampDuration;
        private bool isTimingPaused;

        /// <summary>
        /// 本次运行的状态类型。
        /// </summary>
        public Type StateType { get; }

        /// <summary>
        /// 本次运行的业务标识。
        /// </summary>
        public string StateId { get; }

        /// <summary>
        /// 本次运行用于显示和报告的名称。
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 状态成为当前状态的本地时间。
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// 状态完成 Exit 的本地时间。
        /// 状态仍在运行时为 null。
        /// </summary>
        public DateTime? EndTime { get; private set; }

        /// <summary>
        /// 状态是否已经结束运行。
        /// </summary>
        public bool IsCompleted => EndTime.HasValue;

        /// <summary>
        /// 本次状态运行的结束结果。状态仍在运行时为 Running。
        /// </summary>
        public StateRunResult Result { get; private set; }

        /// <summary>
        /// 本次状态运行的可选备注。
        /// </summary>
        public string Note { get; private set; }

        /// <summary>
        /// 本次状态运行的实际用时。
        /// 状态仍在运行时返回从开始到当前时刻的用时。
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                long currentTimestamp = IsCompleted
                    ? endTimestamp
                    : System.Diagnostics.Stopwatch.GetTimestamp();
                double seconds = (currentTimestamp - startTimestamp) /
                    (double)System.Diagnostics.Stopwatch.Frequency;
                return TimeSpan.FromSeconds(seconds);
            }
        }

        /// <summary>
        /// 排除框架计时暂停期间的有效运行用时。
        /// </summary>
        public TimeSpan ActiveDuration
        {
            get
            {
                long currentTimestamp = IsCompleted
                    ? endTimestamp
                    : System.Diagnostics.Stopwatch.GetTimestamp();
                long pausedDuration = pausedTimestampDuration;
                if (isTimingPaused)
                {
                    pausedDuration += currentTimestamp - pauseStartTimestamp;
                }
                double seconds = (currentTimestamp - startTimestamp - pausedDuration) /
                    (double)System.Diagnostics.Stopwatch.Frequency;
                return TimeSpan.FromSeconds(Math.Max(0, seconds));
            }
        }

        internal StateRunRecord(
            Type stateType,
            string stateId,
            string displayName,
            DateTime startTime)
        {
            StateType = stateType;
            StateId = stateId;
            DisplayName = displayName;
            StartTime = startTime;
            Result = StateRunResult.Running;
            startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
        }

        internal void SetTimingPaused(bool paused)
        {
            if (IsCompleted || isTimingPaused == paused)
            {
                return;
            }

            long currentTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            if (paused)
            {
                pauseStartTimestamp = currentTimestamp;
            }
            else
            {
                pausedTimestampDuration += currentTimestamp - pauseStartTimestamp;
            }
            isTimingPaused = paused;
        }

        internal void Complete(
            DateTime endTime,
            StateRunResult result,
            string note)
        {
            endTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            if (isTimingPaused)
            {
                pausedTimestampDuration += endTimestamp - pauseStartTimestamp;
                isTimingPaused = false;
            }
            EndTime = endTime;
            Result = result == StateRunResult.Running
                ? StateRunResult.Completed
                : result;
            Note = note;
        }
    }

    /// <summary>
    /// 可序列化的单次状态运行快照。
    /// 时间使用 ISO 8601 字符串，用时使用秒数，便于保存或上传。
    /// </summary>
    [Serializable]
    public class StateRunRecordSnapshot
    {
        /// <summary>状态类型全名。</summary>
        public string stateType;
        /// <summary>状态业务标识。</summary>
        public string stateId;
        /// <summary>状态显示名称。</summary>
        public string displayName;
        /// <summary>ISO 8601 格式的本地开始时间。</summary>
        public string startTime;
        /// <summary>ISO 8601 格式的本地结束时间，未结束时为空。</summary>
        public string endTime;
        /// <summary>状态是否已经结束运行。</summary>
        public bool isCompleted;
        /// <summary>状态运行结果名称。</summary>
        public string result;
        /// <summary>状态运行备注。</summary>
        public string note;
        /// <summary>包含暂停时间的实际经过秒数。</summary>
        public double durationSeconds;
        /// <summary>排除框架计时暂停期间的有效秒数。</summary>
        public double activeDurationSeconds;
    }

    /// <summary>
    /// 可序列化的 Flow Framework 运行快照。
    /// </summary>
    [Serializable]
    public class FlowSnapshot
    {
        /// <summary>ISO 8601 格式的快照创建时间。</summary>
        public string createdTime;
        /// <summary>当前状态类型全名。</summary>
        public string currentStateType;
        /// <summary>当前状态业务标识。</summary>
        public string currentStateId;
        /// <summary>当前状态显示名称。</summary>
        public string currentStateDisplayName;
        /// <summary>当前状态的 ISO 8601 格式开始时间。</summary>
        public string currentStateStartTime;
        /// <summary>当前状态包含暂停时间的实际经过秒数。</summary>
        public double currentStateElapsedSeconds;
        /// <summary>当前状态排除框架计时暂停期间的有效秒数。</summary>
        public double currentStateActiveElapsedSeconds;
        /// <summary>按进入顺序排列的状态运行记录。</summary>
        public List<StateRunRecordSnapshot> stateRuns = new List<StateRunRecordSnapshot>();

        /// <summary>
        /// 将快照转换为 JSON。框架只负责生成内容，不决定保存位置或上传方式。
        /// </summary>
        public string ToJson(bool prettyPrint = true)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }
    }

    /// <summary>
    /// 保存 Flow Framework 运行期间的状态上下文。
    /// 由 FlowManager 持有，由 StateContext 维护，外部只能读取框架状态信息。
    /// </summary>
    [Serializable]
    public class FlowContext
    {
        // 历史只保存状态类型，避免长期持有旧状态实例及其引用的场景资源。
        private readonly List<Type> stateHistory = new List<Type>();
        private readonly List<StateRunRecord> stateRunHistory = new List<StateRunRecord>();
        private readonly ReadOnlyCollection<Type> stateHistoryView;
        private readonly ReadOnlyCollection<StateRunRecord> stateRunHistoryView;
        private StateRunRecord currentStateRecord;
        private bool isTimingPaused;

        /// <summary>
        /// 创建 Flow Framework 运行上下文。
        /// </summary>
        public FlowContext()
        {
            stateHistoryView = stateHistory.AsReadOnly();
            stateRunHistoryView = stateRunHistory.AsReadOnly();
        }

        /// <summary>
        /// 状态完成切换后触发。
        /// 参数依次为上一个状态和当前状态，触发时当前状态的 Enter 已执行完成。
        /// </summary>
        public event Action<IState, IState> StateChanged;

        /// <summary>
        /// 一次业务状态运行结束并完成记录后触发。
        /// 参数是刚刚结束的完整运行记录。
        /// </summary>
        public event Action<StateRunRecord> StateRunCompleted;

        /// <summary>
        /// 当前正在运行的状态。
        /// </summary>
        public IState CurrentState { get; internal set; }

        /// <summary>
        /// 最近一次退出的状态。
        /// 相同类型的状态重复切换时，也会记录上一个状态实例。
        /// </summary>
        public IState PreviousState { get; internal set; }

        /// <summary>
        /// 当前状态的具体类型。当前状态为空时返回 null。
        /// </summary>
        public Type CurrentStateType => CurrentState?.GetType();

        /// <summary>
        /// 上一个状态的具体类型。上一个状态为空时返回 null。
        /// </summary>
        public Type PreviousStateType => PreviousState?.GetType();

        /// <summary>
        /// 按实际进入顺序记录的业务状态类型历史。
        /// NullState 是框架占位状态，不包含在历史中。
        /// </summary>
        public IReadOnlyList<Type> StateHistory => stateHistoryView;

        /// <summary>
        /// 按实际进入顺序记录的业务状态运行详情。
        /// 包含每次运行的开始时间、结束时间和用时。
        /// </summary>
        public IReadOnlyList<StateRunRecord> StateRunHistory => stateRunHistoryView;

        /// <summary>
        /// 当前状态的有效用时是否暂停累计。
        /// </summary>
        public bool IsTimingPaused => isTimingPaused;

        /// <summary>
        /// 当前业务状态的开始时间。
        /// 当前状态为空或为 NullState 时返回 null。
        /// </summary>
        public DateTime? CurrentStateStartTime => currentStateRecord?.StartTime;

        /// <summary>
        /// 当前业务状态从开始运行至今的实际用时。
        /// 当前状态为空或为 NullState 时返回 TimeSpan.Zero。
        /// </summary>
        public TimeSpan CurrentStateElapsedTime => currentStateRecord?.Duration ?? TimeSpan.Zero;

        /// <summary>
        /// 当前业务状态排除框架计时暂停期间的有效运行用时。
        /// 当前状态为空或为 NullState 时返回 TimeSpan.Zero。
        /// </summary>
        public TimeSpan CurrentStateActiveElapsedTime =>
            currentStateRecord?.ActiveDuration ?? TimeSpan.Zero;

        /// <summary>
        /// 获取当前状态实例。当前状态不是指定类型时返回 null。
        /// </summary>
        public T GetCurrent<T>() where T : class, IState
        {
            return CurrentState as T;
        }

        /// <summary>
        /// 获取上一个状态实例。上一个状态不是指定类型时返回 null。
        /// </summary>
        public T GetPrevious<T>() where T : class, IState
        {
            return PreviousState as T;
        }

        /// <summary>
        /// 判断当前状态是否为指定类型。
        /// </summary>
        public bool IsCurrent<T>() where T : IState
        {
            return CurrentState is T;
        }

        /// <summary>
        /// 判断上一个状态是否为指定类型。
        /// </summary>
        public bool WasPrevious<T>() where T : IState
        {
            return PreviousState is T;
        }

        /// <summary>
        /// 判断指定类型的业务状态是否曾经运行过。
        /// </summary>
        public bool HasRun<T>() where T : IState
        {
            return GetRunCount<T>() > 0;
        }

        /// <summary>
        /// 获取指定类型的业务状态实际进入过的次数。
        /// </summary>
        public int GetRunCount<T>() where T : IState
        {
            Type stateType = typeof(T);
            int count = 0;
            foreach (Type historyType in stateHistory)
            {
                if (historyType == stateType)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 获取指定类型最近一次进入产生的运行记录。
        /// 没有匹配记录时返回 null。
        /// </summary>
        public StateRunRecord GetLastRecord<T>() where T : IState
        {
            Type stateType = typeof(T);
            for (int i = stateRunHistory.Count - 1; i >= 0; i--)
            {
                if (stateRunHistory[i].StateType == stateType)
                {
                    return stateRunHistory[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定类型的全部运行记录。
        /// 返回独立的只读列表，不允许通过结果修改框架历史。
        /// </summary>
        public IReadOnlyList<StateRunRecord> GetRecords<T>() where T : IState
        {
            Type stateType = typeof(T);
            List<StateRunRecord> records = new List<StateRunRecord>();
            foreach (StateRunRecord record in stateRunHistory)
            {
                if (record.StateType == stateType)
                {
                    records.Add(record);
                }
            }
            return records;
        }

        /// <summary>
        /// 获取指定类型所有运行记录的累计实际用时。
        /// 包含尚未结束记录截至当前时刻的用时。
        /// </summary>
        public TimeSpan GetTotalDuration<T>() where T : IState
        {
            TimeSpan total = TimeSpan.Zero;
            foreach (StateRunRecord record in stateRunHistory)
            {
                if (record.StateType == typeof(T))
                {
                    total += record.Duration;
                }
            }
            return total;
        }

        /// <summary>
        /// 获取指定类型最近一次运行的实际用时。
        /// 没有匹配记录时返回 TimeSpan.Zero。
        /// </summary>
        public TimeSpan GetLastDuration<T>() where T : IState
        {
            StateRunRecord record = GetLastRecord<T>();
            return record?.Duration ?? TimeSpan.Zero;
        }

        /// <summary>
        /// 清除已经积累的状态运行历史。
        /// 当前状态及其正在进行的计时会保留，不会触发状态切换。
        /// </summary>
        public void ClearHistory()
        {
            stateHistory.Clear();
            stateRunHistory.Clear();

            if (currentStateRecord != null)
            {
                stateHistory.Add(currentStateRecord.StateType);
                stateRunHistory.Add(currentStateRecord);
            }
        }

        /// <summary>
        /// 创建当前运行信息的独立可序列化快照。
        /// 后续状态切换不会修改已经创建的快照。
        /// </summary>
        public FlowSnapshot CreateSnapshot()
        {
            FlowSnapshot snapshot = new FlowSnapshot
            {
                createdTime = DateTime.Now.ToString("O"),
                currentStateType = CurrentStateType?.FullName,
                currentStateStartTime = CurrentStateStartTime?.ToString("O"),
                currentStateElapsedSeconds = CurrentStateElapsedTime.TotalSeconds,
                currentStateActiveElapsedSeconds = CurrentStateActiveElapsedTime.TotalSeconds
            };

            if (CurrentState is IStateInfo currentStateInfo)
            {
                snapshot.currentStateId = currentStateInfo.StateId;
                snapshot.currentStateDisplayName = currentStateInfo.DisplayName;
            }

            foreach (StateRunRecord record in stateRunHistory)
            {
                snapshot.stateRuns.Add(new StateRunRecordSnapshot
                {
                    stateType = record.StateType.FullName,
                    stateId = record.StateId,
                    displayName = record.DisplayName,
                    startTime = record.StartTime.ToString("O"),
                    endTime = record.EndTime?.ToString("O"),
                    isCompleted = record.IsCompleted,
                    result = record.Result.ToString(),
                    note = record.Note,
                    durationSeconds = record.Duration.TotalSeconds,
                    activeDurationSeconds = record.ActiveDuration.TotalSeconds
                });
            }

            return snapshot;
        }

        /// <summary>
        /// 记录一次状态切换。
        /// 只能由框架内部调用，确保上下文与实际状态切换保持一致。
        /// </summary>
        internal void RecordStateChange(
            IState state,
            StateRunResult currentStateResult,
            string currentStateNote)
        {
            if (currentStateRecord != null)
            {
                currentStateRecord.Complete(
                    DateTime.Now,
                    currentStateResult,
                    currentStateNote);
                StateRunCompleted?.Invoke(currentStateRecord);
                currentStateRecord = null;
            }

            PreviousState = CurrentState;
            CurrentState = state;

            if (state != null && !(state is NullState))
            {
                Type stateType = state.GetType();
                IStateInfo stateInfo = state as IStateInfo;
                string stateId = stateInfo?.StateId ?? stateType.FullName;
                string displayName = stateInfo?.DisplayName ?? stateType.Name;

                stateHistory.Add(stateType);
                currentStateRecord = new StateRunRecord(
                    stateType,
                    stateId,
                    displayName,
                    DateTime.Now);
                currentStateRecord.SetTimingPaused(isTimingPaused);
                stateRunHistory.Add(currentStateRecord);
            }
        }

        /// <summary>
        /// 设置有效用时的暂停状态，不影响实际经过用时。
        /// </summary>
        internal void SetTimingPaused(bool paused)
        {
            isTimingPaused = paused;
            currentStateRecord?.SetTimingPaused(paused);
        }

        /// <summary>
        /// 通知外部一次状态切换已经完整结束。
        /// </summary>
        internal void NotifyStateChanged()
        {
            StateChanged?.Invoke(PreviousState, CurrentState);
        }
    }

}
