using Cysharp.Threading.Tasks;
using UnityEngine;
 
/// <summary>
/// 앱 시작 
/// </summary>
public class AppBootStrap : Singleton<AppBootStrap>
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override UniTask InitializeAsync()
    {
        return base.InitializeAsync();

        // 매니저들 초기화 로직 추가
    }
}
