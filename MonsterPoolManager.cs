using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

using GoogleGame;

namespace GoogleGame
{
    /**
     * 본 편에서 쓰일 풀 매니저
     * MonsterPoolManager
     *   ㄴ MonsterSet
     *      ㄴ Monster
     * 
     */
    public class MonsterPoolManager : MonoBehaviour
    {
        [SerializeField] PinkiePie pinkyZone;
        [Header(" - 박스 몬스터 스크립트")]
        public Monster boxMonster;
        [Header(" - 대미지 프리팹")]
        public DamageText damText;
        [Header(" - 재활용 배틀 컨트롤")]
        public BattleController BC;
        [Header(" - 몬스터 위치 최상위 오브젝트")]
        [SerializeField] Transform monsterPositionObj;
        [Header(" - 해당 스테이지 몬스터 세트")]
        [SerializeField] MonsterSet[] allMonPrefabs;
        [SerializeField] MonsterSet[] bossMonPrefabs;

        /// <summary>
        /// 실제 배치되는 몬스터 놈들
        /// </summary>
        MonsterSet[] monPrefabSet = new MonsterSet[8];

        public static Monster[] monsters;
        /// <summary>
        /// 배틀 카드에서 참조하는 악마 카드 트리거
        /// </summary>
        public static ObscuredBool isRunDevil;

        private void Awake()
        {
            sysmsg = FindObjectOfType<SystemMessage>();
            monsters = new Monster[128];
        }

        private void Start()
        {
            float screen_y = Camera.main.orthographicSize * 2;
            screen_x = (screen_y / Screen.height * Screen.width) / 6;

            int rand = Random.Range(0, allMonPrefabs.Length);

            /// 초반 3웨이브 몬스터 세팅
            InitMonsterPrefab(3, rand);

        }

        /// <summary>
        /// 노말 몬스터 3웨이브 세팅
        /// </summary>
        /// <param name="lenghgh"></param>
        /// <param name="monIndex"></param>
        void InitMonsterPrefab(int lenghgh, int monIndex)
        {
            /// 3 세트 세팅
            for (int i = 0; i < lenghgh; i++)
            {
                monPrefabSet[i] = allMonPrefabs[monIndex];
            }

            /// 배열 비워줌
            allMonPrefabs = null;
        }

        /// <summary>
        /// 테스트용 보스전 버튼 누르면 세팅
        /// </summary>
        public void TEST_BTS_initboss(int rand)
        {
            //int rand = Random.Range(0, bossMonPrefabs.Length);

            /// 보스 세팅
            InitBossPrefab(rand);
        }


        ObscuredInt _monIndex;

        /// <summary>
        /// 보스몹 1마리 세팅
        /// </summary>
        /// <param name="monIndex"></param>
        void InitBossPrefab(int monIndex)
        {
            _monIndex = monIndex;

            monPrefabSet[3] = bossMonPrefabs[_monIndex];
            /// 배열 비워줌 - TODO : 어드레서블로 바꿔 줄것 
            bossMonPrefabs = null;
        }

        ObscuredFloat bossBuffTimer = 0;

        /// <summary>
        /// 3번 보스 디버프 10초마다 얼음 공격으로 스페셜카드 2개를 얼림
        /// </summary>
        void Boss3Skill(float delay)
        {
            bossBuffTimer += delay;

            /// 몬스터 타이머 10초에 한번씩 리셋 
            if (bossBuffTimer > 10 * _bossCooltime)
            {
                /// 10초에 한번씩 스페셜 카드 2개 얼림
                Debug.LogError($"3번 보스 : 스페셜 카드 2개 얼림");
                pinkyZone.BossIce();
                bossBuffTimer = 0;
            }
        }
        void Boss4Skill(float delay)
        {
            bossBuffTimer += delay;

            /// 몬스터 타이머 10초에 한번씩 리셋 
            if (bossBuffTimer > 10 * _bossCooltime)
            {
                /// 10초에 한번씩 모든 스페셜 카드 공격력/공격속도 20% 감소
                Debug.LogError($"4번 보스 : 모든 스페셜 카드 공격력/공격속도 20% 감소");
                pinkyZone.BossAllDebuff();
                bossBuffTimer = 0;
            }
        }

        void Boss6Skill(float delay)
        {
            bossBuffTimer += delay;

            /// 몬스터 타이머 10초에 한번씩 리셋 
            if (bossBuffTimer > 10 * _bossCooltime)
            {
                /// 10초마다 성벽에 독을 뿌려 매초마다 성벽 최대 체력의 1%씩 감소
                Debug.LogError($"6번 보스 : 10초마다 성벽에 독을 뿌려 매초마다 성벽 최대 체력의 1%씩 감소");
                BC.ReflectionAttackPossin();
                bossBuffTimer = 0;
            }
        }

