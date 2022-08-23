using System;
using System.Collections;
using System.Collections.Generic;
using GoogleGame;
using UnityEngine;

public class AutoPlay : MonoBehaviour
{
    public StartBuffManager startBuffManager;
    public MonsterPoolManager monsterPoolManager;
    public PinkiePie pinkiePie;

    private void OnValidate()
    {
        dasdsa();
    }

    void dasdsa()
    {
        startBuffManager = FindObjectOfType<StartBuffManager>();
        monsterPoolManager = FindObjectOfType<MonsterPoolManager>();
        pinkiePie = FindObjectOfType<PinkiePie>();
    }

    public static AutoPlay instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }


    /// <summary>
    /// 눌러서 오토 플레이
    /// </summary>
    public void TestBtn_AutoPlay()
    {
        dasdsa();
        GameManager.isAutoPlay = true;
        // 버튼 누르면 오토 플레이 시작
        StartCoroutine(EStartAuto());
    }

    IEnumerator EStartAuto()
    {
        startBuffManager.AutoPlay();
        yield return null;
        monsterPoolManager.TEST_BTN_GOGOGO();
        yield return null;
        pinkiePie.ȕȕȕȕȕȖȕȖȖȕȕȕȕȖȕȕȕȖȖȖȖȕȖȕȕȖȕȕȕȖȕȕȖȖȖȕȖȕȕȖȖȖȕȕȕȕȖ();
    }
}
