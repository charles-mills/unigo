using UnityEngine;
using World;
using Vector3 = UnityEngine.Vector3;

public class DestroyAfterTime : MonoBehaviour
{
    public float time;
    public float fadeDuration = 0.6f;
    public float timeTillDestruction;

    public TokenSpawner spawner;
    private bool fadingIn = true;
    private bool fadingOut;

    private Vector3 targetScale;


    private void Start()
    {
        targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    private void Update()
    {
        time += Time.deltaTime;

        if (fadingIn)
        {
            var t = Mathf.Clamp01(time / fadeDuration);
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);

            if (t >= 1f) fadingIn = false;
        }

        var timeLeft = timeTillDestruction - time;

        if (!fadingOut && timeLeft <= fadeDuration) fadingOut = true;

        if (fadingOut)
        {
            var t = Mathf.Clamp01(timeLeft / fadeDuration);
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
        }

        if (time < timeTillDestruction) return;

        spawner.currentTokens--;
        Destroy(gameObject);
    }
}