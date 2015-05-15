using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIFollow : MonoBehaviour 
{
    public Transform followObj;
    public Vector3 offset;
    
    public RectTransform canvasRect;

    public float fadeDistance = 100;

    private RectTransform _rectTransform;

	// Use this for initialization
	void Start () 
    {
        _rectTransform = GetComponent<RectTransform>();
        canvasRect = this.transform.root.GetComponent<RectTransform>();
	}

    void Update()
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, followObj.position + offset);
        _rectTransform.anchoredPosition = screenPoint - canvasRect.sizeDelta / 2f;
    }
}
