using UnityEngine;

public class CircleSpawner : MonoBehaviour
{
    public GameObject greenCirclePrefab;
    public GameObject pinkCirclePrefab;
    public Transform spawnPoint;
    public float spawnInterval = 1f;
    public float moveSpeed = 3f;
    public GameObject judgeZone;

    private float timer = 0f;
    private Rowing_Lvl1 rowingScript;

    void Start()
    {
        if (judgeZone != null)
            rowingScript = judgeZone.GetComponent<Rowing_Lvl1>();
    }

    void Update()
    {
        // 遊戲還沒開始就不生成
        if (rowingScript == null || rowingScript.gameOver) return;

        // 根據分數調整速度
        int score = rowingScript.score;

        if (score >= 3000)
        {
            moveSpeed = 17f;
            spawnInterval = 0.15f;
        }
        else if (score >= 2000)
        {
            moveSpeed = 15f;
            spawnInterval = 0.2f;
        }
        else if (score >= 1500)
        {
            moveSpeed = 13f;
            spawnInterval = 0.3f;
        }
        else if (score >= 1000)
        {
            moveSpeed = 11f;
            spawnInterval = 0.4f;
        }
        else if (score >= 600)
        {
            moveSpeed = 9f;
            spawnInterval = 0.55f;
        }
        else if (score >= 400)
        {
            moveSpeed = 7f;
            spawnInterval = 0.70f;
        }
        else if (score >= 200)
        {
            moveSpeed = 5f;
            spawnInterval = 0.85f;
        }
        else
        {
            moveSpeed = 3f;
            spawnInterval = 1f;
        }

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnCircle();
        }
    }

    void SpawnCircle()
    {
        GameObject prefab = Random.value > 0.5f ? greenCirclePrefab : pinkCirclePrefab;
        GameObject circle = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        // 只處理顯示層級，不改移動邏輯
        SpriteRenderer sr = circle.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 10;
        }

        CircleMove move = circle.AddComponent<CircleMove>();
        move.speed = moveSpeed;

        if (judgeZone != null)
            judgeZone.GetComponent<Rowing_Lvl1>().RegisterCircle(circle);
    }
}