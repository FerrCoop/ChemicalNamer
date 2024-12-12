using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Ring
{
    public List<Carbon> ring;
    private List<Carbon> hydroxyls;
    private HashSet<CovalentBond> doubleBonds;
    private HashSet<CovalentBond> tripleBonds;
    private Namer namer;
    private List<List<Carbon>> sideChains;

    public Ring(List<Carbon> _carbons, Namer _namer)
    {
        ring = _carbons;
        doubleBonds = new HashSet<CovalentBond>();
        tripleBonds = new HashSet<CovalentBond>();
        hydroxyls = new List<Carbon>();
        sideChains = new List<List<Carbon>>();
        namer = _namer;
    }

    public void AddSideChain(List<Carbon> _sidechain)
    {
        if (_sidechain == null)
        {
            Debug.LogError("Null Sidechain Added");
            return;
        }
        Debug.Log("Sidechain length: " + _sidechain.Count);
        sideChains.Add(_sidechain);
    }

    public void Evaluate()
    {
        //check for error cases
        if (ring == null)
        {
            namer.Output(new InvalidChemicalException("Non-carbon atom in ring structure"));
            return;
        }
        else if (ring.Count > namer.STANDARD_PREFIXES.Length)
        {
            namer.Output(new InvalidChemicalException("This compound is too big!"));
            return;
        }

        //reset and evaluate carbons
        foreach (Carbon _carbon in ring)
        {
            _carbon.Evaluate(ring);

            //Get Unsaturation
            if (_carbon.unsaturation == Carbon.Unsaturation.Alkene)
            {
                doubleBonds.Add(_carbon.GetUnsaturatedBond());
            }
            else if (_carbon.unsaturation == Carbon.Unsaturation.Alkyne)
            {
                tripleBonds.Add(_carbon.GetUnsaturatedBond());
            }

            //Get Hydoxyls for later
            foreach (Carbon.FunctionalGroup _functionalGroup in _carbon.functionalGroups)
            {
                if (_functionalGroup == Carbon.FunctionalGroup.Hydroxyl)
                {
                    hydroxyls.Add(_carbon);  
                }
            }
        }
        NumberRing();
        namer.Output(NamePrefixes(ring) + "Cyclo" + namer.STANDARD_PREFIXES[ring.Count - 1] + GetCycloCompoundBody(ring));
    }

    private void NumberRing()
    {
        if (tripleBonds.Count + doubleBonds.Count == 0)
        {
            //number by functional groups
            Carbon[] _sortedCarbons = new Carbon[ring.Count];

            //reset each carbon's number, add to heap and sort up
            for (int i = 0; i < ring.Count; i++)
            {
                _sortedCarbons[i] = ring[i];
                Namer.FunctionalGroupSortUp(_sortedCarbons, i, true);
            }

            //Get Carbon 1
            Carbon _currentCarbon = _sortedCarbons[0];
            _currentCarbon.SetChainNumber(1);

            //Get Carbon 1's next prio neighbor
            List<Carbon> _ringNeighbors = _currentCarbon.GetConnectedCarbons(ring);
            if (_ringNeighbors.Count != 2)
            {
                Debug.LogError("Found one or less or three or more connected carbons in ring");
                return;
            }
            if (_ringNeighbors[0].CompareFunctionalGroups(_ringNeighbors[1]) > 0)
            {
                _currentCarbon = _ringNeighbors[0];
            }
            else
            {
                _currentCarbon = _ringNeighbors[1];
            }
            _currentCarbon.SetChainNumber(2);

            //finish the ring, numbering as we go
            FinishRing(_currentCarbon, 3);            
        }
        else if (doubleBonds.Count + tripleBonds.Count == 1)
        {
            CovalentBond _bond;

            //get bond
            if (doubleBonds.Count > 0)
            {
                _bond = doubleBonds.ToArray()[0];
            }
            else
            {
                _bond = tripleBonds.ToArray()[0];
            }

            //get prio 1 carbon
            Carbon _carbonA = (Carbon)_bond.AtomA;
            Carbon _carbonB = (Carbon)_bond.AtomB;
            int _compared = _carbonA.CompareFunctionalGroups(_carbonB);
            if (_compared > 0)
            {
                _carbonA.SetChainNumber(1);
                _carbonB.SetChainNumber(2);
                FinishRing(_carbonB, 3);

            }
            else if (_compared < 0)
            {
                _carbonB.SetChainNumber(1);
                _carbonA.SetChainNumber(2);
                FinishRing(_carbonA, 3);
            }
            else
            {
                int _complexCompared = _carbonA.CompareFunctionalGroups(_carbonB, _carbonA, _carbonB, ring);
                if (_complexCompared > 0)
                {
                    //next functional is closer to A, so A is number 2
                    _carbonA.SetChainNumber(2);
                    _carbonB.SetChainNumber(1);
                    FinishRing(_carbonA, 3);
                }
                else 
                {
                    //next functional is closer to B, so B is number 2 (or they're equal)
                    _carbonB.SetChainNumber(2);
                    _carbonA.SetChainNumber(1);
                    FinishRing(_carbonB, 3);
                }
                //move through remainging carbons looking for next functional group
            }
            //make sure bond is actually in ring
            //determine higher priority carbon
            //if both are equal in functional groups, walk away from bond until next functional group is found,
            //carbon with closer next functional group is 2, other is 1
        }
        else
        {
            //determine highest priority carbon
            //get all the bonds in the ring
            //
        }
    }

    //TODO
    private string NamePrefixes(List<Carbon> _ring)
    {
        Dictionary<Carbon.FunctionalGroup, List<Carbon>> _functionalGroups = new();

        foreach (Carbon _carbon in ring)
        {
            foreach (Carbon.FunctionalGroup _funcGroup in _carbon.functionalGroups)
            {
                if (_funcGroup != Carbon.FunctionalGroup.Hydroxyl)
                {
                    if(!_functionalGroups.ContainsKey(_funcGroup))
                    {
                        _functionalGroups[_funcGroup] = new();
                    }
                    _functionalGroups[_funcGroup].Add(_carbon);
                }
            }
        }

        Dictionary<string, string> _alphabatizer = new Dictionary<string, string>();
        foreach (Carbon.FunctionalGroup _group in _functionalGroups.Keys)
        {
            if (_group != Carbon.FunctionalGroup.Alkanyl)
            {
                List<int> _indexes = new List<int>();
                foreach (Carbon _carbon in ring)
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
                    string _name = LinearCompound.NameSidechain(_sidechain, _sidechain[_sidechain.Count - 1], _sidechain[0], namer);
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

    //return cyclo-ringCount-unsaturation-e/ol
    private string GetCycloCompoundBody(List<Carbon> _ring)
    {
        string _body = "";

        //name unsaturation
        if(doubleBonds.Count + tripleBonds.Count == 0)
        {
            _body += Namer.ALKANE_SUFFIX;
        }
        else
        {
            if (doubleBonds.Count > 0)
            {
                List<int> _doubleBondIndexes = new List<int>();
                foreach (CovalentBond _bond in doubleBonds)
                {
                    _doubleBondIndexes.Add(_bond.GetLowerAtomIndex());
                }
                _doubleBondIndexes.Sort();
                _body += Namer.IntListToString(_doubleBondIndexes) + namer.NUMERICAL_PREFIXES[doubleBonds.Count - 1] + Namer.ALKENE_SUFFIX;
            }
            if (tripleBonds.Count > 0)
            {
                List<int> _tripleBondIndexes = new();
                foreach (CovalentBond _bond in tripleBonds)
                {
                    _tripleBondIndexes.Add(_bond.GetLowerAtomIndex());
                }
                _tripleBondIndexes.Sort();
                _body += Namer.IntListToString(_tripleBondIndexes) + namer.NUMERICAL_PREFIXES[tripleBonds.Count - 1] + Namer.ALKYNE_SUFFIX;
            }
        }

        //name hydroxyls
        if (hydroxyls.Count >= 1)
        {
            List<int> _hydroxyls = new();
            foreach (Carbon _carbon in hydroxyls)
            {
                _hydroxyls.Add(_carbon.ChainNumber);
            }
            _hydroxyls.Sort();
            _body += "-" + namer.NUMERICAL_PREFIXES[hydroxyls.Count - 1] + Namer.IntListToString(_hydroxyls) + "-ol";
        }
        else
        {
            _body += "e";
        }
        return _body;
    }

    private void FinishRing(Carbon _currentCarbon, int _index)
    {
        while (_index <= ring.Count)
        {
            bool _counted = false;
            foreach (Carbon _carbon in _currentCarbon.GetConnectedCarbons(ring))
            {
                if (_carbon.ChainNumber == 0 && ring.Contains(_carbon))
                {
                    _carbon.SetChainNumber(_index);
                    _currentCarbon = _carbon;
                    _counted = true;
                    _index++;
                    break;
                }
            }
            if (!_counted)
            {
                Debug.LogError("No Connection Found");
                return;
            }
        }
        foreach (Carbon _carbon in ring)
        {
            Debug.Log(_carbon.ChainNumber);
        }
    }
}
