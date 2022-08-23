using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;
using GoogleGame;

namespace GoogleGame
{
    public class Inventory : MonoBehaviour
    {
        [Header(" - 4.NonGotCardDeck 오브젝트")] ObscuredInt CARD_RAND_BOJUNG = 18;
        [SerializeField] Transform nonGotParent;

        /// <summary>
        /// 3.INVENTORY 오브젝트
        /// </summary>
        [Header(" - 3.INVENTORY 오브젝트")] [SerializeField]
        Transform itemsParent;

        [Space]
        [Header(" - 획득한 카드 Item SO")]
        /// <summary>
        /// 획득된 카드만 담을 리스트 (실제 가진 카드)
        /// </summary>
        [SerializeField]
        List<Item> ownnedItems;

        [Header(" - 모든 카드 ItemSlot")]
        /// <summary>
        /// 획득 예정인 카드 오브젝트를 담을 슬롯 (현재 52개)
        /// </summary>
        [SerializeField]
        ItemSlot[] itemSlots;

        public event Action<ItemSlot> OnDown;
        public event Action<ItemSlot> OnUP;
        public event Action<ItemSlot> OnClickedDown;
        public event Action<ItemSlot> OnBeginDragEvent;
        public event Action<ItemSlot> OnEndDragEvent;

        public event Action<ItemSlot> OnDragEvent;
        //public event Action<ItemSlot> OnDropEvent;

        /// <summary>
        /// 현재 뚫린 소유 가능한 카드 수 52 (0927 기준)
        /// </summary>
        ObscuredInt itemSlotslength = 52;


        public void Start()
        {
            //itemSlotslength = itemSlots.Length;
            if (itemSlotslength != 52) itemSlotslength = 52;

            for (int i = 0; i < itemSlotslength; i++)
            {
                itemSlots[i].OnDown += OnDown;
                itemSlots[i].OnUP += OnUP;
                itemSlots[i].OnClickedDown += OnClickedDown;
                itemSlots[i].OnBeginDragEvent += OnBeginDragEvent;
                itemSlots[i].OnEndDragEvent += OnEndDragEvent;
                itemSlots[i].OnDragEvent += OnDragEvent;
                //itemSlots[i].OnDropEvent += OnDropEvent;
            }

            //LoadDataOwendItem();
            ///// 얘가 불리기전에 ownnedItems에 데이터 불러와져야한다.
            //SetStartingItems();
        }


        private void OnValidate()
        {
            if (itemsParent != null)
            {
                itemSlots = itemsParent.GetComponentsInChildren<ItemSlot>();
            }

            //for (int i = 5; i < 52; i++)
            //{
            //    itemSlots[i].Item = itemSlots[i].Item;
            //}
        }

        /// <summary>
        /// 해당 인덱스 슬롯에 있는 아이템 클래스를 바깥으로 빼줌
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Item GetItemSlotsitem(int index)
        {
            return itemSlots[index].Item;
        }
        
        /// <summary>
        /// 해당 값은 클라우드 뽑기에서 호출
        /// </summary>
        /// <param name="GachaKey">아이템 슬롯에 추가할 인덱스</param>
        internal void AddNewCard(int GachaKey)
        {
            if (IsFull())
            {
                Debug.LogError("카드 뽑을 거 다 뽑았다 IsFull");
                return;
            }
            // ownnedItems에 내 카드 추가
            ownnedItems.Add(itemSlots[GachaKey].Item);
        }


        /// <summary>
        /// 미 획득한 카드 탭에서 획득한 카드 탭으로 이동
        /// </summary>
        internal void RefreshItemInven(int MAX)
        {
            /// 해당 카드가 획득되어 (레벨, 카드조각)이 보이는 상태.
            itemSlots[MAX].Item = itemSlots[MAX].Item;
            // 해당 카드 위로 올리기.
            itemSlots[MAX].transform.SetParent(itemsParent);
            // 미획득 섹션을 밑으로 내리기
            nonGotParent.SetAsLastSibling();
        }

        /// <summary>
        /// 씬 전환시 한번만.
        /// Item 클래스 Item 프로퍼티에 Set 되면 이미지 입혀준다.
        /// </summary>
        /// <param name="myList">내가 소유한 카드 인덱스 리스트</param>
        /// <param name="deckLenth">소유한 카드 총 갯수</param>
        /// <returns></returns>
        internal bool SetStartingItems(List<int> myList, int deckLenth)
        {
            for (int i = 0; i < 5; i++)
            {
                // set을 해주면 새로고침 됨.
                itemSlots[i].Item = ownnedItems[i]; 
            }

            /// (플레이어가 소유한 카드 총 갯수) - (기본덱 5장)
            for (int i = 5; i < deckLenth; i++)
            {
                ownnedItems.Add(itemSlots[myList[i]].Item);
                /// 슬롯 새로고침
                itemSlots[myList[i]].Item = ownnedItems[i];
                /// 해당 카드 위로 올리기.
                itemSlots[myList[i]].transform.SetParent(itemsParent);
            }

            /// 미획득 섹션 밑으로 내리기
            nonGotParent.SetAsLastSibling();

            return true;
        }

        /// <summary>
        /// 인벤토리 매니저에서 불러서 데이터박스 새로고침 해줌
        /// </summary>
        /// <param name="myList">내가 소유한 카드 인덱스 리스트</param>
        internal void UpdateItemSlotData(List<int> myList)
        {
            for (int i = 0; i < myList.Count; i++)
            {
                /// 슬롯 새로고침
                itemSlots[myList[i]].Item = ownnedItems[i];
            }
        }


        /// <summary>
        /// 미 획득된 카드를 [획득된 카드]로 등록한다.
        /// </summary>
        /// <param name="item">추가하고 싶은 미획득된 카드</param>
        /// <returns></returns>
        internal bool AddItem(Item item)
        {
            //for (int i = 0; i < itemSlotslength; i++)
            //{
            //    if (itemSlots[i].Item == null)
            //    {
            //        itemSlots[i].Item = item;
            //        return true;
            //    }
            //}

            return false;
        }

        /// <summary>
        /// 사용 안함
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveItem(Item item)
        {
            //int length = itemSlots.Length;
            //for (int i = 0; i < length; i++)
            //{
            //    if (itemSlots[i].Item == item)
            //    {
            //        itemSlots[i].Item = null;
            //        return true;
            //    }
            //}

            return false;
        }

        /// <summary>
        /// 획득 가능 카드 슬롯이 모두 차있으면 True
        /// (획득된 카드가 슬롯에 모두 박혀있는 상태)
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            return ownnedItems.Count >= itemSlots.Length;
        }
    }
}