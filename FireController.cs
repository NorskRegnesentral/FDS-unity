using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class FireController : MonoBehaviour
{
    public string filePath = "Assets\\Resource\\Example_case_hrr.csv"; // Path to your CSV file (can be given in inspector)
    public GameObject fireObject;  // Assign your fire prefab in the inspector
    private List<float> hrrData = new List<float>();
    private List<float> timeData = new List<float>();
    private float timeElapsed = 0f;
    private int currentIndex = 0;

    void Start()
    {

        ReadCSV();

    }

    void Update()
    {
        if (currentIndex < hrrData.Count)
        {
            timeElapsed += Time.deltaTime;
            // Assuming each row represents data for specific time
            if (timeElapsed >= timeData[currentIndex])
            {
                float hrrValue = hrrData[currentIndex];
                Debug.Log($"Inside if test, {hrrValue}");
                AdjustFireIntensity(hrrValue);
                Debug.Log("Inside if test - function called");
                currentIndex++;
            }
        }
    }

    void ReadCSV()
    {
        if (filePath == null)
        {
            Debug.Log($"file not assigned");
            return;
        }
        StreamReader reader = new StreamReader(filePath);
        int linecount = 0;

        while (!reader.EndOfStream)
        {
            linecount++;
            string line = reader.ReadLine();



            if (linecount <= 2)
            {
                continue; //skip the header plus units
            }

            string[] values = line.Split(',');

            if (float.TryParse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float timeValue) &&
                float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float hrrValue))
            {
                timeData.Add(timeValue);
                hrrData.Add(hrrValue);
                Debug.Log($"Parsed values: Time = {timeValue}, HRR = {hrrValue}");
            }
            else
            {
                Debug.Log("Didnt work");
            }

        }

        Debug.Log("Done reading file 1");

        //list the timeData 
        foreach (var tm in timeData)
        {
            Debug.Log("Time value is:" + tm);
        }
    }


    void AdjustFireIntensity(float hrrValue)
    {
        // Example: Adjust fire intensity based on HRR value
        // This is where you'll integrate with the fire asset
        // Assuming the fire prefab has a ParticleSystem component
        Debug.Log($"Ajust fire intensity function, {hrrValue}");
        ParticleSystem fireParticleSystem = fireObject.GetComponent<ParticleSystem>();
        if (fireParticleSystem != null)
        {
            Debug.Log($"object exists");
            var main = fireParticleSystem.main;
            main.startSize = Mathf.Lerp(0.1f, 1.0f, hrrValue/1000.0f); // Adjust as needed
            main.startLifetime = Mathf.Lerp(0.5f, 2.0f, hrrValue/1000.0f); // Adjust as needed

            Debug.Log($"HRR Value: {hrrValue} ");
            Debug.Log($"Start Size: {main.startSize.constant}");
            Debug.Log($"Start Lifetime: {main.startLifetime.constant}");

        }
        else
        {
            Debug.Log($"object doesnt exists");

        }
    }
}
