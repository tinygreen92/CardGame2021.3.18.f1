using System.Collections;
using System.Collections.Generic;
using System.Data;
using Databox;
using UnityEngine;
using UnityEngine.UI;
using GoogleGame;

namespace GoogleGame
{
    /** 02.Main 에서 03.Battle로 넘어올 때 현재 배열만 가져온다. 
     * 배열에서 나의 카드 데이터를 비교해서 **치팅 유저** 걸러내주고
     *  ㄴ (치터가 배열만 수정하면, 내 로컬의 DataBox가 존재치 않기에 치팅이다.)
     * 모든 배열 가져오기에 덱변경도 가능!
     * 
     * 전투 시작 버튼을 누를 때, 해당되는 덱의 모든 정보 역시 불러와야함.
     *  ㄴ 모든정보? DataBox의 데이터 연동시켜!
     */
    public class MiniDeckSaver : MonoBehaviour
    {
        [SerializeField] MonsterPoolManager mpm;
        [Header(" - 출현몬스터 5종")]
        [SerializeField] Image[] monsterPortraits;
        [Header(" - 나의 덱 5종")]
        [SerializeField] Image[] myDeckPortraits;
        [Header(" - BulletController 5종")]
        [SerializeField] BulletController[] bulletShooter;
        [Space]
        [Header(" - 오리지널 데이터 박스")]
        [SerializeField] DataboxObject cardDataBox;
        [Header(" - 아이템 SO")]
        [SerializeField] EquippableItem[] specialCards;



        void OnEnable()
        {
            cardDataBox.OnDatabaseLoaded += DataReady;
        }
        void OnDisable()
        {
            cardDataBox.OnDatabaseLoaded -= DataReady;
        }
        void Start()
        {
            cardDataBox.LoadDatabase();
            //myBox.LoadDatabase();
        }

        void DataReady()
        {
            // Access data
            /// 덱 이미지 입혀줌.
            RefreshMyDeck();
        }


        /// <summary>
        /// 03.BattleScene에서 덱 세이버 슬롯 0~4 중에 택 1
        /// [BTN_ChangeSaverSlot]
        /// </summary>
        /// <param name="slot"></param>
        public void ȕȖȖȖȕȖȖȖȕȕȖȖȖȖȕȖȖȕȖȕȖȖȕȕȖȕȖȕȖȕȖȕȖȖȖȖȖȖȖȕȕȖȖȕȖȕȖ(int slot)
        {
            MyDeckSaver.MySaverIndex = slot;
            /// 그리고 해당 슬롯에 저장된 카드 불러옴
            RefreshMyDeck();
        }


        /// <summary>
        /// 스테이지 팝업에서 내가 세팅한 덱 이미지 불러와줌.
        /// </summary>
        protected virtual void RefreshMyDeck()
        {
            var sscc = MyDeckSaver.SingleCardSaver;

            /// 모든 카드 장착
            mpm.Is51cardTrigger = false;
            Readysetgo(-1);

            /// 1. 전투 시작 전 팝업 이미지 셋팅
            /// 2. 배틀필드 스페셜 카드 이미지 셋팅
            for (int i = 0; i < sscc.Length; i++)
            {
                /// 팝업 이미지 세팅
                myDeckPortraits[i].sprite = specialCards[sscc[i]].Icon;
                /// 총알 디스펜서 스페셜 카드 bulletControll 스크립트
                bulletShooter[i].RefreshMySprite(specialCards[sscc[i]]);

                switch (sscc[i])
                {
                    case 51:
                        /// case 51  모든 카드가 몬스터의 방어력의 40%를 관통
                        mpm.Is51cardTrigger = true;
                        break;

                    case 50:
                        /// case 50 모든 카드의 공격력 20%, 공격속도 10% 증가 
                        Readysetgo(50);
                        break;

                    case 20:
                        /// case 20 배치된 카드 포함, 양 옆의 카드의 공격속도를 20% 증가시킴.
                        SideDishSetGo(i);
                        break;

                    case 36:
                        /// case 36 모든 카드에게 적용되는 디버프 효과를 40%만 적용
                        Readysetgo(36);
                        break;

                    default:
                        break;
                }

            }

        }

        /// <summary>
        /// 배치된 카드 포함, 양 옆의 카드의 공격속도를 20% 증가시킴.
        /// </summary>
        /// <param name="indx">해당 카드의 존 인덱스 </param>
        void SideDishSetGo(int indx)
        {
            /// 0 ~ 4 번째 카드에서
            if (indx > 0 && indx < 4)
            {
                // 양 옆에 카드가 있다 [x 0 x x x] [x x 0 x x] [x x x 0 x] 
                bulletShooter[indx-1].EqipApplySkill(20);
                bulletShooter[indx].EqipApplySkill(20);
                bulletShooter[indx+1].EqipApplySkill(20);
            }
            else
            {
                // 양 끝에 카드를 장착할때는? [0 x x x x] [x x x x 0]
                if (indx.Equals(0))
                {
                    bulletShooter[indx].EqipApplySkill(20);
                    bulletShooter[indx + 1].EqipApplySkill(20);
                }
                else
                {
                    bulletShooter[indx].EqipApplySkill(20);
                    bulletShooter[indx - 1].EqipApplySkill(20);
                }
            }
        }


        /// <summary>
        /// 모든 카드에 적용되는 스페셜 카드 장착 효과 여기서 관리
        /// </summary>
        /// <param name="colorIndex"></param>
        /// <param name="spIndex"></param>
        void Readysetgo(int indx)
        {
            for (int i = 0; i < bulletShooter.Length; i++)
            {
                bulletShooter[i].EqipApplySkill(indx);
            }
        }
        
    }
}