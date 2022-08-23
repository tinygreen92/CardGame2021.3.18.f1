using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using GoogleGame;
using Unity.Mathematics;


public class CardUpgradeTable
{
    /// <summary>
    /// 카드가 다음 레벨로 업그레이드 될 때 필요한 재화를 반환합니다.
    /// </summary>
    /// <param name="initValue">데이터박스에 제공되는 UpgradeCost</param>
    /// <param name="currentlevel">카드의 현재 레벨</param>
    /// <returns></returns>
    public ObscuredInt GetCardNextLevelCost(int itemIndex, int currentlevel)
    {
        switch (itemIndex / 13)
        {
            case 0:
                return (ObscuredInt) (100 * Mathf.Pow(2, currentlevel));
            case 1:
                return (ObscuredInt) (200 * Mathf.Pow(2, currentlevel));
            case 2:
                return (ObscuredInt) (500 * Mathf.Pow(2, currentlevel));
            case 3:
                return (ObscuredInt) (1200 * Mathf.Pow(2, currentlevel));
            default:
                return -1;
        }
    }
    
    /// <summary>
    /// 카드 업그레이드에 필요한 카드 조각수 반환
    /// </summary>
    /// <param name="currentlevel">현재 레벨 입력</param>
    /// <returns>리턴값이 -1 라면 몬가 잘못됐다</returns>
    public ObscuredInt GetUpgradePieces(int currentlevel)
    {
        ObscuredInt needPieces = -1;
        switch (currentlevel)
        {
            case 1: // 레벨이 1에서 2로 증가할때 2개가 필요
                needPieces = 2;
                break;
            case 2: // 레벨이 2에서 3로 증가할때 4개가 필요
                needPieces = 4;
                break;
            case 3:
                needPieces = 10;
                break;
            case 4:
                needPieces = 20;
                break;
            case 5:
                needPieces = 50;
                break;
            case 6:
                needPieces = 100;
                break;
            case 7:
                needPieces = 200;
                break;
            case 8:
                needPieces = 350;
                break;
            case 9:
                needPieces = 600;
                break;
            
                
            default:
                break;
        }
        
        return needPieces;
    }
    
    
    
}