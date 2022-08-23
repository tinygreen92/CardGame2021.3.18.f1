using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Lean.Pool;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;
using GoogleGame;
using Newtonsoft.Json.Linq;

[Flags]
public enum ENorMonsterAbility
{
    None = 0,
    Buff_Shield = 1 << 0, // 체력의 50%의 실드
    DeBf_Melee_Down = 1 << 1, // 물리 대미지 감소
    DeBf_Magic_Down = 1 << 2, // 마법 대미지 감소
    Buff_Self_Heal = 1 << 3, // 매 5초마다 최대 체력의 3%를 회복
    DeBf_Damage_Down = 1 << 4, // 공격력 감소
    DeBf_AtkSpeed_Down = 1 << 5, // 공격속도 감소
    Buff_Speed_Up = 1 << 6, // 이동속도 1.5배
    Buff_Reflection = 1 << 7, // 대미지 반사
    Buff_Stealing_SP = 1 << 8, // SP 훔치기
    Buff_LivingDead = 1 << 9, // 부활 
    Everything = int.MaxValue
}

[Flags]
public enum EBossMonsterAbility
{
    None = 0,
    Immune_Melee = 1 << 0, // 물리 대미지 면역
    Immune_Magic = 1 << 1, // 마법 대미지 면역
    Buff_Self_Heal = 1 << 2, // 매 5초마다 최대 체력의 3%를 회복
    Attack_ICE = 1 << 3, // 얼음 공격
    Attack_SLOW = 1 << 4, // 슬로우 디버프
    Buff_Reflection = 1 << 5, // 대미지 반사
    Attack_POISON = 1 << 6, // 독성 공격
    DeBf_Color_Change = 1 << 7, // 색상 변경
    Everything = int.MaxValue
}

public enum EMonsterAttackType
{
    Melee_Near, // 물리 근접 
    Melee_Far, // 물리 원거리
    Magic_Near, // 마법 근접 
    Magic_Far, // 마법 원거리 
}

public enum EMonsterType
{
    UnDead, // 언데드 - 체력 / 방어력 1.3배 이속 0.8배
    Normal, // 일반 몹
    FinalBoss, // 찐 보스
}


namespace GoogleGame
{
    public class Monster : MonoBehaviour
    {
        /// <summary>
        /// 훈련소 이미지 활성화
        /// </summary>
        internal void GogoRoomActivate()
        {
            mySprite.enabled = true;
        }

        /// <summary>
        /// GonGonJaeRoom 에서 호출하는 임시 생성
        /// </summary>
        internal void TEST_GenerateMonster(DamageText hudDamageText, float maxHp, float armor = 0,
            float moveSpeed = 100f)
        {
            _hudDamageText = hudDamageText;
            _hudDamagePos = _hudDamageText.transform.parent;
            // 체력 세팅
            _Max_HP = maxHp;
            CurrentHp = _Max_HP;
            // 몬스터가 하단으로 내려오는 시간
            _moveSpeed = moveSpeed;
            // 실드는 방어력의 1.5배
            _Armor = armor;
            _Shield = _Armor * 1.5f;
        }

        /**
         * 몬스터 부모 클래스
         * 공통 특징을 다 적어놔라
         */
        [SerializeField] EColorType monsterColor;

        [Space] [SerializeField] EMonsterType monsterType;
        [SerializeField] EMonsterAttackType attackType;
        [SerializeField] ENorMonsterAbility norAbility;
        [SerializeField] EBossMonsterAbility bossAbility;

        internal ENorMonsterAbility GetNorAbility()
        {
            return norAbility;
        }

        ObscuredInt _MonAttack; // 몬스터 공격력

        ObscuredFloat _Max_HP; // 최대 체력
        ObscuredFloat m_current_HP; // 현재 체력

        internal ObscuredFloat CurrentHp
        {
            get
            {
                return m_current_HP;
            }
            set
            {
                /// 현재 체력은 0 밑으로 내려가지 않는다.
                if (value < 1f)
                {
                    m_current_HP = 0;
                }
                else
                {
                    m_current_HP = value; // 현재 남은 체력
                }
            }
        }

        ObscuredFloat _HealCounter = 1f; // 체력 회복력 감소
        ObscuredFloat _HealColorCounter = 1f; // 체력 회복력 감소

        ObscuredFloat _Armor = 0; // 몬스터 방어력 -> 총알 공격력에서 빼줌
        ObscuredFloat _Shield = 0;

        /// 실드 = 체력보다 먼저 감소됨. 실드 량은 몬스터 방어력의 150%.
        ObscuredFloat _moveSpeed; // 몬스터 이동 속도

        ObscuredFloat _ShieldPlusDam = 1f;

        /// <summary>
        /// 스페셜 카드에서 수정하는 몬스터 이동속도
        /// </summary>
        ObscuredFloat _Skilled_MoveSpeed = 1f; // 스킬로 추가 되는 몬스터 이동 속도

        /// <summary>
        /// 스페셜 카드에서 수정하는 방어력
        /// </summary>
        ObscuredFloat _Skilled_Armor = 1f; // 스킬로 깎아주는 방어력

        ObscuredFloat nomaTimer = 5f;
        private static readonly int MonAttack = Animator.StringToHash("monAttack");

        /// <summary>
        /// 얻어맞아서 체력 깎아줌 + 플로팅 대미지
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="fontColor"></param>
        void HitTheSpot(int cardIndex, float damage, Color fontColor)
        {
            /// 근데 이미지 비활성화면 무시해
            if (!mySprite.enabled)
            {
                return;
            }

            // 몬스터 콜라이더에 Hit하면 일단 가한 대미지로 기록한다.
            DamageDealt.RefreshDamegeDealt(cardIndex, damage);


            /// 쉴드가 남아있으면 쉴드부터 때림
            if (_Shield > 1)
            {
                /// 쉴드 추가 대미지가 있으면 적용
                /// 스페셜 카드 실드 추가 대미지 공격력의 120% colorTypeBae[5];
                _Shield -= damage * _ShieldPlusDam;

                /// 실드 대미지 플로팅
                DamageText hudText = LeanPool.Spawn(_hudDamageText, _hudDamageText.transform.parent);
                hudText.InitDamageFont(damage, Color.gray);
                hudText.transform.position = transform.position;

                return;
            }

            /// 일반 공격은 방어력 깎은 대미지를 넣어줌
            ForcedAttack(damage - (_Armor * _Skilled_Armor), fontColor);
        }

