using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Cowart.FlowFramework.Tests
{
    public class FlowContextTests
    {
        private class FirstState : StateBase
        {
            public override string StateId => "first";
            public override string DisplayName => "第一步";
        }

        private class SecondState : StateBase
        {
        }

        [Test]
        public void ChangeState_DefaultResult_CompletesPreviousState()
        {
            FlowContext flowContext = new FlowContext();
            StateContext stateContext = new StateContext(flowContext);

            stateContext.ChangeState(new FirstState());
            stateContext.ChangeState(new SecondState());

            StateRunRecord record = flowContext.StateRunHistory[0];
            Assert.AreEqual(StateRunResult.Completed, record.Result);
            Assert.IsTrue(record.IsCompleted);
        }

        [Test]
        public void ChangeState_ExplicitResult_RecordsResultAndNote()
        {
            FlowContext flowContext = new FlowContext();
            StateContext stateContext = new StateContext(flowContext);

            stateContext.ChangeState(new FirstState());
            stateContext.ChangeState(
                new SecondState(),
                StateRunResult.Failed,
                "步骤验证失败");

            StateRunRecord record = flowContext.StateRunHistory[0];
            Assert.AreEqual(StateRunResult.Failed, record.Result);
            Assert.AreEqual("步骤验证失败", record.Note);
        }

        [Test]
        public void ChangeState_SameTypeRepeatedly_CreatesIndependentRecords()
        {
            FlowContext flowContext = new FlowContext();
            StateContext stateContext = new StateContext(flowContext);

            stateContext.ChangeState(new FirstState());
            stateContext.ChangeState(new FirstState());

            Assert.AreEqual(2, flowContext.GetRunCount<FirstState>());
            Assert.AreEqual(2, flowContext.GetRecords<FirstState>().Count);
        }

        [Test]
        public void PauseTiming_ExcludesPausedTimeFromActiveDuration()
        {
            FlowContext flowContext = new FlowContext();
            StateContext stateContext = new StateContext(flowContext);
            stateContext.ChangeState(new FirstState());

            Thread.Sleep(20);
            flowContext.SetTimingPaused(true);
            Thread.Sleep(40);
            flowContext.SetTimingPaused(false);
            Thread.Sleep(20);
            stateContext.EndCurrentState();

            StateRunRecord record = flowContext.GetLastRecord<FirstState>();
            Assert.Greater(record.Duration, record.ActiveDuration);
            Assert.GreaterOrEqual(
                (record.Duration - record.ActiveDuration).TotalMilliseconds,
                25d);
        }

        [TestCase(false, false, false, false, true)]
        [TestCase(false, false, false, true, false)]
        [TestCase(false, false, true, false, false)]
        [TestCase(false, true, true, true, true)]
        [TestCase(true, false, true, true, true)]
        public void ShouldPauseTiming_RespectsRunInBackground(
            bool pausedByBusiness,
            bool applicationPaused,
            bool applicationFocused,
            bool runInBackground,
            bool expected)
        {
            Assert.AreEqual(
                expected,
                FlowManager.ShouldPauseTiming(
                    pausedByBusiness,
                    applicationPaused,
                    applicationFocused,
                    runInBackground));
        }

        [Test]
        public void ClearHistory_PreservesCurrentStateRecord()
        {
            FlowContext flowContext = new FlowContext();
            StateContext stateContext = new StateContext(flowContext);
            stateContext.ChangeState(new FirstState());
            stateContext.ChangeState(new SecondState());

            flowContext.ClearHistory();

            Assert.AreEqual(1, flowContext.StateRunHistory.Count);
            Assert.AreSame(
                flowContext.GetCurrent<SecondState>(),
                flowContext.CurrentState);
            Assert.IsFalse(flowContext.StateRunHistory[0].IsCompleted);
        }

        [Test]
        public void EndCurrentState_CompletesRecordAndReturnsToNullState()
        {
            FlowContext flowContext = new FlowContext();
            StateContext stateContext = new StateContext(flowContext);
            StateRunRecord completedRecord = null;
            flowContext.StateRunCompleted += record => completedRecord = record;
            stateContext.ChangeState(new FirstState());

            stateContext.EndCurrentState(StateRunResult.Succeeded, "实验完成");

            Assert.IsInstanceOf<NullState>(flowContext.CurrentState);
            Assert.AreEqual(StateRunResult.Succeeded, completedRecord.Result);
            Assert.AreEqual("实验完成", completedRecord.Note);
            Assert.AreEqual(1, flowContext.StateRunHistory.Count);
        }

        [Test]
        public void CreateSnapshot_DoesNotChangeAfterLaterTransitions()
        {
            FlowContext flowContext = new FlowContext();
            StateContext stateContext = new StateContext(flowContext);
            stateContext.ChangeState(new FirstState());

            FlowSnapshot snapshot = flowContext.CreateSnapshot();
            stateContext.ChangeState(new SecondState());

            Assert.AreEqual(1, snapshot.stateRuns.Count);
            Assert.IsFalse(snapshot.stateRuns[0].isCompleted);
            Assert.AreEqual("first", snapshot.stateRuns[0].stateId);
            Assert.AreEqual("第一步", snapshot.stateRuns[0].displayName);
        }

        [Test]
        public void NullState_IsNotIncludedInBusinessHistory()
        {
            FlowContext flowContext = new FlowContext();
            StateContext stateContext = new StateContext(flowContext);

            stateContext.ChangeState(new NullState());

            Assert.AreEqual(0, flowContext.StateHistory.Count);
            Assert.AreEqual(0, flowContext.StateRunHistory.Count);
        }

        [Test]
        public void HistoryViews_CannotBeModifiedThroughCollectionInterface()
        {
            FlowContext flowContext = new FlowContext();
            StateContext stateContext = new StateContext(flowContext);
            stateContext.ChangeState(new FirstState());

            ICollection<Type> stateHistory = (ICollection<Type>)flowContext.StateHistory;
            ICollection<StateRunRecord> runHistory =
                (ICollection<StateRunRecord>)flowContext.StateRunHistory;

            Assert.IsTrue(stateHistory.IsReadOnly);
            Assert.IsTrue(runHistory.IsReadOnly);
            Assert.Throws<NotSupportedException>(() => stateHistory.Clear());
            Assert.Throws<NotSupportedException>(() => runHistory.Clear());
        }
    }
}
