using UnityEngine;
using System.Collections.Generic;
using Akasha;
using System;

public interface ITestCase
{
    string Name { get; }
    void Run();
}

public class RxFrameworkTester : MonoBehaviour
{
    private List<ITestCase> testCases = new();

    private void Awake()
    {
        testCases.Add(new RxVarTest());
        testCases.Add(new RxExprTest());
        testCases.Add(new RxFlagTest());
        testCases.Add(new RxListTest());
        testCases.Add(new RxTriggerTest());
        testCases.Add(new RxStateMachineTest());
        // ... 필요 시 추가
    }

    private void Start()
    {
        foreach (var test in testCases)
        {
            Debug.Log($"[TEST] {test.Name} 시작");
            try { test.Run(); }
            catch (System.Exception e)
            {
                Debug.LogError($"[TEST ERROR] {test.Name} 실패: {e.Message}");
            }
        }

        Debug.Log($"[TEST] 모든 테스트 완료 ({testCases.Count}건)");
    }
}
public class RxVarTest : ITestCase, IInteractLogicalSubscriber
{
    public string Name => "RxVar";

    public void Run()
    {
        var var = new RxVar<int>(0, this);
        int observed = -1;

        var.Bind(v => {
            observed = v;
            Debug.Log($"[RxVarTest] observed = {v}");
        }, this, RxType.Logical);

        var.SetValue(42, this);
        RxQueue.ExecuteAll();
        if (observed != 42)
            throw new System.Exception("RxVar가 정상적으로 값을 전달하지 못함");
    }
}
public class RxExprTest : ITestCase, IFunctionalSubscriber
{
    public string Name => "RxExpr";

    public void Run()
    {
        var a = new RxVar<int>(2, this);
        var b = new RxVar<int>(3, this);
        var sum = new RxExpr<int>(() => a.Value + b.Value, a, b);

        int observed = -1;

        sum.Bind(v => {
            observed = v;
        }, this, RxType.Functional);

        a.SetValue(10, this);
        RxQueue.ExecuteAll();
        if (observed != 13)
            throw new System.Exception("RxExpr 값이 제대로 계산되지 않음");
        var hp = new RxVar<float>(1f, this);
        var expr = new RxExpr<bool>(() => hp.Value < 0.3f, hp);

        bool? exprResult = null;
        expr.Bind(v =>
        {
            exprResult = v;
            Debug.Log($"[RxExprTest] expr = {v}");
        }, this, RxType.Functional);

        hp.SetValue(0.2f, this);
        RxQueue.ExecuteAll();

        if (exprResult != true)
            throw new Exception("RxExpr (bool 조건식)가 반응하지 않음");
    }
}
public class RxFlagTest : ITestCase, IFunctionalSubscriber
{
    public string Name => "RxFlag";

    public void Run()
    {
        var owner = new DummyModel(); // ✅ 소유자 권한 확보
        var hp = new RxVar<float>(1f, owner);
        var isLow = new RxFlag(() => hp.Value < 0.3f, owner, hp);

        bool? flagState = null;

        isLow.Bind(v => {
            flagState = v;
        }, this, RxType.Functional);

        hp.SetValue(0.2f, owner);
        RxQueue.ExecuteAll(); // ✅ 반드시 큐 실행

        if (flagState != true)
            throw new Exception("RxFlag 조건이 반영되지 않음");

    }

    private class DummyModel : IRxModel { } // 최소 권한만 구현
}
public class RxListTest : ITestCase, IInteractLogicalSubscriber
{
    public string Name => "RxList";

    public void Run()
    {
        var list = new RxList<string>();
        List<string> current = null;

        list.Bind(v => {
            current = v;
        }, this, RxType.Logical);

        list.Add("Hello");
        RxQueue.ExecuteAll();
        if (current == null || !current.Contains("Hello"))
            throw new System.Exception("RxList 구독자에게 항목이 반영되지 않음");
    }
}
public class RxTriggerTest : ITestCase, IUnfiniteTriggerSubscriber
{
    public string Name => "RxTrigger";

    public void Run()
    {
        var trigger = new RxTrigger();
        bool wasCalled = false;

        trigger.Bind(() => {
            wasCalled = true;
        }, this);

        trigger.Raise();

        RxQueue.ExecuteAll(); // 큐 안에서 실행되므로 직접 실행

        if (!wasCalled)
            throw new System.Exception("RxTrigger 콜백이 호출되지 않음");
    }
}

public class RxStateMachineTest : ITestCase, IInteractLogicalSubscriber, IFunctionalSubscriber
{
    public string Name => "RxStateMachine";

    public void Run()
    {
        var machine = new TestStateMachine(this);

        bool wasRun = false;

        machine.ActiveState.Bind(state =>
        {
            Debug.Log($"[Test] 상태 변경됨: {state}");
            if (state == TestState.Run) wasRun = true;
        }, this, RxType.Functional);

        // 조건이 false일 때 상태 전이 실패해야 함
        machine.Request(TestState.Run);
        RxQueue.ExecuteAll();

        if (machine.ActiveState.Value == TestState.Run)
            throw new Exception("조건이 false인데도 Run 상태로 전이되었습니다");

        // 조건을 만족시키고 재요청
        machine.AllowRun = true;
        machine.Request(TestState.Run);
        RxQueue.ExecuteAll();

        if (machine.ActiveState.Value != TestState.Run || !wasRun)
            throw new Exception("Run 상태 전이가 실패했습니다");
    }

    public enum TestState
    {
        Idle,
        Run,
        Act
    }

    public class TestStateMachine : RxStateMachine<TestState>
    {
        public bool AllowRun = false;

        public TestStateMachine(object owner) : base(owner, TestState.Idle)
        {
            Register(TestState.Idle);
            Register(TestState.Run, () => AllowRun);
            Register(TestState.Act);
        }
    }
}

