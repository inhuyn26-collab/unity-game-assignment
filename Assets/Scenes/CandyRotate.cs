using UnityEngine;

public class CandyRotate : MonoBehaviour
{
    // Tốc độ xoay của khối kẹo
    public float tocDoXoay = 50f;

    void Update()
    {
        // Lệnh làm khối hộp tự xoay quanh trục thẳng đứng (Y)
        transform.Rotate(Vector3.up * tocDoXoay * Time.deltaTime);
    }
}
