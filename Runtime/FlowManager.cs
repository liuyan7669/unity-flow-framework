using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cowart.FlowFramework
{
    /// <summary>
    /// Flow Framework 的 Unity 生命周期入口。
    /// 负责初始化并持有状态上下文，在 Unity 更新阶段驱动当前状态。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    [Cowart.InspectorGroups.HideScript]
    public class FlowManager : MonoBehaviour
    {
        private static FlowManager instance;

        private readonly FlowContext flowContext = new FlowContext();
        private StateContext context;
        private bool isApplicationPaused;
        private bool isApplicationFocused = true;
        private bool isTimingPausedByBusiness;

        /// <summary>
        /// 获取当前 Flow Framework 的运行上下文。
        /// 上下文由框架维护，外部用于查询当前状态、上一状态和运行历史。
        /// </summary>
        public static FlowContext FlowContext
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("FlowManager 尚未初始化，请确保场景中存在启用的 FlowManager。");
                    return null;
                }
                return instance.flowContext;
            }
        }

        // 在普通业务脚本之前完成初始化，避免首个业务状态被默认状态覆盖。
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                context = new StateContext(flowContext);
                context.ChangeState(new NullState());
            }
            else
            {
                if (instance.gameObject != gameObject)
                {
                    gameObject.SetActive(false);
                    Destroy(gameObject);
                }
            }
        }

        void Update()
        {
            context.Update();
        }

        void LateUpdate()
        {
            context.LateUpdate();
        }

        void FixedUpdate()
        {
            context.FixedUpdate();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            isApplicationPaused = pauseStatus;
            UpdateApplicationTimingState();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            isApplicationFocused = hasFocus;
            UpdateApplicationTimingState();
        }

        private void UpdateApplicationTimingState()
        {
            flowContext.SetTimingPaused(ShouldPauseTiming(
                isTimingPausedByBusiness,
                isApplicationPaused,
                isApplicationFocused,
                Application.runInBackground));
        }

        // 失焦是否暂停应遵循 Player Settings 中的 Run In Background。
        // 应用真正进入后台暂停时，不受该设置影响。
        internal static bool ShouldPauseTiming(
            bool pausedByBusiness,
            bool applicationPaused,
            bool applicationFocused,
            bool runInBackground)
        {
            return pausedByBusiness ||
                applicationPaused ||
                (!runInBackground && !applicationFocused);
        }

        /// <summary>
        /// 暂停当前状态的有效用时累计。
        /// 实际经过用时仍会继续累计，可与应用失焦或后台暂停同时使用。
        /// </summary>
        public static void PauseTiming()
        {
            if (instance == null)
            {
                Debug.LogError("FlowManager 尚未初始化，请确保场景中存在启用的 FlowManager。");
                return;
            }
            instance.isTimingPausedByBusiness = true;
            instance.UpdateApplicationTimingState();
        }

        /// <summary>
        /// 恢复当前状态的有效用时累计。
        /// 如果应用仍处于失焦或后台暂停状态，有效用时会继续保持暂停。
        /// </summary>
        public static void ResumeTiming()
        {
            if (instance == null)
            {
                Debug.LogError("FlowManager 尚未初始化，请确保场景中存在启用的 FlowManager。");
                return;
            }
            instance.isTimingPausedByBusiness = false;
            instance.UpdateApplicationTimingState();
        }

        /// <summary>
        /// 结束当前业务状态并回到 NullState。
        /// 适用于实验最后一步结束，不需要为了封存记录而创建额外状态。
        /// </summary>
        public static void EndCurrentState(
            StateRunResult result = StateRunResult.Completed,
            string note = null)
        {
            if (instance == null)
            {
                Debug.LogError("FlowManager 尚未初始化，请确保场景中存在启用的 FlowManager。");
                return;
            }
            instance.context.EndCurrentState(result, note);
        }

        /// <summary>
        /// 使用已构造的状态实例立即切换状态。
        /// 适用于需要通过构造函数传递参数的状态。
        /// </summary>
        public static void ChangeState(StateBase state)
        {
            if (instance == null)
            {
                Debug.LogError("FlowManager 尚未初始化，请确保场景中存在启用的 FlowManager。");
                return;
            }
            instance.context.ChangeState(state);
        }

        /// <summary>
        /// 使用已构造的状态实例立即切换，并记录当前状态的结束结果和备注。
        /// </summary>
        public static void ChangeState(
            StateBase state,
            StateRunResult currentStateResult,
            string currentStateNote = null)
        {
            if (instance == null)
            {
                Debug.LogError("FlowManager 尚未初始化，请确保场景中存在启用的 FlowManager。");
                return;
            }
            instance.context.ChangeState(state, currentStateResult, currentStateNote);
        }

        /// <summary>
        /// 创建指定类型的无参状态实例并立即切换。
        /// </summary>
        public static void ChangeState<T>() where T : StateBase, new()
        {
            if (instance == null)
            {
                Debug.LogError("FlowManager 尚未初始化，请确保场景中存在启用的 FlowManager。");
                return;
            }
            T state = new T();
            instance.context.ChangeState(state);
        }

        /// <summary>
        /// 创建指定类型的无参状态实例并立即切换，同时记录当前状态的结束结果和备注。
        /// </summary>
        public static void ChangeState<T>(
            StateRunResult currentStateResult,
            string currentStateNote = null) where T : StateBase, new()
        {
            if (instance == null)
            {
                Debug.LogError("FlowManager 尚未初始化，请确保场景中存在启用的 FlowManager。");
                return;
            }
            T state = new T();
            instance.context.ChangeState(state, currentStateResult, currentStateNote);
        }

    }
}
