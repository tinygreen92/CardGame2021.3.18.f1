using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using GoogleGame;
using Lean.Pool;
using UnityEngine;

namespace GoogleGame
{
    /// <summary>
    /// 1. HasKey("IsOldUser") 존재 안한다 
    ///     -> 계약서 읽기 전이다.
    ///         -> 계약서 팝업 생성
    /// 2. HasKey("IsOldUser") 존재하고, IsOldUser = 0 이다
    ///     -> 계약서는 읽었고, 튜토리얼 완료 안했다.
    ///         -> 계약서 팝업 생성 없이 튜토리얼 처음부터 진행
    /// 3. HasKey("IsOldUser") 존재하고, IsOldUser = 1 이다
    ///     -> 계약서도 읽었고, 튜토리얼도 완료했다. 
    ///         -> 계약팝업 && 튜토리얼 다 스킵하고 바로 메인씬으로 넘겨
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [Header("-해당 캔버스 크기 = 검은화면의 크기")] [SerializeField]
        RectTransform canvasRectTransform;

        [Header("-인스턴스로 불러올 튜토 팝업들")] [SerializeField]
        TutorialBind[] popBinds;

        [Space] 
        [SerializeField] RectTransform[] tutorialObjects;
        [SerializeField] RectTransform[] tutorialObjects2;

        /// <summary>
        /// 기기 해상도 대응을 위해 튜토리얼 초기화시 그때 사이즈를 세팅한다.
        /// </summary>
        public void GetTutorialObjects()
        {
            SetTutorialBind(tutorialObjects[0]);
        }

        /// <summary>
        /// 포커스할 버튼의 RectTransform를 넘겨준다.
        /// </summary>
        /// <param name="rect"></param>
        void SetTutorialBind(RectTransform rect)
        {
            TutorialBind tmp = LeanPool.Spawn(popBinds[0]);
            tmp.transform.SetParent(canvasRectTransform, false);
            
            if (tmp.InitTutorialBind(rect.sizeDelta, rect.position))
            {
                tmp.InitCanvasBind(canvasRectTransform.sizeDelta, canvasRectTransform.position);
            }
        }

        private void OnEnable()
        {
            if (popBinds.Length == 0)
            {
                Debug.LogError("튜토리얼 팝업 프리팹이 없습니다.");
                return;
            }

            // PopBind tmp = LeanPool.Spawn(popBinds[0]);
            // tmp.transform.SetParent(transform, false);
            // Debug.LogWarning("튜토리얼 매니저 OnEnable");
        }


        // 튜토리얼 진행도를 저장하는 변수
        private ObscuredInt TutorialProgress
        {
            get
            {
                if (PlayerPrefs.HasKey("TutorialProgress"))
                {
                    return PlayerPrefs.GetInt("TutorialProgress");
                }
                else
                {
                    PlayerPrefs.SetInt("TutorialProgress", 0);
                    PlayerPrefs.Save();
                    return 0;
                }
            }
            set
            {
                PlayerPrefs.SetInt("TutorialProgress", value);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// HasKey("IsOldUser") 존재하고, IsOldUser = 0 이면 튜토리얼 이어서 진쟁
        /// </summary>
        public void CheckTutorial()
        {
            if (TutorialProgress == 0)
            {
                // 튜토리얼 진행도가 0이면 튜토리얼 첫 시작
                StartTutorial();
                return;
            }

            // 튜토리얼 진행도가 0이 아니면 튜토리얼 이어서 진행
            ContinueTutorial(TutorialProgress);
        }

        /// <summary>
        /// 튜토리얼 첫 실행
        /// </summary>
        private void StartTutorial()
        {
            // 튜토리얼 진행도를 하나 올린다.
            TutorialProgress = 1;
            // 튜토리얼 오브젝트를 켜준다.
        }

        /// <summary>
        /// 종료 전 이전단계부터 이어서 진행
        /// </summary>
        /// <param name="Progress"></param>
        private void ContinueTutorial(int Progress)
        {
        }

        /// <summary>
        /// 전달 인자 없으면 튜토리얼 완료하고 다음 단계로 넘어간 것이다.
        /// </summary>
        private void ContinueTutorial()
        {
            TutorialProgress += 1;
            // TODO : 
        }
    }
}