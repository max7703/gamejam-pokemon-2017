using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCameraTitle : MonoBehaviour {
    private bool MoveUp = false;
    private bool MoveRight = false;
    public float vitesse = 0.01f;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
        if(this.transform.position.y >= -5.142044f && MoveUp == false)
        {
            this.transform.Translate(Vector3.down * Time.deltaTime * vitesse, Space.World);
        }

        if (this.transform.position.y <= -5.142044f)
        {
            if (this.transform.position.x >= -1.1f)
            {
                this.transform.Translate(Vector3.left * Time.deltaTime * vitesse, Space.World);
            }
        }

        if (this.transform.position.x <= -1.1f && MoveRight == false)
        {
            MoveUp = true;
            if (this.transform.position.y <= 2.892828f)
                this.transform.Translate(Vector3.up * Time.deltaTime * vitesse, Space.World);
            
        }

        if (this.transform.position.y >= 2.892828f)
        {
            MoveRight = true;
            if (this.transform.position.x <= 5.786432f)
                this.transform.Translate(Vector3.right * Time.deltaTime * vitesse, Space.World);
        }
    }
}
