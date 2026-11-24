using UnityEngine;

public class BounceCloud : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float bounceForce = 15f; // Lực nảy lên cao

    private void OnCollisionEnter(Collision collision)
    {
        // Kiểm tra xem nhân vật có chạm vào đám mây từ trên xuống không
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // Reset vận tốc theo trục Y trước khi đẩy lên để lực nảy luôn đều
                Vector3 velocity = playerRb.linearVelocity;
                velocity.y = 0f;
                playerRb.linearVelocity = velocity;

                // Thêm lực đẩy hướng lên trên (AddForce kiểu Impulse)
                playerRb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);

                Debug.Log("Nhún mây thành công!");

                // (Tùy chọn) Thêm hiệu ứng âm thanh hoặc animation mây nhún ở đây
            }
        }
    }
}