        /// <summary>
        /// 고정 대미지로 깎어
        /// </summary>
        /// <param name="value"></param>
        void ForcedAttack(float value, Color fontColor)
        {
            if (ReferenceEquals(_hudDamageText, null))
            {
                return;
            }

            /// 들어오는 대미지가 0 이상일때만 깎어
            if (value > 0)
            {
                /// 찐으로 체력 깎아줌
                CurrentHp -= value;
            }
            else
            {
                value = 0;
            }

            /// 대미지 플로팅
            //DamageText hudText = _hudDamageText.Spawn(_hudDamageText.transform).GetComponent<DamageText>();
            DamageText hudText = LeanPool.Spawn(_hudDamageText, _hudDamagePos);
            hudText.InitDamageFont(value, fontColor);
            //hudText.transform.position = Camera.main.WorldToScreenPoint(transform.position)
            hudText.transform.position = transform.position;
        }

        public event Action<Monster> OnDestory; // 몬스터 사망
        public event Action<int> OnAttack; // 성벽이 공격당함
        public event Action<float, float> OnRestore; // 스페셜 카드가 공격시 체력 회복 


        internal DamageText _hudDamageText;
        Transform _hudDamagePos;

        SpriteRenderer mySprite;
        Rigidbody2D rb;
        Collider2D coli;
        MonsterCollider childCollidr;
        Animator animator;

        private void Awake()
        {
            mySprite = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            coli = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
            childCollidr = GetComponentInChildren<MonsterCollider>();
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            /// 총알이 아니면 리턴
            if (!other.CompareTag("player_bullet"))
            {
                return;
            }

            /// 총알의 목표인지 확인 후 착탄했을 경우 
            if (!other.GetComponent<Bullet>().IsFactTarget(this, out int itemIndex))
            {
                return;
            }

            /// 1번 카드의 총알은 없애지 말아봐봐
            if (itemIndex == 1 || itemIndex == 40)
            {
                return;
            }

            /// 목표가 총알 한대 맞고 죽을 체력이면?
            if (mySprite.enabled && !IsAlive())
            {
                /// 죽어 몬스터
                DestoryMoster();
            }
        }

        /// <summary>
        /// 30% 만에 부활하신 주 예수
        /// </summary>
        ObscuredBool isJesus;

        /// <summary>
        /// 몬스터 으앙주금
        /// </summary>
        internal void DestoryMoster()
        {
            /// 죽어야하는데 부활 몬스터다?
            if (norAbility == ENorMonsterAbility.Buff_LivingDead)
            {
                if (!isJesus)
                {
                    /// TODO : 몬스터 부활 애니메이션 처리

                    // 30% 체력으로 회복
                    CurrentHp = _Max_HP * 0.3f;
                    Debug.LogError($"{CurrentHp} 의 체력으로 부활");
                    isJesus = true;
                    /// 악마카드 트리거 꺼버림
                    isDevilrun = false;
                    ReMonsterMove();
                    return;
                }
            }

            /// TODO : 몬스터 죽는 애니메이션 처리
            /// 
            //coli.isTrigger = true;
            coli.enabled = false;
            mySprite.enabled = false;
            /// 죽음의 메아리 발동시간 주기 위해 0.3초 뒤 콜라이더 삭제
            Invoke(nameof(InvoDisableChild), 0.3f);
            /// MonsterSet 에서 배열 빼줌.
            OnDestory?.Invoke(this);
            DestoryTimer();

            /// 죽음의 메아리 트리거가 있다면?
            switch (isDeathrattle)
            {
                case 33:
                    /// 몬스터 사망시 3x3 영역에 덫 생성
                    childCollidr.Deathrattle_33();
                    break;

                case 44:
                    /// 몬스터 사망시 3x3 영역에 덫 생성
                    childCollidr.Deathrattle_33();
                    break;

                default:
                    break;
            }
        }

        void InvoDisableChild()
        {
            if (!ReferenceEquals(childCollidr, null))
            {
                childCollidr.enabled = false;
            }
        }


        /// <summary>
        /// 수치를 지정하고 몬스터 생성
        /// </summary>
        /// <param name="hudDamageText">DamageText 붙여줘야함</param>
        /// <param name="maxHp">몬스터 시트 체력</param>
        /// <param name="armor">몬스터 시트 아머</param>
        /// <param name="buffindex">시작 버프 인덱스</param>
        /// <param name="debuffindex">시작 디-버프 인덱스</param>
        /// <param name="moveSpeed">내려오는 속도</param>
        /// <param name="monColor">몬스터 시트 칼라</param>
        internal void GenerateMonster(DamageText hudDamageText, float maxHp, float armor, int monDam = 1,
            int buffindex = -1, int debuffindex = -1, float buffvalue = 1f, float debuffvalue = 1f,
            EColorType monColor = EColorType.Red, float moveSpeed = 1f)
        {
            /// StartBuff에서 끌고 오는 것
            switch (buffindex)
            {
                case 11:
                    /// 몬스터 이동속도 10% 감소
                    if (!monsterType.Equals(EMonsterType.FinalBoss))
                    {
                        moveSpeed *= buffvalue;
                    }

                    break;
                case 12:
                    /// 보스 이동속도 10% 감소
                    if (monsterType.Equals(EMonsterType.FinalBoss))
                    {
                        moveSpeed *= buffvalue;
                    }

                    break;
                case 13:
                    /// 몬스터 '최대' 체력 5% 감소
                    if (!monsterType.Equals(EMonsterType.FinalBoss))
                    {
                        maxHp *= buffvalue;
                    }

                    break;
                case 14:
                    /// 보스 '최대' 체력 5% 감소
                    if (monsterType.Equals(EMonsterType.FinalBoss))
                    {
                        maxHp *= buffvalue;
                    }

                    break;
                default:
                    break;
            }

            switch (debuffindex)
            {
                case 5:
                    /// 몬스터 공격력 10% 증가
                    if (!monsterType.Equals(EMonsterType.FinalBoss))
                    {
                        monDam *= Mathf.RoundToInt(debuffvalue);
                    }

                    break;
                case 6:
                    /// 몬스터 공격속도 10% 증가
                    if (!monsterType.Equals(EMonsterType.FinalBoss))
                    {
                        castleAttackedCoolTime *= debuffvalue;
                    }

                    break;
                case 7:
                    /// 보스 공격력 10% 증가
                    if (monsterType.Equals(EMonsterType.FinalBoss))
                    {
                        monDam *= Mathf.RoundToInt(debuffvalue);
                    }

                    break;
                case 10:
                    /// 보스 공격속도 10% 증가
                    if (monsterType.Equals(EMonsterType.FinalBoss))
                    {
                        castleAttackedCoolTime *= debuffvalue;
                    }

                    break;
                case 11:
                    /// 몬스터 이동속도 10% 증가
                    if (!monsterType.Equals(EMonsterType.FinalBoss))
                    {
                        moveSpeed *= debuffvalue;
                    }

                    break;
                case 12:
                    /// 보스 이동속도 10% 증가
                    if (monsterType.Equals(EMonsterType.FinalBoss))
                    {
                        moveSpeed *= debuffvalue;
                    }

                    break;
                case 13:
                    /// 몬스터 '최대' 체력 5% 증가
                    if (!monsterType.Equals(EMonsterType.FinalBoss))
                    {
                        maxHp *= buffvalue;
                    }

                    break;
                case 14:
                    /// 보스 '최대' 체력 5% 증가
                    if (monsterType.Equals(EMonsterType.FinalBoss))
                    {
                        maxHp *= buffvalue;
                    }

                    break;

                default:
                    break;
            }

            // 플로팅 텍스트 오브젝트 풀
            _hudDamageText = hudDamageText;
            _hudDamagePos = _hudDamageText.transform.parent;

            monsterColor = monColor;
            // 몬스터 공격력
            _MonAttack = monDam;

            // 실드는 방어력의 1.5배
            _Armor = armor;
            _Shield = _Armor * 1.5f;
            // 체력 세팅
            _Max_HP = maxHp;
            CurrentHp = _Max_HP;
            // 몬스터가 하단으로 내려오는 시간
            _moveSpeed = 70f * moveSpeed;

            /// 일반 몬스터 특성
            switch (norAbility)
            {
                case ENorMonsterAbility.None:
                    break;
                case ENorMonsterAbility.Buff_Shield: // 체력의 50%의 실드
                    _Shield += maxHp / 2f;
                    break;

                case ENorMonsterAbility.Buff_Self_Heal: // 매 5초마다 최대체력 1% 회복
                    GameManager.instance.coTimer.SecondAction += NormalHealTimer;
                    break;

                case ENorMonsterAbility.Buff_Speed_Up: // 이동속도 1.5배
                    _moveSpeed *= 1.5f;
                    break;

                case ENorMonsterAbility.Everything:
                    break;
                default:
                    break;
            }

            /// 일반 몬스터인지? 보스몹인지? 언데드인지?
            switch (monsterType)
            {
                case EMonsterType.UnDead:
                    _Max_HP *= 1.3f;
                    _Armor *= 1.3f;
                    _moveSpeed *= 1.2f;
                    break;
                case EMonsterType.Normal:
                    break;
                case EMonsterType.FinalBoss:

                    /// 2번 보스 매 초 최대 체력 3% 회복
                    if (bossAbility.Equals(EBossMonsterAbility.Buff_Self_Heal))
                    {
                        GameManager.instance.coTimer.SecondAction += InfiniteTimer;
                    }

                    break;
                default:
                    break;
            }

            /// 생성 끝났으면 속도값 주고직선 아래로 이동
            DoMoveMonster();
        }

