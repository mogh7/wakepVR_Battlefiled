using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{

    [SerializeField] private Transform rightHandEnemyTrans;

    [SerializeField] private Quaternion offsetWeaponAngle;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = rightHandEnemyTrans.rotation * offsetWeaponAngle;
    }
}
