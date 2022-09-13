using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CodeStage.AntiCheat.ObscuredTypes;
using Databox;
using UnityEngine;
using UnityEngine.UI;
using GoogleGame;
using EasyMobile;

namespace GoogleGame
{
    public class InventoryManager : MonoBehaviour
    {
        /// <summary>
        /// 타노스 DoThanosCreator 를 사용하기 위해
        /// </summary>
        [SerializeField] private MySceneLinker mySceneLinker;

        [Header(" - 인벤토리 스크롤 뷰")] public ScrollRect invenSV; // Pucblic 수정하지 말 것!

        [Header(" - 드래그하는 이미지")] [SerializeField]
        Image draggableItem;

        [Header(" - 카드 팝업")] [SerializeField] ItemTooltip tooltip;

        [Header(" - 카드 UI (Card_EquipmentPanel)")] [SerializeField]
        MyCardDeckPanel equipmentPanel;

        [Header(" - 전투 UI (0.MyDeckPanel)")] [SerializeField]
        MyCardDeckPanel battlePanel;

        [Space] [SerializeField] Inventory inventory;

        [Header(" - 카드 데이터 박스")] [SerializeField]
        DataboxObject allCardOrig;

        [SerializeField] DataboxObject myOwnedCard;

        [Header(" - 페이크 유닉스타임 검증용")] [SerializeField]
        DataboxObject gachaCard; // __MY_DATA7__ / _Mhic / Price

        [Header(" - 재화 데이터 박스")] [SerializeField]
        DataboxObject moneyInven;

        private void Start()
        {
            /// 이 스크립트의 Start가 실행된가는건 ? 
            /// 02. 메인씬에 넘어왔다는건? -> 튜토리얼 끝냈다는 의미
            PlayerPrefs.SetInt("IsOldUser", 1);
            PlayerPrefs.Save();
            // 페이크 타임 로드
            gachaCard.LoadDatabase();
            //NanooManager.Instance.LoadCardGachaProb();
            myOwnedCard.LoadDatabase();
        }

        void OnEnable()
        {
            allCardOrig.OnDatabaseLoaded += ServerDataReady;
            myOwnedCard.OnDatabaseLoaded += DataReady;
        }

        void OnDisable()
        {
            allCardOrig.OnDatabaseLoaded -= ServerDataReady;
            myOwnedCard.OnDatabaseLoaded -= DataReady;
        }

        /// <summary>
        /// 서버에서 Json 받아와서 카드 인벤토리 설명 대체하였는가?
        /// </summary>
        ObscuredBool isServerJsonLoaded;

        void ServerDataReady()
        {
            isServerJsonLoaded = true;
        }

        void DataReady()
        {
            try
            {
                // 0,1,2,3,4 카드 인덱스 리스트 불러오기
                myCardList = myOwnedCard.GetData<IntListType>("MyTempCard", "OwedCard", "CardIndex").Value;
                // 로컬 파일 유효성 검증
                if (!CheckMyTempCardValid(myCardList.Count))
                {
                    NanooManager.Instance.ShowPopOwnedCardError();
                    return;
                }
            }
            catch (NullReferenceException e)
            {
                Debug.LogError("DataReady 에서 널 익셉션 발생!" + e);

#if UNITY_EDITOR
                Debug.LogError("모바일 버전이 아니니까 봐준다.");
#else
                // TODO : 나누서버로 거수자 정보 전송
                NanooManager.Instance.NanooNanooPumpThisGame("abc_Gotcha_Cheater", "로컬 데이터가 손상 널 익셉션");
                /// 팝업 호출
                NanooManager.Instance.popBind.Pop_1Button_Init(APPdown, "널 익셉션 발생!",
                    "로컬 데이터가 손상되었습니다. 임의로 건들였나요????? 강제로 앱을 종료합니다.");
                NanooManager.Instance.popBind.ShowThisPop();
#endif
                

                return;
            }

            // 검증 통과했으면 로컬 파일 로
            mySceneLinker.MainSceneStater();
            Debug.LogWarning("_DATA4_ Data Ready 내가 가진 카드 장수 : " + myCardList.Count);
            /// 서버 카드덱 비교 메서드 호출
            StartCoroutine(SetStartCard());
        }


