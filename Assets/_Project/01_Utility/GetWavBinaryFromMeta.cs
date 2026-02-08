using System;
using System.Collections.Generic;
using System.IO;
using Meta.Voice.Hub.Content;
using UnityEngine;

public class GetWavBinaryFromMeta
{
    public static List<Byte[]> AudioWavDatas = new List<byte[]>();
    public static List<AudioClip> AudioClips = new List<AudioClip>();
 
    public static void PCM16ToWav(MemoryStream memoryStream, byte[] chunkdata)
    {
        BinaryToWav(memoryStream, chunkdata);
    }

    public static AudioClip PCM16ToAudioClip(byte[] chunkdata, int channels, int hz)
    {
        float[] samples = new float[chunkdata.Length / 2]; // AudioClip에서는 한 샘플이 4byte. 따라서 pcm 바이트 배열의 /2를 함

        for(int i = 0; i < samples.Length; i++)
        {
            short shortVal = BitConverter.ToInt16(chunkdata, i * 2); // 
            samples[i] = shortVal / 32767f; // -1f, 1f로 정규화 시도.
        }
        int sampleCount = samples.Length / channels; // 채널 개수까지 모두 고려한 크기이므로 채널 수만큼 나눔

        AudioClip clip = AudioClip.Create("AudioClip", sampleCount, channels, hz, false);
        return clip;
    }
    
    public static void BinaryToWav(MemoryStream memoryStream, byte[] chunkdata)
    {
        byte[] headerPlace = new byte[44];
        memoryStream.Write(headerPlace, 0, 44);

        memoryStream.Write(chunkdata, 0, chunkdata.Length);
        WriteHeader(memoryStream, chunkdata.Length, 16000, 1);

        AudioWavDatas.Add(memoryStream.ToArray());
    }

    public static void WriteHeader(MemoryStream memoryStream, int datalength, int hz, int channels)
    {
        // 스트림 시작점으로 이동하여 헤더 기록 시작
        memoryStream.Seek(0, SeekOrigin.Begin);

        // RIFF 식별자
        memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        // 파일 크기 계산
        memoryStream.Write(BitConverter.GetBytes(memoryStream.Length - 8), 0, 4);
        // WAVE 식별자
        memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        // 포맷 블록 식별자
        memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        // 포맷 크기 (16)
        memoryStream.Write(BitConverter.GetBytes(16), 0, 4);
        // 오디오 포맷 (1 = PCM)
        memoryStream.Write(BitConverter.GetBytes((UInt16)1), 0, 2);
        // 채널 수
        memoryStream.Write(BitConverter.GetBytes((UInt16)channels), 0, 2);
        // 샘플 레이트 (예: 44100)
        memoryStream.Write(BitConverter.GetBytes(hz), 0, 4);
        // 바이트 레이트 (초당 전송량)
        memoryStream.Write(BitConverter.GetBytes(hz * channels * 2), 0, 4);
        // 블록 정렬 (채널 수 * 샘플당 바이트)
        memoryStream.Write(BitConverter.GetBytes((UInt16)(channels * 2)), 0, 2);
        // 샘플당 비트 수 (16-bit)
        memoryStream.Write(BitConverter.GetBytes((UInt16)16), 0, 2);
        // 데이터 블록 식별자
        memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        // 실제 오디오 데이터 크기
        memoryStream.Write(BitConverter.GetBytes(datalength), 0, 4);
    }
    
    public static void ClearMemory()
    {
        AudioWavDatas.Clear();
        AudioClips.Clear();
    }
}
