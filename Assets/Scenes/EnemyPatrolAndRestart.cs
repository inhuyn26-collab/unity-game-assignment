using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyPatrolAndRestart : MonoBehaviour
{
    [Header("Di chuyển qua lại")]
    public bool moveAlongX = true;
    public float moveDistance = 2.2f;
    public float moveSpeed = 2.5f;

    [Header("Chạm Player thì chơi lại")]
    public float restartDelay = 0.15f;

    private Vector3 startPos;
    private bool triggered;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (triggered) return;

        float offset = Mathf.PingPong(Time.time * moveSpeed, moveDistance * 2f) - moveDistance;
        Vector3 pos = startPos;
        if (moveAlongX)
            pos.x += offset;
        else
            pos.z += offset;

        transform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryRestart(other.transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryRestart(collision.transform);
    }

    void TryRestart(Transform hitTransform)
    {
        if (triggered) return;

        if (hitTransform.CompareTag("Player") || hitTransform.root.CompareTag("Player"))
        {
            triggered = true;
            Invoke(nameof(RestartScene), restartDelay);
        }
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}