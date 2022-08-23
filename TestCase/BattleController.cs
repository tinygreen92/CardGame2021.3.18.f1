using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Databox;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Firebase.Analytics;
using UnityEngine.SceneManagement;

namespace GoogleGame
{
    public class BattleController : MonoBehaviour
    {
        ////////////////////////// 테스트 도구 /////////////////////////////
        bool isTime3X = false;

        /// <summary>
        /// BTN_TEST_3XSpeed()
        /// </summary>
        public void BTN_TEST_3XSpeed()
        {
            asdfsda();
        }

        void asdfsda()
        {
            isTime3X = !isTime3X;

            if (isTime3X)
            {
                Time.timeScale = 3.0f;
            }
            else
            {
                Time.timeScale = 1.0f;
            }
        }

        /// <summary>
        /// BTN_TEST_Slow()
        /// </summary>
        public void BTN_TEST_Slow()
        {
            dsfjkl();
        }

        void dsfjkl()
        {
            isTime3X = !isTime3X;

            if (isTime3X)
            {
                Time.timeScale = 0.2f;
            }
            else
            {
                Time.timeScale = 1.0f;
            }
        }

        /// <summary>
        /// 버튼에 붙임
        /// </summary>
        public void Test_SP_Cheet()
        {
            SP.Value += 10000;
        }

        /// <summary>
        /// 버튼에 붙임
        /// </summary>
        public void Test_MANA_Cheet()
        {
            //AddCurMana(100);
        }


        ////////////////////////// END 테스트 도구 /////////////////////////////
        [SerializeField] private PopBind stageSettingPage;

        [Header(" - 총알 발사대 5개")] [SerializeField]
        BulletController[] Zones;

        [SerializeField] StartBuffManager sbm;

        [Header(" - 스테이지 클리어 팝업")] [SerializeField]
        PopBind clearPop;

        [SerializeField] PopBind failPop;
        [Header(" - 성벽 콜라이더")] public SpriteRenderer catlecle;

        [Header(" - UI 성벽 체력 게이지")] [SerializeField]
        Image img_HP;

        [Header(" - UI 몬스터 킬 카운터")] [SerializeField]
        Image img_kill;

        [SerializeField] Text text_kill;

        [Header(" - UI 텍스트")] [SerializeField] Text sp_Text;

        //[SerializeField] Text gold_Text;
        [Space] [Header(" - 데이터 박스")] [SerializeField]
        DataboxObject dataBox;

        #region <배틀 씬에 수치 조절>

        internal IntType SP
        {
            get { return dataBox.GetData<IntType>("BattleData", "currency", "sp"); }
            set { dataBox.SetData<IntType>("BattleData", "currency", "sp", value); }
        }

        internal IntType Gold
        {
            get { return dataBox.GetData<IntType>("BattleData", "currency", "gold"); }
            set { dataBox.SetData<IntType>("BattleData", "currency", "gold", value); }
        }

        internal IntType Killcount
        {
            get { return dataBox.GetData<IntType>("BattleData", "currency", "kill_cnt"); }
            set { dataBox.SetData<IntType>("BattleData", "currency", "kill_cnt", value); }
        }

        internal IntType CastleHp
        {
            get { return dataBox.GetData<IntType>("BattleData", "currency", "castle_hp"); }
            set { dataBox.SetData<IntType>("BattleData", "currency", "castle_hp", value); }
        }

        internal FloatType MAX_CASTLE_HP
        {
            get { return dataBox.GetData<FloatType>("BattleData", "currency", "castle_max_hp"); }
            set { dataBox.SetData<FloatType>("BattleData", "currency", "castle_max_hp", value); }
        }


        internal IntListType _current_manas
        {
            get { return dataBox.GetData<IntListType>("BattleData", "currency", "current_manas"); }
            set { dataBox.SetData<IntListType>("BattleData", "currency", "current_manas", value); }
        }

        internal IntListType _max_manas
        {
            get { return dataBox.GetData<IntListType>("BattleData", "currency", "max_manas"); }
            set { dataBox.SetData<IntListType>("BattleData", "currency", "max_manas", value); }
        }


        void OnSPChanged(DataboxType _data)
        {
            sp_Text.text = $"{SP.Value}";
        }

        void OnGoldChanged(DataboxType _data)
        {
            //gold_Text.text = $"{Gold.Value}";
        }

