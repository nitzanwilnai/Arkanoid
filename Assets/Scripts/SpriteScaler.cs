using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteScaler : MonoBehaviour
{
    int OldWidth;

    private void Awake()
    {
        OldWidth = Screen.width;
        ScaleScreen();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Screen.width != OldWidth)
        {
            ScaleScreen();
        }
    }

    void ScaleScreen()
    {
        float ratio = (float)Screen.width / (float)Screen.height;
        float newWidth = ratio * 1920.0f;

        transform.localScale = Vector3.one * (newWidth / 1080.0f);

        OldWidth = Screen.width;
    }
}
