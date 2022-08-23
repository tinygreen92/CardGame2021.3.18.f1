using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleGame;

namespace GoogleGame
{
    public class GonGonJaeRoom : MonsterPoolManager
    {

        [Header(" - 훈련소 몬스터")]
        [SerializeField] MonsterSet mon2PrefabSet;

        private void Start()
        {
            TEST_BTN_roomTesting();
        }

        /// <summary>
        /// 테스트 룸 시작할 버튼
        /// </summary>
        public void TEST_BTN_roomTesting()
        {
            int lengthh = mon2PrefabSet.transform.childCount;

            mon2PrefabSet.TEST_InitMonSet(BC);

            for (int i = 0; i < lengthh; i++)
            {
                mon2PrefabSet.transform.GetChild(i).GetComponent<Monster>().TEST_GenerateMonster(damText, float.MaxValue);
                //yield return delay;
            }
        }
    }
}