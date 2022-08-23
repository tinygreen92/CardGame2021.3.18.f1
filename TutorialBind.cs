using System.Collections;
using System.Collections.Generic;
using GoogleGame;
using UnityEngine;

namespace GoogleGame
{
    public class TutorialBind : PopBind
    {
        [SerializeField] private RectTransform mask, backgnd;
        
        // public RectTransform GetMyRectTransform()
        // {
        //     return transform as RectTransform;
        // }
        
        /// <summary>
        /// 튜토리얼 매니저에서 불러오는 튜토리얼 팝 세팅
        /// </summary>
        /// <returns></returns>
        public bool InitTutorialBind(Vector2 size, Vector3 pos)
        {
            // 팝에서 세팅하는 것은 구멍 뚫린 마스크의 위치와 크기
            // 마스크 자식의 이미지 위치와 크기
            
            // 구멍 뚫린 마스크
            mask.sizeDelta = size + new Vector2(30, 30);
            mask.position = Camera.main.WorldToScreenPoint(pos);

            return true;
        }

        public void InitCanvasBind(Vector2 size, Vector3 pos)
        {
            // 뒷쪽 불투명 백그라운드
            backgnd.sizeDelta = size;
            backgnd.position = pos;
            
            TurnOn();
        }

    }
}


