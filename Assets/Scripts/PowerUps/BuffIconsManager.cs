using NUnit.Framework.Internal;
using UnityEngine;

public class BuffIconsManager : MonoBehaviour
{
    public static BuffIconsManager Instance;

    public GameObject iconPrefab;
    public Transform iconsParent;



    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (iconsParent == null)
            iconsParent = transform;
    }

    public void ShowEffectIcon(Sprite sprite, float duration)
    {
        if (iconPrefab == null)
        {
            Debug.LogWarning("iconPrefab is null!");
            return;
        }

        var go = Instantiate(iconPrefab, iconsParent);
        var icon = go.GetComponent<BuffIconSimple>();
        if (icon != null)
        {
            icon.Init(sprite, duration);
        }
        else
        {
            Debug.LogWarning("BuffIconSimple component not found on prefab!");
        }
    }
}
