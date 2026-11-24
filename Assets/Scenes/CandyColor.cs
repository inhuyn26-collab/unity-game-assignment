using UnityEngine;

public class CandyColor : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    void Start()
    {
        // Tự động tìm thành phần đồ họa Mesh Renderer trên khối hộp này
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Hàm tự động kích hoạt khi có vật thể dẫm lên khối hộp này
    void OnCollisionEnter(Collision collision)
    {
        // Kiểm tra xem vật thể dẫm lên có phải là người chơi (chú Chibi) hay không
        if (collision.gameObject.name == "PFB_Chibi_Base")
        {
            // Tạo ra 3 thông số màu sắc Đỏ (R), Xanh lá (G), Xanh dương (B) ở dạng nhạt (Pastel)
            // Bằng cách lấy giá trị ngẫu nhiên từ 0.5 đến 1.0 (màu sẽ sáng và ngọt ngào)
            float r = Random.Range(0.5f, 1.0f);
            float g = Random.Range(0.5f, 1.0f);
            float b = Random.Range(0.5f, 1.0f);

            Color mauPastelNgauNhien = new Color(r, g, b);

            // Đổi màu bề mặt của khối hộp sang màu pastel mới
            if (meshRenderer != null)
            {
                meshRenderer.material.color = mauPastelNgauNhien;
            }
        }
    }
}
