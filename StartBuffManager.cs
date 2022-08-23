using System.Collections;
using System.Collections.Generic;
using Databox;
using UnityEngine;
using UnityEngine.UI;
using GoogleGame;
using TMPro;

namespace GoogleGame
{
    public class StartBuffManager : MonoBehaviour
    {
        [SerializeField] PopBind buffPopBind;
        [Space]
        [SerializeField] DataboxObject buffDataBox;
        [Space]
        [Header("구조 대공사 필요")]
        [SerializeField] Button[] buffButton;  // TODO : 구조 대공사 필요
        // 버튼에 붙어있는 0,1,3 박아줌
        [SerializeField] Image[] buffImgs;
        [SerializeField] TMP_Text[] debuffText;
        [SerializeField] TMP_Text[] buffText;
        ColorBlock colorBlockWhite;
        ColorBlock colorBlockGray;

        void OnValidate()
        {
            /// 3개의 버튼 아래에 정보 이미지 바인딩
            //for (int i = 0; i < buffButton.Length; i++)
            //{
            //    buffImgs[i] = buffButton[i].transform.GetChild(0).GetComponent<Image>();
            //    debuffText[i] = buffButton[i].transform.GetChild(1).GetComponent<Text>();
            //    buffText[i] = buffButton[i].transform.GetChild(2).GetComponent<Text>();
            //}
        }

        void OnEnable()
        {
            buffDataBox.OnDatabaseLoaded += DataReady;
        }
        void OnDisable()
        {
            buffDataBox.OnDatabaseLoaded -= DataReady;
        }
        void Start()
        {
            buffDataBox.LoadDatabase();
            colorBlockWhite = buffButton[0].colors;
            colorBlockGray = colorBlockWhite;
            colorBlockGray.normalColor = colorBlockGray.selectedColor;
        }

        void DataReady()
        {
            //Debug.LogWarning("Access data box");

        }


        public void AutoPlay()
        {
            /// 6번 돌려서 리스트 생성
            /// 랜덤 버프에 이미지 박아 넣고
            InitRandomBuff(CreateUnDuplicateRandom(6));
            
        }

        /// <summary>
        /// 전투시작 버튼을 누르면 시작버프 3개 뽑고 팝업 생성
        /// </summary>
        public void BTN_ShowPopAndInit()
        {
            /// 6번 돌려서 리스트 생성
            /// 랜덤 버프에 이미지 박아 넣고
            InitRandomBuff(CreateUnDuplicateRandom(6));
            /// 버프 팝업 보여주기
            buffPopBind.ShowThisPop();
        }
        
        /// <summary>
        /// 디버그 패널 리롤 버튼
        /// </summary>
        public void BTN_RerollBuff()
        {
            RerollRandomBuff();
        }

        /// <summary>
        /// 시작 버프가 준비되면 이미지 박아 넣기.
        /// </summary>
        /// <returns></returns>
        void InitRandomBuff(List<int> buffIndexList)
        {
            /// 1,2,3 번 버프 따로 저장
            intArry[0] = buffIndexList[0];
            intArry[1] = buffIndexList[1];
            intArry[2] = buffIndexList[2];

            /// TODO : 버프랑 디버프 설명 바인딩
            for (int i = 0; i < 3; i++)
            {
                //buffImgs[i].sprite = buffss[buffIndexList[i]].BuffIcon;

                /// 버프 텍스트
                //buffTextTemp[i].text = GetBuffInfomation(buffIndexList[i])[0];
                buffText[i].text = buffDataBox.GetData<StringType>("BuffList", $"BUFF_{buffIndexList[i]}", "Desc").Value;

                /// 디버프 텍스트
                //buffText[i].text = GetDeBuffInfomation(buffIndexList[i + 3])[0];
                debuffText[i].text = buffDataBox.GetData<StringType>("BuffList", $"DEBUFF_{buffIndexList[i + 3]}", "Desc").Value;
            }


            /// 0번 버프가 눌러진 상태로 보이게 한다.
            ȕȕȖȕȕȖȕȕȕȕȕȖȖȖȕȖȖȕȕȖȖȖȖȕȕȕȕȕȖȕȕȖȖȕȖȖȕȕȖȕȕȕȖȕȕȖȕ(0);

        }

        
        /// <summary>
        /// 내가 선택한 버프는 뭘까용
        /// </summary>
        int _MyBuffIndex;

