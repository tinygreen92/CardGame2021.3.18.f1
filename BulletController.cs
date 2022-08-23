using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Lean.Pool;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using GoogleGame;

public enum EColorType
{
    None,
    Red,
    Yellow,
    Blue,
    Orange,
    Purple,
    Green,
}

namespace GoogleGame
{
    public class BulletController : MonoBehaviour, IDropHandler
    {
        private EquippableItem _MyCard;

        [Header(" - 몬스터 풀 매니저")] [SerializeField]
        MonsterPoolManager poolManager;

        [Header(" - 스크립트 땡겨 올 것")] [SerializeField]
        PinkiePie pinky;

        [SerializeField] BattleController BC;

        [Header(" - 전체를 덮는 콜라이더")] [SerializeField]
        MonsterExtraCollider allCollider;

        [Header(" - 덫 능력은 스페셜 카드 5개 중 하나에 귀속된다.")] [SerializeField]
        MonsterExtraCollider extra52Trap;

        [SerializeField] MonsterExtraCollider extra33Trap;

        [Header(" - 스페셜 카드 이미지")] [SerializeField]
        Image cardSlotImg;

        [Header(" - 드래그 되는 카드 정보")] [SerializeField]
        NormalCardIDaggable daggableCard;

        [SerializeField] CardGenerator cardGenerator;

        [Space] [Header(" - Lean Pool 에 등록된 총알 프리팹")] [SerializeField]
        Bullet myBullet;


        /// 몬스터 킬 카운터
        ObscuredInt _Killcount;

        /// 스페셜 카드로 강화되는 총알
        ObscuredFloat _speed = 400f;

        ObscuredFloat _damage = 100f;
        
        /// <summary>
        ///스페셜카드 치명타 피해량 : 기본 배율 공격력의 1.5배 
        /// </summary>
        ObscuredFloat _criticalDam = 1.5f;

        /// <summary>
        /// 스페셜카드 치명타 확률 3% 증가   : 100 나누기 값으로 쓴다 100.00 %로 보라
        /// </summary>
        ObscuredInt _criticalPer = 0;

        
        /// 일반 카드 색상이 적용된다면?
        ObscuredFloat _colorUp_Damage = 1f;

        ObscuredFloat _colorUp_Speed = 1f;

        /// <summary>
        /// 스페셜 카드에 입혀진 총알 색상
        /// </summary>
        EColorType colorType;

        ObscuredFloat[] colorTypeBae = new ObscuredFloat[7] {1f, 1f, 1f, 1f, 1f, 1f, 1f};

        /// 스페셜 카드 스킬 적용 계수
        ObscuredFloat _skilled_Damage = 1f;

        ObscuredFloat _skilled_Speed = 1f;

        /// 장착시 발동 대미지 계수
        ObscuredFloat _equip_Damage = 1f;

        ObscuredFloat _equip_Speed = 1f;

        /// 시작 버프로 속도 올려준다
        ObscuredFloat _staBuff_Damage = 1f;

        ObscuredFloat _staBuff_Speed = 1f;

        /// 모든 디버프 감소
        /// 모든 카드에게 적용되는 디버프 효과의 효율을 40%만 적용
        ObscuredFloat _equip_36card = 1f;


        /// <summary>
        /// 14번 카드 - 매 7회 공격시마다 120%의 대미지로 공격
        /// </summary>
        ObscuredInt Skill_14Cnt;

        /// <summary>
        /// 39번 카드 - 매 11번째 공격에서 모든 몬스터에게 150%의 대미지를 입힘 
        /// </summary>
        ObscuredInt Skill_39Cnt;

        /// <summary>
        /// 42번 카드 - 세 번째 공격을 가할 때 마다 공격력의 30%의 추가 고정대미지를 입힘
        /// </summary>
        ObscuredInt Skill_42Cnt;

        /// <summary>
        /// 8번 카드 - 10회 공격시마다 공격속도 5%씩 상승. 합연산 
        /// </summary>
        ObscuredInt Skill_8Cnt;

        ObscuredVector3 tmpPos;

        private void Start()
        {
            _Killcount = BC.Killcount.Value;
        }


        /// <summary>
        /// 총알 발싸 위치 지정해주고 발싸!
        /// </summary>
        /// <param name="appleloosa">시작버프에서 가져온 값 6개</param>
        internal virtual void ShootingStar(List<float> appleloosa)
        {
            Applebuuu((int)appleloosa[0], appleloosa[1]);
            AppleDEbuuu((int)appleloosa[2], appleloosa[3]);
            /// 카메라 포지션 위치 세팅
            // tmpPos = Camera.main.ScreenToWorldPoint(transform.position);
            tmpPos = transform.position;

            StartCoroutine(gogogogo());
        }

        /// <summary>
        /// 몬스터 처치시 스페셜카드 공격속도 3초간 5% 증가
        /// </summary>
        /// <param name="value"></param>
        internal void SetMySpeedSpeed(float value)
        {
            _staBuff_Speed = value;
        }


        ObscuredBool isCastle4Skill;
        ObscuredBool isCastle5Skill;

