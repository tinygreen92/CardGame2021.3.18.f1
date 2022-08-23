using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using GoogleGame;

namespace GoogleGame
{
    public class MonsterCollider : MonoBehaviour
    {
        protected ObscuredFloat _CurrentDamage; // 깡 대미지 가져오기
        protected ObscuredInt _SkillIndx;

        ObscuredFloat _DeathDamage = 0; // 죽음의 메아리 대미지
        
        protected BoxCollider2D magicCol;
        protected SpriteRenderer sr;



        private void Awake()
        {
            SetMonster();
        }

        protected virtual void SetMonster()
        {
            magicCol = GetComponent<BoxCollider2D>();
            magicCol.size = Vector2.zero;

            sr = GetComponent<SpriteRenderer>();
            sr.enabled = false;
        }


        /// <summary>
        /// 외부에서 인덱스 콜라이더 활성화 시켜주기
        /// </summary>
        /// <param name="index"></param>
        internal virtual void SetColliderActivate(float dam, int skillIndx)
        {
            //Debug.LogError(" 몬스터 자식 콜라이더 활성화");
            _SkillIndx = skillIndx;
            /// 세팅 대미지 가져오기
            _CurrentDamage = dam;
            magicCol.size = Vector2.one;

            switch (_SkillIndx)
            {
                case 3:
                    /// 1.1f 은 5x5 의 크기 이다
                    transform.localScale = Vector2.one * 1.1f;
                    break;

                case 4:
                    /**공격시마다 대상 주위의 몬스터에게 스플래쉬 대미지를 입힘
                        공격대상 주변 3x3 영역에 공격력의 15% 스플래시 대미지를 입힘*/
                    transform.localScale = Vector2.one * 0.66f;
                    break;

                case 27:
                    /** 공격시마다 공격 주변 대상 주변 3x3 영역에 공격력의 80%의 스플래시 대미지를 입힘. */
                    transform.localScale = Vector2.one * 0.66f;
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// 광역 장판 깔아주고 대미지 받게 하쟝
        /// </summary>
        /// <param name="collision"></param>
        private void OnTriggerStay2D(Collider2D collision)
        {
            /// 몬스터가 아니면 리턴
            if (!collision.CompareTag("Monster"))
            {
                return;
            }
            
            if (!gameObject.CompareTag("Trap") ||   // 덫 종류 아닌 카드랑
                _SkillIndx == 13 ||    // 13번 카드 효과이거나
                _SkillIndx == 46 || _SkillIndx == 64 )      // 덫 이지만 46번(64 포함) 카드는 범위에 닿으면 콜라이더 꺼줌
            {
                magicCol.size = Vector2.zero;
            }
            
            /// 닿는 외부 몬스터에게 대미지
            collision.GetComponent<Monster>().OnDamagedOtherMon(_DeathDamage != 0? _DeathDamage:_CurrentDamage, _SkillIndx);

            /// 범위 보여주기
            sr.enabled = true;

            /** 외부에서 범위 꺼줄 목록 
                    ~덫 종류~ */
            if (_SkillIndx == 16 ||
                _SkillIndx == 28)
            {
                return;
            }

            /// 범위 살짝 보여준 다음에 꺼줌
            CancelInvoke(nameof(InvoSpr));
            Invoke(nameof(InvoSpr), 0.6f);

        }



        protected virtual void InvoSpr()
        {
            sr.enabled = false;
            /// 46 카드 3x3 한번 터트릴때만 발동
            if (!is46Active)
            {
                is46Active = true;
                Invo46DoubleAttack();
            }
        }

        ObscuredBool is46Active;

        /// <summary>
        /// 46 카드일때는 3x3 맞고 터지면
        /// </summary>
        void Invo46DoubleAttack()
        {
            /// 8x8 한번 더 터트림
            if (_SkillIndx == 46)
            {
                _SkillIndx = 64;
                magicCol.size = Vector2.one;
                transform.localScale = Vector2.one * 7.2f; // 8x8 사이즈
                sr.enabled = true;
                // 번개 터진 트리거 리셋
                is46Active = false;
            }
        }

        /// <summary>
        /// 46카드 트리거 리셋
        /// </summary>
        internal void Reset46Triger()
        {
            is46Active = false;
            transform.localScale = Vector2.one * 2.7f; // 3x3 사이즈
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(InvoSpr));
            magicCol.enabled = false;
            sr.enabled = false;
        }

        /// <summary>
        /// 죽음의 메아리 효과
        /// </summary>
        /// <param name="dam"></param>
        /// <param name="skillIndx"></param>
        internal void DeathrattleActivate(float dam, int skillIndx)
        {
            Debug.LogWarning($"{skillIndx} 번 스킬 세팅");

            _SkillIndx = skillIndx;
            /// 세팅 대미지 가져오기
            _DeathDamage = dam;
            //magicCol.size = Vector2.one * 1.1f;
        }

        /// <summary>
        /// 사망시 33카드 죽음의 메아리 발동
        /// </summary>
        internal void Deathrattle_33()
        {
            magicCol.size = Vector2.one;
            transform.localScale = new Vector2(0.66f, 0.66f);
        }
    }
}