using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearCompound
{
    private List<Carbon> main;
    private Namer namer;

    public LinearCompound(List<Carbon> _main, Namer _namer)
    {
        main = _main;
        namer = _namer;
    }

    public void AddSubchain(List<Carbon> _subchain)
    {
        if (_subchain == null)
        {
            Debug.LogError("Null Sidechain Added");
            return;
        }

    }

    public void Evaluate()
    {
        if (main.Count > namer.STANDARD_PREFIXES.Length)
        {
            namer.Output(new InvalidChemicalException("Compound is too large!"));
            return;
        }
        NumberChain();
        namer.Output(NamePrefixes() + namer.STANDARD_PREFIXES[main.Count - 1] + GetUnsaturation() + GetPrimaryFunctionalGroup());
    }

    private void NumberChain()
    {

    }

    private string NamePrefixes()
    {
        return "";
    }

    private string GetUnsaturation()
    {
        string _unsaturation = "";
        HashSet<CovalentBond> _doubleBonds = new HashSet<CovalentBond>();
        HashSet<CovalentBond> _tripleBonds = new HashSet<CovalentBond>();

        foreach (Carbon _carbon in main)
        {
            CovalentBond _bond = _carbon.GetUnsaturatedBond();
            if (_bond == null || _bond.BondTier == 1)
            {
                continue;
            }
            Atom _other = _bond.GetOtherAtom(_carbon);
            if (_other.GetType() != typeof(Carbon) || !main.Contains((Carbon)_other))
            {
                continue;
            }
            if (_bond.BondTier == 2)
            {
                _doubleBonds.Add(_bond);
            }
            else
            {
                _tripleBonds.Add(_bond);
            }
        }
        List<int> _doubleBondIndexes = new List<int>();
        List<int> _tripleBondIndexes = new List<int>();
        foreach (CovalentBond _bond in _doubleBonds)
        {
            _doubleBondIndexes.Add(_bond.GetLowerAtomIndex());
        }
        if (_doubleBondIndexes.Count > 0)
        {
            _unsaturation += Namer.IntListToString(_doubleBondIndexes) + "-" + namer.NUMERICAL_PREFIXES[_doubleBondIndexes.Count - 1] + "en";
        }
        foreach (CovalentBond _bond in _tripleBonds)
        {
            _tripleBondIndexes.Add(_bond.GetLowerAtomIndex());
        }
        if(_tripleBondIndexes.Count > 0)
        {
            _unsaturation += Namer.IntListToString(_tripleBondIndexes) + "-" + namer.NUMERICAL_PREFIXES[_tripleBondIndexes.Count - 1] + "yn";
        }
        if (_doubleBondIndexes.Count + _tripleBondIndexes.Count == 0)
        {
            _unsaturation = "an";
        }       
        return _unsaturation;
    }

    private string GetPrimaryFunctionalGroup()
    {
        return "";
    }
}
