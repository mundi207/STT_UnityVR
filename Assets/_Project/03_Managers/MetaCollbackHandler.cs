using System;
using Meta.WitAi.Json;
using Oculus.Voice;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class MetaCollbackHandler : MonoBehaviour
{
    [SerializeField] private AppVoiceExperience voiceExperience;
    [SerializeField] private TMP_Text resultText;

    private List<byte> audioBuff = new List<byte>();

    private void OnEnable()
    {
        // 0. 인식 시작
        voiceExperience.VoiceEvents.OnStartListening.AddListener(OnStartListening);
        // 1. 실제 대화(Transcription)가 인식되었을 때
        voiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription); // 중간 결과
        voiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);       // 최종 문장
        
        // 2. 인텐트(Intent) 분석이 완료되었을 때 
        voiceExperience.VoiceEvents.OnResponse.AddListener(OnWitResponse);
        
        // 3. 에러 발생 시
        voiceExperience.VoiceEvents.OnError.AddListener(OnError);
        
        // 4. 음성 인식 종료 시 (성공/실패 무관)
        voiceExperience.VoiceEvents.OnRequestCompleted.AddListener(OnRequestCompleted);

        // 5. 바이트 배열 수집
        voiceExperience.VoiceEvents.OnByteDataReady.AddListener(OnByteDataReady);
    }

    private void OnDisable()
    {
        voiceExperience.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
        voiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartialTranscription);
        voiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);
        voiceExperience.VoiceEvents.OnResponse.RemoveListener(OnWitResponse);
        voiceExperience.VoiceEvents.OnError.RemoveListener(OnError);
        voiceExperience.VoiceEvents.OnRequestCompleted.RemoveListener(OnRequestCompleted);
    }

    // --- 콜백 함수들 ---
    private void OnStartListening()
    {
        audioBuff.Clear(); // 수집한 오디오 청크 초기화
    }

    private void OnPartialTranscription(string text)
    {
        Debug.Log($"인식 중: {text}");
        resultText.text = text;
    }

    private void OnFullTranscription(string text)
    {
        Debug.Log($"최종 인식 문장: {text}");
        resultText.text = text;
    }

    private void OnWitResponse(WitResponseNode response)
    {
        // 인텐트 결과 처리
        if (!string.IsNullOrEmpty(response["intents"][0]["name"]))
        {
            string intentName = response["intents"][0]["name"];
            float confidence = response["intents"][0]["confidence"].AsFloat;
            
            Debug.Log($"분석된 의도: {intentName} (신뢰도: {confidence})");
        }
    }

    private void OnError(string error, string message) => Debug.LogError($"에러 발생: {error} - {message}");

    private void OnRequestCompleted() => Debug.Log("음성 인식 세션이 종료되었습니다.");

    private void OnByteDataReady(byte[] data, int offset, int length)
    {
        byte[] chunk = new byte[length];
        Array.Copy(data, offset, chunk, 0, length);
        audioBuff.AddRange(chunk);
    }
}
