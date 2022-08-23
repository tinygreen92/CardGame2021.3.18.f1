using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleGame;

namespace GoogleGame
{
    [CreateAssetMenu(fileName = "Item", menuName = "Scriptable Object/Item")]
    public class Item : ScriptableObject
    {
        public string ItemName;
        public int ItemIndex;
        public Sprite Icon;
    }
}