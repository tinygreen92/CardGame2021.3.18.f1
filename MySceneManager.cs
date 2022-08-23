using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GoogleGame;
using Runemark.SCEMA;

namespace GoogleGame
{
    public enum MyLocation
    {
        LoginScene,
        MainScene,
        BattleScene,
    }
    public class MySceneManager : MonoBehaviour
    {
        public Location[] locations;
        
        public static MySceneManager Instance
        {
            get
            {
                return instance;
            }
        }
        private static MySceneManager instance;


        void Start()
        {
            if (instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            // 배틀 타이머 정지 후 바로 시작
            GameManager.instance.coTimer.AllStopAndGo();
        }

        /// <summary>
        /// scema 에셋을 활용한 로딩씬 띄우고 씬 넘기기
        /// </summary>
        /// <param name="myLocation"></param>
        public void JumpScemaLocation(MyLocation myLocation)
        {
            locations[(int)myLocation].Enter();
        }
        
    }
}