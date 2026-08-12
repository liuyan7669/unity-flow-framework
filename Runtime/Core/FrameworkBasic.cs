using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cowart.FlowFramework
{
    /// <summary>
    /// 状态生命周期接口。
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// 进入状态时调用。
        /// </summary>
        void Enter();

        /// <summary>
        /// 状态运行期间由 FlowManager 每帧调用。
        /// </summary>
        void Update();

        /// <summary>
        /// 状态运行期间由 FlowManager 在每次 LateUpdate 时调用。
        /// </summary>
        void LateUpdate();

        /// <summary>
        /// 状态运行期间由 FlowManager 在每次 FixedUpdate 时调用。
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// 离开状态时调用。
        /// </summary>
        void Exit();
    }

    /// <summary>
    /// 为状态提供可选的业务标识和显示名称。
    /// StateBase 已提供类型名称作为默认值，具体状态可按需重写。
    /// </summary>
    public interface IStateInfo
    {
        /// <summary>
        /// 状态的业务标识。同一状态类型用于不同实验步骤时，应提供不同标识。
        /// </summary>
        string StateId { get; }

        /// <summary>
        /// 用于界面、日志和实验报告的状态名称。
        /// </summary>
        string DisplayName { get; }
    }

    /// <summary>
    /// 管理当前状态并执行状态生命周期。
    /// 状态采用立即同步切换：旧状态 Exit、更新上下文、新状态 Enter。
    /// </summary>
    public class StateContext
    {
        private readonly FlowContext flowContext;

        /// <summary>
        /// 创建使用独立 FlowContext 的状态上下文。
        /// </summary>
        public StateContext() : this(new FlowContext())
        {

        }

        /// <summary>
        /// 创建使用指定 FlowContext 的状态上下文。
        /// </summary>
        public StateContext(FlowContext flowContext)
        {
            this.flowContext = flowContext;
        }

        /// <summary>
        /// 立即切换到指定状态。
        /// 此方法返回前，旧状态 Exit 和新状态 Enter 均已执行完成。
        /// </summary>
        public virtual void ChangeState(IState state)
        {
            ChangeState(state, StateRunResult.Completed);
        }

        /// <summary>
        /// 立即切换到指定状态，并记录当前状态的结束结果。
        /// 结果和备注描述的是即将退出的状态，而不是即将进入的新状态。
        /// </summary>
        public virtual void ChangeState(
            IState state,
            StateRunResult currentStateResult,
            string currentStateNote = null)
        {
            if (flowContext.CurrentState != null)
            {
                flowContext.CurrentState.Exit();
            }
            flowContext.RecordStateChange(state, currentStateResult, currentStateNote);
            if (flowContext.CurrentState != null)
            {
                flowContext.CurrentState.Enter();
            }
            flowContext.NotifyStateChanged();
        }

        /// <summary>
        /// 结束当前业务状态并回到 NullState。
        /// 用于最后一个状态结束时完整记录结果、结束时间和用时。
        /// </summary>
        public virtual void EndCurrentState(
            StateRunResult result = StateRunResult.Completed,
            string note = null)
        {
            ChangeState(new NullState(), result, note);
        }

        /// <summary>
        /// 驱动当前状态执行每帧逻辑。
        /// </summary>
        public virtual void Update()
        {
            if (flowContext.CurrentState != null)
            {
                flowContext.CurrentState.Update();
            }
        }

        /// <summary>
        /// 驱动当前状态执行每帧后期逻辑。
        /// </summary>
        public virtual void LateUpdate()
        {
            if (flowContext.CurrentState != null)
            {
                flowContext.CurrentState.LateUpdate();
            }
        }

        /// <summary>
        /// 驱动当前状态执行固定时间步长逻辑。
        /// </summary>
        public virtual void FixedUpdate()
        {
            if (flowContext.CurrentState != null)
            {
                flowContext.CurrentState.FixedUpdate();
            }
        }
    }

    /// <summary>
    /// 状态基类。
    /// 具体状态按需重写生命周期方法，无需实现不使用的生命周期方法。
    /// </summary>
    public class StateBase : IState, IStateInfo
    {
        /// <inheritdoc />
        public virtual string StateId => GetType().FullName;

        /// <inheritdoc />
        public virtual string DisplayName => GetType().Name;

        /// <inheritdoc />
        public virtual void Enter()
        {

        }

        /// <inheritdoc />
        public virtual void Exit()
        {

        }

        /// <inheritdoc />
        public virtual void Update()
        {

        }

        /// <inheritdoc />
        public virtual void LateUpdate()
        {

        }

        /// <inheritdoc />
        public virtual void FixedUpdate()
        {

        }
    }

    /// <summary>
    /// 框架初始化后的默认占位状态，不计入业务状态历史。
    /// </summary>
    public class NullState : StateBase
    {

    }

}

