using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carbon : Atom
{
    public int ChainNumber {get; private set;}

    public override int MAX_BONDS { get { return 4; } }   

    public List<FunctionalGroup> functionalGroups;
    public Unsaturation unsaturation;

    public enum FunctionalGroup
    {
        CarboxyllicAcid,
        Aldehyde,
        Ketone,
        Hydroxyl,
        Amino,
        Alkanyl
    }

    public enum Unsaturation
    {
        Saturated,
        Alkene,
        Alkyne,
    }
    
    public CovalentBond GetUnsaturatedBond()
    {
        foreach (CovalentBond _bond in bondedAtoms)
        {
            if (_bond.BondTier > 1)
            {
                return _bond;
            }
        }
        return null;
    }

    public void Evaluate(List<Carbon> _chain)
    {
        functionalGroups = new List<FunctionalGroup>();
        List<CovalentBond> _oxygens = new();
        foreach (CovalentBond _bond in bondedAtoms)
        {
            Atom _other = _bond.GetOtherAtom(this);
            if (_other.GetType() == typeof(Carbon))
            {
                if (_chain.Contains((Carbon)_other))
                {
                    if (_bond.BondTier - 1 > (int)unsaturation - 1)
                    {
                        unsaturation = (Unsaturation)(_bond.BondTier - 1);
                    }
                    continue;
                }                
                /*TODO: Get Carbon Functional Groups*/
            }
            else if (_other.GetType() == typeof(Oxygen))
            {
                _oxygens.Add(_bond);
            }
            else if (_other.GetType() == typeof(Nitrogen))
            {
                if (_bond.BondTier == 1 && _other.bondedAtoms.Count == 1)
                {
                    functionalGroups.Add(FunctionalGroup.Amino);
                }
            }
        }
        GetOxygenFunctionalGroups(_oxygens);
        functionalGroups.Sort();
        functionalGroups.Reverse();
    }

    private void GetOxygenFunctionalGroups(List<CovalentBond> _oxygens)
    {
        if (_oxygens.Count == 1)
        {
            //ketone, or aldehyde
            if (_oxygens[0].BondTier == 2 && bondedAtoms.Count == 2)
            {
                functionalGroups.Add(FunctionalGroup.Aldehyde);
                return;
            }
            else if (_oxygens[0].BondTier == 2 && bondedAtoms.Count == 3)
            {
                functionalGroups.Add(FunctionalGroup.Ketone);
                return;
            }            
        }
        else if (_oxygens.Count == 2)
        {
            //carboxylic acid
            if (_oxygens[0].BondTier != _oxygens[1].BondTier)
            {
                functionalGroups.Add(FunctionalGroup.CarboxyllicAcid);
                return;
            }
        }
        foreach (CovalentBond _bond in _oxygens)
        {
            if (IsHydroxyl(_bond))
            {
                functionalGroups.Add(FunctionalGroup.Hydroxyl);
            }
        }
    }

    private bool IsHydroxyl(CovalentBond _bond)
    {
        return (_bond.BondTier == 1 && _bond.GetOtherAtom(this).bondedAtoms.Count == 1);
    }

    public void SetChainNumber(int _num)
    {
        ChainNumber = _num;
    }

    public List<Carbon> GetConnectedCarbons()
    {
        List<Carbon> _connectedCarbons = new();
        foreach (CovalentBond _bond in bondedAtoms)
        {
            Atom _other = _bond.GetOtherAtom(this);
            if (_other.GetType() == typeof(Carbon))
            {
                _connectedCarbons.Add((Carbon)_other);
            }
        }
        return _connectedCarbons;
    }

    public List<Carbon> GetConnectedCarbons(List<Carbon> _chain)
    {
        List<Carbon> _connectedCarbons = new();
        foreach (CovalentBond _bond in bondedAtoms)
        {
            if (_bond.GetOtherAtom(this).GetType() != typeof(Carbon))
            {
                continue;
            }
            Carbon _other = (Carbon)_bond.GetOtherAtom(this);
            if (_other != null && _chain.Contains(_other))
            {
                _connectedCarbons.Add(_other);
            }
        }
        return _connectedCarbons;
    }

    public int CompareFunctionalGroups(Carbon _other)
    {
        int _compNum = 0;
        while (true)
        {
            if (functionalGroups.Count ==_compNum && _other.functionalGroups.Count == _compNum)
            {
                return 0;
            }
            else if (functionalGroups.Count == _compNum)
            {
                return -1;
            }
            else if (_other.functionalGroups.Count == _compNum)
            {
                return 1;
            }

            if ((int)functionalGroups[_compNum] != (int)_other.functionalGroups[_compNum])
            {
                return -1 * ((int)functionalGroups[0] - (int)_other.functionalGroups[0]);
            }

            _compNum++;
        }        
    }
}
