using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleGame;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine.U2D;

namespace GoogleGame
{
    public class MonsterSet : MonoBehaviour
    {
        // [SerializeField] private string atlasName;
        // [SerializeField] private string variantName;
        // [SerializeField] private string spriteName;
        /// <summary>
        /// InitMonSet 하면 붙여준다.
        /// </summary>
        BattleController BC;

        int startCnt;
        int maxCnt;
        // WWW loader;

        /// <summary>
        /// 훈련소에서 쓰는 메서드
        /// </summary>
        /// <param name="bacon"></param>
        internal void TEST_InitMonSet(BattleController bacon)
        {
            BC = bacon;

            Monster[] monArray = GetComponentsInChildren<Monster>();

            for (int i = 0; i < 36; i++)
            {
                monArray[i].GogoRoomActivate();

                MonsterPoolManager.monsters[i] = monArray[i];
            }

            for (int i = 36; i < monArray.Length; i++)
            {
                monArray[i].GogoRoomActivate();
            }
        }
        
        internal Monster[] monArray;

        private void Awake()
        {
            monArray = GetComponentsInChildren<Monster>();

        }
        
        private void FixedUpdate()
        {
            foreach (var item in monArray)
            {
                item.FixedUpdateMe();
            }
        }

        ObscuredFloat nomaTimer = 15f;

        /// <summary>
        /// 매 15초마다 보유중인 SP의 15%를 훔쳐감
        /// </summary>
        /// <param name="delay"></param>
        void StealingSp(float delay)
        {
            nomaTimer -= delay;

            if (nomaTimer < 0)
            {
                BC.StorenSPfromMon();
                nomaTimer = 15f;
            }
        }


        /// <summary>
        /// 초기 세팅
        /// BattleController 세팅해줌 0~59 / 60, 61, 62 , 63 
        /// </summary>
        internal void InitMonSet(BattleController bacon, int setMon)
        {
            BC = bacon;

            /// 8번 몬스터 매 15초마다 보유중인 SP의 15%를 훔쳐감
            if (setMon == 0 && monArray[0].GetNorAbility() == ENorMonsterAbility.Buff_Stealing_SP)
            {
                Debug.LogError("8번 몬스터 매 15초마다 보유중인 SP의 15%를 훔쳐감");
                GameManager.instance.coTimer.SecondAction += StealingSp;
            }

            /// 배열 세팅
            startCnt = setMon * 20;
            maxCnt = (setMon + 1) * 20;

            /// 3 로 받아오면 보스임
            if (setMon == 3)
            {
                maxCnt = startCnt + 1;
            }


            for (int i = startCnt; i < maxCnt; i++)
            {
                MonsterPoolManager.monsters[i] = monArray[i - startCnt];
                /// 몬스터 죽음 이벤트
                MonsterPoolManager.monsters[i].OnDestory += Destory;
                /// 성벽을 공격하는 이벤트
                MonsterPoolManager.monsters[i].OnAttack += Attack;
                /// 스페셜카드가 공격시 공격력의 1%만큼 성벽 체력 회복 colorTypeBae[4];
                MonsterPoolManager.monsters[i].OnRestore += AttackBoBoBo;

            }

        }

        /// <summary>
        /// 스페셜카드가 공격시 공격력의 1%만큼 성벽 체력 회복 colorTypeBae[4];
        /// </summary>
        void AttackBoBoBo(float atkDam, float times)
        {
            BC.RestoreCatleHp(atkDam, times);
        }



        /// <summary>
        /// 몬스터 사망시 배열에서 해당 Monster 삭제해주기
        /// </summary>
        /// <param name="monster"></param>
        private void Destory(Monster monster)
        {
            /// 필드에 소환된 몬스터 배열에서 해당 몬스터 삭제
            for (int i = startCnt; i < maxCnt; i++)
            {
                if (MonsterPoolManager.monsters[i] == null)
                {
                    continue;
                }
                if (MonsterPoolManager.monsters[i].Equals(monster))
                {
                    /// 몬스터 사망 게이지 증가시켜
                    BC.KillMonsterBadly();
                    // 배열에서 null 해줌
                    MonsterPoolManager.monsters[i] = null;
                    return;
                }
            }
        }


        /// <summary>
        /// 몬스터의 공격력으로 성벽을 공격한다.
        /// </summary>
        /// <param name="monster"></param>
        private void Attack(int dam)
        {
            BC.MonsterAttackCastle(dam);
        }
        
        void OnDisable()
        {
            GameManager.instance.coTimer.SecondAction -= StealingSp;
        }
        

    }
}