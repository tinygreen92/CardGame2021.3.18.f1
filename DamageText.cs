using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;
using Lean.Pool;
using GoogleGame;
using TMPro;


namespace GoogleGame
{
    public class DamageText : MonoBehaviour
    {
        [SerializeField] TextMeshPro text;

        private ObscuredFloat moveSpeed;
        private ObscuredFloat alphaSpeed;
        private ObscuredFloat destroyTime;

        Color alpha;

        internal ObscuredFloat damage;

        internal void InitDamageFont(float dam, Color color)
        {
            //transform.SetParent(parentPos);

            moveSpeed = 1f;
            alphaSpeed = 1.5f;
            destroyTime = 0.9f;

            damage = dam;
            alpha = color;

            text.color = color;
            text.text = $"{damage:N0}";

            LeanPool.Despawn(gameObject, destroyTime);

            //transform.DOScale((Vector3.one * 1.5f), 0.5f);
            //text.DOFade(1f, 1).SetEase(Ease.InBack);
            //transform.DOMoveY(moveYpos + moveSpeed, 0.9f);
        }

        float tmptime = 1f;
        bool isDamSize;

        private void OnDisable()
        {
            tmptime = 1f;
            isDamSize = false;
        }

        public void UpdateMe()
        {
            if (!isDamSize)
            {
                tmptime += Time.deltaTime * 10f;
                transform.localScale = Vector3.one * tmptime;

                if (tmptime > 1.5f)
                {
                    isDamSize = true;
                }
            }

            if (isDamSize)
            {
                tmptime -= Time.deltaTime;
                transform.localScale = Vector3.one * tmptime;

                if (tmptime < 1.2f)
                {
                    transform.Translate(new Vector3(0, moveSpeed * Time.deltaTime, 0)); // 텍스트 위치
                    alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * alphaSpeed); // 텍스트 알파값
                    text.color = alpha;
                }
            }


        }
    }
}