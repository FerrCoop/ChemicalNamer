using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Namer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameTextObject;

    public string[] STANDARD_PREFIXES = { "Meth", "Eth", "Prop", "But", "Pent", "Hex", "Hept", "Oct", "Non", "Deca", "Hendeca", "Dodeca" };
    public string[] NUMERICAL_PREFIXES = { "", "di", "tri", "quadra", "pent", "hex", "hept", "oct", "non", "deca", "hendeca", "dodeca" };
    public string[] FUNCTIONAL_PREFIXES = { "Carboxyl", "Formyl", "Oxo", "Hydroxyl", "Amino"};
    public string[] FUNCTIONAL_GROUP_ENDINGS = { "oic Acid", "al", "one", "ol", "amine"};
    public const string ALKANE_SUFFIX = "an", ALKENE_SUFFIX = "en", ALKYNE_SUFFIX = "yn";
    const string AMINO_SUFFIX = "amine", AMINO_PREFIX = "amino";

    bool errorThrown = false;

    //initial function, directs to HandleCyclo or HandleLinear
    public void TryNameCompound()
    {
        errorThrown = false;
        Atom _origin = FindAnyObjectByType<Atom>();

        if (_origin == null)
        {
            Output(new InvalidChemicalException("No Chemical Found"));
            return;
        }

        HashSet<Atom> _compoundAtoms = new();
        _compoundAtoms.Add(_origin);
        if (AddConnectedAtoms(_origin, null, _compoundAtoms, null, out CovalentBond _cyclical) == null)
        {
            Output(new InvalidChemicalException("Chemical Has More Than One Ring"));
            return;
        }

        if (Atom.numAtoms != _compoundAtoms.Count)
        {
            Output(new InvalidChemicalException("Please Connect All Atoms"));
            return;
        }

        //Get Compound Type
        if (_cyclical != null)
        {
            Ring _ring = new Ring(GetRing(_compoundAtoms, _cyclical), this);
            _ring.Evaluate();
        }
        else
        {
            HandleLinearCompound(_compoundAtoms);
        } 
    }
    
    //Start recursion to find ring
    private List<Carbon> GetRing(HashSet<Atom> _atoms, CovalentBond _cyclicalBond)
    {
        if (_cyclicalBond.AtomA.GetType() != typeof(Carbon) || _cyclicalBond.AtomB.GetType() != typeof(Carbon))
        {
            //already known non-carbon
            return null;
        }
        //find way from AtomA to AtomB
        List<Carbon> _ring = SearchConnectedCarbons((Carbon)_cyclicalBond.AtomA, (Carbon)_cyclicalBond.AtomB, (Carbon)_cyclicalBond.AtomB);
        return _ring;
    }

    //Recursively search through chemical to find ring
    private List<Carbon> SearchConnectedCarbons(Carbon _queriedCarbon, Carbon _origin, Carbon _target)
    {
        foreach (Carbon _carbon in _queriedCarbon.GetConnectedCarbons()) {
            if (_carbon == _origin)
            {
                continue;
            }
            if (_carbon == _target)
            {
                List<Carbon> _ring = new List<Carbon>();
                _ring.Add(_carbon);
                _ring.Add(_queriedCarbon);
                return _ring;
            }
            List<Carbon> _potentialRing = SearchConnectedCarbons(_carbon, _queriedCarbon, _target);
            if(_potentialRing != null)
            {
                _potentialRing.Add(_queriedCarbon);
                return _potentialRing;
            }
        }
        return null;
    }

    //Get Unsaturation of a chain
    private HashSet<CovalentBond> GetUnsaturation(List<Carbon> _chain)
    {
        HashSet<CovalentBond> _chainUnsaturation = new();
        foreach (Carbon _carbon in _chain)
        {
            foreach (CovalentBond _bond in _carbon.bondedAtoms)
            {
                if (_bond.BondTier > 1)
                {
                    Atom _other = _bond.GetOtherAtom(_carbon);
                    if (_other.GetType() == typeof(Carbon) && _chain.Contains((Carbon) _other))
                    {
                        _chainUnsaturation.Add(_bond);
                    }
                }
            }
        }
        return _chainUnsaturation;
    }

    //TODO: Implement Later
    private void HandleLinearCompound(HashSet<Atom> _atoms)
    {
        //noncyclical: next step is find any functional groups
        //get highest prio chain
        //make sure it and substituent chains less than 12 carbons
        //number chains
        //name main chain

        List<Carbon> _endCarbons = new List<Carbon>();
        
        foreach(Atom _atom in _atoms)
        {
            if (_atom.GetType() != typeof(Carbon))
            {
                continue;
            }
            Carbon _carbon = (Carbon)_atom;
            _carbon.Evaluate();

            if(_carbon.GetConnectedCarbons().Count <= 1)
            {
                _endCarbons.Add(_carbon);
            }
        }

        if (_endCarbons.Count == 0)
        {
            Output(new InvalidChemicalException("No carbons in compound!"));
            return;
        }    

        if(_endCarbons.Count == 1)
        {
            if (_endCarbons[0].functionalGroups.Count == 0)
            {
                Output("Methane");
            }
            else if (_endCarbons[0].functionalGroups.Count == 1)
            {
                Output("Methan" + FUNCTIONAL_GROUP_ENDINGS[(int)_endCarbons[0].functionalGroups[0]]);
            }
            else
            {
                //TODO: Multi functional methane
                Output("Damn complicated methane that is");
            }
            return;
        }

        MainCandidate[] _candidates = new MainCandidate[GetCombinations(_endCarbons.Count)];
        int _combinations = 0;
        for (int i = 0; i < _endCarbons.Count; i++)
        {
            for (int k = i + 1; k < _endCarbons.Count; k++)
            {
                //add new combination _endCarbons[i], _endCarbons[k]
                _candidates[_combinations] = new MainCandidate(_endCarbons[i], _endCarbons[k]);
                _combinations++;
                //TODO: sort up
                int _index = _combinations - 1;
                while (_index > 0)
                {
                    MainCandidate _parent = _candidates[(_index - 1) / 2];
                    if (_candidates[_index].CompareTo(_parent) > 0)
                    {
                        _candidates[(_index - 1) / 2] = _candidates[_index];
                        _candidates[_index] = _parent;
                        _index = (_index - 1) / 2;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        List<Carbon> _mainChain = _candidates[0].chain;
        LinearCompound _compound = new LinearCompound(_candidates[0], this);
        _endCarbons.Remove(_mainChain[0]);
        _endCarbons.Remove(_mainChain[_mainChain.Count - 1]);

        foreach (Carbon _carbon in _endCarbons)
        {
            //path from each end to a carbon in main
            _compound.AddSidechain(_carbon.PathTo(_mainChain, null));
        }

        _compound.Evaluate();
    }

    //Recursively Add Atoms
    private HashSet<Atom> AddConnectedAtoms(Atom _atom, Atom _origin, HashSet<Atom> _connectedAtoms, CovalentBond _currentCyclical, out CovalentBond _cyclical)
    {
        _cyclical = _currentCyclical;
        foreach (CovalentBond _bond in _atom.bondedAtoms)
        {
            Atom _other = _bond.GetOtherAtom(_atom);
            if (_other == _origin)
            {
                continue;
            }
            if(!_connectedAtoms.Add(_other))
            {
                if (_cyclical != null)
                {
                    if (_cyclical == _bond)
                    {
                        continue;
                    }
                    //Multi ring detected
                    return null;
                }
                _cyclical = _bond;
                continue;
            }
            if (AddConnectedAtoms(_other, _atom, _connectedAtoms, _cyclical, out _cyclical) == null)
            {
                return null;
            }
        }
        return _connectedAtoms;
    }

    //Output a chemical name
    public void Output(string _name)
    {
        if (errorThrown)
        {
            return;
        }
        nameTextObject.color = Color.white;
        nameTextObject.text = _name;
    }

    //Output an error
    public void Output(InvalidChemicalException _invalidException)
    {
        nameTextObject.color = Color.red;
        nameTextObject.text = _invalidException.reason;
        errorThrown = true;
    }

    //take a bunch of indexes and convert to a string
    public static string IntListToString(List<int> _list)
    {
        string _string = _list[0].ToString();
        for (int i = 1; i < _list.Count; i++)
        {
            _string += ", " + _list[i];
        }
        _string += "-";
        return _string;
    }

    //heap function to sort carbons
    public static void FunctionalGroupSortUp(Carbon[] _sortedCarbons, int _index, bool _cyclo)
    {
        if (_index == 0)
        {
            return;
        }
        if (_sortedCarbons[_index].CompareFunctionalGroups(_sortedCarbons[(_index - 1) / 2]) > 0)
        {
            Carbon _temp = _sortedCarbons[_index];
            _sortedCarbons[_index] = _sortedCarbons[(_index - 1) / 2];
            _sortedCarbons[(_index - 1) / 2] = _temp;
            if (_index != 0)
            {
                FunctionalGroupSortUp(_sortedCarbons, (_index - 1) / 2, _cyclo);
            }
        }
    }

    public static int GetCombinations(int _num)
    {
        int _combinations = 0;
        _num--;
        while (_num > 0)
        {    
            _combinations += _num;
            _num--;
        }
        return _combinations;
    }
}

public class InvalidChemicalException : System.Exception
{
    public string reason;
    public InvalidChemicalException(string _reason)
    {
        reason = _reason;
    }
}