        /// <summary>
        /// 속박 스킬 발동중 (중복 가능)
        /// </summary>
        ObscuredBool isBinding;

        /// <summary>
        /// True 가 들어가면 리셋 타이머 $$ 하단 좌표 찍기
        /// False 가 들어가면 타이머 새로고침
        /// </summary>
        ObscuredBool isMove;

        internal ObscuredBool IsMove
        {
            get
            {
                return isMove;
            }
            set
            {
                if (value)
                {
                    /// 이동을 시작하면 공격 타이머 초기화.
                    Reset_Timer();
                    targetPos = new Vector2(0, -1f);
                }
                else
                {
                    Check_Timer();
                    /// 이동 멈춰 정지, 스턴, 속박
                    targetPos = Vector2.zero;
                    rb.velocity = Vector2.zero;
                }

                isMove = value;
            }
        }


        ObscuredBool isDevilrun;

        /// <summary>
        /// 악마 카드를 뽑으면 필드위 의 몬스터가 영향 받는다.
        /// </summary>
        internal void RunDevilSet(bool isTrue)
        {
            if (isTrue)
            {
                /// 악마가 나타났다
                isDevilrun = true;
                transform.DOLocalMove(new Vector3(0, 1.2f, 0), 3f)
                    .OnComplete(() =>
                    {
                        /// 트윈 종료 후
                        DestoryMoster();
                    });
            }
            else
            {
                /// 악마는 쓰러졌다.
                isDevilrun = false;
            }
        }


        Vector2 targetPos;

        /// <summary>
        /// 활성화 되면 직선 아래로 이동
        /// </summary>
        void DoMoveMonster()
        {
            mySprite.enabled = true;

            ObscuredFloat pos_X = Random.Range(-MonsterPoolManager.screen_x, MonsterPoolManager.screen_x);
            /// 화면 밖에 있는 위에 위치
            transform.localPosition = new Vector2(pos_X, 2.7f);

            ///targetPos = new Vector2(0, -Mathf.Abs(pos_X));
            IsMove = true;
            //transform.DOLocalMoveY(0, (5.0f * _moveSpeed)).SetEase(Ease.Linear);
        }

        private ObscuredFloat castle_current;
        private ObscuredFloat castleAttackedCoolTime = 2f; // 몬스터가 성벽 때리는 쿨타임
        private ObscuredBool iscastleEnded;

        /// <summary>
        /// 성벽에 처음 닿음
        /// </summary>
        ObscuredBool isFristAtk;

        public void FixedUpdateMe()
        {
            /// 악마 카드 발동중이면 업데이트 움직임은 멈춘다.
            if (isDevilrun)
            {
                IsMove = false;
            }

            /// 몬스터가 죽으면 움직임을 멈춘다
            if (!mySprite.enabled)
            {
                IsMove = false;
                return;
            }

            /// 성벽에 닿거나 / 상태가 정지라면?
            if (transform.position.y <= 0)
            {
                if (!isFristAtk)
                {
                    /// 공격 애니메이션
                    if (animator != null)
                    {
                        animator.SetBool(MonAttack, true);
                    }

                    /// 체력 깎는 이벤트
                    OnAttack?.Invoke(_MonAttack);
                    isFristAtk = true;
                }

                /// 쿨타임에 따라서 공격 후 2초 리셋
                if (iscastleEnded)
                {
                    /// 체력 깎는 이벤트
                    OnAttack?.Invoke(_MonAttack);
                    Reset_Timer();
                }

                IsMove = false;
                return;
            }

            rb.velocity = targetPos * Time.deltaTime * _moveSpeed * _Skilled_MoveSpeed;
        }

        private void Check_Timer()
        {
            if (castle_current > 0)
            {
                castle_current -= Time.deltaTime;
            }
            else if (!iscastleEnded)
            {
                End_Timer();
            }
        }

