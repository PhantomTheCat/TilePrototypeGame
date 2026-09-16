using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DamagePopup : MonoBehaviour
{
    //Properties
    [Header("Settings")]
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float totalSeconds = 2f;
    [SerializeField] private float fadeSpeed = 3f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float scaleSize = 1f;
    [SerializeField] private float critFontSizeIncrease = 5f;

    private float timer = 0f;
    private static int sortingOrder = 0;

    [Header("Colors")]
    [SerializeField] private Color regColor;
    [SerializeField] private Color critColor;
    private Color meshColor;

    //Methods
    public static DamagePopup Create(Vector3 position, int damageAmount, bool isCritical)
    {
        if (UIManager.Instance.DamagePopupPrefab == null) return null;
        DamagePopup damagePopup = Instantiate(UIManager.Instance.DamagePopupPrefab, position, Quaternion.identity);
        if (damagePopup == null) return null;
        if (damagePopup.textMesh == null) return null;
        damagePopup.Setup(damageAmount, isCritical);
        return damagePopup;
    }

    private void Update()
    {
        if (textMesh == null) return;
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        if (timer > totalSeconds * 0.5f)
        {
            //Increase size if in first half of timer
            transform.localScale += Vector3.one * scaleSize * Time.deltaTime;
        }
        else
        {
            //Decrease size if in second half of timer
            transform.localScale -= Vector3.one * scaleSize * Time.deltaTime;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            meshColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = meshColor;
            if (meshColor.a <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void Setup(int damageAmount, bool isCritical)
    {
        //Change font size based on if crit or not
        textMesh.fontSize = isCritical ? textMesh.fontSize + critFontSizeIncrease : textMesh.fontSize;
        textMesh.text = damageAmount.ToString();
        textMesh.color = isCritical ? critColor : regColor;
        timer = totalSeconds;
        meshColor = textMesh.color;

        textMesh.sortingOrder = sortingOrder;
        sortingOrder++;
    }
}