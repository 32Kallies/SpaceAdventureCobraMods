using UnityEngine;

namespace GameOverScreen.UI;

public class FadeInCanvasGroup : MonoBehaviour
{
    public CanvasGroup group;
    public float fadeDuration = 1f;
    private float _alpha;

    private void Update()
    {
        _alpha = Mathf.Clamp01(_alpha + Time.deltaTime / fadeDuration);
        group.alpha = _alpha;
        if (_alpha >= 1f)
        {
            enabled = false;
        }
    }
}