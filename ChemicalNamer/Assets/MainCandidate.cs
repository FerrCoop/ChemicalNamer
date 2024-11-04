using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCandidate
{
    public List<Carbon.FunctionalGroup> functionalGroups;
    public List<Carbon.Unsaturation> unsaturation;

    List<Carbon> chain;

    public MainCandidate(Carbon _carbonA, Carbon _carbonB)
    {
        chain = _carbonA.PathTo(_carbonB, null);
        functionalGroups = new List<Carbon.FunctionalGroup>();
        unsaturation = new List<Carbon.Unsaturation>();
        foreach (Carbon _carbon in chain)
        {           
            functionalGroups.AddRange(_carbon.functionalGroups);
            Carbon.Unsaturation _unsaturation = _carbon.UnsaturationInChain(chain);
            if (_unsaturation != Carbon.Unsaturation.Saturated)
            {
                unsaturation.Add(_carbon.unsaturation);
            }
        }
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
                        return 0;
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

    public List<Carbon> GetChain()
    {
        return chain;
    }
}
