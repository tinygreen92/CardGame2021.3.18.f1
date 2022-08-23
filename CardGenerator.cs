using Lean.Pool;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Random = UnityEngine.Random;

using GoogleGame;

namespace GoogleGame
{
    /// <summary>
    /// 1. SP 모았나 확인하고 버튼 기능 활성화
    /// 2. 버튼 누를때 sp 차감 하고
    /// 3. leanpool 에서 오브젝트 불러와서
    /// 4. 어떤 버튼 눌렀냐에 따라 랜덤 가중치 불러서
    /// 5. 텍스트 입혀주고 출력.
    /// 6. 위치에 박아놔라
    /// </summary>
    public class CardGenerator : MonoBehaviour
    {
        [Header("- 수치 UI")]
        [SerializeField] Text spCountText;
        [SerializeField] Text cardCountText;
        [Header("- 배틀 컨트롤러")]
        [SerializeField] BattleController bc;
        [Space]
        [Header("- 카드 풀")]
        [SerializeField] LeanGameObjectPool cardOrigin;
        [Header("- 카드 생성 위치")]
        [SerializeField] Transform cardSponer;
        [SerializeField] Transform[] cardPos;
        [Header("- 카드 SO")]
        [SerializeField] CardSO normalCardSO;
        [Header("- 셔플된 카드 리스트")]
        [SerializeField] List<NormalCard> normalCardPool;
        [Header("- 내가 들고 있는 3장의 카드")]
        [SerializeField] List<NormalCard> owned3CardPool;

        SystemMessage sysmsg;

        private void Awake()
        {
            sysmsg = FindObjectOfType<SystemMessage>();
        }

        private void Start()
        {
            normalCardPool = new List<NormalCard>(64);

            InitNormalCardPool();
        }

        /// <summary>
        /// 카드 셔플해서 39장 준비해둠.
        /// </summary>
        void InitNormalCardPool()
        {
            normalCardPool.Clear();

            var cardLen = normalCardSO.normalCards.Length;
            for (int i = 0; i < cardLen; i++)
            {
                /// so 에서 리스트에 추가
                NormalCard nc = normalCardSO.normalCards[i];
                normalCardPool.Add(nc);
            }
            cardLen = normalCardPool.Count;
            for (int i = 0; i < cardLen; i++)
            {
                /// 셔플
                int rand = Random.Range(i, cardLen);
                NormalCard tmp = normalCardPool[i];
                normalCardPool[i] = normalCardPool[rand];
                normalCardPool[rand] = tmp;
            }

            cardCountText.text = $"{normalCardPool.Count} 장";
        }

        /// <summary>
        /// 제일 윗 카드 꺼내먹어
        /// </summary>
        /// <returns></returns>
        private NormalCard PopCard()
        {
            if (normalCardPool.Count < 1)
            {
                //cardLean.DespawnAll();
                InitNormalCardPool(); /// 덱 섞어서 다시 세팅
            }

            NormalCard nc = normalCardPool[0];
            normalCardPool.RemoveAt(0);

            cardCountText.text = $"{normalCardPool.Count} 장";

            return nc;
        }


        /// <summary>
        /// true 라면 슬롯에 차있다
        /// </summary>
        private GameObject[] goSlot = new GameObject[3];

        /// <summary>
        /// 외부에서 내 소유 리스트에 접근해서 지우기
        /// </summary>
        /// <param name="card"></param>
        internal void RemoveOwned3CardPool(NormalCard card, int posIndex)
        {
            owned3CardPool.Remove(card);
            cardOrigin.Despawn(goSlot[posIndex]);
            goSlot[posIndex] = null;
        }




        /// <summary>
        /// 카드 뽑기 버튼에 들어감.
        /// 비어있는 위치에 집어 넣어
        /// BTN_TEST_DrawCard()
        /// </summary>
        public void BTN_TEST_DrawCard()
        {
            asdowem();
        }

        void asdowem()
        {
            /// 악마 카드 블랙홀 발동중이면 리턴
            if (isBlackHoleRun)
            {
                return;
            }

            /// 3장 이상이면 리턴
            if (owned3CardPool.Count > 2)
            {
                return;
            }

            /// SP 소모 판독
            if (!bc.SpendSpDrawCard(out int spCount))
            {
                /// TODO : sp 부족하면 행동
                return;
            }

            /// sp 소모 판독 통과했으면 뽑아
            GetGetCardToHand();
            /// sp 소모 해라잇
            spCountText.text = $"{spCount * 10}";
        }

