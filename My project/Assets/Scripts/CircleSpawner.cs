using UnityEngine;

public class CircleSpawner : MonoBehaviour
{
    public GameObject greenCirclePrefab;
    public GameObject pinkCirclePrefab;
    public Transform spawnPoint;  // 生成位置（畫面右邊）
    public float spawnInterval = 1f;
    public float moveSpeed = 3f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnCircle();
        }
    }

    void SpawnCircle()
    {
        // 50% 機率生成綠色或粉色
        GameObject prefab = Random.value > 0.5f ? greenCirclePrefab : pinkCirclePrefab;
        GameObject circle = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        // 給圓圈移動速度
        CircleMove move = circle.AddComponent<CircleMove>();
        move.speed = moveSpeed;
    }
}