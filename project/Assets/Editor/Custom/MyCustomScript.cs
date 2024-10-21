using UnityEngine;
using System.Diagnostics;

public class MyCustomScript : MonoBehaviour
{
    static void Build()
    {
        string prompt = "Do you want to build the project?";
        
        // asset path
        var dataPath = Application.dataPath;
        var ClassName = "MyOtherScript";
        var MethodName = "MyMethod";

        var newPrompt = "'Create a new class called 'myNewClass.cs'" +
                        " Create a method in the class that will send" +
                        " an http request to get api data'";
        
        // Create a new Process object
        var process = new Process();

        // Set the file name of the process to Unity executable
        process.StartInfo.FileName = "Unity.exe";

        // Set the arguments to execute another method using the -executeMethod command
        process.StartInfo.Arguments =
            "-batchmode" +
            " -projectPath" +
            $"{dataPath}" +
            $" -executeMethod {ClassName}.{MethodName}" +
            $" -customArg " + newPrompt;

        // Start the process
        process.Start();

        // Wait for the process to exit
        process.WaitForExit();
    }
}