        /// <summary>
        /// 비어 있는 자리에 카드 꽂아주기
        /// </summary>
        void GetGetCardToHand()
        {
            int tmpIndx = 0;

            /// 비어있는 자리 찾기
            for (int i = 0; i < goSlot.Length; i++)
            {
                if (ReferenceEquals(goSlot[i], null))
                {
                    tmpIndx = i;
                    break;
                }
            }
            //Debug.LogWarning("비어있는 자리" + tmpIndx);
            /// 내 카드에 카드 추가
            owned3CardPool.Add(PopCard());

            /// Spawn 하기
            goSlot[tmpIndx] = cardOrigin.Spawn(cardSponer);
            goSlot[tmpIndx].transform.position = cardPos[tmpIndx].position;
            /// 기본 세팅
            goSlot[tmpIndx].GetComponent<NormalCardInfo>().SetThisCard(owned3CardPool[owned3CardPool.Count - 1], tmpIndx);

            /// 카드를 뽑았는데 악마 카드다.
            if (owned3CardPool[owned3CardPool.Count - 1].numType == "Devil")
            {
                isBlackHoleRun = true;
                StartCoroutine(MoveBlackHole(owned3CardPool[owned3CardPool.Count - 1], tmpIndx));
            }

        }

        /// <summary>
        /// 핸드 풀 아니면 뽑고 아니면  SP
        /// </summary>
        internal void IsFullHand()
        {
            if (owned3CardPool.Count > 2)
            {
                /// 한장 뽑을 SP 지급
                bc.Skill45Returning();
            }
            else
            {
                /// sp 소모없이 1장 뽑기
                GetGetCardToHand();
            }
        }

        /// <summary>
        /// 블랙홀이 작동했는가?
        /// </summary>
        bool isBlackHoleRun;

        /// <summary>
        /// 악마 카드 발동중 카드뽑기 막기
        /// </summary>
        /// <param name="tmpIndx"></param>
        /// <returns></returns>
        IEnumerator MoveBlackHole(NormalCard norCard, int tmpIndx)
        {
            yield return new WaitForSeconds(0.6f);
            /// 카드 크기 1.5배 확대해서 보여주기
            goSlot[tmpIndx].transform.localPosition = Vector3.zero;
            goSlot[tmpIndx].transform.localScale = Vector3.one * 1.5f;
            yield return new WaitForSeconds(0.3f);
            /// 악마카드 제거
            RemoveOwned3CardPool(norCard, tmpIndx);


            /// 악마 카드 발동
            RunDevilRun();


            /// 연출이 끝나야 카드 뽑기 가능
            isBlackHoleRun = false;
        }

        /// <summary>
        /// 디버그 버튼에 달라붙은 악마카드 뽑기
        /// TEST_BTN_RunDevilRun()
        /// </summary>
        public void TEST_BTN_RunDevilRun()
        {
            RunDevilRun();
        }

        /// <summary>
        /// 악마 카드 효과 발동
        /// </summary>
        void RunDevilRun()
        {
            MonsterPoolManager.isRunDevil = true;
            CancelInvoke(nameof(InvoRunDevil));
            Invoke(nameof(InvoRunDevil), 4f);
            /// 일단 블랙홀 발동 -> 필드 위 모든 몬스터를 RunDevilSet 한다
            for (int i = 0; i < MonsterPoolManager.monsters.Length; i++)
            {
                /// _hudDamageText == null 이면 아직 리젠이 안된 것이다
                if (MonsterPoolManager.monsters[i] != null)
                {
                    if (MonsterPoolManager.monsters[i]._hudDamageText != null)
                    {
                        MonsterPoolManager.monsters[i].RunDevilSet(true);
                    }
                }

            }

            float rand = Random.Range(0, 10000f);

            if (rand < 5100f)
            {
                /// 51% 확률로 현재 몬스터의 1.5배 능력 가진 몬스터 소환
                sysmsg.ShowSysBubbleMsg("현재 몬스터의 1.5배 능력 가진 몬스터 소환");


            }
            else
            {
                /// 보물 고블린 등장 -> 골드 지급
                sysmsg.ShowSysBubbleMsg("보물 고블린 등장 -> 골드 지급");

            }




        }