        /// <summary>
        /// 로컬 파일 유효성 검증
        /// </summary>
        /// <param name="listCount">내 카드 갯수</param>
        private bool CheckMyTempCardValid(int listCount)
        {
            // 가장 마지막 카드의 저장 시점
            int lastCardSaveTime = myOwnedCard.GetData<IntListType>("MyTempCard", "OwedCard", "CardValue")
                .Value[listCount - 1];
            Debug.LogError("가장 마지막 카드의 저장 시점 : " + lastCardSaveTime);
            // 마지막 카드 저장 시점과 훼이트 데이터 시각과 일치시킨다.
            int fakeTimePrice = gachaCard.GetData<IntType>("GachaProb", "_Mthic", "Price").Value;
            Debug.LogError("페이크 유닉스타임 : " + fakeTimePrice);
            // 시간 정보 틀리면 죽어라
            if (lastCardSaveTime != fakeTimePrice)
            {
                Debug.LogError("훼이트 데이터 시각 불일치");
                return false;
            }

            // 저장 데이터에 박힌 유저 정보 가져오기
            string userId = myOwnedCard.GetData<StringType>("MyTempCard", "OwedCard", "Owner").Value;
            // 유저 정보틀리면 죽어라
            if (userId != NanooManager.Instance.PlayerID)
            {
                Debug.LogError("저장 데이터에 박힌 유저 정보 불일치");
                return false;
            }

            Debug.LogError("로컬 파일 유효성 검증 통과");
            return true;
        }

        /// <summary>
        /// 앱 종료 + 나누 토큰 해지
        /// </summary>
        public void APPdown()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; //play모드를 false로.
#else
            /// 나누 토큰 해지
            NanooManager.Instance.AccountTokenSignOut();
            GameServices.SignOut();
            Application.Quit();
#endif
        }


        #region <데이터박스> 카드 인벤토리 Charator

        private void Awake()
        {
            inventory.OnDown += BeginDrag;
            inventory.OnUP += OnUP;
            inventory.OnClickedDown += ShowTooltip;
            inventory.OnEndDragEvent += EndDrag;
            inventory.OnDragEvent += Drag;

            equipmentPanel.OnClickedDown += ShowTooltip;
            equipmentPanel.OnDropEvent += Drop;
        }

        private ItemSlot draggedslot;

        /// <summary>
        /// 롱 클릭 시작하면 호출
        /// </summary>
        /// <param name="itemSlot"></param>
        private void BeginDrag(ItemSlot itemSlot)
        {
            draggedslot = itemSlot;
            // 드래그 하는 카드 복사해서
            draggableItem.sprite = itemSlot.Item.Icon;
            var tmpPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tmpPos.z = 10;
            // 마우스 좌표 추적
            draggableItem.transform.position = tmpPos;
            draggableItem.enabled = true;
        }

        private void EndDrag(ItemSlot itemSlot)
        {
            draggedslot = null;
            draggableItem.enabled = false;
        }

        private void OnUP(ItemSlot itemSlot)
        {
            draggableItem.enabled = false;
        }

        private void Drag(ItemSlot itemSlot)
        {
            if (draggableItem.enabled)
            {
                var tmpPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                tmpPos.z = 10;
                draggableItem.transform.position = tmpPos;
            }
        }

        /// <summary>
        /// 인벤 -> 이큅 에서만 허용. 
        /// dropItemSlot 이 draggedItem으로만 바뀌어야함
        /// </summary>
        /// <param name="dropItemSlot"></param>
        private void Drop(EquipmentSlot dropItemSlot)
        {
            if (!ReferenceEquals(draggedslot, null))
            {
                Debug.LogWarning("드롭 이벤트 : InventoryManager");

                /// [전투화면] 카드 새로고침 가능하면
                if (battlePanel.RefreshBattleUI(draggedslot.Item, dropItemSlot.indexOfEquip))
                {
                    dropItemSlot.Item = draggedslot.Item;
                }
                else
                {
                    Debug.LogWarning("드롭 하는 덱에 같은 카드 있음!");
                }
            }
        }


