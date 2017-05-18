using Assets.General;
using Assets.General.UnityExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageFeedback : MonoBehaviour, IUnitDamagedHandler
{
    public TextMeshPro DamagePrefab;

    public void UnitDamaged( int damageDealt )
    {
        TextMeshPro damageObj = Instantiate<TextMeshPro>( DamagePrefab, this.transform, false );
        damageObj.text = damageDealt.ToString();
        RectTransform objTrans = damageObj.GetComponent<RectTransform>();
        StartCoroutine(
            CoroutineHelper.AddAfter(
                CustomAnimation.InterpolateValue(
                    0,
                    0.5f,
                    0.5f,
                    val => objTrans.localPosition = 
                    new Vector3( objTrans.localPosition.x, val, objTrans.localPosition.x ) ),
            () => Destroy( objTrans.gameObject ) ) );
    }
}