        void OnKillChanged(DataboxType _data)
        {
            text_kill.text = $"{Killcount.Value} / 60";
        }

        void OnCastleChanged(DataboxType _data)
        {
            if (CastleHp.Value <= 0)
            {
                img_HP.fillAmount = 0;
                /// 성벽이 뿌셔지면 게임 패배
                if (!Time.timeScale.Equals(0))
                {
                    ClearPopPopPop(false); // 성벽뿌셔져서 패배
                }
            }
            else
            {
                /// 최대 체력의 50%를 넘지 않음
                if (CastleHp.Value > MAX_CASTLE_HP.Value * 1.5f)
                {
                    CastleHp.Value = Mathf.RoundToInt(MAX_CASTLE_HP.Value * 1.5f);
                }

                /// 성벽 체력 이미지 새로고침
                img_HP.fillAmount = CastleHp.Value / MAX_CASTLE_HP.Value;
            }
        }

        /// <summary>
        /// BulletController.Applebuu 에서 가져가는 성벽잃은 체력
        /// </summary>
        /// <returns></returns>
        internal ObscuredFloat GetPhoga()
        {
            return 1f - (CastleHp.Value / MAX_CASTLE_HP.Value);
        }

        /// <summary>
        /// !!!!!!!! 안 씀 !!!!!!!!!!!!!
        /// </summary>
        /// <param name="value"></param>
        public void AddCurMana(int value)
        {
            var length = _max_manas.Value.Count;
            for (int i = 0; i < length; i++)
            {
                _current_manas.Value[i] += value;
                //mana_Texts[i].text =
                //    _current_manas.Value[i] < _max_manas.Value[i] ?         /// 현재 마나가 최대 마나 이하라면
                //$"{_current_manas.Value[i]} / {_max_manas.Value[i]}" :  /// 통상 표기
                //$"{_max_manas.Value[i]} / {_max_manas.Value[i]}";       /// 아니라면 최대 마나 표기
            }
        }

        /// <summary>
        /// !!!!! 안 씀 !!!!!!!!!
        /// </summary>
        public void SetMAX_MANA()
        {
            _max_manas.Value[0] = 100;
            _max_manas.Value[1] = 50;
            _max_manas.Value[2] = 200;
            _max_manas.Value[3] = 150;
            _max_manas.Value[4] = 50;
        }

        #endregion


        void OnEnable()
        {
            Debug.LogError("배틀씬 배틀컨트롤러 온에이블");
            dataBox.OnDatabaseLoaded += DataReady;
        }

        void OnDisable()
        {
            Debug.LogError("배틀씬 배틀컨트롤러 디스에이블");
            dataBox.OnDatabaseLoaded -= DataReady;
            GameManager.instance.coTimer.SecondAction -= JaWolrdTiimi;
        }

        void Start()
        {
            dataBox.LoadDatabase();

            /// TODO : 킬카운터 초기화. -> BOSS 일때는 어떻게? 1마리를 추가해야한다
            InitBattleData();
        }

        /// <summary>
        /// 배틀 씬 진입할때 초기화.
        /// </summary>
        void InitBattleData()
        {
            /// 게이지 초기화
            img_kill.fillAmount = 1f;
            img_HP.fillAmount = 1f;

            /// 초기값
            SP.Value = 30;
            Gold.Value = 0;
            Killcount.Value = -1;

            /// 캐슬 체력
            MAX_CASTLE_HP.Value = 1000;
            CastleHp.Value = Mathf.RoundToInt(MAX_CASTLE_HP.Value);

            /// 몬스터가 뱉어낼 sp 세팅
            MONSTER_DROPPED_SP = 10;

            /// 몬스터 몇마리 나옴?
            MONSTER_MAX_CNT = 60;

            /// 0/60 마리 세팅
            Killcount.Value += 1;
            MovingMania();

            /// 세이브하면 덮어쓰기 되버림! 조심!
            //dataBox.SaveDatabase();
        }


