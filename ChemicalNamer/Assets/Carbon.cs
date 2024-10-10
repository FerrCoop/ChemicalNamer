using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carbon : Atom, IComparable<Carbon>
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
    }

    public enum Unsaturation
    {
        Saturated,
        Alkene,
        Alkyne,
    }    

    public void Evaluate(List<Carbon> _chain)
    {
        functionalGroups = new();
        unsaturation = new();
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

    public int CompareTo(Carbon _other)
    {
        if (_other == this)
        {
            return 0;
        }
        if (_other.functionalGroups == null && this.functionalGroups == null)
        {
            //both unevaluated
            return 0;
        }
        return 0;
    }
}
