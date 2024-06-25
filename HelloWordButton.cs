using UnityEngine;
using UnityEngine.UI;

/* 
This class handles the functionality of a button, when clicked,
prints "Hello, World!" to the console.
*/
public class HelloWorldButton : MonoBehaviour
{
    // The Button component that will trigger the PrintHelloWorld method when clicked.
    public Button myButton;

    // This method is called before the first frame update. It sets up the button click event listener.
    void Start()
    {
        // Ensure that myButton is not null before attempting to add a listener.
        if (myButton != null)
        {
            myButton.onClick.AddListener(PrintHelloWorld);
        }
        else
        {
            Debug.LogError("Button component is not assigned.");
        }
    }
    // This method is called when the button is clicked. It prints "Hello, World!" to the console.
    void PrintHelloWorld()
    {
        Debug.Log("Hello, World!");
    }
}
