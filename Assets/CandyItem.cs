using UnityEngine;

public class CandyItem : MonoBehaviour
{
    public int scoreValue = 10;
    public GameObject collectEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Báo cho hệ thống Combo biết người chơi vừa ăn kẹo
            if (ComboSystem.Instance != null)
            {
                ComboSystem.Instance.AddCandy();
            }

            // Tạo hiệu ứng nổ hạt (Particle)
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

            // Hủy viên kẹo
            Destroy(gameObject);
        }
    }
}