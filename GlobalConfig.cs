using System;
using System.Collections;
using System.Collections.Generic;
using Databox;
using Lean.Gui;
using UnityEngine;

namespace GoogleGame
{
    public class GlobalConfig : MonoBehaviour
    {
        [Header(" - 글로벌 변수 데이터 박스")]
        [SerializeField] DataboxObject dataBox;
        [SerializeField] private LeanToggle frame30, fram60, lowQ, highQ;
        
        void OnEnable()
        {
            Debug.LogError("글로벌 변수 온 에이블");
            dataBox.OnDatabaseLoaded += DataReady;
            dataBox.LoadDatabase();
        }

        private void OnDisable()
        {
            Debug.LogError("글로벌 변수 디스 에이블");
            dataBox.OnDatabaseLoaded -= DataReady;
        }

        void DataReady()
        {
            // Access data
            Debug.LogWarning("Access 글로벌 변수");

            if (IslowFrame.Value) frame30.TurnOn();
            else fram60.TurnOn();

            if (IslowQuality.Value) lowQ.TurnOn();
            else highQ.TurnOn();
        }
        
        /// <summary>
        /// 30fps true / 60fps false
        /// </summary>
        BoolType IslowFrame
        {
            get { return dataBox.GetData<BoolType>("GlobalData", "Config", "LowFrame"); }
            set { dataBox.SetData<BoolType>("GlobalData", "Config", "LowFrame", value); }
        }
        
        /// <summary>
        /// 저품질 true / 고품질 false
        /// </summary>
        BoolType IslowQuality
        {
            get { return dataBox.GetData<BoolType>("GlobalData", "Config", "LowQuality"); }
            set { dataBox.SetData<BoolType>("GlobalData", "Config", "LowQuality", value); }
        }

        /// <summary>
        /// 0123 / 30,60,low,high 데이터베이스 갱신하기
        /// </summary>
        /// <param name="option"></param>
        public void DetectGhaphicOption(int option)
        {
            switch (option)
            {
                case 0:
                    IslowFrame = new BoolType(true);
                    Application.targetFrameRate = (int)EFrameRate.LOW;
                    break;
                case 1:
                    IslowFrame = new BoolType(false);
                    Application.targetFrameRate = (int)EFrameRate.HIGH;
                    break;
                case 2:
                    IslowQuality = new BoolType(true);
                    // TODO : 그래픽 옵션 조절 
                    break;
                case 3:
                    IslowQuality = new BoolType(false);
                    break;
            }
            Debug.LogWarning("데이터 베이스 저장");
            dataBox.SaveDatabase();
        }
        
        
    }
}


