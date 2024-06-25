using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class FireController : MonoBehaviour
{
    public string filePath = "Assets\\Resource\\Example_case_hrr.csv"; // Path to your CSV file (can be given in inspector)


    public ParticleSystem fireParticleSystem; //Assign your fire prefab in the inspector
    public ParticleSystem smokeParticleSystem; //Assign your smoke prefab in the inspector

    private List<float> timeData = new List<float>();
    private List<float> hrrData = new List<float>();
    private List<float> qRads = new List<float>();
    private List<float> qTotals = new List<float>();
    private List<float> mlrAirs = new List<float>();
    private List<float> mlrProducts = new List<float>();

    private int currentTimeStep = 0;
    private float timeElapsed = 0f;
    private float updateInterval = 3f; // Interval in seconds

    void Start()
    {

        ReadCSV();

    }

    void Update()
    {
       
        timeElapsed += Time.deltaTime;
            // Assuming each row represents data for specific time
        if (timeElapsed >= updateInterval && currentTimeStep < timeData.Count)
        {
            float hrr = hrrData[currentTimeStep];
            float qRad = qRads[currentTimeStep];
            float qTotal = qTotals[currentTimeStep];
            float mlrAir = mlrAirs[currentTimeStep];
            float mlrProduct = mlrProducts[currentTimeStep];


            // Map HRR to emission rate
            var fireEmission = fireParticleSystem.emission;
            fireEmission.rateOverTime = hrr * 10f; // Adjust multiplier as needed

            // Map Q_RADI to color and intensity
            var fireMain = fireParticleSystem.main;
            fireMain.startColor = new Color(1.0f, qRad / 500f, 0.0f); // Adjust color mapping

            // Map Q_TOTAL to particle size and lifetime
            fireMain.startSize = (qTotal + 100f)/10; // Adjust size mapping
            fireMain.startLifetime =( qTotal * 200f)/20; // Adjust lifetime mapping

            // Map MLR_AIR and MLR_PRODUCTS to smoke emission
            var smokeEmission = smokeParticleSystem.emission;
            smokeEmission.rateOverTime = (mlrAir + mlrProduct) * 10f; // Adjust multiplier as needed

            currentTimeStep++;
            timeElapsed = 0f; // Reset the timer
        }
        //}
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
                float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float hrrValue) &&
                float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float qRadsValue) &&
                float.TryParse(values[9], NumberStyles.Float, CultureInfo.InvariantCulture, out float qTotalValue) &&
                float.TryParse(values[11], NumberStyles.Float, CultureInfo.InvariantCulture, out float mlrAirValue) &&
                float.TryParse(values[12], NumberStyles.Float, CultureInfo.InvariantCulture, out float mlrProductsValue))
            {
                timeData.Add(timeValue);
                hrrData.Add(hrrValue);
                qRads.Add(qRadsValue);
                qTotals.Add(qTotalValue);
                mlrAirs.Add(mlrAirValue);
                mlrProducts.Add(mlrProductsValue);

                Debug.Log($"Parsed values: Time = {timeValue}, HRR = {hrrValue}, QRads = {qRadsValue}, QTotals = {qTotalValue}, mlrAir = {mlrAirValue}, mlrProducts = {mlrProductsValue}");
            }
            else
            {
                Debug.Log("Didnt work");
            }

        }

        Debug.Log("Done reading file 1");


        foreach (var tm in timeData)
        {
            Debug.Log("Time value is:" + tm);
        }
    }



}
