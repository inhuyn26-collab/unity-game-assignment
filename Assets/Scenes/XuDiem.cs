using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class XuDiem : MonoBehaviour
{
    private static readonly List<XuDiem> cacXuThuongConLai = new List<XuDiem>();

    [Header("Phân loại đồng xu")]
    public bool laXuChienThang = false;
    public bool batBuocAnHetXuThuong = true;

    [Header("Khoảng cách dự phòng quanh đúng 1 xu")]
    public float banKinhAnXu = 0.45f;
    public float doLechCaoY = 1.0f;

    [Header("Hiệu ứng xoay tại chỗ")]
    public bool tuXoay = true;
    public float tocDoXoayY = 140f;

    [Header("Âm thanh / Hiệu ứng (tùy chọn)")]
    public AudioClip amThanhAnXu;
    public ParticleSystem hieuUngAnXu;

    private bool daDuocAn = false;
    private Collider colTrigger;
    private ChibiRunnerController runnerCached;

    void Awake()
    {
        colTrigger = GetComponent<Collider>();
        colTrigger.isTrigger = true;
        runnerCached = FindFirstObjectByType<ChibiRunnerController>();

        if (!laXuChienThang && !cacXuThuongConLai.Contains(this))
        {
            cacXuThuongConLai.Add(this);
        }
    }

    void OnDestroy()
    {
        cacXuThuongConLai.Remove(this);
    }

    void Update()
    {
        if (daDuocAn) return;

        if (tuXoay)
        {
            transform.Rotate(0f, tocDoXoayY * Time.deltaTime, 0f, Space.World);
        }

        if (runnerCached == null)
        {
            runnerCached = FindFirstObjectByType<ChibiRunnerController>();
        }

        if (runnerCached != null)
        {
            Vector3 pCoin = colTrigger != null ? colTrigger.bounds.center : transform.position;
            Vector3 pPlayer = runnerCached.transform.position;

            float dx = pCoin.x - pPlayer.x;
            float dz = pCoin.z - pPlayer.z;
            float distXZ = Mathf.Sqrt(dx * dx + dz * dz);
            float dy = Mathf.Abs(pCoin.y - (pPlayer.y + 0.65f));

            if (distXZ <= banKinhAnXu && dy <= doLechCaoY)
            {
                XuLyKhiCham(runnerCached);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (daDuocAn) return;

        ChibiRunnerController runner = other.GetComponentInParent<ChibiRunnerController>();
        if (runner == null)
        {
            runner = FindFirstObjectByType<ChibiRunnerController>();
        }

        if (runner == null) return;
        XuLyKhiCham(runner);
    }

    private void XuLyKhiCham(ChibiRunnerController runner)
    {
        if (daDuocAn || runner == null) return;

        if (laXuChienThang && batBuocAnHetXuThuong)
        {
            cacXuThuongConLai.RemoveAll(x => x == null || x.daDuocAn || !x.gameObject.activeInHierarchy);
            if (cacXuThuongConLai.Count > 0)
            {
                return;
            }
        }

        daDuocAn = true;

        if (colTrigger != null)
        {
            colTrigger.enabled = false;
        }

        if (!laXuChienThang)
        {
            cacXuThuongConLai.Remove(this);
        }

        if (amThanhAnXu != null)
        {
            AudioSource.PlayClipAtPoint(amThanhAnXu, transform.position);
        }

        if (hieuUngAnXu != null)
        {
            ParticleSystem vfx = Instantiate(hieuUngAnXu, transform.position, Quaternion.identity);
            vfx.Play();
            Destroy(vfx.gameObject, 3f);
        }

        if (laXuChienThang)
        {
            runner.KichHoatChienThang();
        }

        Debug.Log("[XuDiem] Đã ăn: " + gameObject.name);

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        gameObject.SetActive(false);
    }
}