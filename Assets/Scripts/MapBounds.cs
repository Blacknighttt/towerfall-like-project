using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapBounds : MonoBehaviour
{
    public Collider2D mapBoundsL;
    public Collider2D mapBoundsR;
    //public Collider2D mapBoundsT;
   // public Collider2D mapBoundsB;
    private ContactFilter2D filter;
    private float distanceX;
   // private float distanceY;

    


    private void Start()
    {
        DistanceX();
        //DistanceY();
        filter = new ContactFilter2D().NoFilter();
    }

    private void FixedUpdate()
    {
        List<Collider2D> results = new List<Collider2D>();

        mapBoundsL.OverlapCollider(filter, results);
        if (results.Count > 0)
            TeleportRight(results[0].GetComponent<Transform>());

        results.Clear();

        mapBoundsR.OverlapCollider(filter, results);
        if (results.Count > 0)
            TeleportLeft(results[0].GetComponent<Transform>());
        results.Clear();

       /* mapBoundsT.OverlapCollider(filter, results);
        if (results.Count > 0)
            TeleportBottom(results[0].GetComponent<Transform>());

        results.Clear();

        mapBoundsB.OverlapCollider(filter, results);
        if (results.Count > 0)
            TeleportTop(results[0].GetComponent<Transform>());
        results.Clear();*/

    }
    private float DistanceX()
    {
        distanceX = (mapBoundsR.transform.position.x - mapBoundsL.transform.position.x)-3;
        return distanceX;
    }
    /*private float DistanceY()
    {
        distanceY= (mapBoundsR.transform.position.x - mapBoundsL.transform.position.x) - 3;
        return distanceY;
    }*/

    private void TeleportLeft(Transform obj)
    {
        obj.Translate(new Vector2(-distanceX, 0));
    }

    private void TeleportRight(Transform obj)
    {
        obj.Translate(new Vector2(distanceX, 0));
    }
   /* private void TeleportBottom(Transform obj)
    {
        obj.Translate(new Vector2(0,-distanceY));
    }

    private void TeleportTop(Transform obj)
    {
        obj.Translate(new Vector2(0, distanceY));
    }*/
}
