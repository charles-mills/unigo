using System;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;
using UnityEngine.UIElements;

public class InventorySlot : VisualElement
{
    private static readonly Dictionary<string, BrandType> BrandNameMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Nike", BrandType.Nike },
        { "Bike", BrandType.Nike },
        { "Adidas", BrandType.Adidas },
        { "Abibas", BrandType.Adidas },
        { "Dominos", BrandType.Dominos },
        { "Bominos", BrandType.Dominos },
        { "Microsoft", BrandType.Microsoft },
        { "Smallsoft", BrandType.Microsoft }
    };

    private static readonly Dictionary<string, string> BrandPrefabPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Nike", "Prefabs/Tokens/NikeToken" },
        { "Bike", "Prefabs/Tokens/NikeToken" },
    
        { "Adidas", "Prefabs/Tokens/AdidasToken" },
        { "Abibas", "Prefabs/Tokens/AdidasToken" },
    
        { "Dominos", "Prefabs/Tokens/DominosToken" },
        { "Bominos", "Prefabs/Tokens/DominosToken" },
    
        { "Microsoft", "Prefabs/Tokens/MicrosoftToken" },
        { "Smallsoft", "Prefabs/Tokens/MicrosoftToken" }
    };
    
    private readonly VisualElement brandVisual;
    private readonly Label nameLabel;
    private readonly Label codeLabel;
    private readonly Button markAsUsedButton;

    
    private string m_Code;

    public InventorySlot()
    {
        AddToClassList("profile-card");
        AddToClassList("inventory-row");

        style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f, 0.12f));
        style.borderTopWidth = 1;
        style.borderBottomWidth = 1;
        style.borderLeftWidth = 1;
        style.borderRightWidth = 1;
        style.borderTopColor = Color.white;
        style.borderBottomColor = Color.white;
        style.borderLeftColor = Color.white;
        style.borderRightColor = Color.white;


        brandVisual = new VisualElement();
        brandVisual.AddToClassList("inventory-brand-visual");
        Add(brandVisual);

        var textContainer = new VisualElement();
        textContainer.style.flexGrow = 1;
        textContainer.style.flexDirection = FlexDirection.Column;

        nameLabel = new Label();
        nameLabel.AddToClassList("profile-card-title");
        nameLabel.style.color = Color.white;
        textContainer.Add(nameLabel);

        codeLabel = new Label();
        codeLabel.AddToClassList("profile-card-body");
        codeLabel.style.color = Color.white;
        textContainer.Add(codeLabel);

        Add(textContainer);

        markAsUsedButton = new Button(OnMarkAsUsedClicked)
        {
            text = "Used?"
        };
        markAsUsedButton.AddToClassList("markUsedButton");
        Add(markAsUsedButton);
    }

    public event Action<string> MarkAsUsedClicked;
    

    public void SetItem(string itemName, string code)
    {
        SetBrandVisual(itemName);
        nameLabel.text = itemName;
        codeLabel.text = $"Promo code: {code}";
        m_Code = code;
    }

    public void SetItem(string itemName, string code, string expiryText)
    {
        SetBrandVisual(itemName);
        nameLabel.text = itemName;
        codeLabel.text = $"Promo code: {code}";
        m_Code = code;
    }


    private void SetBrandVisual(string itemName)
    {
        var texture = LoadBrandTexture(itemName);

        if (texture != null)
        {
            brandVisual.style.backgroundImage = new StyleBackground(texture);
            brandVisual.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            return;
        }

        brandVisual.style.backgroundImage = StyleKeyword.None;
    }

    private static Texture2D LoadBrandTexture(string itemName)
    {
        var fallback = Resources.Load<Texture2D>("Icons/brandBox");
        if (string.IsNullOrWhiteSpace(itemName)) return fallback;

        var prefab = ResolvePrefab(itemName.Trim());
        if (prefab == null) return fallback;

        return ExtractTextureFromPrefab(prefab) ?? fallback;
    }

    private static GameObject ResolvePrefab(string itemName)
    {
        if (BrandNameMap.TryGetValue(itemName, out var brandType))
        {
            var registry = UnityEngine.Object.FindAnyObjectByType<TokenRegistry>();
            if (registry != null)
            {
                var fromRegistry = registry.GetPrefab(brandType);
                if (fromRegistry != null) return fromRegistry;
            }
        }

        if (!BrandPrefabPaths.TryGetValue(itemName, out var prefabPath)) return null;
        return Resources.Load<GameObject>(prefabPath);
    }

    private static Texture2D ExtractTextureFromPrefab(GameObject prefab)
    {
        var spriteRenderer = prefab.GetComponentInChildren<SpriteRenderer>(true);
        if (spriteRenderer?.sprite?.texture != null)
            return spriteRenderer.sprite.texture;

        var renderer = prefab.GetComponentInChildren<Renderer>(true);
        var material = renderer?.sharedMaterial;
        if (material == null) return null;

        if (material.mainTexture is Texture2D mainTexture)
            return mainTexture;

        if (material.HasProperty("_BaseMap") && material.GetTexture("_BaseMap") is Texture2D baseMap)
            return baseMap;

        return null;
    }

    private void OnMarkAsUsedClicked()
    {
        MarkAsUsedClicked?.Invoke(m_Code);
    }
}
