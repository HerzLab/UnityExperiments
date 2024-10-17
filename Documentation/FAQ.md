# FAQ

This is the Frequently Asked Questions document for the UnityExperiments.

## General

These are general questions often asked.

1. ### Why are there so many experiment files?

    - Because we support a lot of tasks.

## Common Errors

These are common errors when running the project in unity and how to fix them. Please also check the [PsyForge FAQ](/Packages/PsyForge/Documentation/FAQ.rst#CommonErrors) documentation.

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
