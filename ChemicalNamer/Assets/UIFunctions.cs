using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFunctions: MonoBehaviour
{
    public void SpawnAtom(Atom _atom)
    {
        Instantiate(_atom, new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0f), Quaternion.identity);
    }

    public void Clear()
    {
        Atom[] _atoms = (Atom[])FindObjectsOfType(typeof(Atom));
        foreach (Atom _atom in _atoms)
        {
            Destroy(_atom.gameObject);
        }
        CovalentBond[] _bonds = (CovalentBond[])FindObjectsOfType(typeof(CovalentBond));
        foreach (CovalentBond _bond in _bonds)
        {
            Destroy(_bond.gameObject);
        }
    }
}