        /// <summary>
        /// DataReady 시 넣어준다.
        /// </summary>
        /// <returns></returns>
        IEnumerator SetStartCard()
        {
            yield return null;

            /// 나누에서 JSON 받아오기 빌드용
            /// 나누에서 JSON 받아오기 빌드용
            /// 나누에서 JSON 받아오기 빌드용
            while (NanooManager.Instance.TmpCardJson == null || !PlayerPrefs.HasKey("__MY_DATA2__"))
            {
                yield return null; // 나누에서 편집한 카드 인벤토리 받을 때까지 대기
                yield return null; // 나누에서 편집한 카드 인벤토리 받을 때까지 대기
                yield return null; // 나누에서 편집한 카드 인벤토리 받을 때까지 대기
            }

            allCardOrig.LoadDatabase();

            while (!isServerJsonLoaded)
            {
                yield return null;
                yield return null;
                yield return null;
            }
            /// 나누에서 JSON 받아오기 빌드용
            /// 나누에서 JSON 받아오기 빌드용
            /// 나누에서 JSON 받아오기 빌드용


            /// 나누 서버 덱 갯수 호출하고 currencyGetresult 받아오기.
            NanooManager.Instance.CurrencyGet();

            /// 나누 서버에서 불러올때까지 대기
            while (MyDeckSaver.OC_COUNT == 0)
            {
                yield return null;
                yield return null;
                yield return null;
            }

            /// 카드 리스트에 데이터가 들어오면 통과
            while (myCardList.Count == 0)
            {
                yield return null;
                yield return null;
                yield return null;
            }

            var ocValue1 = MyDeckSaver.OC_COUNT;
            var ocValue2 = myCardList.Count;

            Debug.LogError($"서버 OC : {ocValue1} / 로컬 OC : {ocValue2}");


            // 로컬 카드가 서버 카드보다 더 많다? -> DataBox를 수정한 상태
            // 같은 구글 계정이지만 서버 데이터와 로컬 데이터가 불일치한 상태
            if (ocValue1 < ocValue2)
            {
                Debug.LogError("로컬 카드가 서버 카드보다 더 많다? 데이터 불일치 -> 이전 기기에서 클라우드 저장후 진행해주세요.");
                yield return null;
                yield return null;
                yield return null;

                NanooManager.Instance.ShowPopOwnedCardError(); // 핵의심 팝업
                yield break;
            }
            else if (ocValue1 > ocValue2) // 서버 카드가 더 많다? -> DataBox에 기록이 안된 상태
            {
                // 재화 소모도 없는 상태이므로 서버 카드 갯수를 -> ?
                // 서버에 저장된 카드 갯수를 로컬 갯수로 덮어쓰기 해준다.
                NanooManager.Instance.CurrencyOverWrite(ocValue2);

                /// 나누 서버에서 불러올때까지 대기
                while (MyDeckSaver.OC_COUNT != ocValue2)
                {
                    yield return null;
                    yield return null;
                    yield return null;
                }

                ocValue1 = MyDeckSaver.OC_COUNT;
                Debug.LogError($"서버 OC : {ocValue1} / 로컬 OC : {ocValue2}");
            }

            Debug.LogError("로컬 = 서버 값 이상없음");

            /// 서버랑 값 비교해서 이미지 입히기
            inventory.SetStartingItems(myCardList, ocValue1);
            LoadSaverSlot();
        }


        /// <summary>
        /// 02.MainScene에서 덱 세이버 슬롯 0~4 중에 택 1
        /// [BTN_ChangeSaverSlot]
        /// </summary>
        /// <param name="slot">몇 번 버튼 눌렀나</param>
        public void ȕȖȖȖȕȖȖȖȕȕȖȕȖȕȖȕȕȖȕȖȖȕȕȖȕȖȖȖȖȖȕȕȖȖȖȖȖȖȕȖȖȖȖȕȖȕȖ(int slot)
        {
            /// 내가 사용하는 슬롯 지정
            MyDeckSaver.MySaverIndex = slot;
            /// 그리고 해당 슬롯에 저장된 카드 불러옴
            LoadSaverSlot();
        }


