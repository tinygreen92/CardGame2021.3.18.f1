using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoogleGame
{
    public class TransitionAnimation : MonoBehaviour
    {
        [SerializeField]
        private Animator animeStyle7;

        public void TestStartAnime()
        {
            animeStyle7.Play("Expand");
        }

        
        /// <summary>
        /// 트랜지션 끝나면 불리우는 메소드
        /// </summary>
        public void JumpBattleScene()
        {
            MySceneManager.Instance.locations[2].Enter();
        }
        
        
    }
}

