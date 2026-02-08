using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace Samples.Whisper
{
    /// <summary>
    /// Unity의 AudioClip 데이터를 표준 WAV 파일 포맷으로 변환 및 저장하는 유틸리티
    /// </summary>
    public static class SaveWav
    {
        // WAV 헤더 크기는 표준적으로 44바이트입니다.
        private const int HeaderSize = 44;

        /// <summary>
        /// AudioClip을 WAV 바이트 배열로 변환합니다. (Whisper 전달용이나 파일 저장용)
        /// </summary>
        public static byte[] Save(string filename, AudioClip clip)
        {
            if (!filename.ToLower().EndsWith(".wav"))
            {
                filename += ".wav";
            }

            // 저장 경로 설정 (Quest 3의 경우 persistentDataPath는 앱 내부 저장소)
            var filepath = Path.Combine(Application.persistentDataPath, filename);

            // 폴더가 없으면 생성
            Directory.CreateDirectory(Path.GetDirectoryName(filepath) ?? string.Empty);

            using(var memoryStream = CreateEmpty())
            {
                // 1. 오디오 데이터를 16비트 PCM으로 변환하여 작성
                ConvertAndWrite(memoryStream, clip);
                
                // 2. 파일의 앞부분(44바이트)에 WAV 헤더 정보 작성
                WriteHeader(memoryStream, clip);

                // 최종 바이트 배열 반환
                return memoryStream.GetBuffer();
            }
        }

        /// <summary>
        /// 오디오 앞뒤의 무음 구간을 제거하여 Whisper 인식 효율을 높입니다.
        /// </summary>
        public static AudioClip TrimSilence(AudioClip clip, float min)
        {
            var samples = new float[clip.samples];
            clip.GetData(samples, 0); // 오디오 데이터 추출

            return TrimSilence(new List<float>(samples), min, clip.channels, clip.frequency);
        }

        public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool stream = false)
        {
            int i;

            // 1. 앞부분 무음 제거 (임계값 min보다 작은 샘플 스킵)
            for (i = 0; i < samples.Count; i++)
            {
                if (Mathf.Abs(samples[i]) > min) break;
            }
            samples.RemoveRange(0, i);

            // 2. 뒷부분 무음 제거
            for (i = samples.Count - 1; i > 0; i--)
            {
                if (Mathf.Abs(samples[i]) > min) break;
            }
            samples.RemoveRange(i, samples.Count - i);

            // 새로운 클립 생성 및 데이터 세팅
            var clip = AudioClip.Create("TempClip", samples.Count, channels, hz, stream);
            clip.SetData(samples.ToArray(), 0);

            return clip;
        }

        /// <summary>
        /// 헤더가 들어갈 44바이트 자리를 비워둔 메모리 스트림 생성
        /// </summary>
        static MemoryStream CreateEmpty()
        {
            var memoryStream = new MemoryStream();
            byte emptyByte = new byte();

            for (int i = 0; i < HeaderSize; i++)
            {
                memoryStream.WriteByte(emptyByte);
            }

            return memoryStream;
        }

        /// <summary>
        /// Unity의 float(-1.0f ~ 1.0f) 샘플 데이터를 PCM 16비트 정수로 변환
        /// </summary>
        static void ConvertAndWrite(MemoryStream memoryStream, AudioClip clip)
        {
            var samples = new float[clip.samples];
            clip.GetData(samples, 0);

            // float당 2바이트(Int16)가 필요함
            Byte[] bytesData = new Byte[samples.Length * 2];

            // 16비트 오디오의 최대치인 32767을 곱해 정수화
            int rescaleFactor = 32767; 

            for (int i = 0; i < samples.Length; i++)
            {
                short value = (short)(samples[i] * rescaleFactor);
                Byte[] byteArr = BitConverter.GetBytes(value);
                byteArr.CopyTo(bytesData, i * 2);
            }

            memoryStream.Write(bytesData, 0, bytesData.Length);
        }

        /// <summary>
        /// WAV 파일 형식에 필수적인 메타데이터(헤더)를 작성합니다.
        /// </summary>
        static void WriteHeader(MemoryStream memoryStream, AudioClip clip)
        {
            var hz = clip.frequency;
            var channels = clip.channels;
            var samples = clip.samples;

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
            memoryStream.Write(BitConverter.GetBytes(samples * channels * 2), 0, 4);
        }
    }
}