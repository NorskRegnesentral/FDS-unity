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
This is a simple script to become familar with Unity. It contains the functionalities for a simple button which upon clicking prints hello work to the console. 

**Usage**: 

1. Open Unity Hub and Create a New Project
2. Open Unity Hub and Click on "New Project."
3. Choose a template suitable for VR/AR development. For VR, you can select the "3D" template. (optional for this guide) Name your project and click "Create."
4. Once the project is created, the Unity Editor will open.
5. In the hierarchy window: Right click → UI→ canvas
   - Right click on Canvas → UI → Button
   - Resize the button according to preference using the toolbar
6. From the drop down menu of Button click on Text (Note: There will be a pop up, asking to import TMP, Import both)
7. In the inspector view, navigate to text and change “button” to for example “play”.
8. Navigate to Assets folder, create a folder there (Right click → create → folder), Name this folder Scripts. This is where all your C# scripts are saved.
    - In this folder: Right click → create → C# script and name it HelloWorldButton.cs
    - Double click on the script and it will open up in an editor
9. In the script copy the code from HelloWordButton.cs and save. 
10. Under hierarchy view, right click → Create Empty and name this object Controller.
11. Assign the script to this object by clicking on the object and under inspector view click on Add component and find the script from the drop down menu. Or drag the script and drop there. 
12. In the "Hierarchy" window, select the "Controller." In the "Inspector" window, you will see the HelloWorldButton script component with a field for My Button. Drag the Button object from the "Hierarchy" into the My Button field in the Inspector.
13. Save and Run the simulation by pressing play. Upon clicking the button, hello world should appear in the console window. 





