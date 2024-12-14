using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CovalentBond: MonoBehaviour
{
    public Atom AtomA { get; private set;}
    public Atom AtomB {get; private set;}
    public int BondTier { get; private set; }

    private LineRenderer bondRenderer;
    private BoxCollider2D bondCollider;

    public void SetEssentials(Atom atomA, Atom atomB)
    {
        AtomA = atomA;
        AtomB = atomB;
        BondTier = 1;
        bondCollider = GetComponent<BoxCollider2D>();
        bondRenderer = GetComponent<LineRenderer>();
        bondRenderer.widthMultiplier = 0.2f * BondTier;
        UpdateBondPositions();
    }

    public void UpdateBondPositions()
    {
        bondRenderer.SetPosition(0, AtomA.transform.position);
        bondRenderer.SetPosition(1, AtomB.transform.position);
        this.transform.position = new Vector3((AtomA.transform.position.x + AtomB.transform.position.x)/2,
            (AtomA.transform.position.y + AtomB.transform.position.y) / 2, 0f);
        this.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(AtomA.transform.position.y - AtomB.transform.position.y,
            AtomA.transform.position.x - AtomB.transform.position.x) * Mathf.Rad2Deg);
        bondCollider.size = new Vector2(Mathf.Clamp(Vector3.Distance(AtomA.transform.position, AtomB.transform.position) - 1,
            0.1f, Mathf.Infinity), 0.2f * BondTier);
    }

    private void OnMouseDown()
    {
        if (AtomA.Bondable() && AtomB.Bondable())
        {
            IncrementBondLevel();
            bondRenderer.widthMultiplier = 0.2f * BondTier;
            bondCollider.size = new Vector2(bondCollider.size.x, 0.2f * BondTier);
        }        
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            DecrementBondLevel();
            bondRenderer.widthMultiplier = 0.2f * BondTier;
            bondCollider.size = new Vector2(bondCollider.size.x, 0.2f * BondTier);
        }
    }

    private void DecrementBondLevel()
    {
        BondTier--;
        if (BondTier == 0)
        {
            AtomA.TryRemoveConnection(AtomB);
        }
        AtomA.ShowHydrogens();
        AtomB.ShowHydrogens();
    }

    public void IncrementBondLevel()
    {
        BondTier++;
        if (BondTier > 3)
        {
            BondTier = 1;
        }
        AtomA.ShowHydrogens();
        AtomB.ShowHydrogens();
    }

    public Atom GetOtherAtom(Atom _atom)
    {
        if (_atom == AtomA)
        {
            return AtomB;
        }
        else if (_atom == AtomB)
        {
            return AtomA;
        }
        Debug.LogError("Queried atom not in bond");
        return null;
    }

    public int GetLowerAtomIndex()
    {
        Carbon carbonA = (Carbon)AtomA;
        Carbon carbonB = (Carbon)AtomB;
        if (carbonA == null || carbonB == null)
        {
            Debug.LogError("Non Double Carbon Bond");
            return -1;
        }
        else if (Mathf.Abs(carbonA.ChainNumber - carbonB.ChainNumber) > 1)
        {
            Debug.LogError("");
            return -1;
        }
        if (carbonA.ChainNumber < carbonB.ChainNumber)
        {
            return carbonA.ChainNumber;
        }
        return carbonB.ChainNumber;
    }

    public bool Contains (Atom _atom)
    {
        return AtomA == _atom || AtomB == _atom;
    }
}
