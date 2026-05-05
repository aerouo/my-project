using UnityEngine;

public class CircleMove : MonoBehaviour
{
    public float speed = 3f;
    private Rowing_Lvl1 rowingScript;

    void Start()
    {
        var judgeZone = GameObject.Find("JudgeZone");
        if (judgeZone != null)
            rowingScript = judgeZone.GetComponent<Rowing_Lvl1>();
    }

    void Update()
    {
        // ¼È°±®É¶ê°é°±¤î²¾°Ê
        if (rowingScript != null && rowingScript.gamePaused) return;

        transform.Translate(Vector2.left * speed * Time.deltaTime);
        if (transform.position.x < -20f)
            Destroy(gameObject);
    }
}