        void InvoRunDevil()
        {
            MonsterPoolManager.isRunDevil = false;
        }


        /// <summary>
        /// [버튼] 테스트용 코드 SP 10 소모하여 카드풀에서 카드 한장을 뽑는다.
        /// </summary>
        //public void DrawCard()
        //{
        //    if(bc._sp.Value < 10 || tmpIndx > 1) return;
        //    else bc.AddSpAmount(-10);
        //    /// 내 카드에 카드 추가
        //    owned3CardPool.Add(PopCard());
        //    var card = LeanPool.Spawn(cardOrigin, cardPos[0].parent);
        //    card.transform.position = cardPos[tmpIndx++].position;

        //    if (tmpIndx == 1)
        //    {
        //        card.GetComponentInChildren<TextMeshPro>().text = "4";
        //        card.GetComponentInChildren<TextMeshPro>().color = Color.blue;

        //        owned3CardPool[0].numType = "4";
        //        owned3CardPool[0].colorType = "Blue";

        //        card.GetComponent<NormalCardInfo>().SetThisCard(owned3CardPool[0]);
        //    }
        //    else
        //    {
        //        card.GetComponentInChildren<TextMeshPro>().text = "7";
        //        card.GetComponentInChildren<TextMeshPro>().color = Color.red;

        //        owned3CardPool[1].numType = "7";
        //        owned3CardPool[1].colorType = "Red";

        //        card.GetComponent<NormalCardInfo>().SetThisCard(owned3CardPool[1]);
        //    }

        //    //card.GetComponentInChildren<TextMeshPro>().text = owned3CardPool[tmpIndx-1].numType;
        //    //switch (owned3CardPool[tmpIndx - 1].colorType)
        //    //{
        //    //    case "Red":
        //    //        card.GetComponentInChildren<TextMeshPro>().color = Color.red;
        //    //        break;
        //    //    case "Green":
        //    //        card.GetComponentInChildren<TextMeshPro>().color = Color.green;
        //    //        break;
        //    //    case "Blue":
        //    //        card.GetComponentInChildren<TextMeshPro>().color = Color.blue;
        //    //        break;
        //    //    default:
        //    //        break;
        //    //}
        //    ///// 인덱스 초기화
        //    //if (tmpIndx == 3) tmpIndx = 0;
        //}











        #region <TODO : 랜덤 가중치 뽑기> 


        ///// <summary>
        ///// 버튼에 붙여서 nor, low, high
        ///// </summary>
        ///// <param name="cardType"></param>
        //public void PickTheCard(string cardType)
        //{
        //    System.Random seedRnd = new System.Random();
        //    int startIndex = seedRnd.Next();

        //    switch (cardType)
        //    {
        //        case "nor":
        //            RandomPick(++startIndex);
        //            break;

        //    }


        //}

        //float[] probs_S = new float[] { 10f, 20f, 30f, 40f };

        ///// <summary>
        ///// 확률에 따라 랜덤 해제
        ///// </summary>
        ///// <param name="seed"></param>
        //string RandomPick(int seed)
        //{
        //    // 가중치 랜덤
        //    float returnValue = GetRandom(probs_S, seed);

        //    switch (returnValue)
        //    {
        //        case 10f: 
        //            break;
        //        case 20f: 
        //            break;
        //        case 30f: 
        //            break;
        //        case 40f: 
        //            break;
        //        default:
        //            break;
        //    }

        //    return "";
        //}

        ///// <summary>
        /////  랜덤 시드를 통해서 가중치 랜덤 뽑아낸다
        ///// </summary>
        ///// <param name="inputDatas"></param>
        ///// <param name="seed"></param>
        ///// <returns></returns>
        //public float GetRandom(float[] inputDatas, int seed)
        //{
        //    System.Random random = new System.Random(seed);

        //    float total = 0;
        //    for (int i = 0; i < inputDatas.Length; i++)
        //    {
        //        total += inputDatas[i];
        //    }
        //    float pivot = (float)random.NextDouble() * total;
        //    for (int i = 0; i < inputDatas.Length; i++)
        //    {
        //        if (pivot < inputDatas[i])
        //        {
        //            return inputDatas[i];
        //        }
        //        else
        //        {
        //            pivot -= inputDatas[i];
        //        }
        //    }
        //    return inputDatas[inputDatas.Length - 1];
        //}

        #endregion


    }
}