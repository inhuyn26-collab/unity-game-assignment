using UnityEngine;

public class RunnerCameraFollow : MonoBehaviour
{
    [Header("Mục tiêu bám theo")]
    public Transform target; // Kéo PFB_Chibi_Base vào đây

    [Header("Góc nhìn thoáng thấy đường & vật cản")]
    [Tooltip("Độ lùi phía sau và độ cao của camera (vd: X=0, Y=5.5, Z=-7.5)")]
    public Vector3 offset = new Vector3(0f, 5.5f, -7.5f);

    [Tooltip("Điểm nhìn hơi nhích lên/xa phía trước mặt nhân vật để thấy rõ vật cản")]
    public Vector3 lookAheadOffset = new Vector3(0f, 1.2f, 4f);

    public float followSmooth = 8f;
    public float lookSmooth = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // Giữ camera đi theo Z và bám nhẹ theo X của nhân vật để không bị góc khuất
        Vector3 desiredPos = new Vector3(
            target.position.x * 0.65f + offset.x,
            target.position.y + offset.y,
            target.position.z + offset.z
        );

        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);

        Vector3 lookTarget = target.position + lookAheadOffset;
        Quaternion desiredRot = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, lookSmooth * Time.deltaTime);
    }
}