        void Boss7Skill(float delay)
        {
            bossBuffTimer += delay;

            /// 7번 보스 몬스터 타이머 20초에 한번씩 리셋 
            if (bossBuffTimer > 20 * _bossCooltime)
            {
                /// 20초마다 모든 색상이 적용된 스페셜카드의 색상을 랜덤으로 바꿈
                Debug.LogError($"7번 보스 : 색상이 적용된 스페셜카드의 색상을 랜덤으로 바꿈");
                pinkyZone.BossRandomColor();
                bossBuffTimer = 0;
            }
        }


        /// <summary>
        /// case 9: 박스몬스터 생성
        /// </summary>
        internal void InitBoxMonster()
        {
            for (int i = 61; i < 64; i++)
            {
                if (monsters[i] == null)
                {
                    monsters[i] = LeanPool.Spawn(boxMonster, monsterPositionObj);
                    /// 몬스터 죽음 이벤트
                    monsters[i].OnDestory += Destory;
                    /// 몬스터 생성
                    monsters[i].GenerateMonster(damText, 1, 0);
                    break;
                }
            }
        }

        /// <summary>
        /// case 9: 몬스터 사망시 배열에서 해당 Monster 삭제해주기
        /// </summary>
        /// <param name="monster"></param>
        private void Destory(Monster monster)
        {
            for (int i = 61; i < 64; i++)
            {
                if (monsters[i] == null)
                {
                    continue;
                }
                if (monsters[i].Equals(monster))
                {
                    monsters[i] = null;
                    /// 9번 카드 sp 획득
                    BC.Skill9Returning();
                    break;
                }
            }
        }


        /// <summary>
        /// 임시 웨이브 수치 저장
        /// </summary>
        ObscuredInt waveCount = 0;


        /// <summary>
        /// (시작 버프 적용) 버튼 누르면 웨이브 시작
        /// </summary>
        public void TEST_BTN_GOGOGO()
        {
            /// 전투시작 누르고 90 초 후 스테이지 실패
            GameManager.instance.coTimer.SecondAction += Delay_90;
            /// 포켓몬 시작 버프 세팅 하라
            Pokepokeseting();
            /// 몬스터 뭉탱이 불러오기
            wornbvx();
        }

        /// <summary>
        /// SetParentPool 실제 배치되는 몬스터 프리팹 통짜 불러오기
        /// </summary>
        void wornbvx()
        {
            SetParentPool(monPrefabSet[waveCount]);
        }



        bool isStopMove;


        /// <summary>
        /// 보이는 몬스터 멈춰
        /// </summary>
        public void TEST_BTN_ArrayDuplecation()
        {
            ebcvzpsd();
        }

        void ebcvzpsd()
        {
            for (int i = 0; i < monsters.Length; i++)
            {
                if (monsters[i] != null)
                {
                    monsters[i].IsMove = isStopMove;
                }
            }

            isStopMove = !isStopMove;
        }

        /// float screen_y;
        public static float screen_x;

        SystemMessage sysmsg;

        ObscuredFloat _delay90 = 0;
        private void Delay_90(float delay)
        {
            _delay90 += delay;

            if (_delay90 > 90)
            {
                /// TODO : 90초 넘으면 게임 패배!
                if (!Time.timeScale.Equals(0))
                {
                    BC.ClearPopPopPop(false);   // 타임오버 패배
                }
            }
        }

        ObscuredFloat _delay15 = 0;
        private void Delay_15(float delay)
        {
            _delay15 += delay;

            if (_delay15 > 15)
            {
                /// 몬스터 웨이브 시작후 15초 뒤.
                Debug.LogError("웨이브 시작후 15초 경과.");
                GameManager.instance.coTimer.SecondAction -= Delay_15;
                _delay15 = 0;
            }
        }

        /// <summary>
        /// 몬스터 풀에서 장착시 발동
        /// </summary>
        /// <param name="index"></param>
        internal ObscuredBool Is51cardTrigger { get; set; }