        /// <summary>
        /// UI 리프레쉬 -> 장착된 카드 5개만 들어간다.
        /// </summary>
        void LoadSaverSlot()
        {
            ObscuredInt[] cardSaverData = MyDeckSaver.SingleCardSaver;

            for (int i = 0; i < cardSaverData.Length; i++)
            {
                tmpEquipItem = (EquippableItem)inventory.GetItemSlotsitem(cardSaverData[i]);
                /// 1번째 [인벤토리]에 카드 추가
                equipmentPanel.AddItem(tmpEquipItem, i);
                /// 2번째 [전투화면]에 카드 추가
                battlePanel.AddItem(tmpEquipItem, i);
            }
        }


        /// <summary>
        /// 원 클릭으로 팝업 호출할때 해당 카드 정보 담김
        /// </summary>
        private EquippableItem tmpEquipItem;

        private int tmpCardPieces, tmpCardLevel, tmpIndex;

        /// <summary>
        /// 클릭 다운 시에는 쇼 툴팁
        /// </summary>
        private void ShowTooltip(ItemSlot itemSlot)
        {
            tmpEquipItem = itemSlot.Item as EquippableItem;
            
            if (ReferenceEquals(tmpEquipItem, null))
            {
                // TODO : 클릭한 아이템이 없음 
                return;
            }

            tmpIndex = tmpEquipItem.ItemIndex;
            List<string> specList = GetCardBaseInfo(tmpIndex);
            
            // 현재 스페셜 카드 레벨, 카드 조각 갯수 
            tmpCardLevel = int.Parse(specList[0]);
            tmpCardPieces = int.Parse(specList[1]);
                
            // 튤팁 팝업에는 온전한 리스트 넘겨주기
            tooltip.ShowTooltip(tmpEquipItem, specList);
        }

        internal IntType PlayGold
        {
            get { return moneyInven.GetData<IntType>("ItemTable", "Gold", "StackCnt"); }
            set { moneyInven.SetData<IntType>("ItemTable", "Gold", "StackCnt", value); }
        }


        /// <summary>
        /// 카드 툴팁이 팝업된 상태에서 업그레이드 버튼 클릭시 
        /// </summary>
        public void UpgradeCard()
        {
            CardUpgradeTable cut = new CardUpgradeTable();
            ObscuredInt cardUpgradeCost = cut.GetCardNextLevelCost(tmpIndex, tmpCardLevel);
            ObscuredInt cardUpgradePieces = cut.GetUpgradePieces(tmpCardLevel);

            // 카드 강화 재료가 부족하면 리턴
            if (tmpCardPieces < cardUpgradePieces)
            {
                //TODO : 카드조각 부족!
                return;
            }

            // 강화비용이 부족하면 리턴
            if (PlayGold.Value < cardUpgradeCost)
            {
                //TODO : 강화비용 부족!
                return;
            }

            // 카드 재화 소모
            PlayGold.Value -= cardUpgradeCost;
            // 카드 조각 소모
            tmpCardPieces -= tmpCardLevel * 8;
            // 데이터 박스 갱신            
            moneyInven.SaveDatabase();

            // 카드 강화 시키기
            // 6가지 능력치 골고루

            // 카드 강화 후 정보 업데이트
            tooltip.ShowTooltip(tmpEquipItem, GetCardBaseInfo(tmpEquipItem.ItemIndex));
        }


        //private void Equip(ItemSlot itemSlot)
        //{
        //    EquippableItem equippableItem = itemSlot.Item as EquippableItem;
        //    if (equippableItem != null)
        //    {
        //        Equip(equippableItem);
        //    }
        //}


        /// <summary>
        /// [Btn]툴팁 팝업에서 [사용하기] 버튼 누르면
        /// 인벤토리에서 마이 덱으로 착용.
        /// </summary>
        public void BTN_FinalEquip(int clicedIndex)
        {
            if (!ReferenceEquals(tmpEquipItem, null))
            {
                Equip(tmpEquipItem, clicedIndex);

                tmpEquipItem = null;
                /// 툴팁 팝업 꺼줌
                tooltip.HideThisPop();
            }
        }

