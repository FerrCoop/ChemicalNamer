using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public Atom BondOrigin { get; private set; }
    public Atom draggingAtom { get; private set; }

    private Camera mainCam;
    Vector3 offset;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            mainCam = Camera.main;
        }
    }

    private void Update()
    {
        if (draggingAtom != null)
        {
            draggingAtom.transform.position = mainCam.ScreenToWorldPoint(Input.mousePosition) - offset;
            draggingAtom.UpdateBonds();
        }
        if(Input.GetMouseButtonUp(0))
        {
            SetDraggingAtom(null);
        }
    }

    public void SetBondOrigin (Atom _atom)
    {
        BondOrigin = _atom;
    }

    public void SetDraggingAtom(Atom _atom)
    {       
        if (draggingAtom != null)
        {
            draggingAtom.GetComponent<Rigidbody2D>().simulated = true;
            draggingAtom.GetComponent<Collider2D>().enabled = true;
        }
        draggingAtom = _atom;
        if (_atom != null)
        {
            offset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - _atom.transform.position;
            draggingAtom.GetComponent<Rigidbody2D>().simulated = false;
            draggingAtom.GetComponent<Collider2D>().enabled = false;
        }
    }
}
