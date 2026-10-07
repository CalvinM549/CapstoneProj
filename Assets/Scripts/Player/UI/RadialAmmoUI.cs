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

    private readonly List<(Image visual, bool loaded)> pips = new();

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
        //player.Combat.OnWeaponChanged += HandleMaxAmmoChanged;
        GameEvents.OnAmmoUsedEmpty += HandleAmmoUseFailed;
    }

    private void HandleAmmoChanged(int currentAmmo) => UpdatePips(currentAmmo);

    private void HandleMaxAmmoChanged(int maxAmmo) => BuildPips(maxAmmo);

    private void HandleAmmoUseFailed()
    {
        foreach (var p in pips)
        {
            p.visual.DOKill();
            p.visual.color = Color.red;
            p.visual.DOColor(spentColour, 0.5f);
        }
    }

    private void BuildPips(int count)
    {
        Debug.Log("building pips");

        foreach (var p in pips)
        {
            p.visual.DOKill();
            Destroy(p.visual.transform.parent.gameObject);
        }
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

            pips.Add((image, true));
        }
    }

    private void UpdatePips(int currentAmmo)
    {
        if (pips.Count <= 0)
            BuildPips(currentAmmo);

        for (int i = 0; i < pips.Count; i++)
        {

            bool slotLoaded = i < currentAmmo;
            Color targetColour = slotLoaded ? loadedColour : spentColour;

            var p = pips[i];

            Debug.Log($"updating pip {i}, currently {p.loaded} => {slotLoaded}");

            p.visual.DOKill();
            p.visual.transform.DOKill();
            if (slotLoaded && !p.loaded)
            {
                p.visual.color = Color.green;
                p.visual.DOColor(targetColour, 0.4f);

                p.visual.transform.localScale = Vector2.one * pipSizeMult * 2f;
                p.visual.transform.DOScale(Vector2.one * pipSizeMult, 0.4f);
            }
            else
            {
                p.visual.DOColor(targetColour, 0.2f);
            }

            pips[i] = (pips[i].visual, slotLoaded);

            //pips[i]?.DOKill();
            //pips[i].DOColor(targetColour, 0.15f).SetEase(Ease.OutBack);
        }
    }
}
