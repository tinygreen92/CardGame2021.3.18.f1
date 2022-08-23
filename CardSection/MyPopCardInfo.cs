using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleGame;

namespace GoogleGame
{
    public class MyPopCardInfo : MonoBehaviour
    {
        [SerializeField] MyCardDeckPanel myCardDeckPanel;
        [Header(" - 클릭 가능한 5개 슬롯")]
        [SerializeField] Transform[] popSlots;
        [Space]
        [SerializeField] Image[] cardImg;
        [SerializeField] Text[] LevelText;
        [SerializeField] Text[] CardPrices;

        private void OnValidate()
        {
            for (int i = 0; i < popSlots.Length; i++)
            {
                cardImg[i] = popSlots[i].GetChild(0).GetComponent<Image>();
                LevelText[i] = popSlots[i].GetChild(0).GetChild(0).GetComponent<Text>();
                CardPrices[i] = popSlots[i].GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>();
            }
        }


        /// <summary>
        /// 현재 장착된 카드 정보 불러와서 뿌려주기
        /// </summary>
        internal void RefreshCardInfo()
        {
            var spriteIcon = myCardDeckPanel.GetEquipItmeIcon();
            var levels = myCardDeckPanel.GetCardPopInfo(0);
            var prices = myCardDeckPanel.GetCardPopInfo(1);

            for (int i = 0; i < 5; i++)
            {
                cardImg[i].enabled = true;
                cardImg[i].sprite = spriteIcon[i];
                LevelText[i].text = levels[i];
                CardPrices[i].text = prices[i];
            }
        }

    }
}