        /// <summary>
        /// Inventory 에서 아이템 삭제하고, equipSlot에 아이템 장착한다.
        /// 근데, 로카디에서는 인벤에서 아이템 삭제를 안함 -> 장착됨 트리거를 올려주는 것으로
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void Equip(EquippableItem item, int slotindex)
        {
            /// 1번째 [인벤토리]에서 카드 추가
            if (equipmentPanel.AddItem(item, slotindex, out EquippableItem previousItem)) /// 내 덱에 추가하는게 이상없다면,
            {
                if (previousItem != item)
                {
                    /// 2번째 [전투화면]에서 카드 추가
                    battlePanel.AddItem(item, slotindex);
                }
            }
            else
            {
                Debug.LogWarning("몬가 잘못되서 장착 안됐음.");
            }


            //if (inventory.RemoveItem(item))     /// [획득된 카드]에 장착중이 아니고, 
            //{
            //    if (equipmentPanel.AddItem(item, out EquippableItem previousItem))  /// 내 덱에 추가하는게 이상없다면,
            //    {
            //        if (previousItem != null)   /// 내 덱에 원래 있던 애는 장착 풀고, 내 카드를 장착
            //        {
            //            //inventory.AddItem(previousItem);
            //            /// 내 덱에 장착해줘라.
            //            Debug.LogWarning("TODO : 내 덱에 장착해줘라.");
            //        }
            //        else
            //        {
            //            Debug.LogWarning("내 덱에 원래 있던 애가 없음");
            //        }
            //    }
            //}
        }

        #endregion


        /// <summary>
        /// 해당 스페셜 카드 중복 카드 강화 재료 갯수 반환
        /// </summary>
        internal IntType GetCardPrices(int index)
        {
            int namerge = index % 13;
            switch (index / 13)
            {
                case 0: return allCardOrig.GetData<IntType>("CardInventory", $"Normal_{namerge}", "CardPrices");
                case 1: return allCardOrig.GetData<IntType>("CardInventory", $"Rare_{namerge}", "CardPrices");
                case 2: return allCardOrig.GetData<IntType>("CardInventory", $"Epic_{namerge}", "CardPrices");
                case 3: return allCardOrig.GetData<IntType>("CardInventory", $"Legendary_{namerge}", "CardPrices");
                case 4: return allCardOrig.GetData<IntType>("CardInventory", $"Mythic_{namerge}", "CardPrices");
                default:
                    return null;
            }
        }

        /// <summary>
        /// 카드 중복 재료 갯수 수정
        /// </summary>
        /// <param name="index">카드 인덱스</param>
        /// <param name="value">수정할 중복 카드 갯수</param>
        internal void SetCardPrices(int index, IntType value)
        {
            int namerge = index % 13;
            switch (index / 13)
            {
                case 0:
                    allCardOrig.SetData<IntType>("CardInventory", $"Normal_{namerge}", "CardPrices", value);
                    break;
                case 1:
                    allCardOrig.SetData<IntType>("CardInventory", $"Rare_{namerge}", "CardPrices", value);
                    break;
                case 2:
                    allCardOrig.SetData<IntType>("CardInventory", $"Epic_{namerge}", "CardPrices", value);
                    break;
                case 3:
                    allCardOrig.SetData<IntType>("CardInventory", $"Legendary_{namerge}", "CardPrices", value);
                    break;
                case 4:
                    allCardOrig.SetData<IntType>("CardInventory", $"Mythic_{namerge}", "CardPrices", value);
                    break;
                default:
                    return;
            }

            allCardOrig.SaveDatabase();
        }


        List<string> container = new List<string>(16);

