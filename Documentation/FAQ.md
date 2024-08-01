# FAQ

This is the Frequently Asked Questions document for the UnityExperiments.

## General

These are general questions often asked.

1. ### Why are there so many experiment files?

    - Because we support a lot of tasks.

## How To Use

1. Download the most recent build (from box) and put it on the Desktop.
    - Or build it yourself in Unity
1. Copy [configs folder](../installer/configs/) to the Desktop.
    - There is likely another more slimmed down version on box
    - Make sure to change any config variables that you want (ex: 'elememOn' to use Elemem)
1. Copy [resources folder](../installer/resources/) to the Desktop.
    - There is likely another more slimmed down version on box
1. Double click the build and start the game
1. Select the experiment on the startup screen
1. Enter your participant ID
1. Start the experiment

## Common Errors

These are common errors when running the project in unity and how to fix them. Please also check the [UnityEPL FAQ](/Packages/UnityEPL/Documentation/FAQ.rst#CommonErrors) documentation.

1. ### You start the experiment, but all you see is the empty background (looks like the sky)

    - You need to add the following code to start your experiment class.

        ```csharp
        protected void Start() {
            Run();
        }
        ```

    - Or you need to make sure that there is a configs folder defined on your Desktop (when running from the Unity Editor). Just copy the [configs folder](/installer/configs/) from the repo to your Desktop and anywhere that the executable is located.

1. ### You start the experiment and it hangs

    - Check that you don't have two experiments active in your scene.
