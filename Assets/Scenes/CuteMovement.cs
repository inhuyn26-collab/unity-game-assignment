using UnityEngine;

public class CuteMovement : MonoBehaviour
{
    public float tocDoChay = 7f;
    public float lucNhay = 7f;
    public float tocDoXoayCam = 3f;

    // BIẾN MỚI: Nơi để kéo thả dòng chữ UI YOU WIN vào ngoài Unity
    public GameObject khungChuWin;

    private Rigidbody rb;
    private Transform cam;
    private Animator ani;
    private float rotX = 0f;
    private CapsuleCollider capCollider;

    // BIẾN MỚI: Quản lý thành phần loa phát âm thanh nhảy trên nhân vật
    private AudioSource amThanhNhay;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
        ani = GetComponent<Animator>();
        capCollider = GetComponent<CapsuleCollider>();

        // Tự động tìm thành phần Audio Source gắn trên người chú Chibi khi bắt đầu game
        amThanhNhay = GetComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 1. XOAY CAMERA THEO CHUỘT
        float mouseX = Input.GetAxis("Mouse X") * tocDoXoayCam;
        float mouseY = Input.GetAxis("Mouse Y") * tocDoXoayCam;

        transform.Rotate(Vector3.up * mouseX);
        rotX -= mouseY;
        rotX = Mathf.Clamp(rotX, -30f, 60f);
        cam.localRotation = Quaternion.Euler(rotX, 0f, 0f);

        // 2. DI CHUYỂN NHÂN VẬT THEO NÚT W-A-S-D
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z).normalized;

        // TÍCH HỢP HÃM PHANH MƯỢT MÀ CHỐNG TRƯỢT CHÂN TRÊN KHỐI NHỎ
        if (move.magnitude >= 0.1f)
        {
            rb.linearVelocity = new Vector3(move.x * tocDoChay, rb.linearVelocity.y, move.z * tocDoChay);
        }
        else
        {
            // Triệt tiêu ngay vận tốc ngang khi thả tay khỏi phím bấm
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        // ĐỒNG BỘ CHẠY: Truyền tốc độ vào biến "Speed" của Animator Unity
        if (ani != null)
        {
            float tocDoHienTai = move.magnitude * tocDoChay;
            ani.SetFloat("Speed", tocDoHienTai);
            ani.SetBool("Grounded", KiemTraTrenDat()); // Báo cho Animator biết có đang chạm đất không
        }

        // 3. NÚT NHẢY TỰ ĐỘNG (Raycast)
        if (Input.GetButtonDown("Jump") && KiemTraTrenDat())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, lucNhay, rb.linearVelocity.z);
            if (ani != null) ani.SetTrigger("Jump"); // Kích hoạt hoạt ảnh nhảy của Unity

            // LỆNH MỚI: Kích hoạt loa phát tiếng kêu tưng tưng khi nhảy Space
            if (amThanhNhay != null)
            {
                amThanhNhay.Play();
            }
        }
    }

    // Hàm tự động kích hoạt khi người chơi đi xuyên qua vùng cảm biến Trigger
    void OnTriggerEnter(Collider other)
    {
        // [CƠ CHẾ 1]: Kiểm tra nếu ngã trúng tấm thảm tàng hình Vực Thẳm
        if (other.gameObject.name == "VucTham")
        {
            // Reset vận tốc rơi tự do về 0 để nhân vật không bị lag khi hồi sinh
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Đưa nhân vật quay trở lại vị trí (0, 3, 0) - chính là trên bề mặt cái sân hồng ban đầu
            transform.position = new Vector3(0f, 3f, 0f);

            Debug.Log("Úi, ngã rồi! Đã đưa chú Chibi về sân hồng an toàn.");
        }

        // [CƠ CHẾ 2]: Kiểm tra nếu chạm trúng vật phẩm Ngôi Sao Đích ở cuối con đường
        if (other.gameObject.name == "Ngôi Sao Đích")
        {
            // Mở khóa hiện dòng chữ UI YOU WIN lên màn hình
            if (khungChuWin != null) khungChuWin.SetActive(true);

            // Xóa quả cầu ngôi sao đi để tạo cảm giác đã nhặt được vật phẩm
            Destroy(other.gameObject);

            // Giải phóng con trỏ chuột và dừng di chuyển nhân vật để ăn mừng
            Cursor.lockState = CursorLockMode.None;
            rb.linearVelocity = Vector3.zero;

            Debug.Log("Chúc mừng! Bạn đã chinh phục thành công Cơn Say Kẹo Ngọt!");
        }
    }

    bool KiemTraTrenDat()
    {
        float chieuCaoCollider = capCollider != null ? capCollider.height : 1f;
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, (chieuCaoCollider / 2f) + 0.2f);
    }

    // Hàm bổ sung để sửa triệt để lỗi Animation Event của Unity
    void OnLand()
    {
        // Để trống ở đây, mục đích chỉ để làm "đầu nhận" cho hoạt ảnh không báo lỗi đỏ nữa
    }
}
