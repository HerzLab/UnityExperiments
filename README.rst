#############
UnityExperiments
#############

The program containing all Unity Experiments for the Herz lab.

.. contents:: **Table of Contents**
    :depth: 2

*************
Overview
*************
This holds some basic information about getting started with the experiments.

For more information than what is in this document (or to see information about specific experiments), please see the `<Documentation>`_ folder


*************
Development Setup
*************
How to setup everything to develop for Unity

=============
Getting Unity Setup
=============
#. Install VSCode
#. Setup the VSCode extension
    #. Click "View" in the top bar of your computer and then click the "Extensions" option
        - You can also just click the little building blocks looking thing on the left side bar
    #. Search for "Unity"
    #. Install the Unity plugin (and it's dependencies)
#. Install UnityHub
#. Add the experiment to UnityHub
    #. Click the "Add" button in the top right
    #. Navigate to the project directory
    #. Click the "Open" button in the bottom right
#. Install the correct version of Unity for the project
    #. Remember the version number next to the project (ex: 2023.2.13f1)
    #. Click the hazard sign button next to the project
    #. Click "Install Other Editor Version" in the bottom left
    #. Click the "Install" button next to the correct Unity version for the project
        - If the correct version is not present there, then keep following the instructions in this part
    #. Click the "Archive" button on the top right
    #. Click the "download archive" link on the top
    #. Navigate to the correct version of Unity
    #. Select UnityHub version, download, and install it
#. Open the project by clicking its name
#. Open the 


*************
Testing Setup
*************



*************
Making an Experiment
*************
It's really easy to start making a basic experiment.

=============
Basic Instructions
=============

#. git submodule add git@github.com:pennmem/UnityEPL.git Assets/
#. Add asmref for UnityEPL in Scripts
#. Inherit ExperimentBase on your main experiment class
#. Implement the abstract methods PreTrials, TrialStates, and PostTrials

=============
Adding Config variables
=============

#. Add asmref for UnityEPL in Scripts
#. Create a partial class named Config
#. Implement each item in your config, so that it looks like this

.. code:: csharp

    public static bool elememOn { get { return Config.GetSetting<bool>("elememOn"); } }


=============
Types of Experiments and Components Available
=============
There are many types of experiments, but here are a few common ones and the useful components for them.
There is also a list of generally useful componenets

-------------
Word List Experiments
-------------
TextDsplayer
SoundRecorder
VideoPlayer

-------------
Spacial Experiments
-------------
SpawnItems
PickupItems

-------------
Closed-Loop Experiments
-------------
EventLoop
ElememInterface

-------------
General Components
-------------
Config
Logging
ErrorNotifier
NetworkInterface
InputManager
List/Array shuffling (including ones that are consistent per participant)
Random values that are consistent per participant


*************
FAQ
*************
See the FAQ Document


*************
Authors
*************
James Bruska, Connor Keane, Ryan Colyer
