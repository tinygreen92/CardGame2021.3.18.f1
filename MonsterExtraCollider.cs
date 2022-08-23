using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleGame;

namespace GoogleGame
{
    public class MonsterExtraCollider : MonsterCollider
    {
        /**
         * 
         * 
         * 
         *  몬스터 귀속이 아닌 필드에 돌아다니는 범위 공격 콜라이더
         *  Tag : Trap
         *  16번 스페셜 카드
         * 
         * 
         * 
         */


        protected override void SetMonster()
        {
            base.SetMonster();

            // 필드에 돌아다니는 콜라이더라면 비활성화.
            magicCol.enabled = false;
        }

        /// <summary>
        /// allCollider = 모든 몬스터 대상일때 달아줌.
        /// </summary>
        /// <param name="dam"></param>
        internal void SetALLActivate(float dam, int skillIndx)
        {
            Debug.LogWarning($"{skillIndx} 번 스킬 세팅");

            _SkillIndx = skillIndx;
            /// 세팅 대미지 가져오기
            _CurrentDamage = dam;
            magicCol.size = Vector2.one;
            transform.localScale = Vector2.one * 30f;

            magicCol.enabled = true;
            sr.enabled = true;
        }


        internal override void SetColliderActivate(float dam, int skillIndx)
        {
            Debug.LogWarning($"{skillIndx} 번 스킬 세팅");

            _SkillIndx = skillIndx;
            /// 세팅 대미지 가져오기
            _CurrentDamage = dam;

            magicCol.size = Vector2.one;
            magicCol.enabled = true;
            sr.enabled = true;
        }

        /// <summary>
        /// 덫 종류 이미지 끄기
        /// </summary>
        internal void TurnOffTheTrap()
        {
            // 필드에 돌아다니는 콜라이더 비활성화.
            magicCol.size = Vector2.zero;
            magicCol.enabled = false;
            sr.enabled = false;
        }

    }
}