        /// <summary>
        /// 버프 3군데 0,1,2 에 박아넣고 셀렉트 하기
        /// </summary>
        public void ȕȕȖȕȕȖȕȕȕȕȕȖȖȖȕȖȖȕȕȖȖȖȖȕȕȕȕȕȖȕȕȖȖȕȖȖȕȕȖȕȕȕȖȕȕȖȕ(int index)
        {
            /// 사용자가 마우스로 pressed 를 한다면 
            switch (index)
            {
                case 0:
                    buffButton[0].colors = colorBlockGray;
                    buffButton[1].colors = colorBlockWhite;
                    buffButton[2].colors = colorBlockWhite;
                    break;

                case 1:
                    buffButton[0].colors = colorBlockWhite;
                    buffButton[1].colors = colorBlockGray;
                    buffButton[2].colors = colorBlockWhite;
                    break;

                case 2:
                    buffButton[0].colors = colorBlockWhite;
                    buffButton[1].colors = colorBlockWhite;
                    buffButton[2].colors = colorBlockGray;
                    break;

                default:
                    break;
            }

            _MyBuffIndex = index;
        }


        /// <summary>
        /// 버프 데이터 3개 디버프 데이터 3개 던져 줌.
        /// </summary>
        /// <returns></returns>
        internal List<float> GetAppleBear()
        {
            List<float> buffList = new List<float>
            {
                    // 버프
                    buffIndexList[_MyBuffIndex],
                    buffDataBox.GetData<FloatType>("BuffList", $"BUFF_{buffIndexList[_MyBuffIndex]}", "Value1").Value,
                    // 디버프
                    buffIndexList[_MyBuffIndex + 3],
                    buffDataBox.GetData<FloatType>("BuffList", $"DEBUFF_{buffIndexList[_MyBuffIndex + 3]}", "Value1").Value
           };

            Debug.Log($"버프 인덱스 : {buffList[0]}, 버프 밸류 : {buffList[1]}");
            Debug.Log($"디버프 인덱스 : {buffList[2]}, 디버프 밸류 : {buffList[3]}");

            return buffList;
        }

        /// <summary>
        /// 1번 통 2번 통 3번 통 - 리롤
        /// </summary>
        void RerollRandomBuff()
        {
            while (true)
            {
                var reBuff = CreateUnDuplicateRandom(6);

                /// 각 버프 통에 중복되는게 있으면?
                if (intArry[0].Equals(reBuff[0]) ||
                    intArry[1].Equals(reBuff[1]) ||
                    intArry[2].Equals(reBuff[2]))
                {
                    Debug.LogWarning("버프통 다시 돌리기");
                    continue;
                }
                else
                {
                    Debug.LogWarning("No 중복 통과");

                    /// 이미지 세팅
                    InitRandomBuff(reBuff);
                    break;
                }
            }

        }


        int[] intArry = new int[3];

        /// <summary>
        /// 난수로 돌린 랜덤 인덱스 0,1 = 버프 / 2,3 = 디버프
        /// </summary>
        List<int> buffIndexList = new List<int>(6);

        /// <summary>
        /// 중복없는 난수 생성
        /// 버프 3개 + 디버프 3개 = 6번의 랜덤 
        /// </summary>
        /// <param name="tryCnt">시행 횟수</param>
        List<int> CreateUnDuplicateRandom(int tryCnt)
        {
            buffIndexList.Clear();

            int currentNumber = Random.Range(0, 16);

            for (int i = 0; i < tryCnt;)
            {
                if (buffIndexList.Contains(currentNumber))
                {
                    currentNumber = Random.Range(0, 16);
                }
                else
                {
                    buffIndexList.Add(currentNumber);

                    i += 1;
                }
            }

            return buffIndexList;
        }



    }
}