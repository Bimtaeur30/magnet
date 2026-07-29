using Assets._MemberWorkspace.KTJ._02_Script.Agent.Enemy;
using Game.UI;
using GameLib.EventChannelSystem;
using GGMLib.ModuleSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

public class EnemyAgent : ModuleOwner
{
    [SerializeField] private EnemyProfileUIView EnemyProfileUIView;
    [SerializeField] private EventChannelSO enemyEventChannel;
#if UNITY_EDITOR
    [SerializeField] private Transform testAttackStartPoint;
    [SerializeField, Min(0)] private int testAttackDamage = 10;
#endif

    private IHealthModule _healthModule;
    private IStateMachineModule _stateMachineModule;
    private EnemyProfileUIViewModel _profileViewModel;

    protected override void Awake()
    {
        base.Awake();

        Debug.Assert(enemyEventChannel != null, "EnemyAgent의 enemyEventChannel을 할당하세요.", this);
        enemyEventChannel?.AddListener<EnemyAttackEvent>(OnEnemyAttack);
    }

    protected override void InitializeModules()
    {
        base.InitializeModules();
        _healthModule = GetModule<IHealthModule>();
        _stateMachineModule = GetModule<IStateMachineModule>();
    }

    public void InitializeEnemyData(EnemyDataSO enemyDataSO)
    {
        Debug.Assert(EnemyProfileUIView != null, "에너미 에이전트 인스펙터에서 EnemyProfileUIView를 추가하세요.");
        _profileViewModel = EnemyProfileUIView.ViewModel;

        _profileViewModel.HealthMaxValue = enemyDataSO.MaxHealth;
        _profileViewModel.Health = enemyDataSO.MaxHealth;
        _profileViewModel.HealthMinValue = 0;
        _profileViewModel.NameTxt = "[ " + enemyDataSO.EnemyName + " ]";
        _profileViewModel.No = "NO." + enemyDataSO.NoNumber;

        _healthModule.InitializeData(enemyDataSO);
        _stateMachineModule.StartStateMachine();
    }

    private void OnDestroy()
    {
        enemyEventChannel?.RemoveListener<EnemyAttackEvent>(OnEnemyAttack);
    }

    private void OnEnemyAttack(EnemyAttackEvent attackEvent)
    {
        _healthModule.Damage(attackEvent.Damage);

        if (_profileViewModel != null)
            _profileViewModel.Health = _healthModule.CurrentHealth;

        if (_healthModule.CurrentHealth > 0)
            _stateMachineModule.ChangeState(EnemyStateId.Damage);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Keyboard.current?.tKey.wasPressedThisFrame == true)
        {
            Vector3 attackStartWorldPosition = testAttackStartPoint != null
                ? testAttackStartPoint.position
                : transform.position + Vector3.left * 5f;

            enemyEventChannel.RaiseEvent(
                EnemyEvents.EnemyAttackRequestEvent.Init(
                    attackStartWorldPosition,
                    testAttackDamage));
        }
    }
#endif

    [ContextMenu("테스트 죽음")]
    private void TestDead()
    {
        _stateMachineModule.ChangeState(EnemyStateId.Dead);
    }
}


// 에너미 에이전트가 에너미 데이터를 가지고 있어야 헬스 모듈이 그걸 참조해서 세팅할 수 있다. 
// 그럼 데이터가 에이전트를 들고있을 필요가 없다.
