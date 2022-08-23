using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleGame;
using TMPro;

namespace GoogleGame
{
    public class ItemTooltip : MonoBehaviour
    {
        [SerializeField] MyPopCardInfo myPopCardInfo;
        [SerializeField] PopBind popobject;
        [Space]
        [SerializeField] GameObject popCardPannel;
        [SerializeField] GameObject cardInfoPanel;
        [Space]
        [SerializeField] Image ItemIcon;
        [SerializeField] Text ItemName;
        [SerializeField] Text ItemDesc;
        [Space]
        [SerializeField] TMP_Text[] Abilities;      // 능력 설명 + 수치 MAX 6
        //[SerializeField] Text[] AbilityStat;    // 능력 수치 MAX 6
        [SerializeField] Image[] AbIcon;      // 어빌리티 아이콘

        private void OnValidate()
        {
            for (int i = 0; i < Abilities.Length; i++)
            {
                AbIcon[i] = Abilities[i].GetComponentInChildren<Image>();
            }
        }

        
        /// <summary>
        /// 아이템 슬롯 클릭하면 사용하기 팝업 나오게
        /// </summary>
        /// <param name="item">스크립테이블에서 정의된 아이템</param>
        /// <param name="list">DataBox에서 받아오는 데이터</param>
        internal void ShowTooltip(EquippableItem item, List<string> list)
        {
            /// 속패널 제어
            popCardPannel.SetActive(false);
            cardInfoPanel.SetActive(true);

            ItemIcon.sprite = item.Icon;
            ItemName.text = item.ItemName;

            ItemDesc.text = list[2];

            /// TODO : 카드 인벤토리 상세 정보 채워넣기

            for (int i = 0; i < Abilities.Length; i++)
            {
                Abilities[i].text = $"{list[i + 5]} : {list[i + 11]}";
            }

            myPopCardInfo.RefreshCardInfo();
            /// 팝업 표기
            popobject.ShowThisPop();
        }

        /// <summary>
        /// [Btn]툴팁 팝업에서 사용하기 버튼 누르면
        /// 교체 카드 고르셈 화면으로 넘어감.
        /// </summary>
        public void BTN_ClickedToolUseBtn()
        {
            /// 속패널 제어
            popCardPannel.SetActive(true);
            cardInfoPanel.SetActive(false);
        }


        internal void HideThisPop()
        {
            popobject.HideThisPop();
        }


    }
}