        /// <summary>
        /// 버프 적용
        /// </summary>
        void Applebuuu(int index, float value1)
        {
            switch (index)
            {
                case 0:
                    /// 스페셜카드 공격력 10% 증가
                    _staBuff_Damage = value1;
                    break;
                case 1:
                    /// 스페셜카드 공격속도 10% 증가
                    _staBuff_Speed = value1;
                    break;
                case 2:
                    /// 스페셜카드 치명타 확률 3% 증가
                    _criticalPer += Mathf.RoundToInt(value1);
                    break;
                case 3:
                    /// 스페셜카드 치명타 피해량 15% 증가
                    _criticalDam += value1;
                    break;
                case 4:
                    /// 성벽 잃은 체력 1%당 공격력 1% 증가
                    isCastle4Skill = true;
                    break;
                case 5:
                    /// 성벽 잃은 체력 1%당 공격속도 1% 증가
                    isCastle5Skill = true;
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
            switch (index)
            {
                case 0:
                    /// 스페셜카드 공격력 10% 감소
                    _staBuff_Damage = value1;
                    break;
                case 1:
                    /// 스페셜카드 공격속도 10% 감소
                    _staBuff_Speed = value1;
                    break;
                case 2:
                    /// 스페셜카드 치명타 확률 5% 감소
                    _criticalPer -= (int)value1;
                    break;
                case 3:
                    /// 스페셜카드 치명타 피해량 20% 감소
                    _criticalDam -= value1;
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// TODO : 카드가 공격 시작하는 옵션 정해야함
        /// 1. 게임 시작후 몇초 뒤 공격 1.8
        /// 2. 공속 어떻게? 0.6
        /// </summary>
        /// <returns></returns>
        IEnumerator gogogogo()
        {
            WaitForSeconds delay = new WaitForSeconds(0.6f);

            yield return delay;
            yield return delay;
            yield return delay; // 게임 시작 1.8 후에 시작

            while (true)
            {
                RefleshMonsterWave();
                yield return delay;
            }
        }


        /// <summary>
        /// 호출될 때 가까운 놈 한명 공격
        /// </summary>
        protected void RefleshMonsterWave()
        {
            Vector3 tmpPosmonsters = Vector3.zero;

            if (_MyCard.ItemIndex.Equals(12))
            {
                /**
                 * 체력 낮은거 탐색
                 */
                float lowestHealth = Mathf.Infinity;
                Monster lowestMonster = null;

                for (int i = 0; i < MonsterPoolManager.monsters.Length; i++)
                {
                    if (MonsterPoolManager.monsters[i] == null)
                    {
                        continue;
                    }
                    else if (MonsterPoolManager.monsters[i]._hudDamageText == null)
                    {
                        continue;
                    }

                    float currentHealth = MonsterPoolManager.monsters[i].CurrentHp;

                    /// 제일 낮은 체력 갱신
                    if (currentHealth < lowestHealth)
                    {
                        lowestHealth = currentHealth;
                        lowestMonster = MonsterPoolManager.monsters[i];
                    }
                }

                /// 근처에 살아있는 몬스터가 있다면 총알 소환
                if (!ReferenceEquals(lowestMonster, null))
                {
                    LaunchBullet(lowestMonster);
                }
            }
            else if (_MyCard.ItemIndex.Equals(25))
            {
                /**
                 * 체력 가장 높은거
                 */
                float highestHealth = 0;
                Monster highestMonster = null;

                for (int i = 0; i < MonsterPoolManager.monsters.Length; i++)
                {
                    if (MonsterPoolManager.monsters[i] == null)
                    {
                        continue;
                    }
                    else if (MonsterPoolManager.monsters[i]._hudDamageText == null)
                    {
                        continue;
                    }

                    /// 제일 낮은 체력 갱신
                    if (MonsterPoolManager.monsters[i].CurrentHp > highestHealth)
                    {
                        highestMonster = MonsterPoolManager.monsters[i];
                        highestHealth = highestMonster.CurrentHp;
                    }
                }

                /// 근처에 살아있는 몬스터가 있다면 총알 소환
                if (!ReferenceEquals(highestMonster, null))
                {
                    LaunchBullet(highestMonster);
                }
            }
            else if (_MyCard.ItemIndex.Equals(17))
            {
                /** case 17: 공격력의 70%의 구체를 3개씩 발사 
                    가장 가까운 순위 1, 2, 3에게 
                        -> BulletController 에서 처리
                */

                Monster[] neerMonsters = new Monster[4];

                for (int j = 0; j < 3; j++)
                {
                    float shotDist = Mathf.Infinity;

                    for (int i = 0; i < MonsterPoolManager.monsters.Length; i++)
                    {
                        if (MonsterPoolManager.monsters[i] == null)
                        {
                            continue;
                        }
                        else if (MonsterPoolManager.monsters[i]._hudDamageText == null)
                        {
                            continue;
                        }
                        else
                        {
                            tmpPosmonsters = MonsterPoolManager.monsters[i].transform.position;
                        }

                        for (int k = 0; k < 3; k++)
                        {
                            if (MonsterPoolManager.monsters[i] == neerMonsters[k] &&
                                !ReferenceEquals(neerMonsters[k], null))
                            {
                                i++;
                                continue;
                            }
                        }

                        float distToMonster = Vector3.SqrMagnitude(tmpPos - tmpPosmonsters);

                        /// 제일 가까운 놈은 얘다!
                        if (distToMonster < shotDist)
                        {
                            shotDist = distToMonster;
                            neerMonsters[j] = MonsterPoolManager.monsters[i];
                        }
                    }

                    /// 가까운 몹 공격!
                    if (!ReferenceEquals(neerMonsters[j], null))
                    {
                        LaunchBullet(neerMonsters[j]);
                    }
                }
            }
            else
            {
                float shotDist = Mathf.Infinity;
                Monster neerMonster = null;

                for (int i = 0; i < MonsterPoolManager.monsters.Length; i++)
                {
                    if (MonsterPoolManager.monsters[i] == null)
                    {
                        continue;
                    }
                    else if (MonsterPoolManager.monsters[i]._hudDamageText == null)
                    {
                        continue;
                    }
                    else
                    {
                        tmpPosmonsters = MonsterPoolManager.monsters[i].transform.position;
                    }

                    float distToMonster = Vector3.SqrMagnitude(tmpPos - tmpPosmonsters);

                    /// 제일 가까운 놈은 얘다!
                    if (distToMonster < shotDist)
                    {
                        shotDist = distToMonster;
                        neerMonster = MonsterPoolManager.monsters[i];
                    }
                }

                /// 근처에 살아있는 몬스터가 있다면 총알 소환
                if (!ReferenceEquals(neerMonster, null))
                {
                    LaunchBullet(neerMonster);
                }
            }
        }

        /// <summary>
        /// 전투가 시작되기 전에 세팅이 되어야 함
        /// </summary>
        /// <param name="myCard"></param>
        internal void RefreshMySprite(EquippableItem myCard)
        {
            _MyCard = myCard;
            cardSlotImg.sprite = _MyCard.Icon;
        }


        /// <summary>
        /// 장착시 카드 효과 발동
        /// </summary>
        /// <param name="index"></param>
        internal void EqipApplySkill(int index)
        {
            switch (index)
            {
                case 36:
                    /// 공격력 감소 디버프 (DeBf_Damage_Down) : 
                    /// 공격력의 5% -> 2%를 감소시킴
                    /// 
                    /// 공격속도 감소 디버프 (DeBf_AtkSpeed_Down) : 
                    /// 공격속도의 10% -> 4%를 감소시킴
                    /// 
                    /// 슬로우 디버프 (Attack_SLOW) : 
                    /// 10초마다 공격력과 공격속도를 20 -> 8% 감소시킴
                    _equip_36card = 0.4f;

                    Debug.LogError("카드 36 장착 효과");
                    break;


                case 20:
                    /** 배치된 카드 포함, 양 옆의 카드의 공격속도를 20% 증가시킴. */
                    _equip_Speed *= 1.2f;
                    Debug.LogError("카드 20 장착 효과");
                    break;

                case 50:
                    /** 모든 카드의 공격력 20%, 공격속도 10% 증가 */
                    Debug.LogError("카드 50 장착 효과");
                    _equip_Damage *= 1.2f;
                    _equip_Speed *= 1.1f;
                    break;

                default:
                    _equip_Damage = 1f;
                    _equip_Speed = 1f;
                    _equip_36card = 1f;
                    break;
            }
        }

        /// <summary>
        /// 20초마다 모든 색상이 적용된 스페셜카드의 색상을 랜덤으로 바꿈
        /// </summary>
        internal void BossColorChange()
        {
            /// 색 없는 카드는 변경하지 않음
            if (cardSlotImg.color.Equals(Color.white))
            {
                return;
            }

            /// 해당 스페셜 카드 색상 변경
            cardSlotImg.color = GetMyColor(InsertRandomColor());
        }

        /// <summary>
        /// 10초 마다 줄어드는 4번 보스 디버프
        /// </summary>
        internal void BossAllDebuff()
        {
            // 4번 보스는 10초마다 모든 스페셜카드의 공격력과 공격속도를 20% 감소시킴
            if (_equip_Damage > 0)
            {
                _equip_Damage *= 0.8f * _equip_36card;
            }

            if (_equip_Speed > 0)
            {
                _equip_Speed *= 0.8f * _equip_36card;
            }
        }

        /// <summary>
        /// 15 스타트 버프 : 매 웨이브 시작마다 2초간 스페셜카드 공격속도 40% 감소
        /// </summary>
        internal void BuffWave2speed40(float speedValue)
        {
            Debug.LogError("15 스타트 버프 : 매 웨이브 시작마다 2초간 스페셜카드 공격속도 40% 감소");
            _staBuff_Speed = speedValue;
        }

        /// <summary>
        /// 10초마다 아이스크림 효과
        /// </summary>
        internal void BossIceCream()
        {
            /// 특성, 스킬, 쿨타임이 일시정시
            GameManager.instance.coTimer.SecondAction += Boss_Timer;
            isIce = true;
        }

        /// <summary>
        /// 아이스 디버프 걸리면 기능 정지
        /// </summary>
        ObscuredBool isIce;

        /// <summary>
        /// 총알 생성 후 발사
        /// </summary>
        void LaunchBullet(Monster neerMon)
        {
            /// 악마카드 발동중이거나? 보스 아이스 디버프가 걸리면 -> 총알 생성 안함
            if (MonsterPoolManager.isRunDevil || isIce)
            {
                return;
            }

            Bullet bullet = LeanPool.Spawn(myBullet, myBullet.transform.parent);
            if (bullet != null)
            {
                var transform1 = bullet.transform;
                transform1.localScale = Vector3.one;
                transform1.position = tmpPos;

                /// 정상적으로 총알이 발싸 되었다면 특수능력 발동 
                ActivateSpecialSkill();

                /// 조건형 시작버프 적용
                if (isCastle4Skill)
                {
                    _staBuff_Damage = 1f + BC.GetPhoga();
                }
                else if (isCastle5Skill)
                {
                    _staBuff_Speed = 1f + BC.GetPhoga();
                }

                Debug.Log($"댐{_damage}/ 색상강화{_colorUp_Damage}/ " +
                          $"스킬댐{_skilled_Damage}/ 장착댐{_equip_Damage}/ 버프댐{_staBuff_Damage}" +
                          $" = 합계댐 : {_damage * _colorUp_Damage * _skilled_Damage * _equip_Damage * _staBuff_Damage}");

                /// 타겟 지정해주고 발싸!
                bullet.RunBullet
                (neerMon, // Monster 스크립트 <타겟>
                    (_damage * _colorUp_Damage * _skilled_Damage * _equip_Damage * _staBuff_Damage), // 총알 대미지
                    (_speed * _colorUp_Speed * _skilled_Speed * _equip_Speed * _staBuff_Speed), // 총알 스피드
                    _MyCard, colorType, _equip_36card); // 장착  스크립트
            }
        }

        /// <summary>
        /// 트랩 발동시 true 가 되서 중복 트랩 방지해줌
        /// </summary>
        ObscuredBool isTrapped;


        /// <summary>
        /// 총알이 발사될때 타이머 개념 스킬 발동 체크
        /// </summary>
        private void ActivateSpecialSkill()
        {
            if (isTrapped)
            {
                return;
            }

            float rand = Random.Range(0, 10000f);

            /// 덫 종류 스킬의 위치 초기화.
            switch (_MyCard.ItemIndex)
            {
                case 16:
                    /**
                     * "20초마다 랜덤구역에 덫을 설치해 일정시간동안 대미지를 입힘.
                        5x2 영역에 2초간 유지되는 덫을 설치하여 몬스터가 밟으면 공격력의 10%의
                        대미지를 7틱동안 입힘.  "
                     */
                    /// 콜라이더 활성화
                    extra52Trap.SetColliderActivate((_damage), 16);
                    extra52Trap.transform.localPosition = BC.GetRandomTrapPos();
                    /// 타이머 온
                    _Delay = 20f; //20초마다 랜덤구역에 덫을 설치
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;
                    /// 2초 뒤 덫 꺼줌
                    CancelInvoke(nameof(TurnOffTrap_52));
                    Invoke(nameof(TurnOffTrap_52), 2f); //5x2 영역에 2초간 유지되는 덫을 설치

                    break;

                case 28:
                    /** 
                     * 15초마다 3x3 영역에 5초간 유지되는 불구덩이를 생성하여 
                     * 공격력의 80%의 대미지를 입힘. 5틱간 유지
                     */
                    /// 대미지랑 스킬 넘버 넘겨주기
                    extra33Trap.SetColliderActivate((_damage), 28);
                    extra33Trap.transform.localPosition = BC.GetRandomTrapPos_33();
                    /// 타이머 온
                    _Delay = 15f;
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;
                    /// 5초 뒤 덫 꺼줌
                    CancelInvoke(nameof(TurnOffTrap_33));
                    Invoke(nameof(TurnOffTrap_33), 5f);
                    break;

                case 13:
                    /**"몬스터가 밟으면 폭발하는 지뢰를 설치
                        20초마다 랜덤 위치에 생성, 몬스터가 밟으면 3x3 영역에 공격력의 150%의
                        대미지를 일괄적으로 입힘. 폭발과 동시에 사라짐."
                     */
                    /// 대미지랑 스킬 넘버 넘겨주기
                    extra33Trap.SetColliderActivate((_damage), 13);
                    extra33Trap.transform.localPosition = BC.GetRandomTrapPos_33();
                    /// 타이머 온
                    _Delay = 20f;
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;
                    /// 밟을 때 덫 꺼줌
                    CancelInvoke(nameof(TurnOffTrap_33));
                    Invoke(nameof(TurnOffTrap_33), 19f);
                    break;


                case 26:
                    /** 20초마다 화면에 존재하는 모든 몬스터에게 공격력의 30%의 대미지를 입힘 
                        -> BulletController 에서 처리
                     */

                    /// 대미지랑 스킬 넘버 넘겨주기
                    allCollider.SetALLActivate((_damage), 26);
                    /// 타이머 온
                    _Delay = 20f;
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;
                    ///대미지 입히고 즉시(0.6초) 꺼주기
                    CancelInvoke(nameof(TurnOffTrap_All));
                    Invoke(nameof(TurnOffTrap_All), 0.6f);
                    break;


                case 37:
                    /** 7%의 확률로 모든 몬스터에게 공격력의 80%의 대미지를 입힘 
                        -> BulletController 에서 처리
                     */
                    if (rand < 700)
                    {
                        /// 대미지랑 스킬 넘버 넘겨주기
                        allCollider.SetALLActivate((_damage), 37);
                        ///대미지 입히고 즉시(0.6초) 꺼주기
                        CancelInvoke(nameof(TurnOffTrap_All));
                        Invoke(nameof(TurnOffTrap_All), 0.6f);
                    }


                    break;


                case 46:
                    /** 15초마다 임의의 3x3 영역에 150%의 대미지를 가진 번개 소환,
                        번개가 지면에 닿은 뒤 해당 영역 주변 8x8 영역에 80%의 스플래시 대미지 
                     */
                    extra33Trap.transform.localPosition = BC.GetRandomTrapPos_33();
                    /// 8x8 영역 리셋
                    extra33Trap.Reset46Triger();
                    /// 대미지랑 스킬 넘버 넘겨주기
                    extra33Trap.SetColliderActivate((_damage), 46);
                    /// 타이머 온
                    _Delay = 15f;
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;
                    break;


                case 14:
                    /** 매 7회 공격시마다 120%의 대미지로 공격 
                     */
                    Skill_14Cnt += 1;

                    if (Skill_14Cnt > 6)
                    {
                        Skill_14Cnt = -1;
                        _skilled_Damage = 1.2f;
                    }
                    else
                    {
                        _skilled_Damage = 1f;
                    }

                    break;

                case 22:
                    /** 매 10초마다 기본 공격력이 10%씩 증가함. 
                     */
                    /// 타이머 온
                    _Delay = 10f;
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;
                    /// 공격력 10% 합연산 증가
                    _skilled_Damage += 0.1f;
                    break;

                case 39:
                    /** 10회 공격시마다 모든 몬스터에게 강력한 대미지와 함께 넉백시킴
                        매 11번째 공격에서 모든 몬스터에게 150%의 대미지를 입힘
                     -> BulletController 에서 처리
                    */
                    Skill_39Cnt += 1;

                    if (Skill_39Cnt > 10)
                    {
                        Skill_39Cnt = 0;
                        Debug.LogWarning("매 11번째 공격에서 모든 몬스터에게 150%의 대미지를 입힘 ");
                        /// 대미지랑 스킬 넘버 넘겨주기
                        allCollider.SetALLActivate((_damage), 39);
                        ///대미지 입히고 즉시(0.6초) 꺼주기
                        CancelInvoke(nameof(TurnOffTrap_All));
                        Invoke(nameof(TurnOffTrap_All), 0.6f);
                    }

                    break;


                case 42:
                    /** 세 번째 공격을 가할 때 마다 공격력의 30%의 추가 고정대미지를 입힘 
                     -> BulletController 에서 처리 */
                    Skill_42Cnt += 1;

                    if (Skill_42Cnt > 2)
                    {
                        Skill_42Cnt = 0;
                        _skilled_Damage = 1.3f;
                    }
                    else
                    {
                        _skilled_Damage = 1f;
                    }

                    break;


                case 49:
                    /** 20초마다 모든 몬스터의 이동을 2초간 금지시킴 
                     -> BulletController 에서 처리 */

                    /// 대미지랑 스킬 넘버 넘겨주기
                    allCollider.SetALLActivate(0, 49);
                    /// 타이머 온
                    _Delay = 20f;
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;
                    ///대미지 입히고 즉시(0.6초) 꺼주기
                    CancelInvoke(nameof(TurnOffTrap_All));
                    Invoke(nameof(TurnOffTrap_All), 0.6f);
                    break;

                case 30:
                    /** 5%의 확률로 2초간 모든 몬스터가 스턴에 걸림 */
                    if (rand < 500f)
                    {
                        /// 대미지랑 스킬 넘버 넘겨주기
                        allCollider.SetALLActivate(0, 30);
                        ///대미지 입히고 즉시(0.6초) 꺼주기
                        CancelInvoke(nameof(TurnOffTrap_All));
                        Invoke(nameof(TurnOffTrap_All), 0.6f);
                    }

                    break;


                case 8:
                    /** 10회 공격시마다 공격속도 5%씩 상승. 합연산 
                     -> BulletController 에서 처리 */

                    Skill_8Cnt += 1;

                    if (Skill_8Cnt % 10 == 0)
                    {
                        _skilled_Speed += 0.05f;
                        Debug.LogWarning($"10회 공격시 {_skilled_Speed} 스피드 업");
                    }

                    break;

                case 35:
                    /** 10초마다 3초간 공격속도 100% 증가 
                     -> BulletController 에서 처리 */

                    _skilled_Speed = 2f;
                    CancelInvoke(nameof(StopBulletSpeedUp));
                    Invoke(nameof(StopBulletSpeedUp), 3f);

                    /// 타이머 온
                    _Delay = 10f;
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;

                    break;

                case 9:
                    /** 랜덤상자는 몬스터 체력과 동일 20초마다 생성
                     *  상자 처치시 몬스터 SP의 1.5배 지급 */
                    poolManager.InitBoxMonster();

                    /// 타이머 온
                    _Delay = 20f;
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;
                    break;

                case 19:
                    /** 15초마다 현재 웨이브 획득 SP의 1.5배를 자동으로 충전함. 
                     -> BulletController 에서 처리 */
                    BC.ChargeSP(1.5f);
                    /// 타이머 온
                    _Delay = 15f;
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;
                    break;

                case 34:
                    /** 10초마다 3초간 공격력 100% 증가 ( 2배 ) 
                     -> BulletController 에서 처리 */

                    _skilled_Damage = 2f;
                    CancelInvoke(nameof(StopBulletPower));
                    Invoke(nameof(StopBulletPower), 3f);

                    /// 타이머 온
                    _Delay = 10f;
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;

                    break;

                case 41:
                    /** 일정 시간마다 버닝상태로 전환하여 강력한 공격
                        20초마다 3초간 공격력 100% 증가, 공격속도 100% 증가
                     * -> BulletController 에서 처리 */

                    _skilled_Speed = 2f;
                    _skilled_Damage = 2f;


                    CancelInvoke(nameof(StopBulletSpeedUp));
                    Invoke(nameof(StopBulletSpeedUp), 3f);

                    CancelInvoke(nameof(StopBulletPower));
                    Invoke(nameof(StopBulletPower), 3f);

                    /// 타이머 온
                    _Delay = 20f;
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;

                    break;

                case 43:
                    /** 몬스터 1마리 처치시마다 
                     *  공격력 1%, 공격속도 0.5%씩 증가.
                        최대 공격력 200% 공격속도 100% 
                    -> BulletController 에서 처리 */

                    if (BC.Killcount.Value < _Killcount)
                    {
                        _Killcount -= 1;

                        if (_skilled_Damage < 2f)
                        {
                            _skilled_Damage += 0.01f;
                        }

                        if (_skilled_Speed < 2f)
                        {
                            _skilled_Speed += 0.02f;
                        }
                    }

                    break;

                case 45:
                    /** 20초마다 무료로 카드 1장을 지급
                        카드가 3칸 모두 차있는 경우 카드 1장을 뽑을 수 있는 SP 지급 
                        -> BulletController 에서 처리 */
                    cardGenerator.IsFullHand();

                    /// 타이머 온
                    _Delay = 20f;
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;

                    break;


                case 38:
                    /** 일정 시간마다 카드의 스킬 쿨타임을 감소시킴
                    12초마다 모든 카드의 스킬 쿨타임을 1초 감소시킴
                    */
                    pinky.CoolSkillDown();

                    /// 타이머 온
                    _Delay = 12f;
                    GameManager.instance.coTimer.SecondAction += Trapped_Timer;
                    isTrapped = true;
                    break;

                case 47:
                    /** 몬스터 사망시마다 성벽의 최대체력의 3%씩 회복 
                     -> BC.KillMonsterBadly() 에서 처리 */
                    if (!BC.On47Skill)
                    {
                        BC.On47Skill = true;
                    }

                    break;


                default:
                    break;
            }
        }

        /// <summary>
        /// 공속을 1배속으로 되돌려줌
        /// </summary>
        private void StopBulletSpeedUp()
        {
            _skilled_Speed = 1f;
        }

        /// <summary>
        /// 공격력 정상화 
        /// </summary>
        private void StopBulletPower()
        {
            _skilled_Damage = 1f;
        }


        void TurnOffTrap_All()
        {
            allCollider.TurnOffTheTrap();
        }

        void TurnOffTrap_52()
        {
            extra52Trap.TurnOffTheTrap();
        }

        void TurnOffTrap_33()
        {
            extra33Trap.TurnOffTheTrap();
        }

        /// <summary>
        /// 본래 스킬 타이머
        /// </summary>
        ObscuredFloat _Delay = 0;

        /// <summary>
        /// 스킬 타이머 끝나야 능력 재발동
        /// </summary>
        /// <param name="delay"></param>
        private void Trapped_Timer(float delay)
        {
            /// 보스 디버프 아이스 상태면 타이머 감소하지 않는다.
            if (isIce)
            {
                return;
            }

            _Delay -= delay;
            if (_Delay < 0)
            {
                GameManager.instance.coTimer.SecondAction -= Trapped_Timer;
                _Delay = 0;
                isTrapped = false;
            }
        }


        ObscuredFloat _BossDelay = 2f;

        /// <summary>
        /// 보스 아이스 효과 2초 고정
        /// </summary>
        /// <param name="delay"></param>
        private void Boss_Timer(float delay)
        {
            _BossDelay -= delay;
            if (_BossDelay < 0)
            {
                GameManager.instance.coTimer.SecondAction -= Boss_Timer;
                _BossDelay = 2f;
                isIce = false;
            }
        }


        /// <summary>
        /// 스킬 쿨다운 돌려줌
        /// </summary>
        internal void ExtraCoolDown()
        {
            if (_Delay > 0)
            {
                _Delay -= 1f;
            }
        }


        /// <summary>
        /// 스페셜 카드 5개 따리 위치에
        /// 각각의 일반 카드 드롭하면?
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrop(PointerEventData eventData)
        {
            /// 스페셜 카드에 장착시 효과
            NormalCard dragCopyCard = daggableCard.GetDeepCard();

            /// 카드 슬롯 색상과 드롭하는 색상이 다르면?
            if (cardSlotImg.color != GetMyColor(dragCopyCard.colorType))
            {
                /// 스페셜카드 비어있을 때
                if (cardSlotImg.color == Color.white)
                {
                    /// 흰색 카드에 KK 카드 넣으면 삭제
                    if (dragCopyCard.numType.Contains("KK"))
                    {
                        /// 적용된 히어로 카드 색상을 삭제시킴
                        dragCopyCard.colorType = null;
                    }
                    else if (dragCopyCard.numType.Contains("K"))
                    {
                        /// 삽입된 스페셜카드 카드 색상을 랜덤으로 바꿈
                        dragCopyCard.colorType = InsertRandomColor();
                    }
                    else
                    {
                        /// 카드 공격력 증가시켜주고 + 색상 적용
                        dragCopyCard.colorType = ApplyCardDam(dragCopyCard);
                    }
                }
                /// 드롭 카드 색과 다르지만 비어있지 않을 때
                else
                {
                    ///// 근데 K 카드는 색깔이 달라도 적용할거야
                    //if (dragCopyCard.numType.Contains("K"))
                    //{
                    //    /// 삽입된 스페셜카드 카드 색상을 랜덤으로 바꿈
                    //    while (true)
                    //    {
                    //        var tmp = InsertRandomColor();

                    //        if (dragCopyCard.colorType != tmp)
                    //        {
                    //            dragCopyCard.colorType = tmp;
                    //            break;
                    //        }
                    //    }

                    //}
                    //else if (dragCopyCard.numType.Contains("KK"))
                    //{
                    //    /// 적용된 히어로 카드 색상을 삭제시킴
                    //    dragCopyCard.colorType = null;
                    //}
                    //else
                    //{
                    //    /// 드롭하는 색상 안맞으면 아무 일도 없다
                    //    return;
                    //}

                    return;
                }
            }
            /// 카드 슬롯과 드랍 카드 색상 일치 -> 카드 삽입
            else
            {
                /// 카드 공격력 증가시켜주고 + 색상 적용
                dragCopyCard.colorType = ApplyCardDam(dragCopyCard);
            }

            /// 해당 스페셜 카드 색상 변경
            cardSlotImg.color = GetMyColor(dragCopyCard.colorType);

            /// 포개지는 카드를 핸드에서 빼줌
            cardGenerator.RemoveOwned3CardPool(daggableCard.GetRemovedCard(out int posIndex), posIndex);
        }


        /// <summary>
        /// 실제 카드 공격력 적용
        /// </summary>
        /// <param name="dragCopyCard"></param>
        /// <returns></returns>
        ObscuredString ApplyCardDam(NormalCard dragCopyCard)
        {
            /// 공격력 및 스탯 수치 변경
            switch (dragCopyCard.numType)
            {
                case "J":
                    /// 삽입된 스페셜카드의 치명타 확률 1% 증가 (합연산)
                    _criticalPer += 100;
                    break;

                case "Q":
                    /// 삽입된 스페셜카드의 치명타 피해 30% 증가 (합연산)
                    _criticalDam += 0.3f;
                    break;

                case "K":
                    /// 삽입된 스페셜카드 카드 색상을 랜덤으로 바꿈
                    while (true)
                    {
                        var tmp = InsertRandomColor();

                        if (dragCopyCard.colorType != tmp)
                        {
                            dragCopyCard.colorType = tmp;
                            break;
                        }
                    }

                    break;
                case "KK":
                    /// 적용된 히어로 카드 색상을 삭제시킴
                    dragCopyCard.colorType = null;

                    break;

                case "JJ":
                    /// 삽입된 스페셜카드의 치명타 확률 3% 증가
                    _criticalPer += 300;

                    break;
                case "QQ":
                    /// 삽입된 스페셜카드의 치명타 피해 70% 증가
                    _criticalDam += 0.7f;

                    break;

                case "A":

                    _damage += 11;

                    break;


                default:

                    int tmpDam = int.Parse(dragCopyCard.numType);

                    /// 합성된 카드의 숫자가 21 이면 실제 적용되는 공격력을 400 % 증가시켜.
                    if (tmpDam == 21)
                    {
                        tmpDam *= 5;
                    }
                    /// 합성된 카드의 숫자가 20 이면 실제 적용되는 공격력을 200 % 증가시켜.
                    else if (tmpDam == 20)
                    {
                        tmpDam *= 3;
                    }
                    /// 합성된 카드의 숫자가 15 이상이면 실제 적용되는 공격력을 100 % 증가시켜.
                    else if (tmpDam >= 15)
                    {
                        tmpDam *= 2;
                    }
                    /// 합성된 카드의 숫자가 10 이상이면 실제 적용되는 공격력을 50% 증가시켜.
                    else if (tmpDam >= 10)
                    {
                        tmpDam = Mathf.RoundToInt(tmpDam * 1.5f);
                    }
                    else
                    {
                        /// 1배 
                    }

                    /// 일반 숫자 대미지 2~21
                    _damage += tmpDam;

                    break;
            }

            return dragCopyCard.colorType;
        }

        /// <summary>
        /// 해당 칼라의 색상 능력치 강화
        /// </summary>
        /// <param name="colorIndex"></param>
        /// <param name="spIndex"></param>
        internal void SetColorBae(int colorIndex, int spIndex)
        {
            switch (spIndex)
            {
                case 1:
                    colorTypeBae[colorIndex] = 1.1f;
                    break;
                case 2:
                    colorTypeBae[colorIndex] = 1.3f;
                    break;
                case 3:
                    colorTypeBae[colorIndex] = 1.6f;
                    break;
                case 4:
                    colorTypeBae[colorIndex] = 2f;
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// 6가지 무지개 색상 적용 하기
        /// </summary>
        /// <param name="cardColor"></param>
        /// <returns></returns>
        Color GetMyColor(string cardColor)
        {
            Debug.LogWarning($"색상 적용 : {cardColor}");

            switch (cardColor)
            {
                case "Red":
                    /// 스페셜 카드 공격력 5% 상승 colorTypeBae[1];
                    /// 빨간색의 기본 특성은 공격력 5% 증가 
                    _colorUp_Damage = 1.05f * colorTypeBae[1];
                    _colorUp_Speed = 1f;

                    colorType = EColorType.Red;
                    return new Color(244 / 255f, 67 / 255f, 54 / 255f);

                case "Yellow":
                    /// 몬스터의 체력회복률을 20% 감소시킴 colorTypeBae[2];
                    /// 몬스터 체력 회복률 20% 에서 10% 증가한 22% 감소
                    /// -> Monster.cs에서 관리
                    _MyCard.colorpower = 0.2f * colorTypeBae[2];

                    _colorUp_Damage = 1f;
                    _colorUp_Speed = 1f;

                    colorType = EColorType.Yellow;
                    return new Color(253 / 255f, 216 / 255f, 53 / 255f);

                case "Blue":
                    /// 스페셜 카드 공격속도가 5% 상승함 colorTypeBae[3];
                    /// 공격속도 5% 에서 10% 증가한 5.5% 증가
                    _colorUp_Damage = 1f;
                    _colorUp_Speed = 1.05f * colorTypeBae[3];

                    colorType = EColorType.Blue;
                    return new Color(33 / 255f, 150 / 255f, 243 / 255f);

                case "Orange":
                    /// 스페셜카드가 공격시 공격력의 1%만큼 성벽 체력 회복 colorTypeBae[4];
                    /// 1% 체력 회복에서 10% 증가한 1.1% 체력 회복
                    _MyCard.colorpower = colorTypeBae[4];

                    _colorUp_Damage = 1f;
                    _colorUp_Speed = 1f;

                    colorType = EColorType.Orange;
                    return new Color(255 / 255f, 152 / 255f, 0);

                case "Purple":
                    /// 스페셜 카드 실드 추가 대미지 공격력의 120% colorTypeBae[5];
                    /// 실드 추가 대미지 20% 에서 10% 증가한 22% 실드 추가 대미지
                    _MyCard.colorpower = 1.2f * colorTypeBae[5];

                    _colorUp_Damage = 1f;
                    _colorUp_Speed = 1f;

                    colorType = EColorType.Purple;
                    return new Color(156 / 255f, 39 / 255f, 176 / 255f);

                case "Green":
                    /// 언데드 몬스터 공격력 공격력의 115% colorTypeBae[6];
                    /// 언데드 추가 대미지 15% 에서 10% 증가한 15.2% 언데드 추가 대미지
                    /// -> Monster.cs에서 관리
                    _MyCard.colorpower = 1.15f * colorTypeBae[6];

                    _colorUp_Damage = 1f;
                    _colorUp_Speed = 1f;

                    colorType = EColorType.Green;
                    return new Color(76 / 255f, 175 / 255f, 80 / 255f);


                default:
                    /// 카드 색상이 바뀌면 일단 초기화.
                    _colorUp_Damage = 1f;
                    _colorUp_Speed = 1f;

                    colorType = EColorType.None;
                    return Color.white;
            }
        }

        /// <summary>
        /// 컬러 랜덤 스트링 리턴
        /// </summary>
        /// <returns></returns>
        private ObscuredString InsertRandomColor()
        {
            int rand = Random.Range(0, 6);

            switch (rand)
            {
                case 0:
                    return "Red";
                case 1:
                    return "Yellow";
                case 2:
                    return "Blue";
                case 3:
                    return "Orange";
                case 4:
                    return "Purple";
                case 5:
                    return "Green";
                default:
                    return null;
            }
        }


        private void OnDisable()
        {
            GameManager.instance.coTimer.SecondAction -= Trapped_Timer;
            GameManager.instance.coTimer.SecondAction -= Boss_Timer;
        }
    }
}