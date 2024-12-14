using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oxygen : Atom
{
    public override int MAX_BONDS { get { return 2; } }
    public override char ABBREVIATION { get { return 'O'; } }
}
