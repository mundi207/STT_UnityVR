using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RecordingManager : Singleton<RecordingManager>
{
    [SerializeField] private float defaultMicCheckTime = 1f;

    private CancellationTokenSource cts;

    // 녹음 대상 마이크 디바이스 이름
    private string curMetaMicDevice;
    private bool isActiveNic;

    public override async UniTask InitializeAsync()
    {
        cts = new CancellationTokenSource();
        // 마이크 인식 시작
        isActiveNic = await OnCheckMicDevice(cts.Token);
    }

    /// <summary>
    /// 현재 인식 된 모든 마이크를 체크 합니다.
    /// </summary>
    public async UniTask<bool> OnCheckMicDevice(CancellationToken token = default)
    {
        float curtime = 0f;

        Debug.Log("마이크 체크 중...");

        while(curtime <= defaultMicCheckTime)
        {
            if(token.IsCancellationRequested) return false;
            curtime += Time.deltaTime;

            foreach(var device in Microphone.devices)
            {
                if(device.Contains("Meta")) // VR과 관련된 마이크만 찾음
                {
                    curMetaMicDevice = device;
                    Debug.Log("마이크 체크 완료");
                    return true;
                }
            }
            await UniTask.Yield();
        }
        Debug.Log("현재 연결된 VR 마이크가 없습니다!");
        return false;
    }
}
