using UnityEngine;

public class DebugActiveChildren : MonoBehaviour
{
    public GameObject playersHolder;

    void Update()
    {
        if (playersHolder != null)
        {
            LogActiveStateOfActiveChildren(playersHolder.transform);
        }
        else
        {
            Debug.LogError("playersHolder is not assigned.");
        }
    }

    void LogActiveStateOfActiveChildren(Transform parent)
    {
        if (parent.gameObject.activeSelf)
        {
            int childCount = parent.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);

                if (child.gameObject.activeSelf)
                {
                    Debug.Log($"Active Child {i}: {child.name} is active.");

                    // 再帰的に子オブジェクトのアクティブ状態を確認
                    LogActiveStateOfActiveChildren(child);
                }
            }
        }
    }
}
