using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFunctions: MonoBehaviour
{
    public void SpawnAtom(Atom _atom)
    {
        Instantiate(_atom, new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0f), Quaternion.identity);
    }
}