        private void End_Timer()
        {
            castle_current = 0;
            iscastleEnded = true;
        }


        private void Reset_Timer()
        {
            castle_current = castleAttackedCoolTime;
            iscastleEnded = false;
        }

        /// <summary>
        /// 현재 몬스터가 체력이 0 이상인가?
        /// </summary>
        /// <returns></returns>
        internal bool IsAlive()
        {
            if (CurrentHp > 0)
            {
                return true;
            }

            return false;
        }


        #region <상태이상 스킬 필드>

        /// <summary>
        /// 도트 타이머
        /// </summary>
        ObscuredFloat _DoT_delay;

        /// <summary>
        /// 스킬 딜레이 타이머
        /// </summary>
        ObscuredFloat _Skill_delay;

        ObscuredFloat _Self_Dot_Damage; // 자기 자신의 도트 대미지 가져오기
        ObscuredInt _myCardIndex;

        ObscuredBool isDoting; // [0번][3번] 도트 대미지 선착순 true 동안 다른 도트댐 무시
        ObscuredInt isDeathrattle = 0; // [33번] 죽음의 메아리 효과

        #endregion


        /// <summary>
        /// 전이 속성 랜덤 몬스터 탐색
        /// </summary>
        void RefleshMonsterRandom()
        {
            Monster neerMonster = null;

            /// 셔플 어레이하면 64개 배열 딱코 해놓은거 사라짐 해결방법 모색
            //MonsterPoolManager.ShuffleArray();
            for (int i = 0; i < MonsterPoolManager.monsters.Length; i++)
            {
                /// 백분율 * 100 합시다
                int rand = Random.Range(0, MonsterPoolManager.monsters.Length);

                /// _hudDamageText == null 이면 아직 리젠이 안된 것이다
                if (MonsterPoolManager.monsters[rand] == null ||
                    MonsterPoolManager.monsters[rand]._hudDamageText == null)
                {
                    continue;
                }

                neerMonster = MonsterPoolManager.monsters[rand];
                break;
            }

            /// 근처에 생존한 몬스터가 있다면 총알 소환
            if (!ReferenceEquals(neerMonster, null))
            {
                //Debug.LogWarning("총알 목표 : " + neerMonster.name);
                _Bullet.RunBullet(neerMonster);
            }
        }


        #region <어떤 총알인가 메서드>

        /// <summary>
        /// 전이공격 Despan 시키지 않은 총알 
        /// </summary>
        Bullet _Bullet;

        internal void OnDamaged_1(float dam, int likedcnt, Bullet bullet, out int constIndex)
        {
            _Bullet = bullet;
            /// 백분율 * 100 합시다
            float rand = Random.Range(0, 10000f);

            if (rand < 700f || likedcnt > 1) /// 700f 이다
            {
                /// 7% 확률로 전이 공격
                constIndex = likedcnt;
            }
            else
            {
                /// 평타 침
                constIndex = 4;
                OnDamaged(1, dam);
                return;
            }

            switch (likedcnt)
            {
                case 1:
                    /// 1번 카드로 대미지 받음 80%
                    OnDamaged(1, dam * 0.8f);
                    break;

                case 2:
                    /// 1번 카드로 대미지 받음 50%
                    OnDamaged(1, dam * 0.5f);
                    break;

                case 3:
                    /// 1번 카드로 대미지 받음 10%
                    OnDamaged(1, dam * 0.1f);
                    break;

                default:
                    break;
            }
        }

