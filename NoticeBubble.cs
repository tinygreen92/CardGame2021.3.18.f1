using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleGame;

namespace GoogleGame
{
    public class NoticeBubble : MonoBehaviour
    {
        Image SysMsgImage;
        Text SysMsgText;

        private void Awake()
        {
            SysMsgImage = GetComponent<Image>();
            SysMsgText = GetComponentInChildren<Text>();
        }

        /// <summary>
        /// 스폰할때 호출하자
        /// </summary>
        private void OnEnable()
        {
            SysMsgText.text = "";
        }


    }
}