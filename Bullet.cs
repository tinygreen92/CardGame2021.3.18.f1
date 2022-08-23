using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using GoogleGame;
using Lean.Pool;

namespace GoogleGame
{
    public class Bullet : MonoBehaviour
    {
        [Header(" - 총알 기본 설정")] 
        Monster _target;

        ObscuredFloat _speed = 400f;
        ObscuredFloat _rotSpeed = 64f;
        ObscuredFloat _damage = 1f;

        [Header(" - 총알 회전체 부분")] float angle;
        Quaternion rotTarget;
        Vector3 dir;
        Rigidbody2D rb;

        EquippableItem _myCard;

        /// <summary>
        /// 총알 생성시 받아오는 칼라타입
        /// </summary>
        EColorType _eColorType;

        /// <summary>
        /// 전이 공격 카운터
        /// </summary>
        private ObscuredInt linkedCnt;

        private void OnEnable()
        {
            rb = GetComponent<Rigidbody2D>();
            _myCard = null;
            _target = null;
        }

        private void OnDisable()
        {
            linkedCnt = 0;
        }


        /// <summary>
        /// 올바른 타겟에 대미지를 가하면 TRUE 반환
        /// + 내부 로직 여기서 처리해준다 ;;
        /// </summary>
        /// <param name="target">목표 타겟 Monster </param>
        /// <param name="itemIndex">1,40번 카드의 총알을 살리기 위해</param>
        /// <returns></returns>
        public bool IsFactTarget(Monster target, out int itemIndex)
        {

            if (ReferenceEquals(_myCard, null))
            {
                itemIndex = -1;
                return false;
            }
            // 닿은 총알 인덱스 반환 값 저장
            itemIndex = _myCard.ItemIndex;

            /// 목표로 한 타겟 = 진짜 닿은 타겟 일 경우에
            if (_target == target)
            {
                // 1번 카드 총알인 경우?
                if (itemIndex == 1)
                {
                    _target.OnDamaged_1(_damage, ++linkedCnt, this, out int newCnt);

                    linkedCnt = newCnt;

                    if (linkedCnt > 2)
                    {
                        itemIndex = -1;
                    }
                    
                    return true;
                }
                // 40번 카드 총알인 경우?
                if (itemIndex == 40)
                {
                    _target.OnDamaged_40(_damage, ++linkedCnt, this);

                    if (linkedCnt > 4)
                    {
                        itemIndex = -1;
                    }
                    
                    return true;
                }

                /// 그 외 통상 공격일때
                _target.OnDamaged(_myCard, _damage, _isMagicMssile, _eColorType);
                return true;
            }

            /// 충돌한 놈이 총알 목표가 아닌데 어캄?
            return false;
        }


        bool isLaunch;
        ObscuredBool _isMagicMssile;

        /// <summary>
        /// 총알 생성후 기본 정보 세팅 후 업데이트 실행
        /// </summary>
        /// <param name="target"></param>
        /// <param name="damage"></param>
        /// <param name="speed"></param>
        /// <param name="eColorType"></param>
        /// <param name="debuffBlock"></param>
        /// <param name="myCard"></param>
        public void RunBullet(Monster target, float damage, float speed, EquippableItem myCard, EColorType eColorType,
            float debuffBlock = 1f)
        {
            isLaunch = false;

            _target = target;
            _myCard = myCard;

            _speed = speed;
            _damage = damage;

            _eColorType = eColorType;

            /// 공격타입
            /// 물리 = false / 매직 = true
            if (_myCard.magicType.Equals(MagicType.Magic))
            {
                // _target.OnDamaged 에 영향을 끼쳐야 함
                _isMagicMssile = true;
            }
            else
            {
                // _target.OnDamaged 에 영향을 끼쳐야 함
                _isMagicMssile = false;
            }

            /// 스페셜 카드에게 노말 몬스터 특성 적용
            switch (_target.GetNorAbility())
            {
                case ENorMonsterAbility.DeBf_Melee_Down: // 스페셜카드의 물리대미지를 15% 감소시킴
                    _damage = damage * 0.85f * debuffBlock;
                    break;
                case ENorMonsterAbility.DeBf_Magic_Down: // 스페셜카드의 마법대미지를 15% 감소시킴
                    _damage = damage * 0.85f * debuffBlock;
                    break;
                case ENorMonsterAbility.DeBf_Damage_Down: // 모든 스페셜카드 공격력의 5%를 감소시킴 
                    _damage = damage * 0.95f * debuffBlock;
                    break;
                case ENorMonsterAbility.DeBf_AtkSpeed_Down: // 모든 스페셜카드 공격속도의 10%를 감소시킴
                    _speed = speed * 0.9f * debuffBlock;
                    break;
                default:
                    break;
            }

            //_rotSpeed = rot;
            isLaunch = true;
        }

        /// <summary>
        /// 근접 몬스터가 있으면 총알 소환
        /// </summary>
        /// <param name="target"></param>
        public void RunBullet(Monster target)
        {
            isLaunch = false;

            _target = target;
            //_myCard = null;

            isLaunch = true;
        }

        /*private void FixedUpdate()
        {
            FixedUpdateMe();
        }*/

        /// <summary>
        /// List로 돌리는 개인적인 픽스드없데이트 FixedUpdate
        /// </summary>
        internal void FixedUpdateMe()
        {
            if (!isLaunch)
            {
                return;
            }

            dir = (_target.transform.position - transform.position).normalized;
            angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rotTarget = Quaternion.AngleAxis(angle, Vector3.forward);
            //transform.rotation = Quaternion.Slerp(transform.rotation, rotTarget, Time.deltaTime * _rotSpeed);
            rb.velocity = dir * _speed * Time.deltaTime;

            if ((_target.transform.position - transform.position).magnitude < 0.2f)
            {
                LeanPool.Despawn(this);
            }
        }
        
    }
}