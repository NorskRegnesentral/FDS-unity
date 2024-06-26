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
    public ParticleSystem whitesmokeParticle;
    public ParticleSystem smokeColumnSystem;

    private List<float> timeData = new List<float>();
    private List<float> hrrData = new List<float>();
    private List<float> qRads = new List<float>();
    private List<float> qTotals = new List<float>();
    private List<float> mlrAirs = new List<float>();
    private List<float> mlrProducts = new List<float>();

    private int currentTimeStep = 0;
    private float timeElapsed = 0f;
    public float updateInterval = 0.5f; // Interval in seconds
    private bool isSimulationRunning = false;

    private float loopDuration;
    private float loopTimer;
    private bool isLooping;
    private float starttime;
    private float stoptime;

    private List<ParticleSystem> fireParticleSytems = new List<ParticleSystem>();
    private List<ParticleSystem> smokeParticleSystems = new List<ParticleSystem>();
    
    // Start is called before the first frame update
    void Start()
    {
        fireParticleSystem.Stop();
        smokeParticleSystem.Stop();
        whitesmokeParticle.Stop();
        smokeColumnSystem.Stop();

        ReadCSV();// Read data from CSV file
        ConfigureParticleSystems();// Setup particle systems


        //Add a listener to button to start simulation
        if (startSimulationbtn != null)
        {
           startSimulationbtn.onClick.AddListener(StartSimulation);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (isSimulationRunning) {
        timeElapsed += Time.deltaTime;

            // Process the simulation frame if the interval is met and data remains
            if (timeElapsed >= updateInterval && currentTimeStep < timeData.Count)
            {
                starttime = Time.time;
                Debug.Log($"Data processing begins...{starttime}");
                //Direct storing without scaling with interpolation 
                float hrr = hrrData[currentTimeStep];
                float qRad = qRads[currentTimeStep];
                float qTotal = qTotals[currentTimeStep];
                float mlrAir = mlrAirs[currentTimeStep];
                float mlrProduct = mlrProducts[currentTimeStep];

                ApplyFirSettings(hrr, qRad, qTotal);
                
                var whiteMain = whitesmokeParticle.main;
                whiteMain.startSize = 4f;
                var whiteEmission = whitesmokeParticle.emission;
                whiteEmission.enabled = true;
                whiteEmission.rateOverTime = (mlrAir + mlrProduct) * 10000f;
                
                ApplySmokeSettings(mlrAir, mlrProduct);

                //whitesmokeParticle.Stop();



                currentTimeStep++;
                timeElapsed = 0f; // Reset the timer
              
               
            }
            

            if (currentTimeStep >= timeData.Count)
            {
                stoptime = Time.time;
                float duration = stoptime - starttime;
                Debug.Log($"All data from file is processed once at time {stoptime}");
                StopSimulation();
            }
        }
    }
    // Apply fire settings based on the simulation data
    void ApplyFirSettings(float hrr, float qRad, float qTotal)
    {
        // Map HRR to emission rate
        var fireEmission = fireParticleSystem.emission;
        fireEmission.rateOverTime = hrr * 100f;//Ajust multiplier as needed

        // Map Q_RADI to color and intensity
        var fireColorOverLifetime = fireParticleSystem.colorOverLifetime;
        // Interpolate between yellow and red based on qRad
        fireColorOverLifetime.color = new ParticleSystem.MinMaxGradient(
            Color.Lerp(Color.yellow, Color.red, Mathf.Clamp01(qRad / 100f)));

        // Map Q_TOTAL to particle size and lifetime
        var fireMain = fireParticleSystem.main;

        // not realistic:
        //fireMain.startSize = (qTotal); // Adjust size mapping
        //fireMain.startLifetime = (qTotal); // Adjust lifetime mapping

        // Lerp and Clamp used here to adjust particle size and lifetime dynamically based on qTotal 
        fireMain.startSize = Mathf.Lerp(0.5f, 3f, Mathf.Clamp01(qTotal / 500f)); //prefered to scale this way 
        
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
        
        float combinedMLR = mlrAir + mlrProduct;
        // Adjusting the smoke emission rate
        var smokeEmission = smokeParticleSystem.emission;
        var smokeColumn = smokeColumnSystem.emission; smokeColumn.enabled = true;

        smokeColumn.rateOverTime = combinedMLR * 2000f;
        smokeEmission.rateOverTime = combinedMLR * 1000f;  // Adjust this factor based on visual feedback

        var smokeMain = smokeParticleSystem.main;
        var smoke = smokeColumnSystem.main;
        smoke.startSize = 3f; 
        
        smokeMain.startSize = 2f; // Static starting size, consider making dynamic if needed
        //smokeMain.startSize = Mathf.Lerp(1f, 5f, Mathf.Clamp01(combinedMLR / 50)); //smoother transition 

        // Adjusting size over lifetime for smoke based on MLR_PRODUCTS
        var smokeSizeOverLifetime = smokeParticleSystem.sizeOverLifetime;
        smokeSizeOverLifetime.enabled = true;
        smokeSizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, new AnimationCurve(
            new Keyframe(0, 1), // Start at normal size
            new Keyframe(1, Mathf.Clamp((mlrAir + mlrProduct), 1f, 3f)) // End size adjusted by MLR sum
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
        whitesmokeParticle.Play();
        smokeColumnSystem.Play();
    }

    public void StopSimulation()
    {   
        isSimulationRunning = false;
        fireParticleSystem.Stop();
        smokeParticleSystem.Stop();
        whitesmokeParticle.Stop();
        smokeColumnSystem.Stop();
    }




}
/*
Mathf.Lerp is used to simulate transitions between data points more smoothly.
For instance, if data updates every second but you render frames at a higher frequency, 
you might interpolate values for every frame.
Mathf.Clamp ensures that despite the direct application of experimental data,
the visual parameters (like color intensity or particle size) do not exceed realistic 
or visually coherent limits in the simulation environment.
 */