        void DataReady()
        {
            // Access data
            Debug.LogWarning("Access data box");

            if (!GameManager.isAutoPlay)
            {
                // 전투 팝업창 생성
                stageSettingPage.ShowThisPop();
            }

            /// 덫 생성 좌표 넣어주기

            ///// Currency 단 초기화
            //SP = dataBox.GetData<IntType>("BattleData", "currency", "sp");
            //Gold = dataBox.GetData<IntType>("BattleData", "currency", "gold");
            //Killcount = dataBox.GetData<IntType>("BattleData", "currency", "kill_cnt");
            ///// 마나 쓸거냐?
            //_current_manas = dataBox.GetData<IntListType>("BattleData", "currency", "current_manas");
            //_max_manas = dataBox.GetData<IntListType>("BattleData", "currency", "max_manas");

            /// 밸류 체인지 붙여주기
            SP.OnValueChanged += OnSPChanged;
            Gold.OnValueChanged += OnGoldChanged;
            Killcount.OnValueChanged += OnKillChanged;

            /// 성벽 체력 밸류 체인지
            CastleHp.OnValueChanged += OnCastleChanged;


            /// 재시작할때 실행되는 구문
            if (GameManager.isAutoPlay)
            {
                MonsterPoolManager.isRunDevil = false;
                AutoPlay.instance.TestBtn_AutoPlay();
            }
        }

        public void DataReset()
        {
            dataBox.ResetToInitValues("BattleData");
        }


        /// <summary>
        /// 데이터 박스에서 백터 좌표 랜덤으로 가져옴
        /// </summary>
        /// <returns></returns>
        internal Vector2 GetRandomTrapPos()
        {
            /// 필드 좌표 뽑아내기
            int rand = Random.Range(0, 50);

            return dataBox.GetData<Vector2Type>("BattleData", "TrapPos", $"position_{rand}").Value;
        }

        /// <summary>
        /// 3x3 용으로 보정한 것
        /// </summary>
        /// <returns></returns>
        internal Vector2 GetRandomTrapPos_33()
        {
            /// 필드 좌표 뽑아내기
            int rand = Random.Range(0, 50);
            Vector2 tmp = dataBox.GetData<Vector2Type>("BattleData", "TrapPos", $"position_{rand}").Value;

            return new Vector2(tmp.x + 0.9f, tmp.y - 0.45f);
        }


        /// <summary>
        /// [BTN] 리롤 하면 초기 Value 는 10으로 만들어버리기
        /// 12.POP_CANVAS_StartBuff - POP_RerollWarrning - BTN_Summit
        /// </summary>
        public void ȕȕȕȕȕȖȖȖȖȖȕȖȖȕȖȖȖȖȕȖȕȖȕȖȖȖȕȕȕȖȕȖȕȖȕȖȖȖȖȕȖȖȖȕȖȕȖ()
        {
            SP.Value = 10;
        }

        /// <summary>
        /// 외부에서 배율 만큼 증가
        /// </summary>
        /// <param name="value">배율</param>
        internal void ChargeSP(float value)
        {
            SP.Value += Mathf.RoundToInt(MONSTER_DROPPED_SP * value);
        }

        /// <summary>
        /// 8번 몬스터 매 15초마다 보유중인 SP의 15%를 훔쳐감
        /// </summary>
        /// <param name="value">배율</param>
        internal void StorenSPfromMon()
        {
            Debug.LogError($"SP {SP.Value} 에서");
            SP.Value -= Mathf.RoundToInt(SP.Value * 0.15f);
            Debug.LogError($"SP {SP.Value} 가 되다.");
        }


        /// <summary>
        /// 1웨이브에 60마리 디폴트
        /// </summary>
        ObscuredInt MONSTER_MAX_CNT { get; set; }

        /// <summary>
        /// 몬스터가 죽으면 떨구는 sp양
        /// </summary>
        ObscuredInt MONSTER_DROPPED_SP { get; set; }

        /// <summary>
        /// 시작버프에서 증가하는 몬스터 죽이고 얻는 SP 획득량 증/감
        /// </summary>
        internal void PlusPlusSp(float valueTime)
        {
            MONSTER_DROPPED_SP = Mathf.RoundToInt(MONSTER_DROPPED_SP * valueTime);
            Debug.LogWarning("시작버프에서 증가하는 몬스터 죽이고 얻는 SP 획득량 증/감 : " + MONSTER_DROPPED_SP);
        }


        /// <summary>
        /// 몬스터 처치시 스페셜카드 공격속도 3초간 5% 증가
        /// </summary>
        /// <param name="isOn"> true 라면 공속 up / false 라면 공속 원래대로</param>
        void TheWolrd(bool isOn)
        {
            for (int i = 0; i < Zones.Length; i++)
            {
                if (isOn)
                {
                    Zones[i].SetMySpeedSpeed(1.05f);
                }
                else
                {
                    Zones[i].SetMySpeedSpeed(1f);
                }
            }
        }