        /// <summary>
        /// [기본 상태]스페셜 카드 팝업 상세 설명에 박아넣을 내용들
        /// 강화된 내용은 여기를 불러오면 안돼?나? 이유는?
        /// </summary>
        internal List<string> GetCardBaseInfo(int index)
        {
            container.Clear();  // List<string> container = new List<string>(16);

            int namerge = index % 13;
            switch (index / 13)
            {
                case 0:
                    container.Add(allCardOrig.GetData<IntType>("CardInventory", $"Normal_{namerge}", "CardLevel").Value.ToString());    // 0 
                    container.Add(allCardOrig.GetData<IntType>("CardInventory", $"Normal_{namerge}", "CardPrices").Value.ToString());   // 1
                    // 
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Normal_{namerge}", "AbilityStat").Value[0]);   // 2 공격력 항목
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Normal_{namerge}", "AbilityStat").Value[1]);   // 3 공격 속도 항목
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Normal_{namerge}", "AbilityStat").Value[2]);   // 4
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Normal_{namerge}", "AbilityStat").Value[3]);   // 5
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Normal_{namerge}", "AbilityStat").Value[4]);   // 6
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Normal_{namerge}", "AbilityStat").Value[5]);   // 7

                    return container;
                case 1:
                    container.Add(allCardOrig.GetData<IntType>("CardInventory", $"Rare_{namerge}", "CardLevel").Value.ToString());    // 0 
                    container.Add(allCardOrig.GetData<IntType>("CardInventory", $"Rare_{namerge}", "CardPrices").Value.ToString());   // 1
                    // 
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Rare_{namerge}", "AbilityStat").Value[0]);   // 2 공격력 항목
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Rare_{namerge}", "AbilityStat").Value[1]);   // 3 공격 속도 항목
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Rare_{namerge}", "AbilityStat").Value[2]);   // 4
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Rare_{namerge}", "AbilityStat").Value[3]);   // 5
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Rare_{namerge}", "AbilityStat").Value[4]);   // 6
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Rare_{namerge}", "AbilityStat").Value[5]);   // 7
                    
                    return container;
                case 2:
                    container.Add(allCardOrig.GetData<IntType>("CardInventory", $"Epic_{namerge}", "CardLevel").Value.ToString());    // 0 
                    container.Add(allCardOrig.GetData<IntType>("CardInventory", $"Epic_{namerge}", "CardPrices").Value.ToString());   // 1
                    // 
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Epic_{namerge}", "AbilityStat").Value[0]);   // 2 공격력 항목
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Epic_{namerge}", "AbilityStat").Value[1]);   // 3 공격 속도 항목
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Epic_{namerge}", "AbilityStat").Value[2]);   // 4
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Epic_{namerge}", "AbilityStat").Value[3]);   // 5
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Epic_{namerge}", "AbilityStat").Value[4]);   // 6
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Epic_{namerge}", "AbilityStat").Value[5]);   // 7
                    
                    return container;
                case 3:
                    container.Add(allCardOrig.GetData<IntType>("CardInventory", $"Legendary_{namerge}", "CardLevel").Value.ToString());    // 0 
                    container.Add(allCardOrig.GetData<IntType>("CardInventory", $"Legendary_{namerge}", "CardPrices").Value.ToString());   // 1
                    // 
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Legendary_{namerge}", "AbilityStat").Value[0]);   // 2 공격력 항목
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Legendary_{namerge}", "AbilityStat").Value[1]);   // 3 공격 속도 항목
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Legendary_{namerge}", "AbilityStat").Value[2]);   // 4
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Legendary_{namerge}", "AbilityStat").Value[3]);   // 5
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Legendary_{namerge}", "AbilityStat").Value[4]);   // 6
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Legendary_{namerge}", "AbilityStat").Value[5]);   // 7

                    return container;
                case 4:
                    container.Add(allCardOrig.GetData<IntType>("CardInventory", $"Mythic_{namerge}", "CardLevel").Value.ToString());    // 0 
                    container.Add(allCardOrig.GetData<IntType>("CardInventory", $"Mythic_{namerge}", "CardPrices").Value.ToString());   // 1
                    // 
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Mythic_{namerge}", "AbilityStat").Value[0]);   // 2 공격력 항목
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Mythic_{namerge}", "AbilityStat").Value[1]);   // 3 공격 속도 항목
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Mythic_{namerge}", "AbilityStat").Value[2]);   // 4
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Mythic_{namerge}", "AbilityStat").Value[3]);   // 5
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Mythic_{namerge}", "AbilityStat").Value[4]);   // 6
                    container.Add(allCardOrig.GetData<StringListType>("CardInventory", $"Mythic_{namerge}", "AbilityStat").Value[5]);   // 7
                    
                    return container;

                default:
                    return container;
            }
        }


