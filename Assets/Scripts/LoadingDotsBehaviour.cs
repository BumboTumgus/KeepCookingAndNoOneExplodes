using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingDotsBehaviour : MonoBehaviour
{
    private Text textfield;
    private int counter = 0;
    private int counterTarget = 5;
    private float timer = 0;
    private float timerTarget = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        textfield = GetComponent<Text>();
        textfield.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if(isActiveAndEnabled)
        {
            timer += Time.deltaTime;

            if(timer >= timerTarget)
            {
                timer -= timerTarget;

                counter++;
                if (counter > counterTarget)
                    counter = 0;

                string newText = "";
                for (int index = 0; index < counter; index++)
                    newText += ".";

                textfield.text = newText;
            }
        }
    }
}
