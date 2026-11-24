using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class ChibiRunnerController : MonoBehaviour
{
    [Header("Chặn sinh bản sao đường thừa")]
    public bool xoaDoanDuongCloneKhiVaoGame = true;

    [Header("Di chuyển theo phím")]
    public float moveSpeed = 7f;
    public float laneLimitX = 2.5f;

    [Header("Bật nhảy nhẹ (phím Space)")]
    public bool enableJump = true;
    [Tooltip("Độ cao bật nhảy nhẹ khi nhấn Space")]
    public float jumpHeight = 0.85f;
    [Tooltip("Thời gian 1 nhịp bật nhảy (giây)")]
    public float jumpDuration = 0.32f;
    [Tooltip("Thời gian nghỉ giữa 2 lần nhảy")]
    public float jumpCooldown = 0.12f;

    [Header("Chạm quái vật thì chơi lại")]
    public bool restartOnEnemyTouch = true;
    public string enemyTag = "Enemy";
    [Tooltip("Bán kính kiểm tra chạm quái quanh nhân vật")]
    public float enemyHitRadius = 1.25f;
    public float restartDelay = 0.05f;

    [Header("Chặn vật cản (không đi xuyên qua)")]
    [Tooltip("Khoảng đệm nhỏ để dừng trước mặt vật cản")]
    public float obstacleSkinWidth = 0.05f;
    public LayerMask obstacleLayer = ~0;

    [Header("Góc xoay khi né sang bên (chống quay 90 độ)")]
    public float maxSteerAngleY = 20f;
    public float turnSpeed = 10f;

    [Header("Bám mặt đường dứt điểm (chống rơi & bật)")]
    public float fallbackRoadY = 0f;
    public float groundOffset = 0.05f;
    public LayerMask groundLayer = ~0;

    [Header("Animation tay chân")]
    public Animator animator;
    public string speedParam = "Speed";
    public string motionSpeedParam = "MotionSpeed";
    public string winTriggerParam = "Win";

    [Header("Chiến thắng & Hiệu ứng ăn mừng")]
    public GameObject winPanelOrText;
    public ParticleSystem winEffect;
    public AudioClip winSound;
    [Tooltip("Chiều cao mỗi cú nhảy ăn mừng")]
    public float winJumpHeight = 1.15f;
    [Tooltip("Số lần nhảy tưng bừng khi thắng")]
    public int winJumpCount = 4;
    [Tooltip("Thời gian mỗi nhịp nhảy")]
    public float winJumpDuration = 0.34f;

    private Rigidbody rb;
    private CapsuleCollider col;
    private bool isFinished = false;
    private bool isRestarting = false;
    private bool hasSpeedParam;
    private bool hasMotionSpeedParam;
    private bool hasWinTrigger;
    private Vector3 inputDir;
    private float baseYawY;
    private Vector3 baseLocalScale;

    private bool isJumping = false;
    private float jumpTimer = 0f;
    private float jumpOffsetY = 0f;
    private float nextJumpTime = 0f;

    void Awake()
    {
        if (xoaDoanDuongCloneKhiVaoGame)
        {
            XoaCacBanSaoDoanDuongClone();
        }

        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        if (animator == null)
            animator = GetComponent<Animator>();

        baseYawY = transform.eulerAngles.y;
        baseLocalScale = transform.localScale;

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (Mathf.Approximately(fallbackRoadY, 0f))
        {
            fallbackRoadY = LayDoCaoMatDatHoacHienTai(transform.position.y);
        }

        if (animator != null)
        {
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == speedParam) hasSpeedParam = true;
                if (p.name == motionSpeedParam) hasMotionSpeedParam = true;
                if (p.name == winTriggerParam && p.type == AnimatorControllerParameterType.Trigger)
                    hasWinTrigger = true;
            }
        }

        if (winPanelOrText != null)
            winPanelOrText.SetActive(false);
    }

    private void XoaCacBanSaoDoanDuongClone()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GameObject go in allObjects)
        {
            if (go == null || go == gameObject) continue;

            if (go.name.StartsWith("DoanDuongMau") && go.name.Contains("(Clone)"))
            {
                go.SetActive(false);
                Destroy(go);
            }
        }
    }

    private bool PressedJumpKeyThisFrame()
    {
        if (!Application.isFocused) return false;

        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            pressed = true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (!pressed && Input.GetKeyDown(KeyCode.Space))
        {
            pressed = true;
        }
#endif
        return pressed;
    }

    void Update()
    {
        if (isFinished || isRestarting)
        {
            inputDir = Vector3.zero;
            return;
        }

        float h = 0f;
        float v = 0f;
#if ENABLE_LEGACY_INPUT_MANAGER
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
#endif
#if ENABLE_INPUT_SYSTEM
        if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f) && Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;
        }
