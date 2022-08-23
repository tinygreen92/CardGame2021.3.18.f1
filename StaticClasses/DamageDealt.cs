using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public static class DamageDealt
{
    /// <summary>
    /// 몬스터에게 입힌 피해량의 총합 (순수 피해 발생량)
    /// 실드에 대한 피해량을 포함하는가?
    /// 방어력으로 깎인 감소량을 포함하는가?
    /// </summary>
    public static Dictionary<int, float> damageDealt = new Dictionary<int, float>();
    
    /// <summary>
    /// 인덱스를 받아와서 딕셔너리가 존재하지 않으면 새로 생성하고, 존재한다면 값을 갱신시킨다
    /// </summary>
    /// <param name="cardIndex">대미지를 기록한 카드 인덱스</param>
    /// <param name="damage">받은 대미지</param>
    public static void RefreshDamegeDealt(int cardIndex, float damage)
    {
        if (damageDealt.ContainsKey(cardIndex))
        {
            damageDealt[cardIndex] += damage;
        }
        else
        {
            damageDealt.Add(cardIndex, damage);
        }
    }

    /// <summary>
    /// 통계 수치 초기화
    /// </summary>
    private static void DealtReset()
    {
        damageDealt.Clear();
    }

    // 전투가 끝나고 나서 전투 통계 모두 모아서 넘겨줌
    public static List<string> GetAllDamageDealtLog()
    {
        float AllDamageDealtLog = 0;
        List<string> damageList = new List<string>();
        foreach (var card in damageDealt)
        {
            // 각 카드의 피해량
            damageList.Add(card.Key + " : " + card.Value);
            AllDamageDealtLog += card.Value;
        }
        
        damageList.Add(AllDamageDealtLog.ToString());

        DealtReset();
        return damageList;
    }
}