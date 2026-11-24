using UnityEngine;
using TMPro; // Sử dụng TextMeshPro để hiển thị chữ trên màn hình UI

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance;

    [Header("UI References")]
    public TextMeshProUGUI comboText; // Kéo Text UI vào đây trong Inspector

    [Header("Settings")]
    public float comboTimeout = 2.0f; // Thời gian tối đa giữa 2 lần ăn kẹo để giữ chuỗi combo
    private int currentCombo = 0;
    private float lastEatTime;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // Nếu quá thời gian mà không ăn thêm kẹo, chuỗi combo sẽ bị reset
        if (currentCombo > 0 && Time.time - lastEatTime > comboTimeout)
        {
            ResetCombo();
        }
    }

    public void AddCandy()
    {
        currentCombo++;
        lastEatTime = Time.time;
        UpdateComboUI();
    }

    void UpdateComboUI()
    {
        if (comboText == null) return;

        if (currentCombo >= 10)
        {
            comboText.text = "SUGAR RUSH! x" + currentCombo;
            comboText.color = Color.magenta;
        }
        else if (currentCombo >= 5)
        {
            comboText.text = "SUPER! x" + currentCombo;
            comboText.color = Color.red;
        }
        else if (currentCombo >= 3)
        {
            comboText.text = "GREAT! x" + currentCombo;
            comboText.color = Color.yellow;
        }
        else
        {
            comboText.text = "";
        }
    }

    void ResetCombo()
    {
        currentCombo = 0;
        if (comboText != null) comboText.text = "";
    }
}