        /// <summary>
        /// 불러온 몬스터 프리팹 세트를 몬스터 풀에 붙여준다.
        /// </summary>
        /// <returns></returns>
        void SetParentPool(MonsterSet mongtang)
        {
            MonsterSet mongstar = LeanPool.Spawn(mongtang, monsterPositionObj);
            mongstar.transform.localScale = Vector3.one;
            /// 배틀 컨트롤러 붙여준다.
            mongstar.InitMonSet(BC, waveCount);
            waveCount += 1;

            /// 이전 세트 몬스터 출현 시작 후 15초가 경과되면 현재 맵에 몇마리의 몬스터가 존재하든지, 다음 세트 몬스터 출현을 시작함.
            GameManager.instance.coTimer.SecondAction += Delay_15;

            /// 순차적으로 포켓몬 보내라
            StartCoroutine(LezGoPokemon(mongstar));
        }

        ObscuredInt buffIndex;
        ObscuredInt debuffIndex;
        ObscuredFloat _buff_value = 1f;
        ObscuredFloat _debuff_value = 1f;
        ObscuredFloat _bossCooltime = 1f;

        /// <summary>
        /// 버프랑 디버프 정보 가져와서 적용시킴
        /// </summary>
        public void Pokepokeseting()
        {
            List<float> appleloosa = pinkyZone.GetAppleLoona();

            /// 버프 3개와 디버프 3개 넘김
            Applebuuu((int)appleloosa[0], appleloosa[1]);
            AppleDEbuuu((int)appleloosa[2], appleloosa[3]);
        }

