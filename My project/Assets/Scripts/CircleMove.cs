using UnityEngine;

public class CircleMove: MonoBehaviour
{
    public float speed = 3f;

    void Update()
    {
        // 往左移動
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        // 超出畫面左邊就刪掉
        if (transform.position.x < -12f)
        {
            Destroy(gameObject);
        }
    }
}