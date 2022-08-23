using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleGame;

namespace GoogleGame
{
    public class PerfectPlaneFit : MonoBehaviour
    {
        SpriteRenderer sr;
        float sprite_x;
        float sprite_y;
        float screen_y;
        float screen_x;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();

            sprite_x = sr.sprite.bounds.size.x;
            sprite_y = sr.sprite.bounds.size.y;
            screen_y = Camera.main.orthographicSize * 2;
            screen_x = screen_y / Screen.height * Screen.width;

            transform.localScale = new Vector2(screen_x / sprite_x, screen_y / sprite_y);
        }
    }
}