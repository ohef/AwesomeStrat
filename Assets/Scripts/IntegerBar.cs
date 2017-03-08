using UnityEngine;
using System.Collections;

//Assumes raw mode is enabled
//TODO: Don't rely on raw mode.
public class IntegerBar : MonoBehaviour {

    private RectTransform m_trans;
    private float initMaxAnchor;

	// Use this for initialization
	void Awake () {
        m_trans = this.GetComponent<RectTransform>();
        initMaxAnchor = m_trans.anchorMax.x;
    }

    public void UpdateBar( float currentVal, float maxVal )
    {
        float calcMaxAnchor = ( currentVal / maxVal ) * ( initMaxAnchor - m_trans.anchorMin.x ) + m_trans.anchorMin.x;
        m_trans.anchorMax = new Vector2( calcMaxAnchor, m_trans.anchorMax.y );
    }
}
