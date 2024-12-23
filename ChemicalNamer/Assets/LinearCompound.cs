using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LinearCompound
{
    private MainCandidate main;
    private Namer namer;
    private List<List<Carbon>> sideChains;

    public LinearCompound(MainCandidate _main, Namer _namer)
    {
        main = _main;
        namer = _namer;
        sideChains = new List<List<Carbon>>();
    }

    public void AddSidechain(List<Carbon> _sidechain)
    {
        if (_sidechain == null)
        {
            Debug.LogError("Null Sidechain Added");
            return;
        }
        Debug.Log("Sidechain length: " + _sidechain.Count);
        sideChains.Add(_sidechain);
    }

    public static string NameSidechain(List<Carbon> _sidechain, Carbon _endCarbon, Carbon _connectedCarbon, Namer _namer)
    {
        Debug.Log("Num: " + _endCarbon.bondedAtoms.Count);
        string _name = _namer.STANDARD_PREFIXES[_sidechain.Count - 2];
        Carbon.Unsaturation _unsaturation = Carbon.Unsaturation.Saturated;
        foreach (Carbon _carbon in _sidechain)
        {
            if (_carbon == _connectedCarbon)
            {
                continue;
            }
            if(_carbon != _endCarbon && _carbon.functionalGroups.Count > 0)
            {
                _namer.Output(new InvalidChemicalException("Branched sidechains are unsupported."));
            }
            CovalentBond _bond = _carbon.GetUnsaturatedBond();
            if (_bond != null && (int)_bond.BondTier > (int)_unsaturation && _bond.AtomA.GetType() == typeof(Carbon) && _bond.AtomB.GetType() == typeof(Carbon))
            {
                _unsaturation = (Carbon.Unsaturation)(_bond.BondTier - 1);
            }
        }
        string _unsaturationName = "";
        if (_unsaturation == Carbon.Unsaturation.Alkene)
        {
            _unsaturationName = "en";
        }
        else if (_unsaturation == Carbon.Unsaturation.Alkyne)
        {
            _unsaturationName = "yn";
        }
        _name += _unsaturationName;
        if (_endCarbon.functionalGroups.Count == 0)
        {
            _name += "yl";
        }
        else if (_endCarbon.functionalGroups.Count == 1)
        {
            _name += _namer.FUNCTIONAL_GROUP_ENDINGS[(int)_endCarbon.functionalGroups[0]];
        }
        else
        {
            _namer.Output(new InvalidChemicalException("End Carbon has more than one functional group."));
        }
        return _name;
    }

    public void Evaluate()
    {
        if (main.chain.Count > namer.STANDARD_PREFIXES.Length)
        {
            namer.Output(new InvalidChemicalException("Compound is too large!"));
            return;
        }
        NumberChain();
        //namer.Output("Safe to 84");
        //return;
        Carbon.FunctionalGroup _primaryGroup = GetPrimaryFunctionalGroup();
        namer.Output(NamePrefixes(_primaryGroup) + namer.STANDARD_PREFIXES[main.chain.Count - 1] + GetUnsaturation() + NamePrimaryFunctionalGroup(_primaryGroup));
    }

    private void NumberChain()
    {
        Carbon[] _carbonHeap = new Carbon[main.chain.Count];
        int _heapSize = 0;

        foreach (Carbon _carbon in main.chain)
        {
            if(_carbon.functionalGroups.Count > 0 || _carbon.unsaturation != Carbon.Unsaturation.Saturated)
            {
                _carbonHeap[_heapSize] = _carbon;
                int _index = _heapSize;
                _heapSize++;
                while (_index > 0)
                {
                    Carbon _parent = _carbonHeap[(_index - 1) / 2];
                    if (_carbonHeap[_index].LinearCompare(_parent, main.chain[0], main.chain[main.chain.Count - 1]) > 0)
                    {
                        _carbonHeap[(_index - 1) / 2] = _carbonHeap[_index];
                        _carbonHeap[_index] = _parent;
                        _index = (_index - 1) / 2; 
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        

        while (_heapSize > 0)
        {
            Carbon _prioCarbon = _carbonHeap[0];
            _heapSize--;
            _carbonHeap[0] = _carbonHeap[_heapSize];

            //if heapSize 2 or more, reheap down
            if (_heapSize > 1)
            {
                //reheap down
                while (true)
                {
                    int _index = 0;
                    int _left = 2 * _index + 1, _right = 2 * _index + 2;
                    int _prioChild = _left;
                    if (_left < _heapSize)
                    {
                        if (_right < _heapSize && _carbonHeap[_right].LinearCompare(_carbonHeap[_index], main.chain[0], main.chain[main.chain.Count - 1]) > 0)
                        {
                            if (_carbonHeap[_right].LinearCompare(_carbonHeap[_left], main.chain[0], main.chain[main.chain.Count - 1]) > 0)
                            {
                                _prioChild = _right;
                            }
                        }
                        if (_carbonHeap[_prioChild].LinearCompare(_carbonHeap[_index], main.chain[0], main.chain[main.chain.Count - 1]) > 0)
                        {
                            Carbon _temp = _carbonHeap[_index];
                            _carbonHeap[_index] = _carbonHeap[_prioChild];
                            _carbonHeap[_prioChild] = _temp;
                            _index = _prioChild;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            int _numFromBeginning = main.chain[0].PathTo(_prioCarbon, null).Count, _numFromEnd = main.chain[main.chain.Count - 1].PathTo(_prioCarbon, null).Count;
            if (_numFromBeginning < _numFromEnd)
            {
                //beginning is 1
                for (int i = 1; i <= main.chain.Count; i++)
                {
                    main.chain[i - 1].SetChainNumber(i);
                }
                return;
            }
            else if (_numFromBeginning > _numFromEnd)
            {
                //end is 1
                main.chain.Reverse();
                for (int i = 1; i <= main.chain.Count; i++)
                {
                    main.chain[i - 1].SetChainNumber(i);
                }
                return;
            }
        }

        for (int i = 1; i <= main.chain.Count; i++)
        {
            main.chain[i - 1].SetChainNumber(i);
        }
    }

    private string NamePrefixes(Carbon.FunctionalGroup _primaryGroup)
    {
        Dictionary<string, string> _alphabatizer = new Dictionary<string, string>();
        foreach (Carbon.FunctionalGroup _group in main.functionalGroupDict.Keys)
        {
            if ((_group == _primaryGroup && _primaryGroup != Carbon.FunctionalGroup.Alkanyl) || main.functionalGroupDict[_group].Count == 0)
            {
                continue;
            }
            if (_group != Carbon.FunctionalGroup.Alkanyl)
            {
                List<int> _indexes = new List<int>();
                foreach (Carbon _carbon in main.functionalGroupDict[_group])
                {
                    _indexes.Add(_carbon.ChainNumber);
                }
                _alphabatizer.Add(namer.FUNCTIONAL_PREFIXES[(int)_group], Namer.IntListToString(_indexes) + namer.NUMERICAL_PREFIXES[_indexes.Count - 1] + namer.FUNCTIONAL_PREFIXES[(int)_group]);
            }       
            else
            {
                Dictionary<string, List<int>> _alkanylDict = new Dictionary<string, List<int>>();
                foreach (List<Carbon> _sidechain in sideChains)
                {
                    string _name = NameSidechain(_sidechain, _sidechain[_sidechain.Count - 1], _sidechain[0], namer);
                    if (_alkanylDict.ContainsKey(_name))
                    {
                        _alkanylDict[_name].Add(_sidechain[0].ChainNumber);
                    }
                    else
                    {
                        _alkanylDict.Add(_name, new List<int> { _sidechain[0].ChainNumber });
                    }
                }
                foreach (string s in _alkanylDict.Keys)
                {
                    _alphabatizer.Add(s, Namer.IntListToString(_alkanylDict[s]) + namer.NUMERICAL_PREFIXES[_alkanylDict[s].Count - 1] + s);
                }
            }
        }
        string _prefixes = "";
        List<string> keys = _alphabatizer.Keys.ToList<string>();
        keys.Sort();
        foreach (string _key in keys)
        {
            _prefixes += _alphabatizer[_key] + "-";
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
