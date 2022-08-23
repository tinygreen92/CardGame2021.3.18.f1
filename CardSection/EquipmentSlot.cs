using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GoogleGame;

namespace GoogleGame
{
    public class EquipmentSlot : ItemSlot, IDropHandler
    {
        public event Action<EquipmentSlot> OnDropEvent;

        [Header(" - 장착 슬롯 인덱스")]
        public int indexOfEquip;
        protected override void OnValidate()
        {
            base.OnValidate();
        }

        public override bool CanReceiveItem(Item item)
        {
            if (item == null)
            {
                return true;
            }

            return false;
            //EquippableItem equippableItem = item as EquippableItem;
            //return equippableItem != null && equippableItem.targetEnemyType == TargetEnemyType.Single;
        }

        internal override void OnLongPress()
        {
            Debug.LogWarning("Equip 롱클릭 아무기능없음 : EquipmentSlot");
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("온 드롭 발생");
            OnDropEvent?.Invoke(this);
        }

    }
}