using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialAmmoUI : MonoBehaviour
{
    [SerializeField] private GameObject pipPrefab;
    [SerializeField] private RectTransform pipContainer;

    [SerializeField] private float pipSizeMult;
    [SerializeField] private float pipRadius;
    [SerializeField] private float arcCenterAngle;
    [SerializeField] private float gapBetween;

    [SerializeField] private Color loadedColour;
    [SerializeField] private Color spentColour;

    private PlayerUI playerUI;
    private Player player;

    private readonly List<Image> pips = new();

    private void Awake()
    {
        playerUI = GetComponent<PlayerUI>();
    }

    private void OnEnable()
    {
        playerUI.Initialized += HandleUIReady;
        if (playerUI.player != null) HandleUIReady(playerUI.player);
    }

    private void OnDisable()
    {
        playerUI.Initialized -= HandleUIReady;
        GameEvents.OnAmmoUsedEmpty -= HandleAmmoUseFailed;
    }

    private void HandleUIReady(Player p)
    {
        player = p;
        player.Combat.OnAmmoChanged += HandleAmmoChanged;
        GameEvents.OnAmmoUsedEmpty += HandleAmmoUseFailed;
    }

    private void HandleAmmoChanged(int currentAmmo) => UpdatePips(currentAmmo);

    private void HandleMaxAmmoChanged(int maxAmmo) => BuildPips(maxAmmo);

    private void HandleAmmoUseFailed()
    {
        for (int i = 0; i < pips.Count; i++)
        {
            pips[i].DOKill();
            pips[i].color = Color.red;
            pips[i].DOColor(spentColour, 0.5f);
        }
    }

    private void BuildPips(int count)
    {
        foreach (var pip in pips)
            if (pip != null) Destroy(pip.transform.parent.gameObject);
        pips.Clear();

        if (count <= 0) return;

        var gap = count <= 15 ? gapBetween : gapBetween / 1.6f;

        for (int i = 0; i < count; i++)
        {
            float angle = RadialUIHelper.AngleForIndex(i, count, arcCenterAngle, gap);
            RadialUIHelper.CreatePivot(pipContainer, pipPrefab, -angle, pipRadius, out var instance);

            instance.transform.localScale = Vector3.one * pipSizeMult;
            var image = instance.GetComponent<Image>() ?? instance.GetComponentInChildren<Image>();
            image.color = loadedColour;
            pips.Add(image);
        }
    }

    private void UpdatePips(int currentAmmo)
    {
        if (pips.Count <= 0)
            BuildPips(currentAmmo);

        for (int i = 0; i < pips.Count; i++)
        {
            bool loaded = i < currentAmmo;
            Color target = loaded ? loadedColour : spentColour;

            pips[i].DOKill();
            pips[i].DOColor(target, 0.15f).SetEase(Ease.OutBack);
        }
    }
}