        /// <summary>
        /// 메모리에 올라가있는 내가 소유한 카드 인덱스 리스트 DataBox
        /// </summary>
        List<int> myCardList = new List<int>(64);

        /// <summary>
        /// 뽑기 중복 방지 트리거 true 일때 뽑기 안됨
        /// </summary>
        private ObscuredBool isRunRandCard;

        /// <summary>
        /// 버튼 누르면 스페셜 카드 뽑는다
        /// </summary>
        /// <param name="cardpoll">0123 카드 풀 인덱스</param>
        public void BTN_TEST_RunCloundGacha(int cardpoll)
        {
            if (isRunRandCard)
            {
                return;
            }

            isRunRandCard = true;

            switch (cardpoll)
            {
                case 0:
                    NanooManager.Instance.GetGotChaProbability(NanooSaveKey._Normal);
                    break;
                case 1:
                    NanooManager.Instance.GetGotChaProbability(NanooSaveKey._Rare);
                    break;
                case 2:
                    NanooManager.Instance.GetGotChaProbability(NanooSaveKey._Epic);
                    break;
                case 3:
                    NanooManager.Instance.GetGotChaProbability(NanooSaveKey._Legendary);
                    break;
                default: return;
            }

            Debug.LogError("RunGachaDataResult 코루틴 시작");
            StartCoroutine(RunGachaDataResult());
        }

        IEnumerator RunGachaDataResult()
        {
            /// 클라우드 코드 작동시까지 대기
            while (NanooManager.Instance.GotChaCardResult == -1)
            {
                yield return null;
                yield return null;
                yield return null;
            }

            // 확정된 카드 인덱스
            ObscuredInt gotNewCardIndex = NanooManager.Instance.GotChaCardResult;
            Debug.LogError("클라우드 코드 작동 확인 " + gotNewCardIndex);

            /// 중복카드 검증
            foreach (var cardIndex in myCardList)
            {
                if (cardIndex == gotNewCardIndex)
                {
                    // TODO : 중복 카드 뽑았다 = 카드 수량 +1
                    var tmp = GetCardPrices(cardIndex);
                    tmp.Value++;
                    yield return null;
                    SetCardPrices(cardIndex, tmp);
                    yield return null;
                    // 카드 슬롯 갱신하기
                    inventory.UpdateItemSlotData(myCardList);
                    Debug.LogError("중복 카드 뽑았다 " + cardIndex);
                    yield return null;
                    // 로컬 파일로 쪼개서 저장하기
                    mySceneLinker.DoThanosCreator(
                        PlayerPrefs.GetString("__MY_DATA2__"),
                        MyPersistentDataPath.LEFT_02,
                        MyPersistentDataPath.RIGHT_02);
                    /// 카드 뽑기 가능하게 트리거 off
                    isRunRandCard = false;
                    // 코루틴 빠져나가기 
                    yield break;
                }
            }

            // 중복 카드가 없다면 새로운 카드다
            Debug.LogError("새 카드 뽑았다 " + gotNewCardIndex);
            /// 서버 등록될 때까지 대기
            StartCoroutine(GatChaNewCard(gotNewCardIndex));
        }


