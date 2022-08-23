using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using GoogleGame;

namespace GoogleGame
{
    public class NormalCardITrashZone : MonoBehaviour, IDropHandler
    {

        [Header(" - SP 에 접근할 배틀 컨트롤러")]
        [SerializeField] BattleController bc;
        [Header(" - 드롭되는 카드 정보")]
        [SerializeField] NormalCardIDaggable daggableCard;
        [SerializeField] CardGenerator cardGenerator;
        [Space]
        [SerializeField] Image _image;

        /// <summary>
        /// ture = 드래그 중에는 레이캐스트를 활성화
        /// false = 드래그 아닌 중에서는 비활성화 해서 해당 버튼이 클릭 가능하게
        /// </summary>
        internal void OnRaycastActive(bool swich)
        {
            _image.enabled = swich;
            //_image.raycastTarget = swich;
        }


        /// <summary>
        /// 카드 버리면 SP 돌려줌
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrop(PointerEventData eventData)
        {
            //Debug.LogError("카드 버릴래");

            /// 카드 버리면 SP 돌려줌
            bc.RunHalfSpCount();
            /// 카드 버려
            cardGenerator.RemoveOwned3CardPool(daggableCard.GetRemovedCard(out int posIndex), posIndex);
        }
    }
}