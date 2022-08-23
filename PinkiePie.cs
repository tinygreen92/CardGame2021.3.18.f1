using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleGame;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System;
using Random = UnityEngine.Random;

namespace GoogleGame
{
    /// <summary>
    /// 버튼 기합
    /// </summary>
    public class PinkiePie : MonoBehaviour
    {
        [SerializeField] StartBuffManager sbm;
        [SerializeField] BattleController battleController;
        [SerializeField] BulletController[] Zones;
        [Space]
        [SerializeField] TextMeshProUGUI[] tmps;

        // 하나의 버튼이 정상적으로 눌려지면 5개의 불렛컨트롤 모두 값이 들어가야함
        // (현재 색상이 아니더라도 나중에 변할 가능성 있기 때문)

        /// <summary>
        /// 색상별 강화 단계 보관
        /// </summary>
        Dictionary<int, int> colorDic = new Dictionary<int, int>()
        {
            {(int)EColorType.Red, 0},
            {(int)EColorType.Yellow, 0},
            {(int)EColorType.Blue, 0},
            {(int)EColorType.Orange, 0},
            {(int)EColorType.Purple, 0},
            {(int)EColorType.Green, 0},
        };

        /// <summary>
        /// 색상 강화 1,2,3,4 단계 sp 소모값
        /// </summary>
        private ObscuredInt[] spendColorSp = new ObscuredInt[4] { 100, 200, 400, 700 };

        public void BestPony(int eColor)
        {
            SuperPony(eColor);
        }

        void SuperPony(int eColor)
        {
            /// 해당 색상 소모 SP 반환
            if (colorDic.TryGetValue(eColor, out int spIndex))
            {
                if (spIndex > 3)
                {
                    /// TODO : 업그레이드 끝나면 버튼 회색으로 만들고 MAX 출력
                    return;
                }

                /// SP 소모할 수 있는가?
                if (battleController.ColorSpendSp(spendColorSp[spIndex]))
                {
                    colorDic[eColor] = ++spIndex;      // 강화단계 올려줌
                    JaWolrd(eColor, spIndex);
                    // 텍스트 갱신
                    if (spIndex < 4)
                    {
                        tmps[eColor - 1].text = $"{spendColorSp[spIndex]}";
                    }
                    else
                    {
                        tmps[eColor - 1].text = "MAX";
                    }

                }
                else
                {
                    return;
                }
            }
        }

        void JaWolrd(int colorIndex, int spIndex)
        {
            for (int i = 0; i < Zones.Length; i++)
            {
                Zones[i].SetColorBae(colorIndex, spIndex);
            }
        }


        /// <summary>
        /// 7번 보스(색상 변경) 등장시 20초마다 색상 랜덤
        /// </summary>
        internal void BossRandomColor()
        {
            for (int i = 0; i < Zones.Length; i++)
            {
                Zones[i].BossColorChange();
            }
        }


        /// <summary>
        /// 4번 보스(슬로우 디버프) 등장시 모든 스페셜 카드에서 효과 발동
        /// </summary>
        internal void BossAllDebuff()
        {
            for (int i = 0; i < Zones.Length; i++)
            {
                Zones[i].BossAllDebuff();
            }
        }

        /// <summary>
        /// 매 웨이브 시작마다 2초간 스페셜카드 공격속도 변화 1.55 or 0.6 
        /// </summary>
        internal void Wave2speed40(float value)
        {
            for (int i = 0; i < Zones.Length; i++)
            {
                Zones[i].BuffWave2speed40(value);
            }
            /// 2초뒤 원래대로 공속 돌려줌.
            GameManager.instance.coTimer.SecondAction += StartDeBuff15Skill;
        }

        ObscuredFloat _DebuffDelay = 2f;

        /// <summary>
        /// 매 웨이브 시작마다 2초간 스페셜카드 공격속도
        /// </summary>
        /// <param name="delay"></param>
        private void StartDeBuff15Skill(float delay)
        {
            _DebuffDelay -= delay;
            if (_DebuffDelay < 0)
            {
                GameManager.instance.coTimer.SecondAction -= StartDeBuff15Skill;
                _DebuffDelay = 2f;
                Wave2speed40(1f);
            }
        }


        /// <summary>
        /// 10초마다 얼음 공격으로 스페셜카드 2개를 얼림
        /// (스페셜 카드의 특성, 스킬, 쿨타임이 일시정시
        /// 단, 과거의 발동했던 특수능력, 스킬, 쿨타임이랑, 장착했을때 효과는 예외)
        /// </summary>
        internal void BossIce()
        {
            int rand, rand2;
            rand = Random.Range(0, 5);
            Zones[rand].BossIceCream();

            while (true)
            {
                rand2 = Random.Range(0, 5);
                /// 1과 2가 다를 때만 루프 탈출
                if (rand != rand2)
                {
                    Zones[rand2].BossIceCream();
                    break;
                }
            }
        }


        /// <summary>
        /// 38 카드에서 12초마다 스킬 쿨타임 줄여줌
        /// </summary>
        internal void CoolSkillDown()
        {
            for (int i = 0; i < Zones.Length; i++)
            {
                Zones[i].ExtraCoolDown();
            }
        }

        /// <summary>
        /// MonsterPoolManager 로 sbm 던져줌.
        /// </summary>
        /// <returns></returns>
        internal List<float> GetAppleLoona()
        {
            return sbm.GetAppleBear();
        }


        /// <summary>
        /// (시작 버프 적용) 누르면 총알 발싸
        /// 12.POP_CANVAS_StartBuff - POP_StartBuff - StartBuffPENNEL - BTN_Summit
        /// </summary>
        public void ȕȕȕȕȕȖȕȖȖȕȕȕȕȖȕȕȕȖȖȖȖȕȖȕȕȖȕȕȕȖȕȕȖȖȖȕȖȕȕȖȖȖȕȕȕȕȖ()
        {
            var appleloosa = GetAppleLoona();

            /// 디버프 3개랑 버프 3개
            Applebuuu((int)appleloosa[0], appleloosa[1]);
            AppleDEbuuu((int)appleloosa[2], appleloosa[3]);

            for (int i = 0; i < 5; i++)
            {
                Zones[i].ShootingStar(appleloosa);
            }
        }

        /// <summary>
        /// 버프 적용
        /// </summary>
        void Applebuuu(int index, float value1)
        {
            switch (index)
            {
                case 6:
                    /// 몬스터 처치시 스페셜카드 공격속도 3초간 5% 증가
                    battleController.On6Buff = true;
                    break;
                case 7:
                    /// 몬스터 1킬 당, 성벽 '최대' 체력의 1%를 쉴드로 생성
                    battleController.On7Buff = true;
                    break;
                case 8:
                    /// SP 획득량 10% 증가
                    battleController.PlusPlusSp(value1);
                    break;
                case 9:
                    /// 성벽 '최대' 체력 10% 증가
                    battleController.MAX_CASTLE_HP.Value *= value1;
                    break;
                case 10:
                    /// 게임 시작 SP 2배 지급
                    battleController.SP.Value *= Mathf.RoundToInt(value1);
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
                case 8:
                    /// SP 획득량 10% 감소
                    battleController.PlusPlusSp(value1);
                    break;
                case 9:
                    /// 성벽 체력 10% 감소
                    battleController.MAX_CASTLE_HP.Value *= value1;
                    break;

                default:
                    break;
            }
        }



        private void OnDisable()
        {
            GameManager.instance.coTimer.SecondAction -= StartDeBuff15Skill;
        }



    }
}