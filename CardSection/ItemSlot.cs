using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GoogleGame;
using Lean.Gui;
using Lean.Transition;
using UnityEngine.Serialization;

namespace GoogleGame
{
    public class ItemSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] InventoryManager inventoryManager;
        [Space]
        [SerializeField] Image image;
        [Space]
        [SerializeField] Text LevelText;
        [FormerlySerializedAs("CardPrices")] [SerializeField] Text CardPieces;
        [Space]

        private float holdTime = 0.3f;
        private bool isDragging = false;
        private bool isFingerDown = false;
        private bool held = false;

        public event Action<ItemSlot> OnDown;
        public event Action<ItemSlot> OnUP;
        public event Action<ItemSlot> OnClickedDown;
        public event Action<ItemSlot> OnBeginDragEvent;
        public event Action<ItemSlot> OnEndDragEvent;
        public event Action<ItemSlot> OnDragEvent;


        [SerializeField] Item _item;
        public Item Item
        {
            get { return _item; }
            set
            {
                _item = value;
                if (_item == null)  /// 해당 카드가 미획득일때
                {
                    //image.enabled = false;
                    image.color = Color.gray;
                    LevelText.enabled = false;
                    CardPieces.enabled = false;
                }
                else  /// 해당 카드가 획득되어 (레벨, 카드조각)이 보이는 상태.
                {
                    //image.enabled = true;
                    image.sprite = _item.Icon;
                    image.color = Color.white;
                    LevelText.enabled = true;
                    CardPieces.enabled = true;
                    /// 데이터박스 정보로 새로고침
                    RefreshItemInfo(_item.ItemIndex);
                }
            }
        }

        protected virtual void OnValidate()
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }

            if (inventoryManager == null)
            {
                inventoryManager = FindObjectOfType<InventoryManager>();
            }
        }



        public virtual bool CanReceiveItem(Item item)
        {
            return true;
        }

        /// <summary>
        /// 데이터 박스에 접근해서 세부정보 빼오기
        /// </summary>
        /// <param name="index">해당 카드 인덱스</param>
        void RefreshItemInfo(int index)
        {
            /// __DATA2__ 카드정보 가져오기
            List<string> list = inventoryManager.GetCardBaseInfo(index);
            CardUpgradeTable cut = new CardUpgradeTable();

            LevelText.text = $"Lv. {list[0]}";  // 데이터 박스  (레벨)
            CardPieces.text = $"{list[1]} / {cut.GetUpgradePieces(int.Parse(list[0]))}"; // 데이터 박스 (보유 조각 수)

            //Debug.LogWarning($"인덱스 {index} 인 카드다.");
        }


        /// <summary>
        /// 유니티 텍스트 내용 읽어서 바로 리턴
        /// </summary>
        /// <param name="intCase">0이면 레벨 1이면 카드조각수 반환</param>
        /// <returns></returns>
        public string GetLevelAndPrices(int intCase)
        {
            if (intCase == 0)
            {
                return LevelText.text;
            }
            else
            {
                return CardPieces.text;
            }
        }

        /// <summary>
        ///  포인트 다운되면 재생합니다.
        /// </summary>
        private void DownAnimating()
        {
            
        }

        #region <터치 & 드래그>

        public void OnPointerDown()
        {
            /// 미획득 감지
            if (image.color != Color.white) return;

            Debug.Log("포인트 다운 ");

            held = false;
            isFingerDown = true;
            Invoke(nameof(OnLongPress), holdTime);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            /// 미획득 감지
            if (image.color != Color.white) return;

            Debug.Log("포인트 다운 ");

            held = false;
            isFingerDown = true;
            Invoke(nameof(OnLongPress), holdTime);
        }
        internal virtual void OnLongPress()
        {
            // 04-25 기준 0.3초
            Debug.Log($"Long Down : {holdTime}초 드래깅 시작");

            OnDown?.Invoke(this);

            held = true;
            isDragging = true;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isFingerDown) return;
            //Debug.Log("포인트 Exit");

            CancelInvoke(nameof(OnLongPress));
            isFingerDown = false;
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            /// 미획득 감지
            if (image.color != Color.white) return;

            Debug.Log("포인트 업");

            if (OnUP != null)
            {
                OnUP(this);
            }

            CancelInvoke(nameof(OnLongPress));
            /// holdTime 유지하지 않고 손가락 뗐다면 -> 단일 클릭
            if (!held && isFingerDown)
            {
                if (OnClickedDown != null)
                {
                    OnClickedDown(this);
                }
            }

            isDragging = false;
            isFingerDown = false;
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            CancelInvoke(nameof(OnLongPress));
            /// 인벤토리 스크롤
            inventoryManager.invenSV.OnBeginDrag(eventData);
            held = true;
            /// 롱 다운 통과시에만 드래그 시작
            if (isDragging)
            {
                inventoryManager.invenSV.vertical = false;

                if (OnBeginDragEvent != null)
                {
                    OnBeginDragEvent(this);
                }
            }
        }
        public void OnDrag(PointerEventData eventData)
        {
            /// 롱 클릭후 드래그가 아니면
            ///     -> 스크롤뷰 드래그 해주고
            if (!isDragging)
            {
                inventoryManager.invenSV.OnDrag(eventData);
            }
            else
            {
                if (OnDragEvent != null)
                {
                    OnDragEvent(this);
                }
            }
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("엔드 드래그");
            inventoryManager.invenSV.OnEndDrag(eventData);

            isDragging = false;
            inventoryManager.invenSV.vertical = true;

            if (OnEndDragEvent != null)
            {
                OnEndDragEvent(this);
            }
        }

        #endregion
    }
}