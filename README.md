# Fire Dynamics Simulator (FDS) in Unity (FDS-unity)

This repository contains two scripts:

## 1. FireController.cs

**Description**:   
The `FireController.cs` script handles the behavior and simulation of fire within the Unity environment. It controls the ignition, spread, colour, size based on four parameters. This script parses through the CSV file *_hrr.csv and modifies Unity's fire assets based on that. The concept of parsing the CSV file was inspired by this [video](https://www.youtube.com/watch?app=desktop&v=xwnL4meq-j8) and the application of these ideas to Unity was influenced by this [Designing and Developing a VR Environment for Indoor Fire Simulation](https://www.researchgate.net/publication/349828252_Designing_and_Developing_a_VR_Environment_for_Indoor_Fire_Simulation)

**Usage**:  
(Assuming the scene is set, fire assests or particle system is imported along with a button)
1. In the scripts folder, import this script (right click -> import asset)
2. Attach the `FireController` script to an empty GameObject in your Unity scene.
3. Configure the fire controller variables in the Unity Inspector by attaching the game objects to their corresponding boxes under inspector.
4. By default the scripts has an example path for the location of the csv file. Make sure the file is imported to the project under Resources and that the correct path is changed in inspector.
5. In run mode, once the button has been clicked, the simulation will begin. 

**Parameters**:  
- fireParticleSystem: Assign fire prefab in the inspector
- smokeParticleSystem: Assign smoke prefab (Burning Smoke) in the inspector
- whitesmokeParticle: This is the heat dissipation present in the VFX fire Asset 
- smokeColumnSystem: VFX asset has another smoke prefab which is a continous coloumn of smoke.

  These parameters can be reduced depending on the desired output. 

**Dependencies**:  
Mention any dependencies or required components for the script to function correctly. For example:  
- Requires the `VFX Fire Assets` package. (or any fire asset)
- Requires a csv file

## 2. HelloWorld.cs

**Description**: 

**Usage**: 

**Parameters**: 

**Dependencies**: 




