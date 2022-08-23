using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleGame;

namespace GoogleGame
{
    /// <summary>
    /// 이 스크립트가 붙은 SpriteRenderer 는 카메라에 꽉차게 보인다.
    /// </summary>
    public class PerfectSpriteFit : MonoBehaviour
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

            //transform.localScale = new Vector2(screen_x / sprite_x, screen_y / sprite_y);
            transform.localScale = new Vector2(screen_y / sprite_y, screen_y / sprite_y);
        }

        //private void FixedUpdate()
        //{
        //    sprite_x = sr.sprite.bounds.size.x;
        //    sprite_y = sr.sprite.bounds.size.y;
        //    screen_y = Camera.main.orthographicSize * 2;
        //    screen_x = screen_y / Screen.height * Screen.width;

        //    transform.localScale = new Vector2(screen_x / sprite_x, screen_y / sprite_y);
        //}
    }
}