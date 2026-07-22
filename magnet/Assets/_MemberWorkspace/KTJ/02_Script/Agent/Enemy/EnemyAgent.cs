using Assets._MemberWorkspace.KTJ._02_Script.Agent.Enemy;
using Game.UI;
using GGMLib.ModuleSystem;
using UnityEngine;

public class EnemyAgent : ModuleOwner
{
    [SerializeField] private EnemyProfileUIView EnemyProfileUIView;
    private IHealthModule _healthModule;
    private EnemyProfileUIViewModel _profileViewModel;

    protected override void InitializeModules()
    {
        base.InitializeModules();
        _healthModule = GetModule<IHealthModule>();
        //_profileViewModel.Health = ;
    }

    private void Start()
    {
        Debug.Assert(EnemyProfileUIView != null, "에너미 에이전트 인스펙터에서 EnemyProfileUIView를 추가하세요.");
        _profileViewModel = EnemyProfileUIView.ViewModel;
    }

    public void InitializeEnemyData(EnemyDataSO enemyDataSO)
    {
        _profileViewModel.HealthMaxValue = enemyDataSO.MaxHealth;
        _profileViewModel.Health = enemyDataSO.MaxHealth;
        _profileViewModel.HealthMinValue = 0;
        _profileViewModel.NameTxt = "[ " + enemyDataSO.EnemyName + " ]";
        _profileViewModel.No = "NO." + enemyDataSO.NoNumber;
    }
}


// 에너미 에이전트가 에너미 데이터를 가지고 있어야 헬스 모듈이 그걸 참조해서 세팅할 수 있다. 
// 그럼 데이터가 에이전트를 들고있을 필요가 없다.