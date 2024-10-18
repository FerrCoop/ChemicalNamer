using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Namer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameTextObject;

    private string[] STANDARD_PREFIXES = {"Meth", "Eth", "Prop", "But", "Pent", "Hex", "Hept", "Oct", "Non", "Deca", "Hendeca", "Dodeca"};
    const string ALKANE_SUFFIX = "an", ALKENE_SUFFIX = "en", ALKYNE_SUFFIX = "yn";
    const string AMINO_SUFFIX = "amine", AMINO_PREFIX = "amino";

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
        if (AddConnectedAtoms(_origin, null, _compoundAtoms, out CovalentBond _cyclical) == null)
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
            HandleCycloCompound(_compoundAtoms, _cyclical);
        }
        else
        {
            HandleLinearCompound();
        } 
    }

    private void Output(string _name)
    {
        nameTextObject.color = Color.white;
        nameTextObject.text = _name;
    }

    private void Output(InvalidChemicalException _invalidException)
    {
        nameTextObject.color = Color.red;
        nameTextObject.text = _invalidException.reason;
    }
    
    private void HandleCycloCompound(HashSet<Atom> _atoms, CovalentBond _cyclicalBond)
    {
        List<Carbon> _ring = GetRing(_atoms, _cyclicalBond);
        if(_ring == null)
        {
            Output(new InvalidChemicalException("Non-carbon atom in ring structure"));
            return;
        }
        else if (_ring.Count > STANDARD_PREFIXES.Length)
        {
            Output(new InvalidChemicalException("This compound is too big!"));
            return;
        }
        
        foreach (Carbon _carbon in _ring)
        {
            _carbon.Evaluate(_ring);
        }

        NumberRing(_ring);
        Output("Cyclo" + STANDARD_PREFIXES[_ring.Count - 1] + GetCycloCompoundBody(_ring));
    }

    private void NumberRing(List<Carbon> _ring)
    {
        Carbon[] _sortedCarbons = new Carbon[_ring.Count];

        //add carbons to heap and sort up
        for (int i = 0; i < _ring.Count; i++)
        {
            _sortedCarbons[i] = _ring[i];
            SortUp(_sortedCarbons, i);
        }

        _sortedCarbons[0].SetChainNumber(1);
        //remove 0 from heap
        //keep removing until find a neighbor of carbon 1
        //that's 2
        //figure out how to number rest of circle
    }

    private void SortUp(Carbon[] _sortedCarbons, int _index)
    {
        if (_index == 0)
        {
            return;
        }
        if (_sortedCarbons[_index].CompareTo(_sortedCarbons[(_index - 1) / 2]) > 0)
        {
            Carbon _temp = _sortedCarbons[_index];
            _sortedCarbons[_index] = _sortedCarbons[(_index - 1) / 2];
            _sortedCarbons[(_index - 1) / 2] = _temp;
            SortUp(_sortedCarbons, (_index - 1) / 2);
        }
    }

    private string GetCycloCompoundBody(List<Carbon> _ring)
    {
        string _body = "";
        List<int> _doubleBonds = new(), _tripleBonds = new();
        List<int> _hydroxyls = new();
        foreach (Carbon _carbon in _ring)
        {
            if (_carbon.unsaturation == Carbon.Unsaturation.Alkene)
            {
                _doubleBonds.Add(_carbon.ChainNumber);
            }
            else if (_carbon.unsaturation == Carbon.Unsaturation.Alkyne)
            {
                _tripleBonds.Add(_carbon.ChainNumber);
            }

            if (_carbon.functionalGroups.Contains(Carbon.FunctionalGroup.Hydroxyl))
            {
                _hydroxyls.Add(_carbon.ChainNumber);
            }            
        }

        if (_doubleBonds.Count + _tripleBonds.Count != 0)
        {
            if (_doubleBonds.Count >= 1)
            {
                _body += "-";
                _doubleBonds.Sort();
                _body += IntListToString(_doubleBonds) + ALKENE_SUFFIX;
            }
            if (_tripleBonds.Count >= 1)
            {
                _body += "-";
                _tripleBonds.Sort();
                _body += IntListToString(_tripleBonds) + ALKYNE_SUFFIX;
            }
        }
        else
        {
            _body += ALKANE_SUFFIX;
        }  

        if (_hydroxyls.Count >= 1)
        {
            _body += "-";
            _hydroxyls.Sort();
            return _body + IntListToString(_hydroxyls) + "ol";
        }
        return _body + "e";
    }

    private string IntListToString(List<int> _list)
    {
        string _string = _list[0].ToString();
        for (int i = 1; i < _list.Count; i++)
        {
            _string += ", " + _list[i];
        }
        _string += "-";
        return _string;
    }

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

    private HashSet<Atom> AddConnectedAtoms(Atom _atom, Atom _origin, HashSet<Atom> _connectedAtoms, out CovalentBond _cyclical)
    {
        _cyclical = null;
        Debug.Log(_cyclical);
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
            if (AddConnectedAtoms(_other, _atom, _connectedAtoms, out _cyclical) == null)
            {
                return null;
            }
        }
        return _connectedAtoms;
    }

    private class InvalidChemicalException : System.Exception 
    {
        public string reason;
        public InvalidChemicalException (string _reason)
        {
            reason = _reason;
        }
    }
}