#endif

        inputDir = new Vector3(h, 0f, v);
        if (inputDir.sqrMagnitude > 1f)
            inputDir.Normalize();

        if (enableJump && PressedJumpKeyThisFrame() && !isJumping && Time.time >= nextJumpTime)
        {
            isJumping = true;
            jumpTimer = 0f;
            nextJumpTime = Time.time + jumpDuration + jumpCooldown;
        }
    }

    void FixedUpdate()
    {
        if (isFinished || isRestarting)
        {
            SetRunAnimation(0f);
            return;
        }

        float currentSpeed = inputDir.magnitude * moveSpeed;
        Vector3 horizontalMove = new Vector3(inputDir.x, 0f, inputDir.z) * moveSpeed * Time.fixedDeltaTime;

        Vector3 allowedHorizontalMove = TinhBuocDiChuyenKhongXuyenVatCan(horizontalMove);

        Vector3 targetPos = rb.position + allowedHorizontalMove;
        targetPos.x = Mathf.Clamp(targetPos.x, -laneLimitX, laneLimitX);

        float groundedY = LayDoCaoMatDatHoacHienTai(targetPos.y);

        if (isJumping)
        {
            jumpTimer += Time.fixedDeltaTime;
            float u = Mathf.Clamp01(jumpTimer / Mathf.Max(0.05f, jumpDuration));
            jumpOffsetY = 4f * jumpHeight * u * (1f - u);

            if (jumpTimer >= jumpDuration)
            {
                isJumping = false;
                jumpTimer = 0f;
                jumpOffsetY = 0f;
            }
        }
        else
        {
            jumpOffsetY = 0f;
        }

        targetPos.y = groundedY + jumpOffsetY;

        rb.MovePosition(targetPos);

        if (restartOnEnemyTouch)
        {
            KiemTraChamQuaiTheoKhoangCach();
        }

        float targetYaw = baseYawY + (inputDir.x * maxSteerAngleY);
        Quaternion targetRot = Quaternion.Euler(0f, targetYaw, 0f);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));

        SetRunAnimation(currentSpeed);
    }

    private Vector3 TinhBuocDiChuyenKhongXuyenVatCan(Vector3 desiredMove)
    {
        float dist = desiredMove.magnitude;
        if (dist <= 0.0001f || col == null)
            return desiredMove;

        Vector3 dir = desiredMove / dist;
        float scaleY = Mathf.Abs(transform.lossyScale.y);
        float scaleXZ = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));

        float radius = Mathf.Max(0.05f, col.radius * scaleXZ - 0.02f);
        float height = Mathf.Max(radius * 2f, col.height * scaleY);
        float halfSpan = Mathf.Max(0f, height * 0.5f - radius);

        Vector3 centerWorld = transform.TransformPoint(col.center);
        centerWorld.y += 0.12f;

        Vector3 point1 = centerWorld + Vector3.up * halfSpan;
        Vector3 point2 = centerWorld - Vector3.up * halfSpan;

        RaycastHit[] hits = Physics.CapsuleCastAll(point1, point2, radius, dir, dist + obstacleSkinWidth, obstacleLayer, QueryTriggerInteraction.Ignore);

        float nearestDist = float.MaxValue;
        bool hitSolidObstacle = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)) continue;
            if (hit.collider.gameObject.tag == "Finish") continue;
            if (hit.normal.y > 0.7f) continue;

            if (hit.distance < nearestDist)
            {
                nearestDist = hit.distance;
                hitSolidObstacle = true;
            }
        }

        if (hitSolidObstacle)
        {
            float safeDist = Mathf.Max(0f, nearestDist - obstacleSkinWidth);
            return dir * safeDist;
        }

        return desiredMove;
    }

    private float LayDoCaoMatDatHoacHienTai(float currentY)
    {
        float halfHeight = col != null ? (col.height * 0.5f * transform.lossyScale.y) : 0.5f;
        float checkBaseY = currentY - jumpOffsetY;
        Vector3 rayOrigin = new Vector3(transform.position.x, checkBaseY + halfHeight + 1.5f, transform.position.z);

        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 10f, groundLayer, QueryTriggerInteraction.Ignore);
        float bestGroundY = float.NegativeInfinity;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)) continue;

            if (hit.point.y > bestGroundY)
            {
                bestGroundY = hit.point.y;
            }
        }

        if (bestGroundY > float.NegativeInfinity)
        {
            fallbackRoadY = bestGroundY + groundOffset;
            return fallbackRoadY;
        }

        return fallbackRoadY;
    }

    private void SetRunAnimation(float currentSpeed)
    {
        if (animator == null) return;

        if (hasSpeedParam)
            animator.SetFloat(speedParam, currentSpeed);

        if (hasMotionSpeedParam)
            animator.SetFloat(motionSpeedParam, currentSpeed > 0.05f ? 1f : 0f);
    }

    private bool LaQuaiVat(Transform target)
    {
        if (target == null) return false;

        Transform curr = target;
        while (curr != null)
        {
            if (!string.IsNullOrEmpty(enemyTag) && curr.gameObject.tag == enemyTag)
                return true;

            string n = curr.gameObject.name.ToLower();
            if (n.Contains("starfish") || n.Contains("blueslime") || n.Contains("slime") || n.Contains("enemy"))
                return true;

            Component[] comps = curr.GetComponents<Component>();
            foreach (Component c in comps)
            {
                if (c == null) continue;
                if (c.GetType().Name == "EnemyPatrolAndRestart")
                    return true;
            }

            curr = curr.parent;
        }

        return false;
    }

    private void KiemTraChamQuaiTheoKhoangCach()
    {
        if (isFinished || isRestarting) return;

        Collider[] nearby = Physics.OverlapSphere(rb.position + Vector3.up * 0.5f, enemyHitRadius, ~0, QueryTriggerInteraction.Collide);
        foreach (Collider c in nearby)
        {
            if (c == null) continue;
            if (c.transform == transform || c.transform.IsChildOf(transform)) continue;

            if (LaQuaiVat(c.transform))
            {
                Debug.Log("[ChibiRunnerController] Chạm quái vật -> Chơi lại từ đầu: " + c.name);
                KichHoatChoiLai();
                return;
            }
        }
    }

    private void KichHoatChoiLai()
    {
        if (isFinished || isRestarting) return;
        isRestarting = true;
        isJumping = false;
        jumpOffsetY = 0f;
        SetRunAnimation(0f);
        Debug.Log("[ChibiRunnerController] Chạm quái vật -> Chơi lại từ đầu!");
        Invoke(nameof(NapLaiManChoi), restartDelay);
    }

    private void NapLaiManChoi()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    public void KichHoatChienThang()
    {
        if (isFinished || isRestarting) return;

        isFinished = true;
        SetRunAnimation(0f);

        if (animator != null && hasWinTrigger)
        {
            animator.SetTrigger(winTriggerParam);
        }

        if (winSound != null)
        {
            AudioSource.PlayClipAtPoint(winSound, transform.position);
        }

        if (winEffect != null)
        {
            winEffect.Play();
        }
        else
        {
            TaoHieuUngPhaoHoaDonGian();
        }

        StartCoroutine(HienBangChienThangNay());
        StartCoroutine(NhayMungChienThang());
    }

    private IEnumerator HienBangChienThangNay()
    {
        if (winPanelOrText == null) yield break;

        winPanelOrText.SetActive(true);
        TrangTriDoBongChienThang();
        Transform uiTf = winPanelOrText.transform;
        Vector3 targetScale = uiTf.localScale;
        if (targetScale.sqrMagnitude < 0.001f) targetScale = Vector3.one;

        float t = 0f;
        float duration = 0.35f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float bounce = 1f + Mathf.Sin(p * Mathf.PI) * 0.22f * (1f - p);
            uiTf.localScale = targetScale * (p * bounce);
            yield return null;
        }
        uiTf.localScale = targetScale;
    }

    private void TrangTriDoBongChienThang()
    {
        if (winPanelOrText == null) return;

        Color mauVien = new Color(0.32f, 0.06f, 0.24f, 1f);
        Color mauBongDo = new Color(0.10f, 0.02f, 0.08f, 0.88f);

        UnityEngine.UI.Text legacyText = winPanelOrText.GetComponentInChildren<UnityEngine.UI.Text>(true);
        if (legacyText != null)
        {
            if (legacyText.GetComponent<UnityEngine.UI.Outline>() == null)
            {
                var outline = legacyText.gameObject.AddComponent<UnityEngine.UI.Outline>();
                outline.effectColor = mauVien;
                outline.effectDistance = new Vector2(4f, -4f);
            }

            if (legacyText.GetComponent<UnityEngine.UI.Shadow>() == null)
            {
                var shadow = legacyText.gameObject.AddComponent<UnityEngine.UI.Shadow>();
                shadow.effectColor = mauBongDo;
                shadow.effectDistance = new Vector2(4f, -9f);
            }
        }

        Component[] allComponents = winPanelOrText.GetComponentsInChildren<Component>(true);
        foreach (Component comp in allComponents)
        {
            if (comp == null) continue;
            System.Type type = comp.GetType();
            if (type.Name == "TextMeshProUGUI" || type.Name == "TextMeshPro")
            {
                var matProp = type.GetProperty("fontMaterial");
                if (matProp != null)
                {
                    Material currentMat = matProp.GetValue(comp) as Material;
                    if (currentMat != null)
                    {
                        Material instMat = new Material(currentMat);

                        if (instMat.HasProperty("_OutlineColor"))
                            instMat.SetColor("_OutlineColor", mauVien);
                        if (instMat.HasProperty("_OutlineWidth"))
                            instMat.SetFloat("_OutlineWidth", 0.24f);

                        if (instMat.HasProperty("_UnderlayColor"))
                            instMat.SetColor("_UnderlayColor", mauBongDo);
                        if (instMat.HasProperty("_UnderlayOffsetX"))
                            instMat.SetFloat("_UnderlayOffsetX", 0.45f);
                        if (instMat.HasProperty("_UnderlayOffsetY"))
                            instMat.SetFloat("_UnderlayOffsetY", -0.65f);
                        if (instMat.HasProperty("_UnderlayDilate"))
                            instMat.SetFloat("_UnderlayDilate", 0.25f);
                        if (instMat.HasProperty("_UnderlaySoftness"))
                            instMat.SetFloat("_UnderlaySoftness", 0.45f);

                        instMat.EnableKeyword("UNDERLAY_ON");
                        matProp.SetValue(comp, instMat);
                    }
                }
            }
        }
    }

    private IEnumerator NhayMungChienThang()
    {
        Vector3 startPos = transform.position;
        float groundY = LayDoCaoMatDatHoacHienTai(startPos.y);
        startPos.y = groundY;

        Quaternion startRot = transform.rotation;
        Quaternion celebrateRot = Quaternion.Euler(0f, 180f, 0f);

        float rotElapsed = 0f;
        while (rotElapsed < 0.25f)
        {
            rotElapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, celebrateRot, rotElapsed / 0.25f);
            yield return null;
        }
        transform.rotation = celebrateRot;

        for (int i = 0; i < winJumpCount; i++)
        {
            float strength = (i == winJumpCount - 1) ? 0.55f : 1f;
            float jumpH = winJumpHeight * strength;
            float elapsed = 0f;

            while (elapsed < winJumpDuration)
            {
                elapsed += Time.deltaTime;
                float u = Mathf.Clamp01(elapsed / winJumpDuration);

                float yOffset = 4f * jumpH * u * (1f - u);
                transform.position = new Vector3(startPos.x, groundY + yOffset, startPos.z);

                float squash = 1f + Mathf.Sin(u * Mathf.PI * 2f) * 0.1f;
                transform.localScale = new Vector3(
                    baseLocalScale.x / Mathf.Sqrt(squash),
                    baseLocalScale.y * squash,
                    baseLocalScale.z / Mathf.Sqrt(squash)
                );

                if (i == 1 || i == 2)
                {
                    float yaw = 180f + u * 360f;
                    transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                }

                yield return null;
            }

            transform.position = new Vector3(startPos.x, groundY, startPos.z);
            transform.localScale = baseLocalScale;
            transform.rotation = celebrateRot;
        }
    }

    private void TaoHieuUngPhaoHoaDonGian()
    {
        GameObject burstObj = new GameObject("WinBurstEffect");
        burstObj.transform.position = transform.position + Vector3.up * 1.2f;

        ParticleSystem ps = burstObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.duration = 0.6f;
        main.startLifetime = 1.1f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
        main.gravityModifier = 0.8f;
        main.maxParticles = 45;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 35) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.35f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(new Color(1f, 0.85f, 0.2f), 0f),
                new GradientColorKey(new Color(1f, 0.45f, 0.75f), 0.5f),
                new GradientColorKey(new Color(0.4f, 0.9f, 1f), 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = grad;

        ps.Play();
        Destroy(burstObj, 2.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFinished || isRestarting) return;
        if (other.gameObject.tag == "Finish")
        {
            KichHoatChienThang();
            return;
        }
        if (restartOnEnemyTouch && LaQuaiVat(other.transform))
        {
            Debug.Log("[ChibiRunnerController] Chạm quái (Trigger): " + other.name);
            KichHoatChoiLai();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFinished || isRestarting) return;
        if (collision.gameObject.tag == "Finish")
        {
            KichHoatChienThang();
            return;
        }
        if (restartOnEnemyTouch && LaQuaiVat(collision.transform))
        {
            Debug.Log("[ChibiRunnerController] Chạm quái (Collision): " + collision.gameObject.name);
            KichHoatChoiLai();
        }
    }

    public void OnFootstep() { }
    public void OnLand() { }
}