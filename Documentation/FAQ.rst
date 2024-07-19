#############
FAQ
#############
This is the Frequently Asked Questions document for the UnityExperiments

.. contents:: **Table of Contents**
    :depth: 2

*************
General
*************
These are general questions often asked 

=============
What does the name stand for?
=============


*************
Common Unity Errors
*************
These are general questions often asked 

=============
You start the experiment, but all you see is the empty background (looks like the sky)
=============
You need to add the following code to start your experiment class.

.. code:: csharp

    protected void Start() {
        Run();
    }

Or you need to make sure that there is a configs folder defined on your Desktop (when running from the Unity Editor).
Just copy the configs folder from the UnityEPL repo to your Desktop and anywhere that the executable is located.

=============
You start the experiment and it hangs
=============
Check that you don't have two experiments active in your scene

