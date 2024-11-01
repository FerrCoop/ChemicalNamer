using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Namer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameTextObject;

    public string[] STANDARD_PREFIXES = {"Meth", "Eth", "Prop", "But", "Pent", "Hex", "Hept", "Oct", "Non", "Deca", "Hendeca", "Dodeca"};
    public string[] NUMERICAL_PREFIXES = { "", "di", "tri", "quadra", "pent", "hex", "hept", "oct", "non", "deca", "hendeca", "dodeca" };
    public const string ALKANE_SUFFIX = "an", ALKENE_SUFFIX = "en", ALKYNE_SUFFIX = "yn";
    const string AMINO_SUFFIX = "amine", AMINO_PREFIX = "amino";

    //initial function, directs to HandleCyclo or HandleLinear
    public void TryNameCompound()
    {
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
            HandleLinearCompound();
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
    private void HandleLinearCompound()
    {
        //noncyclical: next step is find any functional groups
        //try find carboxyl
        //try find aldehyde
        //try find ketone
        //try find hydroxyl
        //try find amino

        //no functional groups
        //get unsaturation

        //get functional groups
        //get unsaturation
        //get longest carbon chain
        //get end carbons, find path from each end carbon to each other end carbon, choose longest
        Output(new InvalidChemicalException("Sorry, that chemical is too complicated"));
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
        nameTextObject.color = Color.white;
        nameTextObject.text = _name;
    }

    //Output an error
    public void Output(InvalidChemicalException _invalidException)
    {
        nameTextObject.color = Color.red;
        nameTextObject.text = _invalidException.reason;
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
}

public class InvalidChemicalException : System.Exception
{
    public string reason;
    public InvalidChemicalException(string _reason)
    {
        reason = _reason;
    }
}