using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class FireController : MonoBehaviour
{
    public string filePath = "Assets\\Resource\\Example_case_hrr.csv"; // Path to your CSV file (can be given in inspector)
    public UnityEngine.UI.Button startSimulationbtn; // Assign the UI Button in the inspector


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
    private bool isSimulationRunning = false;

    private List<ParticleSystem> fireParticleSytems = new List<ParticleSystem>();
    private List<ParticleSystem> smokeParticleSystems = new List<ParticleSystem>();

    void Start()
    {
        fireParticleSystem.Play();
        smokeParticleSystem.Play();

        ReadCSV();
        ConfigureParticleSystems();


        //Add a listener to button 
        if (startSimulationbtn != null)
        {
           startSimulationbtn.onClick.AddListener(StartSimulation);
        }
    }

    void Update()
    {
        if (isSimulationRunning) {
        timeElapsed += Time.deltaTime;
            // Assuming each row represents data for specific time
            if (timeElapsed >= updateInterval && currentTimeStep < timeData.Count)
            {   /*
                float progress = timeElapsed / updateInterval;
                float hrr = Mathf.Lerp(hrrData[currentTimeStep], hrrData[currentTimeStep + 1], progress);
                float qRad = Mathf.Lerp(qRads[currentTimeStep], qRads[currentTimeStep + 1], progress);
                float qTotal = Mathf.Lerp(qTotals[currentTimeStep], qTotals[currentTimeStep + 1], progress);
                float mlrAir = Mathf.Lerp(mlrAirs[currentTimeStep], mlrAirs[currentTimeStep + 1], progress);
                float mlrProduct = Mathf.Lerp(mlrProducts[currentTimeStep], mlrProducts[currentTimeStep + 1], progress);
                */
                float hrr = hrrData[currentTimeStep];
                float qRad = qRads[currentTimeStep];
                float qTotal = qTotals[currentTimeStep];
                float mlrAir = mlrAirs[currentTimeStep];
                float mlrProduct = mlrProducts[currentTimeStep];
            

                ApplyFire(hrr, qRad, qTotal);
                ApplySmokeSettings(mlrAir, mlrProduct);

                //if (timeElapsed >= updateInterval)
                //{
                currentTimeStep++;
                timeElapsed = 0f; // Reset the timer
                //}
            }
        }
    }

    void ApplyFire(float hrr, float qRad, float qTotal)
    {
        // Map HRR to emission rate
        var fireEmission = fireParticleSystem.emission;
        fireEmission.rateOverTime = hrr * 10f;//Ajust multiplier as needed

        // Map Q_RADI to color and intensity
        var fireColorOverLifetime = fireParticleSystem.colorOverLifetime;
        fireColorOverLifetime.color = new ParticleSystem.MinMaxGradient(
            Color.Lerp(Color.yellow, Color.red, Mathf.Clamp01(qRad / 100f)));

        // Map Q_TOTAL to particle size and lifetime
        var fireMain = fireParticleSystem.main;
        //reMain.startSize = (qTotal + 100f) / 10; // Adjust size mapping
        fireMain.startSize = Mathf.Lerp(0.5f, 3f, Mathf.Clamp01(qTotal / 500f));
        //firestartLifetime = (qTotal * 200f) / 20; // Adjust lifetime mapping
        fireMain.startLifetime = Mathf.Lerp(1f, 4f, Mathf.Clamp01(qTotal / 500f));  // These ranges should be adjusted
        
        var fireSizeOverLifetime = fireParticleSystem.sizeOverLifetime;
        fireSizeOverLifetime.enabled = true;
        fireSizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, new AnimationCurve(
            new Keyframe(0, Mathf.Clamp(qTotal / 100, 0.5f, 1.5f)), // Start size adjusted by Q_TOTAL
            new Keyframe(1, 0.1f)  // End size assumes reduction as fire dissipates
        ));
    }

    void ApplySmokeSettings(float mlrAir, float mlrProduct)
    {
        // Adjusting the smoke emission rate
        var smokeEmission = smokeParticleSystem.emission;
        smokeEmission.rateOverTime = (mlrAir + mlrProduct) * 20f;  // Adjust this factor based on visual feedback

        var smokeMain = smokeParticleSystem.main;
        smokeMain.startSize = 2f; // Static starting size, consider making dynamic if needed

        // Adjusting size over lifetime for smoke based on MLR_PRODUCTS
        var smokeSizeOverLifetime = smokeParticleSystem.sizeOverLifetime;
        smokeSizeOverLifetime.enabled = true;
        smokeSizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, new AnimationCurve(
            new Keyframe(0, 1), // Start at normal size
            new Keyframe(1, Mathf.Clamp((mlrAir + mlrProduct) / 10, 1f, 3f)) // End size adjusted by MLR sum
        ));

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

    void ConfigureParticleSystems()
    {
        // Configure fire particle system
        var fireMain = fireParticleSystem.main;
        fireMain.startLifetime = 2f;
        fireMain.startSize = 1f;
        fireMain.startColor = Color.red;

        var fireEmission = fireParticleSystem.emission;
        fireEmission.enabled = true;
        fireEmission.rateOverTime = 10f;

        var fireShape = fireParticleSystem.shape;
        fireShape.enabled = true;
        fireShape.shapeType = ParticleSystemShapeType.Hemisphere;

        var fireColorOverLifetime = fireParticleSystem.colorOverLifetime;
        fireColorOverLifetime.enabled = true;
        fireColorOverLifetime.color = new ParticleSystem.MinMaxGradient(Color.red, Color.yellow);

        var fireSizeOverLifetime = fireParticleSystem.sizeOverLifetime;
        fireSizeOverLifetime.enabled = true;
        fireSizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 2f);

        var fireNoise = fireParticleSystem.noise;
        fireNoise.enabled = true;
        fireNoise.strength = 1f;

        var fireRenderer = fireParticleSystem.GetComponent<ParticleSystemRenderer>();
        fireRenderer.enabled = true;


        // Configure smoke particle system
        var smokeMain = smokeParticleSystem.main;
        smokeMain.startLifetime = 5f;
        smokeMain.startSize = 2f;
        smokeMain.startColor = Color.white;

        var smokeEmission = smokeParticleSystem.emission;
        smokeEmission.enabled = true;
        smokeEmission.rateOverTime = 10f;

        var smokeShape = smokeParticleSystem.shape;
        smokeShape.enabled = true;
        smokeShape.shapeType = ParticleSystemShapeType.Hemisphere;

        var smokeColorOverLifetime = smokeParticleSystem.colorOverLifetime;
        smokeColorOverLifetime.enabled = true;
        smokeColorOverLifetime.color = new ParticleSystem.MinMaxGradient(Color.gray, Color.black);

        var smokeSizeOverLifetime = smokeParticleSystem.sizeOverLifetime;
        smokeSizeOverLifetime.enabled = true;
        smokeSizeOverLifetime.size = new ParticleSystem.MinMaxCurve(2f, 4f);

        var smokeNoise = smokeParticleSystem.noise;
        smokeNoise.enabled = true;
        smokeNoise.strength = 1f;

        var smokeRenderer = smokeParticleSystem.GetComponent<ParticleSystemRenderer>();
        smokeRenderer.enabled = true;
       
    }

    public void StartSimulation()
    {
        isSimulationRunning = true;
        fireParticleSystem.Play();
        smokeParticleSystem.Play();
    }


}