        /// <summary>
        /// 서버에 등록되는거 확인하고 로컬에 등록해줌.
        /// </summary>
        IEnumerator GatChaNewCard(int nonIndex)
        {
            Debug.LogError("서버에 등록되는거 확인하고 로컬에 등록해줌.");

            var isSeverGetCur = MyDeckSaver.OC_COUNT;

            yield return null;

            /// 나누 서버 내 카드 갯수 한장 증가 (서버 재화 + 1)
            NanooManager.Instance.CurrencyCharge();
            // 1개가 증가할 때 까지 대기
            while (isSeverGetCur == MyDeckSaver.OC_COUNT)
            {
                yield return null;
                yield return null;
                yield return null;
            }

            // 신규 카드라면 서버 등록되는거 확인하고
            // 인벤토리에 추가하기
            inventory.AddNewCard(nonIndex);
            ///  메모리에 결과를 올린다.
            myCardList.Add(nonIndex);
            // DataBox에 기록하고 로컬 저장 완료
            myOwnedCard.SetData<IntListType>("MyTempCard", "OwedCard", "CardIndex", new IntListType(myCardList));
            yield return null;
            NanooManager.Instance.GetServerTime();
            while (NanooManager.Instance.NanooTimestamp == 0)
            {
                yield return null;
                yield return null;
                yield return null;
            }

            // 카드 벨류 검증 시간 하나 추가
            IntListType tmp = myOwnedCard.GetData<IntListType>("MyTempCard", "OwedCard", "CardValue");
            tmp.Value.Add(NanooManager.Instance.NanooTimestamp);
            // 검증 시간 다른 곳에 하나더 추가
            gachaCard.SetData<IntType>("GachaProb", "_Mthic", "Price",
                new IntType(NanooManager.Instance.NanooTimestamp));
            gachaCard.SaveDatabase();
            // 카드 인덱스에 따른 유닉스 타임으로 체크해서 위조 방지
            myOwnedCard.SetData<IntListType>("MyTempCard", "OwedCard", "CardValue", tmp);
            // 아이디 저장
            myOwnedCard.SetData<StringType>("MyTempCard", "OwedCard", "Owner",
                new StringType(NanooManager.Instance.PlayerID));
            myOwnedCard.SaveDatabase();
            yield return null;

            Debug.LogError("DataBox에 기록하고 로컬 저장 완료");

            yield return null;
            yield return null;
            yield return null;

            /// UI Refresh
            inventory.RefreshItemInven(nonIndex);
            yield return new WaitForEndOfFrame(); // 코루틴 안에서 이미지를 새로고침할때는 이거 사용

            // MyTempCard 로컬 파일로 쪼개서 저장하기
            mySceneLinker.DoThanosCreator(
                PlayerPrefs.GetString("__MY_DATA4__"),
                MyPersistentDataPath.LEFT_00,
                MyPersistentDataPath.RIGHT_00);

            yield return null;

            // GachaProb 로컬 파일로 쪼개서 저장하기
            mySceneLinker.DoThanosCreator(
                PlayerPrefs.GetString("__MY_DATA7__"),
                MyPersistentDataPath.LEFT_01,
                MyPersistentDataPath.RIGHT_01);

            // 로컬 저장 끝났으면 뽑기 가능 상태 만들어줌
            isRunRandCard = false;
        }

        /// <summary>
        /// [디버그 패널] 데이터 박스 내 소유 카드 기본 5장으로 초기화 하기.
        /// </summary>
        public void BTN_TEST_ResetIndex()
        {
            // 덱 세이버 저장된 배열 초기화
            MyDeckSaver.ResetDeckSaversArray();
            // 서버 카드 수량 5장으로 초기화
            NanooManager.Instance.CurrencyInitOC();
            myCardList.Clear();
            // 로컬 파일 삭제
            if (File.Exists(GetPersisentPath(MyPersistentDataPath.LEFT_00)))
            {
                File.Delete(GetPersisentPath(MyPersistentDataPath.LEFT_00));
            }

            if (File.Exists(GetPersisentPath(MyPersistentDataPath.RIGHT_00)))
            {
                File.Delete(GetPersisentPath(MyPersistentDataPath.RIGHT_00));
            }

            // 프리페어런스 삭제
            PlayerPrefs.DeleteKey("__MY_DATA4__");
            // 초기화 완료 됐으니 팝업 띄운다
            NanooManager.Instance.DarkUserWarnning();
        }

        private string GetPersisentPath(string path)
        {
            return Path.Combine(Application.persistentDataPath, path);
        }


        /// <summary>
        /// 구글 로그아웃 + IsOldUser = true + 로그인씬으로 돌아감 + 토큰해지
        /// </summary>
        public void BTN_TEST_GoogleLogout()
        {
            /// 나누 토큰 해지
            NanooManager.Instance.AccountTokenSignOut();
            GameServices.SignOut();
            /// IsOldUser 삭제
            PlayerPrefs.DeleteKey("IsOldUser");
            MySceneManager.Instance.JumpScemaLocation(MyLocation.LoginScene);
        }
    }
}