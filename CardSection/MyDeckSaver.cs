using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

using GoogleGame;
using CodeStage.AntiCheat.Storage;

namespace GoogleGame
{
    /// <summary>
    /// 1,2,3,4,5 카드 프리셋을 가짐.
    /// </summary>
    public class MyDeckSaver
    {
        /// <summary>
        /// 서버에 저장된 내 카드의 총 갯수 (기본값 5) 나누 재화 OC와 연동된다.
        /// </summary>
        public static ObscuredInt OC_COUNT { get; set; }

        static ObscuredInt tmpSaveCnt = 0;

        /// <summary>
        /// 수동으로 초기화해줘야함
        /// </summary>
        public static void MyStart()
        {
            /// 0과 4는 세이버 슬롯 5개일때,
            /// 1,2,3 은 세이버 슬롯 3개일때
            MySaverIndex = 1;

            /// 로컬 데이터 로드
            if (!ObscuredPrefs.HasKey("DeckSavers_Pref"))
            {
                return;
            }

            string aaa = ObscuredPrefs.GetString("DeckSavers_Pref");
            var split_text = aaa.Split('*');

            for (int i = 0; i < 5; i++)
            {
                var split_text2 = split_text[i].Split(',');
                for (int j = 0; j < 5; j++)
                {
                    deckSavers[i, j] = int.Parse(split_text2[j]);
                }
            }
        }

        /// <summary>
        /// 나누 매니저에서 카드 수량 수정하면 실행
        /// </summary>
        /// <param name="str"></param>
        public static void UpdateOCcount(object str)
        {
            OC_COUNT = int.Parse(str.ToString());
            //Debug.LogWarning("AddOCcount : " + OC_COUNT);
        }


        /// <summary>
        /// [임시]각 저장 슬롯 임시
        /// </summary>
        static ObscuredInt[,] deckSavers = {
            {0,1,2,3,4},    /// 0번 슬롯의 카드 다섯 장을 보여줌
            {0,1,2,3,4},
            {0,1,2,3,4},
            {0,1,2,3,4},
            {0,1,2,3,4},    /// 4번 슬롯에 저장된 카드 다섯 장을 보여줌
        };

        /// <summary>
        /// 로컬 카드 데이터가 초기화 되면 덱세이버 어레이도 같이 초기화 시켜준다
        /// </summary>
        public static void ResetDeckSaversArray()
        {
            deckSavers = new ObscuredInt[,]
            {
                {0, 1, 2, 3, 4}, /// 0번 슬롯의 카드 다섯 장을 보여줌
                {0, 1, 2, 3, 4}, 
                {0, 1, 2, 3, 4}, 
                {0, 1, 2, 3, 4},
                {0, 1, 2, 3, 4}, /// 4번 슬롯에 저장된 카드 다섯 장을 보여줌
            };
            TEST_Save_Pref();
            Debug.LogWarning("덱세이버 카드 초기화 완료");
        }
        
        


        static void TEST_Save_Pref()
        {
            string aaa = string.Empty;

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (j == 0)
                    {
                        aaa += deckSavers[i, j];
                    }
                    else
                    {
                        aaa += "," + deckSavers[i, j];
                    }
                }
                aaa += "*";
            }
            ObscuredPrefs.SetString("DeckSavers_Pref", aaa);
            ObscuredPrefs.Save();

            Debug.LogWarning($"PlayerPrefs.Save 횟수 : {++tmpSaveCnt}");
        }

        /// <summary>
        /// 슬롯 기본 0 ~ 4 중에 내가 사용할 카드슬롯 지정
        /// </summary>
        public static ObscuredInt MySaverIndex { get; set; }


        /// <summary>
        /// 현재 카드가 저장되는 저장소
        /// </summary>
        /// <returns>TestSaverIndex 의 result[5]</returns>
        public static ObscuredInt[] SingleCardSaver
        {
            get
            {
                ObscuredInt[] result = new ObscuredInt[5];
                for (int i = 0; i < 5; i++)
                {
                    /// result[1,2,3,4,5] 는 카드 슬롯
                    result[i] = deckSavers[MySaverIndex, i];
                }
                return result;
            }
            set
            {
                var tmp = value;

                for (int i = 0; i < 5; i++)
                {
                    deckSavers[MySaverIndex, i] = tmp[i];
                }
                /// 프리페어런스에 저장
                TEST_Save_Pref();
            }
        }



    }
}