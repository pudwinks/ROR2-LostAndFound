using System.Collections;
using RoR2;
using UnityEngine;

[RequireComponent(typeof(EffectComponent))]
public class EquipmentTakenOrbEffect : MonoBehaviour
{
    public TrailRenderer trailToColor;

    public ParticleSystem[] particlesToColor;

    public SpriteRenderer[] spritesToColor;

    public SpriteRenderer iconSpriteRenderer;

    private void OnEnable()
    {
        StartCoroutine(DelayedUpdateSprite());
    }

    private IEnumerator DelayedUpdateSprite()
    {
        yield return 0;
        EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef((EquipmentIndex)Util.UintToIntMinusOne(GetComponent<EffectComponent>().effectData.genericUInt));
        ColorCatalog.ColorIndex colorIndex = ColorCatalog.ColorIndex.Error;
        Sprite sprite = null;
        if (equipmentDef != null)
        {
            colorIndex = equipmentDef.colorIndex; 
            sprite = equipmentDef.pickupIconSprite;
        }
        Color color = ColorCatalog.GetColor(colorIndex);
        if (trailToColor != null)
        {
            trailToColor.startColor *= color;
            trailToColor.endColor *= color;
        }
        for (int i = 0; i < particlesToColor.Length; i++)
        {
            ParticleSystem obj = particlesToColor[i];
            ParticleSystem.MainModule main = obj.main;
            main.startColor = color;
            obj.Play();
        }
        for (int j = 0; j < spritesToColor.Length; j++)
        {
            spritesToColor[j].color = color;
        }
        iconSpriteRenderer.sprite = sprite;
    }
}
