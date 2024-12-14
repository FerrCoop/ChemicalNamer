using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;

public abstract class Atom : MonoBehaviour
{
    [SerializeField] private CovalentBond covalentBondPrefab;
    [SerializeField] private TextMeshProUGUI label;

    public List<CovalentBond> bondedAtoms = new();
    public static int numAtoms;

    public abstract int MAX_BONDS { get; }
    public abstract char ABBREVIATION { get; }

    private void Awake()
    {
        numAtoms++;
    }

    private void OnDestroy()
    {
        numAtoms--;
    }

    public void ShowHydrogens()
    {
        int _totalBonds = 0;
        foreach (CovalentBond _bond in bondedAtoms)
        {
            _totalBonds += _bond.BondTier;
        }
        if (_totalBonds == MAX_BONDS)
        {
            label.text = ABBREVIATION.ToString();
            return;
        }
        else if (MAX_BONDS - _totalBonds == 1)
        {
            label.text = ABBREVIATION + "H";
            return;
        }
        label.text = ABBREVIATION + "H" + (MAX_BONDS - _totalBonds).ToString();
    }

    public void AddConnection(Atom _atomA, Atom _atomB)
    {
        foreach (CovalentBond _bond in _atomA.bondedAtoms)
        {
            if (_bond.GetOtherAtom(_atomA) == _atomB)
            {
                _bond.IncrementBondLevel();
                return;
            }
        }
        CovalentBond _newBond = Instantiate(covalentBondPrefab);
        _newBond.SetEssentials(_atomA, _atomB);
        _atomA.bondedAtoms.Add(_newBond);
        _atomB.bondedAtoms.Add(_newBond);
        _atomA.ShowHydrogens();
        _atomB.ShowHydrogens();
        Debug.Log(_newBond.BondTier);
    }

    public void UpdateBonds()
    {
        foreach(CovalentBond _bond in bondedAtoms)
        {
            _bond.UpdateBondPositions();
        }
    }

    public bool TryRemoveConnection(Atom _atom)
    {
        CovalentBond _targetBond = null;
        foreach (CovalentBond _bond in bondedAtoms)
        {
            if (_bond.Contains(_atom))
            {
                _targetBond = _bond;
                break;
            }
        }
        if (_targetBond != null)
        {
            bondedAtoms.Remove(_targetBond);
            _targetBond.GetOtherAtom(this).bondedAtoms.Remove(_targetBond);
            _targetBond.AtomA.ShowHydrogens();
            _targetBond.AtomB.ShowHydrogens();
            Destroy(_targetBond);
            return true;
        }
        return false;
    }

    private void OnMouseOver()
    {
        //if right clicked
        if (Input.GetMouseButtonDown(1))
        {
            PlayerManager.Instance.SetBondOrigin(this);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            Debug.Log("Mouse Button Released Over Atom");
            TryFormBond(PlayerManager.Instance.BondOrigin);
        }
    }

    private void OnMouseDown()
    {
        PlayerManager.Instance.SetDraggingAtom(this);
    }

    private void TryFormBond(Atom _atom)
    {
        if (_atom == this) return;
        if (this.Bondable() && _atom.Bondable())
        {
            AddConnection(this, _atom);
            Debug.Log("Bond Formed");
        }
    }

    public bool Bondable()
    {
        int _totalBonds = 0;
        foreach (CovalentBond _bond in bondedAtoms)
        {
            _totalBonds += _bond.BondTier;
        }
        return _totalBonds < MAX_BONDS;
    }
}
