using System;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using GoogleGame;

namespace GoogleGame
{
    public class MyCardDeckPanel : MonoBehaviour
    {
        [Header("기본 equipBattlePanel")]
        [SerializeField] Transform equipmentSlotsParent;
        [SerializeField] EquipmentSlot[] equipmentSlots;



        public event Action<ItemSlot> OnClickedDown;
        //public event Action<ItemSlot> OnBeginDragEvent;
        //public event Action<ItemSlot> OnEndDragEvent;
        //public event Action<ItemSlot> OnDragEvent;
        public event Action<EquipmentSlot> OnDropEvent;

        public void Start()
        {
            var length = equipmentSlots.Length;
            for (int i = 0; i < length; i++)
            {
                equipmentSlots[i].OnClickedDown += OnClickedDown;
                //equipmentSlots[i].OnBeginDragEvent += OnBeginDragEvent;
                //equipmentSlots[i].OnEndDragEvent += OnEndDragEvent;
                //equipmentSlots[i].OnDragEvent += OnDragEvent;
                equipmentSlots[i].OnDropEvent += OnDropEvent;
            }
        }

        private void OnValidate()
        {
            equipmentSlots = equipmentSlotsParent.GetComponentsInChildren<EquipmentSlot>();
        }


        /// <summary>
        /// 2번째 [전투 화면]에서 카드 추가시에만 사용됨!! 주의!!!
        /// -> 초기 로딩때는 ]인벤토리 화면]에서도 사용됨!!
        /// </summary>
        /// <param name="item"></param>
        /// <param name="slotindex"></param>
        /// <returns></returns>
        public bool AddItem(EquippableItem item, int slotindex)
        {
            equipmentSlots[slotindex].Item = item;
            Debug.Log($"AddItem 전투화면 장착 카드 인덱스 <{item.ItemIndex}>");
            return true;
        }

        /// <summary>
        /// [획득된 카드]중에서 내 덱에 추가한다.
        /// </summary>
        /// <param name="item">추가할 카드</param>
        /// <param name="previousItem">덱에 원래 있던 카드</param>
        /// <returns></returns>
        public bool AddItem(EquippableItem item, int slotindex, out EquippableItem previousItem)
        {
            previousItem = (EquippableItem)equipmentSlots[slotindex].Item;

            if (IsSameCardinDeck(item.ItemIndex, slotindex) && !ReferenceEquals(previousItem, null))
            {
                Debug.LogWarning("같은 카드 있음!");
                return false;
            }

            /// 드롭하는 카드가 같은 카드가 아니라면 교체해줌.
            if (item != previousItem)
            {
                equipmentSlots[slotindex].Item = item;
                /// 드롭으로 슬롯에 카드 장착시 해당 정보 저장
                ObscuredInt[] cardSaverData = MyDeckSaver.SingleCardSaver;
                cardSaverData[slotindex] = item.ItemIndex;
                MyDeckSaver.SingleCardSaver = cardSaverData;

                return true;
            }
            else if (item == previousItem)
            {
                previousItem = null;
                Debug.LogWarning("해당 슬롯은 같은 카드 장착중임!");
                return false; /// 이미 장착중이면 false
            }
            else
            {
                Debug.LogWarning("previousItem 이 Null");
                return false; /// 이미 장착중이면 false
            }

        }

        /// <summary>
        /// [InventoryManager]에서 드롭으로 Item 바꿔주면 [전투화면]의 카드덱 새로고침 해준다.
        /// </summary>
        /// <param name="draggedItem"></param>
        /// <param name="slotindex"></param>
        internal bool RefreshBattleUI(Item draggedItem, int slotindex)
        {
            /// 덱에 같은 카드가 있는게 아니다.
            if (!IsSameCardinDeck(draggedItem.ItemIndex, slotindex))
            {
                equipmentSlots[slotindex].Item = draggedItem;
                /// 드롭으로 슬롯에 카드 장착시 해당 정보 저장
                ObscuredInt[] cardSaverData = MyDeckSaver.SingleCardSaver;
                cardSaverData[slotindex] = draggedItem.ItemIndex;
                MyDeckSaver.SingleCardSaver = cardSaverData;

                Debug.LogWarning("드롭으로 카드 장착함!");
                return true;
            }
            else
            {
                Debug.LogWarning("드롭할 때 같은 카드 있음!");
                return false;
            }

        }

        /// <summary>
        /// 해당 슬롯에 아이템을 드롭할 때, 내 세이버에 같은 카드가 있는지 검증. 
        /// </summary>
        /// <param name="slotindex"></param>
        /// <returns>true 라면 중복카드가 있는 것이니 리턴.</returns>
        bool IsSameCardinDeck(int itemIndex, int slotindex)
        {
            var tmpArray = MyDeckSaver.SingleCardSaver;
            /// 현재 내 덱에 이미 중복 카드가 있다?
            for (int i = 0; i < tmpArray.Length; i++)
            {
                if (itemIndex == tmpArray[i])
                {
                    Debug.LogWarning($"slotindex.ItemIndex / tmpArray[i] : {itemIndex} / {tmpArray[i]}");
                    return true;
                }
            }
            /// 중복 카드 없음 -> 통과.
            Debug.LogWarning("중복 카드 없음 -> 통과.");
            return false;

        }


        internal Sprite[] GetEquipItmeIcon()
        {
            Sprite[] result = new Sprite[5];
            for (int i = 0; i < 5; i++)
            {
                result[i] = equipmentSlots[i].Item.Icon;
            }

            return result;
        }

        /// <summary>
        /// MyPopCardInfo 에서 호출하는 장착 카드 정보 불러오기
        /// </summary>
        internal string[] GetCardPopInfo(int intCase)
        {
            string[] result = new string[5];
            for (int i = 0; i < 5; i++)
            {
                result[i] = equipmentSlots[i].GetLevelAndPrices(intCase);
            }

            return result;
        }



        ///// <summary>
        ///// 내 덱에서 카드제거?
        ///// </summary>
        ///// <param name="item"></param>
        ///// <returns></returns>
        //public bool RemoveItem(EquippableItem item)
        //{
        //    var itemLength = equipmentSlots.Length;
        //    for (int i = 0; i < itemLength; i++)
        //    {
        //        /// 만약 추가하려는 카드가 이미 덱에 장착되어 있지 않다면?
        //        if (!item.isEquip)
        //        {
        //            equipmentSlots[i].Item = null;
        //            return true;
        //        }
        //    }
        //    /// 이미 장착중이면 false
        //    return false;
        //}

    }
}