        ObscuredFloat _JaDelay = 3f;

        private void JaWolrdTiimi(float delay)
        {
            _JaDelay -= delay;
            if (_JaDelay < 0)
            {
                TheWolrd(false);
                GameManager.instance.coTimer.SecondAction -= JaWolrdTiimi;
                _JaDelay = 3f;
            }
        }

        /// <summary>
        /// 오렌지 스페셜카드가 공격시 {공격력}의 1%만큼 성벽 체력 회복 colorTypeBae[4];
        /// </summary>
        internal void RestoreCatleHp(float atkDam, float times)
        {
            CastleHp.Value += Mathf.RoundToInt(atkDam * times);
            Debug.LogError($"오렌지 카드로 성벽 회복 {atkDam * times}");
        }


        internal ObscuredBool On47Skill { get; set; }
        internal ObscuredBool On6Buff { get; set; }
        internal ObscuredBool On7Buff { get; set; }

        /// <summary>
        /// 당신은 한 가정을 파괴했습니다.
        /// </summary>
        internal void KillMonsterBadly()
        {
            /// 47스킬 활성화라면 성벽 회복
            if (On47Skill)
            {
                CastleHp.Value += Mathf.RoundToInt(MAX_CASTLE_HP.Value * 0.03f);
                Debug.LogError($"47 스페셜 카드 {Mathf.RoundToInt(MAX_CASTLE_HP.Value * 0.03f)} 회복");
            }

            /// 시작 6버프가 활성화되면 몬스터 처치시 스페셜카드 공격속도 3초간 5% 증가
            if (On6Buff)
            {
                TheWolrd(true);
                _JaDelay = 3f;
                Debug.LogError("스페셜카드 공격속도 3초간 5% 증가");
                GameManager.instance.coTimer.SecondAction += JaWolrdTiimi;
            }

            /// 시작 7버프가 활성화되면 1킬당 체력 1% 쉴드 생성
            if (On7Buff)
            {
                CastleHp.Value += Mathf.RoundToInt(MAX_CASTLE_HP.Value * 0.01f);
                Debug.LogError($"시작 7버프 {Mathf.RoundToInt(MAX_CASTLE_HP.Value * 0.01f)} 회복");
            }

            /// 킬 카운터 1 증가
            Killcount.Value += 1;
            /// SP 10 회복
            SP.Value += MONSTER_DROPPED_SP;
            /// 킬카운터 가로 이미지 움직이기
            MovingMania();
        }

        void MovingMania()
        {
            img_kill.fillAmount = 1.0f - (Killcount.Value / (MONSTER_MAX_CNT * 1f));
            /// 일반 몹 60마리 다 잡았으면?
            if (Killcount.Value == MONSTER_MAX_CNT)
            {
                img_kill.fillAmount = 0f;
            }
        }

        /// <summary>
        /// 보스 디버프 카운트 -> 10초마다 1%씩 증가
        /// </summary>
        ObscuredInt castlePosinCnt;

        /// <summary>
        /// 성벽 최대 체력의 1%씩 감소 -> 10초마다 중복
        /// </summary>
        internal void ReflectionAttackPossin()
        {
            castlePosinCnt += 1;
            AttackedCastle(Mathf.RoundToInt(MAX_CASTLE_HP.Value * castlePosinCnt * 0.01f));
            Debug.LogError($"6번 보스 성벽 최대 체력의 {Mathf.RoundToInt(MAX_CASTLE_HP.Value * castlePosinCnt * 0.01f)} 감소");
        }

        /// <summary>
        /// 밑으로 내려와서 몬스터가 성벽 깨부시기 아니면
        /// // 일반 몬스터 3% 반사 / 보스 몬스터 2% 반사
        /// </summary>
        internal void MonsterAttackCastle(int dam)
        {
            AttackedCastle(dam);

            /// TODO : 성벽 공격당하는 연출 추가
        }

