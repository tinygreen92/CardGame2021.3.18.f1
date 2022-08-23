using System.Collections;
using UnityEngine;
using System;
using GoogleGame;

namespace GoogleGame
{
    public class Timer : MonoBehaviour
    {
        /// <summary>
        /// 0.1초 단위 타이머
        /// </summary>
        public event Action<float> PointOneSecondAction = delegate { };
        /// <summary>
        /// 1초 단위 타이머
        /// </summary>
        public event Action<float> SecondAction = delegate { };
        
        /// <summary>
        /// 타이머가 씬을 넘어가면 코루틴 중지했다가 다시 시작
        /// </summary>
        internal void AllStopAndGo()
        {
            //StopCoroutine(PointOneSecond());
            //StopCoroutine(OneSecond());
            // 다시 스타트
            AllCoroutineStart();
        }
        
        private void AllCoroutineStart()
        {
            StartCoroutine(PointOneSecond());
            StartCoroutine(OneSecond());
        }

        private IEnumerator PointOneSecond()
        {
            WaitForSeconds delay = new WaitForSeconds(0.1f);

            while (true)
            {
                yield return delay;

                PointOneSecondAction(0.1f);
            }
        }

        private IEnumerator OneSecond()
        {
            WaitForSeconds delay = new WaitForSeconds(1f);

            while (true)
            {
                yield return delay;
                SecondAction(1f);
            }
        }
    }
}