        internal void OnDamaged_40(float dam, int likedcnt, Bullet bullet)
        {
            _Bullet = bullet;
            switch (likedcnt)
            {
                case 1:
                    OnDamaged(40, dam);
                    break;

                case 2:
                    OnDamaged(40, dam * 0.8f);
                    break;

                case 3:
                    OnDamaged(40, dam * 0.6f);
                    break;

                case 4:
                    OnDamaged(40, dam * 0.4f);
                    break;

                case 5:
                    OnDamaged(40, dam * 0.2f);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 오버라이드
        /// </summary>
        /// <param name="eitem"></param>
        /// <param name="dam"></param>
        internal void OnDamaged(EquippableItem eitem, float dam, bool isMagic, EColorType bulletColor)
        {
            /// 일반 몬스터 특성 적용
            switch (norAbility)
            {
                case ENorMonsterAbility.Buff_Reflection: // 대미지의 3% 성벽에게 반사함
                    Debug.LogError($"7번 몬스터 대미지 반사 ${dam * 0.03f}");
                    OnAttack?.Invoke(Mathf.RoundToInt(dam * 0.03f));
                    break;

                default:
                    break;
            }

            /// 보스 몬스터 특성 적용
            switch (bossAbility)
            {
                case EBossMonsterAbility.Immune_Melee: // 물리 공격시 대미지를 입지 않음
                    if (!isMagic)
                    {
                        Debug.LogError($"0번 보스 물리 면역");
                        dam = 0;
                    }

                    break;

                case EBossMonsterAbility.Immune_Magic: // 마법 공격시 대미지를 입지 않음
                    if (isMagic)
                    {
                        Debug.LogError($"1번 보스 마법 면역");
                        dam = 0;
                    }

                    break;

                case EBossMonsterAbility.Buff_Reflection: // 입은 피해의 2%를 성벽에게 반사함
                    Debug.LogError($"5번 보스 대미지 반사 ${dam * 0.02f}");
                    OnAttack?.Invoke(Mathf.RoundToInt(dam * 0.02f));
                    break;

                default:
                    break;
            }

            /// 총알 색상 적용
            switch (bulletColor)
            {
                case EColorType.None:
                    break;
                case EColorType.Red:
                    /// 공격력 5% 상승 colorTypeBae[1];
                    // -> BulletController 에서
                    if (monsterColor.Equals(EColorType.Blue)) // 우세 공격력 15%
                    {
                        dam *= 1.15f;
                    }
                    else if (monsterColor.Equals(EColorType.Yellow)) // 열세 공격력 15%
                    {
                        dam *= 0.85f;
                    }

                    break;
                case EColorType.Yellow:
                    /// 몬스터의 체력회복률을 20% 감소시킴 colorTypeBae[2];
                    _HealColorCounter = eitem.colorpower;

                    if (monsterColor.Equals(EColorType.Red)) // 우세 공격력 15%
                    {
                        dam *= 1.15f;
                    }
                    else if (monsterColor.Equals(EColorType.Blue)) // 열세 공격력 15%
                    {
                        dam *= 0.85f;
                    }

                    break;
                case EColorType.Blue:
                    /// 공격속도가 5% 상승함 colorTypeBae[3];
                    // -> BulletController 에서
                    if (monsterColor.Equals(EColorType.Yellow)) // 우세 공격력 15%
                    {
                        dam *= 1.15f;
                    }
                    else if (monsterColor.Equals(EColorType.Red)) // 열세 공격력 15%
                    {
                        dam *= 0.85f;
                    }

                    break;
                case EColorType.Orange:
                    /// 공격시 공격력의 1%만큼 성벽 체력 회복 colorTypeBae[4];
                    OnRestore?.Invoke(dam * 0.01f, eitem.colorpower);

                    if (monsterColor.Equals(EColorType.Purple)) // 우세 공격력 15%
                    {
                        dam *= 1.15f;
                    }
                    else if (monsterColor.Equals(EColorType.Green)) // 열세 공격력 15%
                    {
                        dam *= 0.85f;
                    }

                    break;
                case EColorType.Purple:
                    /// 실드 추가 대미지 공격력의 120% colorTypeBae[5];
                    _ShieldPlusDam = eitem.colorpower;

                    if (monsterColor.Equals(EColorType.Green)) // 우세 공격력 15%
                    {
                        dam *= 1.15f;
                    }
                    else if (monsterColor.Equals(EColorType.Orange)) // 열세 공격력 15%
                    {
                        dam *= 0.85f;
                    }

                    break;
                case EColorType.Green:
                    /// 언데드 몬스터 공격력 공격력의 115% colorTypeBae[6];
                    if (monsterType.Equals(EMonsterType.UnDead))
                    {
                        dam *= eitem.colorpower;
                    }

                    if (monsterColor.Equals(EColorType.Orange)) // 우세 공격력 15%
                    {
                        dam *= 1.15f;
                    }
                    else if (monsterColor.Equals(EColorType.Purple)) // 열세 공격력 15%
                    {
                        dam *= 0.85f;
                    }

                    break;
                default:
                    break;
            }

            /// Bullet 한테서 Monster가 대미지 받음
            OnDamaged(eitem.ItemIndex, dam);
        }


        /// <summary>
        /// Bullet 한테서 Monster가 대미지 받음
        /// </summary>
        /// <param name="myCardIndex">총알을 생성한 스페셜 카드</param>
        /// <param name="dam">일반카드에서 가져오는 대미지</param>
        internal void OnDamaged(int myCardIndex, float dam)
        {
            /// TODO : 몬스터 타격시 살짝 뒤로 밀까?
            //transform.DOJump(transform.position, 0.1f, 1, 0.2f);
            /// 백분율 * 100 합시다
            float rand = Random.Range(0, 10000f);

            /// 스페셜 카드별 대미지 로직 처리
            switch (myCardIndex)
            {
                /**
                 *  도트
                 *  "공격시마다 해당 대상에게 추가 화염 대미지를 입힘.
                    틱당 공격력의 5%, 총 5틱동안 유지
                 */
                case 0:
                    if (!isDoting)
                    {
                        _Self_Dot_Damage = dam;
                        mySprite.color = Color.red;
                        isDoting = true;
                        /// 해당 몬스터 도트 댐
                        _DoT_delay = 4f; // 5틱 타이머
                        GameManager.instance.coTimer.SecondAction += Dot_5;
                    }

                    break;

                /**
                 * "공격대상 주변에 독을 풀어 도트 대미지를 입힘.
                    12초마다 시전, 5x5 영역, 틱당 공격력의 5%, 5틱동안 유지"
                 */
                case 3:
                    if (_Skill_delay != 0)
                    {
                        // 딜레이 동안은 일반 공격
                        HitTheSpot(myCardIndex, dam, Color.white);
                        return;
                    }

                    /// 12초마다 시전
                    if (!isDoting)
                    {
                        _Self_Dot_Damage = dam;
                        mySprite.color = Color.green;
                        isDoting = true;
                        /// 해당 몬스터 도트댐
                        _DoT_delay = 4f; // 5틱 타이머
                        GameManager.instance.coTimer.SecondAction += Dot_5;
                        /// 활성화하면서 범위 대미지 넘겨주기
                        childCollidr.SetColliderActivate(dam, 3);
                        /// 12초 타이머
                        _Skill_delay = 12f; // 스킬 딜레이 12초
                        GameManager.instance.coTimer.SecondAction += Skill_Timer;
                    }

                    break;


                /**"일정 시간마다 강한 공격으로 공격
                    7초마다 공격력의 120%의 대미지로 공격"
                */
                case 5:
                    if (_Skill_delay != 0)
                    {
                        // 딜레이 동안은 일반 공격
                        HitTheSpot(myCardIndex, dam, Color.white);
                    }
                    else
                    {
                        /// 7초마다 공격력의 120%의 대미지로 공격
                        HitTheSpot(myCardIndex, dam * 1.2f, Color.red);
                        _Skill_delay = 7f; // 스킬 딜레이 7초
                        GameManager.instance.coTimer.SecondAction += Skill_Timer;
                    }

                    return;


                /// 일정 확률로 몬스터를 중독시켜, 중독된 몬스터 사망시 전염 발동
                /// 7 % 의 확률로 몬스터에게 중독을 걸어 
                /// 해당 몬스터가 "사망시" 3x3 영역에 3초간 틱당 공격력의 40 % 의 대미지를 주는 전염병 발동.
                case 33:

                    /// 7% 확률로 진입 -> 중독 시의 공격력 넘겨줌 
                    if (isDeathrattle == 0 && rand < 700f)
                    {
                        mySprite.color = Color.blue;
                        isDeathrattle = 33;

                        /// 죽음의 메아리 세팅하기
                        childCollidr.DeathrattleActivate(dam, myCardIndex);
                    }

                    break;


                /**
                 *  연결
                 */


                case 1:
                    /**"일정 확률로 대상과 연결된 대상에게 전이 대미지를 입힘.
                        7%의 확률로 3명에게 전이, 대미지는 공격력의 80% → 50% → 10%로 감소"

                        `   -> 확률 계산은 IsFactTarget 에서
                    */

                    RefleshMonsterRandom();
                    break;


                case 40:
                    /**
                     * 공격시마다 전이공격으로 공격
                        공격시마다 100% → 80% → 60% → 40% → 20%의 전이공격*/
                    RefleshMonsterRandom();
                    break;


                /**
                 * 무작위 대미지
                 * 공격시마다 공격대상에게 무작위로 대미지를 입힘.
                    공격력의 70%~120% 사이의 랜덤 대미지를 입힘
                 */

                case 2:
                    rand = Random.Range(0.7f, 1.2f);
                    HitTheSpot(myCardIndex, dam * rand, Color.magenta);
                    return;

                /**
                 * 범위
                 */

                case 4:
                    /**공격시마다 대상 주위의 몬스터에게 스플래쉬 대미지를 입힘
                        공격대상 주변 3x3 영역에 공격력의 15% 스플래시 대미지를 입힘*/

                    /// 활성화하면서 범위 대미지 넘겨주기
                    childCollidr.SetColliderActivate(dam, 4);

                    break;


                case 27:
                    /** 공격시마다 공격 주변 대상 주변 3x3 영역에 공격력의 80%의 스플래시 대미지를 입힘. */
                    /// 활성화하면서 범위 대미지 넘겨주기
                    childCollidr.SetColliderActivate(dam, 27);

                    break;


                /**
                 * 추가 대미지
                 */


                case 12:
                    /** 체력이 가장 낮은 몬스터를 우선 공격하며 공격력의 10%의 추가대미지를 입힘 
                     BulletController.RefleshMonsterWave() 에서 관리 */
                    HitTheSpot(myCardIndex, dam * 1.1f, Color.blue);
                    return;


                case 15:
                    /** 몬스터의 체력이 50% 미만인 경우, 20%의 대미지를 추가로 입힘. */
                    if (CurrentHp < (_Max_HP / 2))
                    {
                        HitTheSpot(myCardIndex, dam * 1.2f, Color.red);
                        return;
                    }

                    break;

                case 25:
                    /** 체력 많은 몬스터 우선 공격 보스 추가대미지 20% */
                    if (monsterType.Equals(EMonsterType.FinalBoss))
                    {
                        HitTheSpot(myCardIndex, dam * 1.2f, Color.blue);
                        return;
                    }

                    break;

                /**
                 * 이동
                 */

                case 6:
                    /** 5%의 확률로 공격중인 몬스터의 이동속도를 20% 감소시킴.
                        최대 3회 중복 가능. */
                    if (rand < 500f)
                    {
                        _Skilled_MoveSpeed -= 0.2f;

                        if (_Skilled_MoveSpeed < 0.4f)
                        {
                            _Skilled_MoveSpeed = 0.4f;
                        }
                    }

                    break;

                case 7:
                    /** 3%의 확률로 공격중인 몬스터의 이동을 1초간 금지시킴. 중복 불가 */
                    if (rand < 300f)
                    {
                        /// 움직이고 있을때만 적용
                        if (IsMove)
                        {
                            IsMove = false;
                            CancelInvoke(nameof(ReMonsterMove));
                            Invoke(nameof(ReMonsterMove), 1f); // 1초 정지
                        }
                    }

                    break;

                case 18:
                    /** 15%의 확률로 몬스터의 이동속도를 30% 감소시키며, 
                     * 7%의 확률로 2초간 이동을 금지시킴. */
                    if (rand < 1500f)
                    {
                        // 몬스터의 이동속도를 30 % 감소
                        _Skilled_MoveSpeed -= 0.3f;

                        if (rand < 700f)
                        {
                            /// 움직이고 있을때만 적용
                            if (IsMove)
                            {
                                IsMove = false;
                                CancelInvoke(nameof(ReMonsterMove));
                                Invoke(nameof(ReMonsterMove), 2f); // 2초 정지
                            }
                        }
                    }

                    break;

                case 24:
                    /** 5%의 확률로 몬스터에게 공포를 걸어 1칸 뒤로 이동시킴 */
                    if (rand < 500f)
                    {
                        mySprite.color = Color.black;
                        transform.DOMoveY(transform.position.y + 0.5f, 1.6f).OnComplete(Move24Complete);
                        //transform.DOJump(tmpPos, 0.3f, 1, 0.6f);
                    }

                    break;


                case 31:
                    /** 12%의 확률로 3초간 해당 몬스터의 이동을 금지. 중복 가능.
                        → 속박에 걸린 몬스터에게 다시 속박이 발동되면 3초 추가 */
                    if (rand < 1200f)
                    {
                        if (isBinding)
                        {
                            CancelInvoke(nameof(ReMonsterMove));
                            Invoke(nameof(ReMonsterMove), 3f); // 3초 정지

                            // 딜레이 동안은 일반 공격
                            HitTheSpot(myCardIndex, dam, Color.white);
                            return;
                        }

                        /// 움직이고 있을때만 적용
                        if (IsMove)
                        {
                            IsMove = false;
                            isBinding = true;
                            CancelInvoke(nameof(ReMonsterMove));
                            Invoke(nameof(ReMonsterMove), 3f); // 3초 정지
                        }
                    }

                    break;


                /**
                 * 방어력 관통 공격
                 */

                case 10:
                    /** 8%의 확률로 공격시 방어력의 15%를 관통하는 공격을 시전함 
                     *  스페셜카드의 공격력이 10이고, 몬스터의 방어력이 10이면 대미지는 0을 입힘.
                        이 떄 스페셜카드의 방어력 관통력이 5라면?
                        몬스터의 방어력이 10-5=5가 되어 대미지가 5가 들어가가됨 
                        --> 몬스터의 방어력 감소
                    */
                    if (rand < 800f)
                    {
                        _Skilled_Armor = 0.85f;
                        HitTheSpot(myCardIndex, dam, Color.blue);
                        _Skilled_Armor = 1f;
                        return;
                    }

                    break;

                case 21:
                    /** 카드의 공격이 몬스터의 방어력 30%를 관통함. */
                    _Skilled_Armor = 0.7f;
                    HitTheSpot(myCardIndex, dam, Color.blue);
                    _Skilled_Armor = 1f;

                    return;

                case 51:
                    /** 모든 카드가 몬스터의 방어력의 40%를 관통 
                        -> 몬스터 생성자에 아머를 깎아주면 되는거 아?님 맞음!
                     */
                    /// -> MonsterPoolManager.Is51cardTrigger 에서 처리
                    break;


                /**
                 * 실드 삭제
                 */

                case 11:
                    /** 7%의 확률로 공격중인 몬스터의 실드를 삭제함 */
                    if (rand < 700f)
                    {
                        _Shield = 0;
                        Debug.LogWarning($"실드 삭제!");
                    }

                    break;


                /**
                 * 체력 비례 대미지 공격
                 */

                case 29:
                    /** 공격시마다 몬스터 체력의 1%의 고정대미지를 추가로 입힘. */
                    ForcedAttack((CurrentHp * 0.01f), Color.magenta);
                    break;


                /**
                 * 즉사 (보스 제외)
                 */

                case 32:
                    /** 4%의 확률로 공격중인 몬스터 즉시 사망.  */
                    if (rand < 400f)
                    {
                        if (!monsterType.Equals(EMonsterType.FinalBoss))
                        {
                            CurrentHp = 0;
                        }
                    }

                    break;

                case 44:
                    /** 7%의 확률로 공격적인 몬스터 즉시 사망. (보스 제외)
                     * 사망한 몬스터가 폭발하여 주변 반경 3x3 영역에 몬스터 체력의 5%의 고정대미지를 입힘 */
                    if (isDeathrattle == 0 && rand < 700f)
                    {
                        if (!monsterType.Equals(EMonsterType.FinalBoss))
                        {
                            CurrentHp = 0;
                            /// 죽음의 메아리 세팅
                            isDeathrattle = 44;
                            childCollidr.DeathrattleActivate(dam, myCardIndex);
                        }
                    }

                    break;


                /**
                 * 면역 (디버프 40% 적용)
                 */

                case 36: // 장착시 발동
                    /** 모든 카드에게 적용되는 디버프 효과를 40%만 적용
                        -> BulletController 에서 처리 */

                    break;


                /**
                 * 치유 효과
                 */

                case 23:
                    /** 공격 성공시마다 몬스터에게 치유 15% 감소 효과 
                        -> 치유 효과가 달려있는 몬스터 */

                    _HealCounter *= 0.85f;
                    break;

                case 48:
                    /** 공격 성공시마다 몬스터에게 치유 40% 감소 효과 */
                    _HealCounter *= 0.6f;
                    break;

                default:
                    break;
            }

            // 특수 카드 아니면 일반적으로 체력 깎아
            HitTheSpot(myCardIndex, dam, Color.white);
        }


        /// <summary>
        /// 공포 걸어서 뒤로 점프가 완료되면 다시 움직여
        /// </summary>
        void Move24Complete()
        {
            mySprite.color = Color.white;
            IsMove = true;
        }

        /// <summary>
        /// 이동하게 만들고 멈추 상태 풀어줌
        /// </summary>
        private void ReMonsterMove()
        {
            IsMove = true;
            isBinding = false;
        }

        #endregion


        #region <스킬 딜레이 타이머>

        /// <summary>
        /// 12초마다 시전 case 3:
        /// 혹은 7초마다 시전 case 5:
        /// </summary>
        /// <param name="delay"></param>
        private void Skill_Timer(float delay)
        {
            _Skill_delay -= delay;

            if (_Skill_delay < 0)
            {
                _Skill_delay = 0;
                GameManager.instance.coTimer.SecondAction -= Skill_Timer;
            }
        }

        #endregion


        #region <외부의 광역 도트댐 전용 메서드>

        /// <summary>
        /// 덫을 활성화 했을 때, 몬스터가 이걸 박으면 해당 효과 가능 걸루다가 적용
        /// </summary>
        /// <param name="dam"></param>
        /// <param name="skillIndex"></param>
        internal void OnDamagedOtherMon(float dam, int skillIndex)
        {
            switch (skillIndex)
            {
                case 3:
                    if (!isDoting)
                    {
                        _Self_Dot_Damage = dam;
                        mySprite.color = Color.green;
                        isDoting = true;
                        _DoT_delay = 4f; // 5틱 타이머
                        GameManager.instance.coTimer.SecondAction += Dot_5;
                    }

                    break;

                case 16:
                    if (!isDoting)
                    {
                        _Self_Dot_Damage = dam;
                        mySprite.color = Color.yellow;
                        isDoting = true;
                        _DoT_delay = 6f; // 7틱 타이머
                        GameManager.instance.coTimer.SecondAction += Dot_7;
                    }

                    break;

                case 28:
                    if (!isDoting)
                    {
                        _Self_Dot_Damage = dam;
                        mySprite.color = Color.cyan;
                        isDoting = true;
                        _DoT_delay = 4f; // 5틱 타이머
                        GameManager.instance.coTimer.SecondAction += Do28t_5;
                    }

                    break;

                case 33:
                    if (!isDoting)
                    {
                        _Self_Dot_Damage = dam;
                        mySprite.color = Color.blue;
                        isDoting = true;
                        _DoT_delay = 2f; // 3틱 타이머
                        GameManager.instance.coTimer.SecondAction += Dot_3;
                    }

                    break;

                case 4:
                    /// 4번 카드 - 공격력의 15% 대미지
                    HitTheSpot(4, dam * 0.15f, Color.cyan);

                    break;


                case 13:
                    /// 공격력의 150%의 대미지를 일괄적으로 입힘. 폭발과 동시에 사라짐.
                    HitTheSpot(13, dam * 1.5f, Color.cyan);
                    break;


                case 27:
                    /// 27번 카드 - 공격력의 80% 대미지
                    HitTheSpot(27, dam * 0.8f, Color.cyan);
                    break;

                case 46:
                    /** 15초마다 임의의 3x3 영역에 150%의 대미지를 가진 번개 소환, 1.5f
                        번개가 지면에 닿은 뒤 해당 영역 주변 8x8 영역에 80%의 스플래시 대미지  0.8f
                     */
                    HitTheSpot(46, dam * 0.8f, Color.cyan);

                    break;

                case 64: // 46 카드의 2번째 공격은 64로 받아준다.
                    if (!isDoting)
                    {
                        isDoting = true;
                        Invoke(nameof(InvoEnd), 1.0f);
                        HitTheSpot(46, dam * 1.5f, Color.cyan);
                    }

                    break;

                case 26:
                    /** 20초마다 화면에 존재하는 모든 몬스터에게 공격력의 30%의 대미지를 입힘 
                     */
                    if (!isDoting)
                    {
                        isDoting = true;
                        Invoke(nameof(InvoEnd), 1.0f);
                        HitTheSpot(26, dam * 0.3f, Color.cyan);
                    }

                    break;

                case 37:
                    /** 7%의 확률로 모든 몬스터에게 공격력의 80%의 대미지를 입힘
                     * -> 확률은 BulletController 에서 처리
                     */
                    if (!isDoting)
                    {
                        isDoting = true;
                        Invoke(nameof(InvoEnd), 1.0f);
                        HitTheSpot(37, dam * 0.8f, Color.cyan);
                    }

                    break;

                case 30:
                    /** 5%의 확률로 2초간 모든 몬스터가 스턴에 걸림
                   * -> 확률은 BulletController 에서 처리
                     */
                    /// 움직이고 있을때만 적용
                    if (IsMove)
                    {
                        IsMove = false;
                        CancelInvoke(nameof(ReMonsterMove));
                        Invoke(nameof(ReMonsterMove), 2f); // 2초 정지
                    }

                    break;

                case 39:
                    /** 매 11번째 공격에서 모든 몬스터에게 150%의 대미지를 입힘 
                    */
                    if (!isDoting)
                    {
                        isDoting = true;
                        Invoke(nameof(InvoEnd), 1.0f);
                        HitTheSpot(39, dam * 1.5f, Color.cyan);
                    }

                    break;

                case 49:
                    /** 20초마다 모든 몬스터의 이동을 2초간 금지시킴 
                    */

                    /// 움직이고 있을때만 적용
                    if (IsMove)
                    {
                        IsMove = false;
                        CancelInvoke(nameof(ReMonsterMove));
                        Invoke(nameof(ReMonsterMove), 2f); // 2초 정지
                    }

                    break;

                case 44:
                    /** 7%의 확률로 공격적인 몬스터 즉시 사망. 
                    * 사망한 몬스터가 폭발하여 주변 반경 3x3 영역에 몬스터 체력의 5%의 고정대미지를 입힘 */
                    if (!isDoting)
                    {
                        isDoting = true;
                        Invoke(nameof(InvoEnd), 1.0f);
                        ForcedAttack((_Max_HP * 0.05f), Color.magenta);
                    }

                    break;


                default:
                    break;
            }
        }

        /// <summary>
        /// 1초 후에 도트 댐 꺼준다.
        /// </summary>
        void InvoEnd()
        {
            isDoting = false;
        }

        #endregion


        /// <summary>
        /// 매 5초마다 최대체력 1% 회복
        /// </summary>
        /// <param name="delay"></param>
        void NormalHealTimer(float delay)
        {
            nomaTimer -= delay;

            if (nomaTimer < 0)
            {
                Debug.LogError($"매 5초마다 최대체력 1% 회복 {_Max_HP * 0.03f}");

                float tmpHp = CurrentHp + (_Max_HP / 0.01f);
                /// 체력회복 감소율 적용
                tmpHp *= _HealCounter * _HealColorCounter;
                /// 회복된 양이 최대 체력을 넘으면 최대체력 만큼만
                if (tmpHp >= _Max_HP)
                {
                    CurrentHp = _Max_HP;
                }
                else /// 안 넘으면 회복량만큼
                {
                    CurrentHp = tmpHp;
                }

                nomaTimer = 5f;
            }
        }


        /// <summary>
        /// 매 초마다 보스의 최대 체력의 3%씩 회복
        /// </summary>
        void InfiniteTimer(float delay)
        {
            float tmpHp = CurrentHp + (_Max_HP * 0.03f);
            /// 체력회복 감소율 적용
            tmpHp *= _HealCounter * _HealColorCounter;
            Debug.LogError($"2번 보스 매 초 최대 체력 3% 회복 {_Max_HP * 0.03f}");

            /// 회복된 양이 최대 체력을 넘으면 최대체력 만큼만
            if (tmpHp >= _Max_HP)
            {
                CurrentHp = _Max_HP;
            }
            else /// 안 넘으면 회복량만큼
            {
                CurrentHp = tmpHp;
            }
        }


        #region <직접적인 대미지 도트 대미지>

        /// <summary>
        /// 0번카드 5틱 도트 댐 타이머
        /// </summary>
        /// <param name="delay"></param>
        private void Dot_5(float delay)
        {
            _DoT_delay -= delay;
            /// 0번 카드 - 틱당 공격력의 5%, 총 5틱동안 유지
            HitTheSpot(0, _Self_Dot_Damage * 0.05f, Color.green);


            if (_DoT_delay < 0)
            {
                mySprite.color = Color.white;
                _DoT_delay = 0;
                GameManager.instance.coTimer.SecondAction -= Dot_5;
                isDoting = false;
            }
        }

        /// <summary>
        /// 28번 스페셜 5틱 도트댐
        /// </summary>
        /// <param name="delay"></param>
        private void Do28t_5(float delay)
        {
            _DoT_delay -= delay;
            /// 28번 카드 - 틱당 공격력의 80%, 총 5틱동안 유지
            HitTheSpot(28, _Self_Dot_Damage * 0.8f, Color.green);

            if (_DoT_delay < 0)
            {
                mySprite.color = Color.white;
                _DoT_delay = 0;
                GameManager.instance.coTimer.SecondAction -= Do28t_5;
                isDoting = false;
            }
        }


        /// <summary>
        /// 3틱 도트 댐 타이머
        /// </summary>
        /// <param name="delay"></param>
        private void Dot_3(float delay)
        {
            _DoT_delay -= delay;
            /// 33번 카드 - 틱당 공격력의 40%, 총 3틱동안 유지
            HitTheSpot(33, _Self_Dot_Damage * 0.4f, Color.green);

            if (_DoT_delay < 0)
            {
                mySprite.color = Color.white;
                _DoT_delay = 0;
                GameManager.instance.coTimer.SecondAction -= Dot_3;
                isDoting = false;
            }
        }

        /// <summary>
        /// 7틱 도트 댐 타이머
        /// </summary>
        /// <param name="delay"></param>
        private void Dot_7(float delay)
        {
            _DoT_delay -= delay;
            /// 16번 카드 - 틱당 공격력의 10%, 총 7틱
            HitTheSpot(16, _Self_Dot_Damage * 0.1f, Color.green);

            if (_DoT_delay < 0)
            {
                mySprite.color = Color.white;
                _DoT_delay = 0;
                GameManager.instance.coTimer.SecondAction -= Dot_7;
                isDoting = false;
            }
        }

        #endregion


        /// <summary>
        /// 몬스터 죽을때 타이머 다 떼줌
        /// </summary>
        void DestoryTimer()
        {
            GameManager.instance.coTimer.SecondAction -= Dot_3;
            GameManager.instance.coTimer.SecondAction -= Dot_5;
            GameManager.instance.coTimer.SecondAction -= Dot_7;
            GameManager.instance.coTimer.SecondAction -= Do28t_5;
            GameManager.instance.coTimer.SecondAction -= Skill_Timer;
            GameManager.instance.coTimer.SecondAction -= InfiniteTimer;
            GameManager.instance.coTimer.SecondAction -= NormalHealTimer;
        }

        /// <summary>
        /// 몬스터 비활성화인때 타이머 다 떼버리기
        /// </summary>
        private void OnDisable()
        {
            DestoryTimer();
        }


        /// <summary>
        /// 몬스터 태그에 중복되는 태그 있는지 검사
        /// </summary>
        void MonsterTagDupucateCheak()
        {
            ENorMonsterAbility skillTag = ENorMonsterAbility.DeBf_AtkSpeed_Down | ENorMonsterAbility.Buff_Reflection;
            string sadas = skillTag.ToString();
            Debug.LogWarning(sadas);

            if (skillTag.HasFlag(ENorMonsterAbility.DeBf_AtkSpeed_Down))
            {
                Debug.LogWarning("있음");
            }
            else
            {
                Debug.LogWarning("없음");
            }
        }
    }
}