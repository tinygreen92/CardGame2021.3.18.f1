using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lean.Pool;
using UnityEngine;
using UnityEngine.PlayerLoop;


namespace GoogleGame
{
    public class BulletListPool : MonoBehaviour
    {
        private void FixedUpdate()
        {
            var list = GetComponentsInChildren<Bullet>();
            foreach (var item in list)
            {
                item.FixedUpdateMe();
            }
        }
    }
}
