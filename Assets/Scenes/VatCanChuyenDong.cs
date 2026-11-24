using UnityEngine;

public class VatCanChuyenDong : MonoBehaviour
{
    [Header("Chuyển động lên xuống")]
    public bool chuyenDongLenXuong = true;
    [Tooltip("Biên độ nhấp nhô lên xuống (mét)")]
    public float bienDoLenXuong = 0.8f;
    [Tooltip("Tốc độ dao động lên xuống")]
    public float tocDoLenXuong = 2.5f;
    [Tooltip("Lệch pha thời gian để các vật cản không nhấp nhô cùng nhịp")]
    public float lechPha = 0f;

    [Header("Chuyển động xoay")]
    public bool xoayVatCan = true;
    [Tooltip("Tốc độ xoay theo từng trục (độ/giây). Ví dụ (0, 120, 0) là xoay quanh trục đứng")]
    public Vector3 tocDoXoay = new Vector3(0f, 120f, 0f);
    public Space heToaDoXoay = Space.Self;

    private Vector3 viTriGoc;

    void Awake()
    {
        viTriGoc = transform.localPosition;
    }

    void Update()
    {
        if (chuyenDongLenXuong)
        {
            float offsetY = Mathf.Sin((Time.time + lechPha) * tocDoLenXuong) * bienDoLenXuong;
            transform.localPosition = viTriGoc + new Vector3(0f, offsetY, 0f);
        }

        if (xoayVatCan)
        {
            transform.Rotate(tocDoXoay * Time.deltaTime, heToaDoXoay);
        }
    }
}