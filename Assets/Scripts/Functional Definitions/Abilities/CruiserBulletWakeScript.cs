using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CruiserBulletWakeScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SurvivalTimer());
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, 10, Space.Self);
    }

    IEnumerator SurvivalTimer()
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(this);
    }
}
