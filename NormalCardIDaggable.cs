using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleGame;

namespace GoogleGame
{
    public class NormalCardIDaggable : MonoBehaviour
    {

        [Header(" - 트래쉬 존 활성화")]
        public NormalCardITrashZone nctz;
        [Header(" - 현재 오브젝트 자식")]
        [SerializeField] Text _numText;
        [SerializeField] Image _thisImage;
        /// <summary>
        /// NormalCard 클래쓰~
        /// </summary>
        private NormalCard _DeepCopyCard;
        private NormalCard _OriginCard;

        /// <summary>
        /// 총알 업데이트 용
        /// </summary>
        /// <returns></returns>
        internal NormalCard GetDeepCard() => _DeepCopyCard;

        /// <summary>
        /// 카드 풀에서 제거용
        /// </summary>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        internal NormalCard GetRemovedCard(out int posIndex)
        {
            DestroyDaggable();

            posIndex = _posIndex;
            if (_OriginCard != null)
            {
                return _OriginCard;
            }
            return null;
        }

        /// <summary>
        /// 얘가 소환된 위치
        /// </summary>
        private int _posIndex;

        /// <summary>
        /// 드래그 시작하는 카드 색상 복사한 후 트래쉬존 활성화.
        /// </summary>
        /// <param name="origCard"></param>
        internal void InitDaggable(NormalCard origCard, NormalCard deepCopyCard, int posIndex)
        {
            /// _OriginCard 는 사라져야 할 카드
            _OriginCard = origCard;
            _DeepCopyCard = deepCopyCard;

            /// 인덱스
            _posIndex = posIndex;
            _numText.text = _DeepCopyCard.numType;

            switch (_DeepCopyCard.colorType)
            {
                case "Red":
                    _numText.color = new Color(244 / 255f, 67 / 255f, 54 / 255f);
                    break;
                case "Yellow":
                    _numText.color = new Color(253 / 255f, 216 / 255f, 53 / 255f);
                    break;
                case "Blue":
                    _numText.color = new Color(33 / 255f, 150 / 255f, 243 / 255f);
                    break;
                case "Orange":
                    _numText.color = new Color(255 / 255f, 152 / 255f, 0);
                    break;
                case "Purple":
                    _numText.color = new Color(156 / 255f, 39 / 255f, 176 / 255f);
                    break;
                case "Green":
                    _numText.color = new Color(76 / 255f, 175 / 255f, 80 / 255f);
                    break;
                default:
                    break;
            }

            /// TODO : 스프라이트 받아 올 것.
            // _thisImage.sprite = spr;

            gameObject.SetActive(true);
            nctz.OnRaycastActive(true);


        }

        /// <summary>
        /// 위에 떨어지는 카드 정보 반환하기.
        /// </summary>
        internal NormalCard GetDraggingInfo()
        {
            return _DeepCopyCard;
        }




        /// <summary>
        /// 드래그 아이템과 트레쉬 존을 비활성화한다.
        /// </summary>
        internal void DestroyDaggable()
        {
            gameObject.SetActive(false);
            nctz.OnRaycastActive(false);
        }
    }
}