using System;
using System.Collections.Generic;
using UnityEngine;

public class ThemedSpriteSet : MonoBehaviour
{
    [Serializable]
    public class ThemeSpriteEntry
    {
        public BlockTheme theme;
        public Sprite sprite;
    }

    [Serializable]
    public class RendererBinding
    {
        public SpriteRenderer targetRenderer;
        public List<ThemeSpriteEntry> themeSprites = new List<ThemeSpriteEntry>();
    }

    [SerializeField] private List<RendererBinding> bindings = new List<RendererBinding>();
    [SerializeField] private BlockTheme currentTheme = BlockTheme.Set1;

    public void ApplyTheme(BlockTheme theme)
    {
        currentTheme = theme;

        for (int i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];
            if (binding == null || binding.targetRenderer == null)
                continue;

            Sprite sprite = GetSpriteForTheme(binding, theme);
            if (sprite != null)
                binding.targetRenderer.sprite = sprite;
        }
    }

    private Sprite GetSpriteForTheme(RendererBinding binding, BlockTheme theme)
    {
        if (binding.themeSprites == null)
            return null;

        for (int i = 0; i < binding.themeSprites.Count; i++)
        {
            var entry = binding.themeSprites[i];
            if (entry != null && entry.theme == theme)
                return entry.sprite;
        }

        return null;
    }

    [ContextMenu("Rebuild Bindings From Child SpriteRenderers")]
    private void RebuildBindingsFromChildren()
    {
        bindings.Clear();

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            RendererBinding binding = new RendererBinding();
            binding.targetRenderer = renderers[i];
            bindings.Add(binding);
        }
    }
}