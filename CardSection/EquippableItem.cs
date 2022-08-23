using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleGame;
using CodeStage.AntiCheat.ObscuredTypes;

public enum TargetEnemyType
{
    Single,
    Multiple
}

public enum MagicType
{
    Melee,
    Magic
}

public enum AttackType
{
    DoT,                // 도트
    Link,               // 연결 = 전이
    Random,             // 무작위
    AoE,                // 범위
    ExtraAttack,        // 추가타
    Movement,           // 이동
    AttackSpeed,        // 공속
    SP,                 // sp
    ArmorPenetration,   // 방관 = 방어 관통
    Shild,              // 실드
    MultipleAttack,     // 다중
    BonusHealthAttack,  // 체비 = 체력 비례
    InstantKill,        // 즉사
    PowerUp,            // 공증
    Immune,             // 면역
    CoolTime,           // 쿨탐
    CardGenerate,       // 카드
    Heal                // 회복
}

namespace GoogleGame
{
    [CreateAssetMenu(fileName = "EquippableItem", menuName = "Scriptable Object/EquippableItem")]
    public class EquippableItem : Item
    {
        // public string ItemName;
        // public int ItemIndex;
        // public Sprite Icon;
        [Space]
        public TargetEnemyType targetEnemyType; /// 대상 단수, 복수
        public MagicType magicType; /// 물리, 마법
        [Space]
        public AttackType attackType;   /// 공격 타입 구분 -> 사용할지 말지 고민
        [Space]
        public ObscuredFloat colorpower;



        //internal void GetCardInfo()
        //{
        //    switch (ItemIndex)
        //    {
        //        case 0:

        //            break;


        //        default:
        //            Debug.LogError("정의되지 않은 ItemIndex 에러 : " + ItemIndex);
        //            break;
        //    }


        //}


    }
}