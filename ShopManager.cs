using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    // 토글 그룹
    [SerializeField] private ToggleGroup toto;
    [Header("- 속 내용물")]
    [SerializeField] private Image[] itemIcon;
    [SerializeField] private TextMeshProUGUI [] itemTitle;
    [SerializeField] private TextMeshProUGUI[] itemPrice;
    [Header("- 벨리데이트용")]
    [SerializeField] private Transform[] nineNine;


    // private void OnValidate()
    // {
    //     for (int i = 0; i < nineNine.Length; i++)
    //     {
    //         itemIcon[i] = nineNine[i].GetChild(1).GetComponent<Image>();
    //         itemTitle[i] = nineNine[i].GetChild(0).GetComponent<TextMeshProUGUI>();
    //         itemPrice[i] = nineNine[i].GetChild(2).GetComponent<TextMeshProUGUI>();
    //         
    //         //itemIcon[i].sprite = null;
    //         itemTitle[i].text = $"{i} 번째 아이템 이름";
    //         itemPrice[i].text = $"{i} 번째 아이템 가격";
    //     }
    // }
    
    /// <summary>
    /// 나중에 이름 변경
    /// </summary>
    private enum 카테고리
    {
        카테고리0,
        카테고리1,
        카테고리2,
        카테고리3,
        카테고리4,
        카테고리5,
        카테고리6,
        카테고리7,
    }

    /// <summary>
    /// 해당 카테고리가 isOn 이 되면 실행
    /// </summary>
    public void GetCategoryInfo(int index)
    {
        // 클릭한 카테고리 받아서 처리 -> 그냥 일반적인 순서로 갱신해준다.
        switch (index)
        {
            case (int)카테고리.카테고리0: 
                break;
            case (int)카테고리.카테고리1: 
                break;
            case (int)카테고리.카테고리2:
                break;
            case (int)카테고리.카테고리3: 
                break;
            case (int)카테고리.카테고리4: 
                break;
            case (int)카테고리.카테고리5:
                break;
            case (int)카테고리.카테고리6:
                break;
            case (int)카테고리.카테고리7:
                break;
        }
        
        /// 카테고리를 고른 것은 내가아님
        foreach (var VARIABLE in itemTitle)
        {
            VARIABLE.text = $"{index}의 카테고리 아이템";
        }

    }
    
    
    /// <summary>
    /// 카테고리 클릭시 해당 토글 버튼 가져옴
    /// </summary>
    public void ClickedShopCategory()
    {
        Toggle firstOrDefault = toto.ActiveToggles().FirstOrDefault();
    }
    
}
