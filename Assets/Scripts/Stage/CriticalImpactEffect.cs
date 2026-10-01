using System.Collections;
using UnityEngine;

// Applies a bright flash inside the enemy sprite's existing alpha silhouette.
public sealed class CriticalImpactEffect : MonoBehaviour
{
    private const float Hold = 0.07f;
    private const float Fade = 0.08f;
    private const float MaxFlashAmount = 0.55f;
    private static readonly Color FlashColor = new Color(1f, 0.78f, 0.62f, 1f);

    private Material material;
    private SpriteRenderer[] renderers;
    private Material[] originalMaterials;
    private Coroutine routine;

    public static void Play(GameObject target)
    {
        if (target == null || !target.activeInHierarchy) return;
        CriticalImpactEffect effect = target.GetComponent<CriticalImpactEffect>();
        if (effect == null) effect = target.AddComponent<CriticalImpactEffect>();
        effect.Begin();
    }

    private void Awake()
    {
        Shader shader = Resources.Load<Shader>("CriticalImpact");
        if (shader != null && shader.isSupported)
            material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
    }

    private void Begin()
    {
        if (material == null) return;
        if (routine != null) StopCoroutine(routine);
        RestoreMaterials();

        material.SetColor("_FlashColor", FlashColor);

        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].sharedMaterial;
            renderers[i].sharedMaterial = material;
        }
        routine = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        material.SetFloat("_Amount", MaxFlashAmount);
        float elapsed = 0f;
        while (elapsed < Hold)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < Fade)
        {
            elapsed += Time.unscaledDeltaTime;
            material.SetFloat("_Amount", MaxFlashAmount * (1f - Mathf.Clamp01(elapsed / Fade)));
            yield return null;
        }
        RestoreMaterials();
        routine = null;
    }

    private void RestoreMaterials()
    {
        if (renderers == null || originalMaterials == null) return;
        int count = Mathf.Min(renderers.Length, originalMaterials.Length);
        for (int i = 0; i < count; i++)
            if (renderers[i] != null && renderers[i].sharedMaterial == material)
                renderers[i].sharedMaterial = originalMaterials[i];
        renderers = null;
        originalMaterials = null;
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        RestoreMaterials();
    }

    private void OnDestroy()
    {
        RestoreMaterials();
        if (material != null) Destroy(material);
    }
}
