using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GoogleGame
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [SerializeField] Sound[] sfx = null;
        [SerializeField] Sound[] bgm = null;

        [SerializeField] AudioSource bgmPlayer = null;
        [SerializeField] AudioSource[] sfxPlayer = null;

        private void Awake()
        {
            Instance = this;
        }

        internal void PlayBGM(string p_bgmName)
        {
            for (int i = 0; i < bgm.Length; i++)
            {
                if (p_bgmName == bgm[i].name)
                {
                    bgmPlayer.clip = bgm[i].clip;
                    bgmPlayer.Play();
                }
            }
        }

        internal void StopBGM()
        {
            bgmPlayer.Stop();
        }

        internal void PlaySFX(string p_sfxName)
        {
            for (int i = 0; i < sfx.Length; i++)
            {
                if (p_sfxName == sfx[i].name)
                {
                    for (int j = 0; j < sfxPlayer.Length; j++)
                    {
                        Debug.LogWarning($"{j}/{sfxPlayer.Length} 시도중");
                        // SFXPlayer에서 재생 중이지 않은 Audio Source를 발견했다면 
                        if (!sfxPlayer[j].isPlaying)
                        {
                            sfxPlayer[j].clip = sfx[i].clip;
                            sfxPlayer[j].Play();
                            return;
                        }
                    }

                    Debug.LogError("모든 오디오 플레이어가 재생중입니다.");
                    return;
                }
            }

            Debug.LogError(p_sfxName + " 이름의 효과음이 없습니다.");
        }
    }
}