        /// <summary>
        /// 성벽이 공격 당함
        /// </summary>
        void AttackedCastle(int dam)
        {
            /// 정지 상태일때 성벽 공격 당하지 않는다.
            if (Time.timeScale.Equals(0))
            {
                return;
            }

            catlecle.color = Color.red;
            CancelInvoke(nameof(InvoColor));
            Invoke(nameof(InvoColor), 0.3f);
            /// 성벽 체력 깎아줘라
            CastleHp.Value -= dam;

            //Debug.LogWarning($"성벽 남은 체력 : {CastleHp.Value}");
        }

        /// <summary>
        /// 피격후 성벽 색 원래대로 돌려주기
        /// </summary>
        void InvoColor()
        {
            catlecle.color = Color.white;
        }


        /// <summary>
        /// 죽거나 승리하거나 둘 중 하나다
        /// </summary>
        internal void ClearPopPopPop(bool isClear)
        {
            /// 자동 플레이 테스트
            if (GameManager.isAutoPlay)
            {
                StartCoroutine(Replay());
                return;
            }

            var sscc = MyDeckSaver.SingleCardSaver;

            for (int i = 0; i < sscc.Length; i++)
            {
                /// 전투 팝업 뜨면 파이어베이스에 현재 덱 저장
                FirebaseAnalytics.LogEvent($"card_deck_num_{sscc[i]}");
            }

            // 각 덱의 피해량 통계
            var dealList = DamageDealt.GetAllDamageDealtLog();
            foreach (var card in dealList)
            {
                Debug.LogError($"전투 통계 : {card}");
            }


            /// 게임종료 팝업이 노출되면, 게임 정지
            Time.timeScale = 0;

            if (isClear)
            {
                clearPop.ShowThisPop();
            }
            else
            {
                failPop.ShowThisPop();
            }
        }

        IEnumerator Replay()
        {
            AsyncOperation asyncOper = SceneManager.LoadSceneAsync("03.BattleScene", LoadSceneMode.Single);
            yield return asyncOper;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName("03.BattleScene"));
            yield return null;
        }


        ObscuredInt _spCount = 1;

        /// <summary>
        /// 카드 버리기하면 SP 환원
        /// </summary>
        internal void RunHalfSpCount()
        {
            /// 카드 뽑은 SP 의 절반
            SP.Value += ((_spCount - 1) * 5);
        }

        /// <summary>
        /// 45 카드 효과로 sp 회복
        /// </summary>
        internal void Skill45Returning()
        {
            /// 카드 한장 뽑을 SP
            SP.Value += (_spCount * 10);
        }

        internal void Skill9Returning()
        {
            /// 9번 카드 sp 1.5배
            SP.Value += (_spCount * 15);
        }

        /// <summary>
        /// 카드 뽑을 때 SP 소모량 10씩 증가
        /// </summary>
        internal bool SpendSpDrawCard(out int spCount)
        {
            spCount = _spCount;
            /// SP 가 모자라면 fasle 
            if (SP.Value < spCount * 10)
            {
                return false;
            }
            else
            {
                /// SP 를 소모하고 true 반환
                SP.Value -= spCount * 10;
                /// 다음 회차 값 10 증가
                spCount = ++_spCount;
                return true;
            }
        }


        /// <summary>
        /// 카드 뽑을때 SP 소모 가능한지 리턴
        /// </summary>
        /// <param name="spendSpValue">소비할 SP</param>
        /// <returns></returns>
        internal bool ColorSpendSp(int spendSpValue)
        {
            if (SP.Value >= spendSpValue)
            {
                SP.Value -= spendSpValue;
                return true;
            }
            else
            {
                return false;
            }
        }


        //IEnumerator CountTo(float target, float current)
        //{
        //    Debug.LogError(current);
        //    float duration = 0.2f; // 카운팅에 걸리는 시간 설정. 
        //    float offset = (target - current) / duration;

        //    while (current < target)
        //    {
        //        current += offset * Time.deltaTime;
        //        sp_Text.text = $"{(int)current}";
        //        yield return null;
        //    }

        //    current = target;
        //    sp_Text.text = $"{(int)current}";
        //}


        //IEnumerator CountTo(int amount)
        //{
        //    sp_Text.text = $"{SP.Value}";

        //    int start = SP.Value;
        //    int target = start + 10;

        //    for (float timer = 0; timer < duration; timer += Time.deltaTime)
        //    {
        //        sp_Text.text = $"{(int)Mathf.Lerp(start, target, timer / duration)}";
        //        yield return null;
        //    }

        //    SP.Value = target;
        //}
    }
}