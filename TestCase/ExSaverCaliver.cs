using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using GoogleGame;

namespace GoogleGame
{
    public class ExSaverCaliver : MiniDeckSaver
    {

        /// <summary>
        /// 스테이지에서 사용하는 카드 덱 불러오기.
        /// </summary>
        protected override void RefreshMyDeck()
        {
            //base.RefreshMyDeck();
            /// 시작시 세팅은 일단 대기
        }

        /// <summary>
        /// 해당 슬롯의 카드를 바꿔줄거임
        /// </summary>
        int inputInt;

        public void fasfasdsa(int slotindex)
        {
            ObscuredInt[] cardSaverData = MyDeckSaver.SingleCardSaver;
            cardSaverData[slotindex] = inputInt;
            MyDeckSaver.SingleCardSaver = cardSaverData;
        }



        /// <summary>
        /// 수동으로 덱 빌딩 새로고침
        /// </summary>
        public void BTN_ActivatePassiveDeck()
        {
            nxnbceud();
        }

        void nxnbceud()
        {
            MyDeckSaver.SingleCardSaver = new ObscuredInt[] { 0, 3, 16, 28, 33 }; // 도트 세팅/;                                              
            base.RefreshMyDeck();
        }

        int TestCnt;

        /// <summary>
        /// 임의로 5개 씩 밀어주기
        /// </summary>
        /// <param name="isNext"></param>
        public void BTN_NextLevel(bool isNext)
        {
            /// >>
            if (isNext)
            {
                MyDeckSaver.SingleCardSaver = new ObscuredInt[] { TestCnt++, TestCnt++, TestCnt++, TestCnt++, TestCnt++ };
            }
            else  /// << previous
            {
                MyDeckSaver.SingleCardSaver = new ObscuredInt[] { TestCnt--, TestCnt--, TestCnt--, TestCnt--, TestCnt-- };
            }

            if (TestCnt > 52)
            {
                MyDeckSaver.SingleCardSaver = new ObscuredInt[] { 47, 48, 49, 50, 51 };
                TestCnt = 46;
            }

            if (TestCnt < 0)
            {
                MyDeckSaver.SingleCardSaver = new ObscuredInt[] { 0, 1, 2, 3, 4 };
                TestCnt = 5;
            }

            base.RefreshMyDeck();
        }

    }
}