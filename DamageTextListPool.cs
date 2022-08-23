using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoogleGame
{
    public class DamageTextListPool : MonoBehaviour
    {
        private void Update()
        {
            var list = GetComponentsInChildren<DamageText>();
            foreach (var item in list)
            {
                item.UpdateMe();
            }
        }
    }
}