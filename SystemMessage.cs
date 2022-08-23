using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Pool;
using DG.Tweening;
using GoogleGame;

namespace GoogleGame
{
    public class SystemMessage : MonoBehaviour
    {
        LeanGameObjectPool SysOrigin;
        Transform p_Pos;

        private void Start()
        {
            SysOrigin = GetComponent<LeanGameObjectPool>();
            p_Pos = transform.parent;
        }

        /// <summary>
        /// 테스트용
        /// </summary>
        public void BTN_ShowSysBubbleMsg(string _msg)
        {
            ShowSysBubbleMsg(_msg);
        }



        /// <summary>
        /// 99.캔버스의 시스템 메시지 호출
        /// </summary>
        /// <param name="_msg">텍스트</param>
        /// <param name="_time">지속시간</param>
        internal void ShowSysBubbleMsg(string _msg, float _time = 1.5f)
        {
            var newSysMsg = SysOrigin.Spawn(p_Pos);
            newSysMsg.transform.GetChild(0).GetComponent<Text>().text = _msg;
            //newSysMsg.transform.position = SysOrigin.transform.position;
            newSysMsg.GetComponent<DOTweenAnimation>().DORestart();

            SysOrigin.Despawn(newSysMsg, _time);
        }


    }
}