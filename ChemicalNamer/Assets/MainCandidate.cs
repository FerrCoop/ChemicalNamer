using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCandidate
{
    public List<Carbon> chain;
    public List<Carbon.FunctionalGroup> functionalGroups;
    public Dictionary<Carbon.FunctionalGroup, List<Carbon>> functionalGroupDict;
    public List<Carbon.Unsaturation> unsaturation;
    
    public MainCandidate(Carbon _carbonA, Carbon _carbonB)
    {
        chain = _carbonA.PathTo(_carbonB, null);
        functionalGroups = new List<Carbon.FunctionalGroup>();
        functionalGroupDict = new Dictionary<Carbon.FunctionalGroup, List<Carbon>>();
        unsaturation = new List<Carbon.Unsaturation>();
        foreach (Carbon _carbon in chain)
        {
            foreach (Carbon.FunctionalGroup _group in _carbon.functionalGroups)
            {
                functionalGroups.Add(_group);
                if (functionalGroupDict.TryGetValue(_group, out List<Carbon> _list))
                {
                    _list.Add(_carbon);
                }
                else
                {
                    functionalGroupDict.Add(_group, new List<Carbon> { _carbon });
                }
            }
            Carbon.Unsaturation _unsaturation = _carbon.UnsaturationInChain(chain);
            if (_unsaturation != Carbon.Unsaturation.Saturated)
            {
                unsaturation.Add(_carbon.unsaturation);
            }
        }
        functionalGroups.Sort();
    }

    public int CompareTo(MainCandidate _other)
    {
        int i = 0;
        while (true)
        {
            if (i >= functionalGroups.Count && i >= _other.functionalGroups.Count)
            {
                int k = 0;
                while (true)
                {
                    if (k >= unsaturation.Count && k >= _other.unsaturation.Count)
                    {
                        return chain.Count - _other.chain.Count;
                    }
                    else if (k >= unsaturation.Count)
                    {
                        return -1;
                    }
                    else if (k >= _other.unsaturation.Count)
                    {
                        return 1;
                    }
                    else
                    {
                        if (unsaturation[k] == _other.unsaturation[k])
                        {
                            k++;
                            continue;
                        }
                        return (int)unsaturation[k] - (int)_other.unsaturation[k];
                    }
                }
            }
            else if (i>= functionalGroups.Count)
            {
                return -1;
            }
            else if (i >= _other.functionalGroups.Count)
            {
                return 1;
            }
            else
            {
                if (functionalGroups[i] == _other.functionalGroups[i])
                {
                    i++;
                    continue;
                }
                return -1 * ((int)functionalGroups[i] - (int)_other.functionalGroups[i]);
            }
        }
    }
}
