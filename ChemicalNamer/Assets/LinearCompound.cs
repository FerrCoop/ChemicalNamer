using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearCompound
{
    private MainCandidate main;
    private Namer namer;

    public LinearCompound(MainCandidate _main, Namer _namer)
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
        if (main.chain.Count > namer.STANDARD_PREFIXES.Length)
        {
            namer.Output(new InvalidChemicalException("Compound is too large!"));
            return;
        }
        NumberChain();
        Carbon.FunctionalGroup _primaryGroup = GetPrimaryFunctionalGroup();
        namer.Output(NamePrefixes(_primaryGroup) + namer.STANDARD_PREFIXES[main.chain.Count - 1] + GetUnsaturation() + NamePrimaryFunctionalGroup(_primaryGroup));
    }

    private void NumberChain()
    {
        Carbon _prioCarbon = null;

        //heap
        foreach (Carbon _carbon in main.chain)
        {
            
        }

        //find num from both sides
        //choose lower
        //if equal grab next carbon

    }

    private string NamePrefixes(Carbon.FunctionalGroup _primaryGroup)
    {
        string _prefixes = "";
        foreach (Carbon.FunctionalGroup _group in main.functionalGroupDict.Keys)
        {
            if (_group == _primaryGroup || main.functionalGroupDict[_group].Count == 0)
            {
                continue;
            }
            List<int> _indexes = new List<int>();
            foreach (Carbon _carbon in main.functionalGroupDict[_group])
            {
                _indexes.Add(_carbon.ChainNumber);
            }
            //TODO: Alphabetize
            _prefixes += Namer.IntListToString(_indexes) + namer.NUMERICAL_PREFIXES[_indexes.Count - 1];
        }
        return _prefixes;
    }

    private string GetUnsaturation()
    {
        string _unsaturation = "";
        HashSet<CovalentBond> _doubleBonds = new HashSet<CovalentBond>();
        HashSet<CovalentBond> _tripleBonds = new HashSet<CovalentBond>();

        foreach (Carbon _carbon in main.chain)
        {
            CovalentBond _bond = _carbon.GetUnsaturatedBond();
            if (_bond == null || _bond.BondTier == 1)
            {
                continue;
            }
            Atom _other = _bond.GetOtherAtom(_carbon);
            if (_other.GetType() != typeof(Carbon) || !main.chain.Contains((Carbon)_other))
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

    private Carbon.FunctionalGroup GetPrimaryFunctionalGroup()
    {
        int _primary = (int)Carbon.FunctionalGroup.Alkanyl;
        if (main.functionalGroupDict.Count > 0)
        {
            //find primary functional group            
            foreach (Carbon.FunctionalGroup _group in main.functionalGroupDict.Keys)
            {
                if ((int)_group < _primary)
                {
                    _primary = (int)_group;
                }
            }
        }
        return (Carbon.FunctionalGroup) _primary;
    }

    private string NamePrimaryFunctionalGroup(Carbon.FunctionalGroup _primaryGroup)
    {
        if (_primaryGroup == Carbon.FunctionalGroup.Alkanyl)
        {
            return "e";
        }

        List<int> _funcGroupIndexes = new List<int>();
        foreach (Carbon _carbon in main.functionalGroupDict[_primaryGroup])
        {
            _funcGroupIndexes.Add(_carbon.ChainNumber);
        }
        return Namer.IntListToString(_funcGroupIndexes) + namer.FUNCTIONAL_GROUP_ENDINGS[(int)_primaryGroup];
    }
}