        /// <summary>
        /// 버프 적용
        /// </summary>
        void Applebuuu(int index, float value1)
        {
            buffIndex = index;

            switch (index)
            {
                case 11:
                    /// 몬스터 이동속도 10% 감소
                    _buff_value = value1;
                    break;
                case 12:
                    /// 보스 이동속도 10% 감소
                    _buff_value = value1;
                    break;
                case 13:
                    /// 몬스터 '최대' 체력 5% 감소
                    _buff_value = value1;
                    break;
                case 14:
                    /// 보스 '최대' 체력 5% 감소
                    _buff_value = value1;
                    break;
                case 15:
                    /// 매 웨이브 시작마다 2초간 스페셜카드 공격속도 변화 1.55 or 0.6 
                    pinkyZone.Wave2speed40(value1);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 디버프 적용
        /// </summary>
        void AppleDEbuuu(int index, float value1)
        {
            debuffIndex = index;

            switch (index)
            {
                case 4:
                    ///
                    /// 보스 특수능력(특성) 발동 쿨타임 20% 감소
                    /// 

                    _bossCooltime = value1; // 0.8


                    break;
                case 5:
                    /// 몬스터 공격력 10% 증가
                    _debuff_value = value1;
                    break;
                case 6:
                    /// 몬스터 공격속도 10% 증가
                    _debuff_value = value1;
                    break;
                case 7:
                    /// 보스 공격력 10% 증가
                    _debuff_value = value1;
                    break;
                case 10:
                    /// 보스 공격속도 10% 증가
                    _debuff_value = value1;
                    break;
                case 11:
                    /// 몬스터 이동속도 10% 증가
                    _debuff_value = value1;
                    break;
                case 12:
                    /// 보스 이동속도 10% 증가
                    _debuff_value = value1;
                    break;
                case 13:
                    /// 몬스터 '최대' 체력 5% 증가
                    _debuff_value = value1;
                    break;
                case 14:
                    /// 보스 '최대' 체력 5% 증가
                    _debuff_value = value1;
                    break;
                case 15:
                    /// 매 웨이브 시작마다 2초간 스페셜카드 공격속도 변화 1.55 or 0.6 
                    pinkyZone.Wave2speed40(value1);

                    break;

                default:
                    break;
            }
        }



        /// <summary>
        /// 20마리 등장
        /// 6 + 6 + 8 로 나타난다.
        /// </summary>
        /// <param name="mongtang"></param>
        /// <returns></returns>
        IEnumerator LezGoPokemon(MonsterSet mongstar)
        {
            yield return null;

            WaitForSeconds delay = new WaitForSeconds(1f);

            /// 방어력 관통율 
            float ArmorPenetration = 1f;    

            if (Is51cardTrigger)
            {
                // 모든 카드가 몬스터의 방어력의 40%를 관통
                ArmorPenetration = 0.6f;
            }

            int randColor = Random.Range(1, 7);

            yield return delay;

            for (int i = 0; i < 6; i++)
            {
                mongstar.monArray[i].GenerateMonster(damText, 500f, 0 * ArmorPenetration, 1, buffIndex, debuffIndex, _buff_value, _debuff_value,(EColorType)randColor);
                yield return null;
            }

            yield return delay;


            for (int i = 6; i < 6 + 6; i++)
            {
                mongstar.monArray[i].GenerateMonster(damText, 1000f, 0 * ArmorPenetration, 1, buffIndex, debuffIndex, _buff_value, _debuff_value, (EColorType)randColor);
                yield return null;
            }

            yield return delay;


            for (int i = 6 + 6; i < mongstar.monArray.Length; i++)
            {
                mongstar.monArray[i].GenerateMonster(damText, 2000f, 0 * ArmorPenetration, 1, buffIndex, debuffIndex, _buff_value, _debuff_value, (EColorType)randColor);
                yield return null;
            }

            sysmsg.ShowSysBubbleMsg($"{waveCount}세트 모두 등장 완료");

            while (true)
            {
                yield return delay;
                /// 현재 세트가 다 죽었나?
                if (IsAllDie())
                {
                    /// 코루틴 탈출
                    break;
                }
                /// 15초가 경과했나?
                if (_delay15 == 0)
                {
                    /// 코루틴 탈출
                    break;
                }

            }


            if (waveCount < 3)
            {
                /// 몬스터 뭉태기 불러오기
                wornbvx();
                //Time.timeScale = 0;
            }
            else
            {
                /// 3웨이브 끝나고 보스 등장 대기중이면?
                if (!ReferenceEquals(monPrefabSet[3], null))
                {
                    Debug.LogError("보스 등장");

                    /// 보스 등장
                    Bossbo(monPrefabSet[3]);
                }

                /// 3웨이브 끝났다
                //MySceneManager.Instance.ChangeScene("03.BattleScene");
                while (true)
                {
                    yield return delay;
                    /// 현재 세트가 다 죽었나?
                    if (IsAllDie())
                    {
                        if (!Time.timeScale.Equals(0))
                        {
                            BC.ClearPopPopPop(true);    // 게임 승리 팝업
                        }
                        
                    }
                }

            }

        }

        /// <summary>
        /// 보스 몬스터 소환
        /// </summary>
        /// <param name="mongtang"></param>
        void Bossbo(MonsterSet mongtang)
        {
            /// 방어력 관통율 
            float ArmorPenetration = 1f;

            if (Is51cardTrigger)
            {
                // 모든 카드가 몬스터의 방어력의 40%를 관통
                ArmorPenetration = 0.6f;
            }

            MonsterSet mongstar = LeanPool.Spawn(mongtang, monsterPositionObj);
            mongstar.transform.localScale = Vector3.one;

            /// 배틀 컨트롤러 붙여준다. (3이 들어가면 보스 세팅)
            mongstar.InitMonSet(BC, 3);
            mongstar.monArray[0].GenerateMonster(damText, 50000f, 10f * ArmorPenetration);


            /// 보스몹이 나오면 전체 영향을 주는거 세팅
            switch (_monIndex)
            {
                case 3:
                    /* 10초마다 얼음 공격으로 스페셜카드 2개를 얼림
                        (스페셜 카드의 특성, 스킬, 쿨타임이 일시정시
                        과거의 발동했던 특,스,쿨 이랑, 장착했을때 효과는 예외) */
                    GameManager.instance.coTimer.SecondAction += Boss3Skill;
                    break;

                case 4:
                    // 4번 보스는 10초마다 모든 스페셜카드의 공격력과 공격속도를 20% 감소시킴
                    GameManager.instance.coTimer.SecondAction += Boss4Skill;
                    break;

                case 6:
                    // "10초마다 성벽에 독을 뿌려 매초마다 성벽 최대 체력의 1%씩 감소
                    // → 중첩 가능 2번 맞으면 초마다 2 %, 3번 맞으면 초마다 3 % "
                    GameManager.instance.coTimer.SecondAction += Boss6Skill;
                    break;

                case 7:
                    // 20초마다 모든 색상이 적용된 스페셜카드의 색상을 랜덤으로 바꿈
                    GameManager.instance.coTimer.SecondAction += Boss7Skill;
                    break;


                default:
                    break;
            }
        }


        bool IsAllDie()
        {
            for (int i = 0; i < monsters.Length; i++)
            {
                if (monsters[i] != null)
                {
                    return false;
                }
            }
            /// 전부 null 이라면 true
            return true;
        }

        private void OnDisable()
        {
            GameManager.instance.coTimer.SecondAction -= Delay_15;
            GameManager.instance.coTimer.SecondAction -= Delay_90;
            GameManager.instance.coTimer.SecondAction -= Boss3Skill;
            GameManager.instance.coTimer.SecondAction -= Boss4Skill;
            GameManager.instance.coTimer.SecondAction -= Boss6Skill;
            GameManager.instance.coTimer.SecondAction -= Boss7Skill;
        }

    }

   
}