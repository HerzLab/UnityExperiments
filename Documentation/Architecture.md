# Architecture

This is the overall architecture and components in UnityExperiments. I will reference many things from UnityEPL, so if something is not explained here, check that library [here](/Packages/UnityEPL/README.md).

## Overview

There are certain coding practices that should always be applied when coding in the UnityEPL. Some are recommendations and some are requirements (code will break if you do not follow them). We try to make as many errors occur at compile time, but there are always limitations of the language.

## Important Code Structures

These are the important structures within the code.

### MainManager

The MainManager has two main jobs:

1. Hold the objects for all EventLoops
1. Allow other event loops to interact with unity objects/functions

This is a bit monolithic, but it allows the objects to be held in one consistent object and allows for a clean interface with Unity. This also means the MainManager is running on the main Unity thread.

### Config (MyConfig.cs)

These are the configuration variables that are accessed in either the config.json or the specific experiment.json. It is set up this way so that you won't have runtime errors by misspelling your config variable names in the code.

### LangStrings (MyLangStrings.cs)

These are the strings used within your experiment. They are all in one location, so it is very easy to add translations later. It is also set up this way so that you won't have runtime errors by misspelling a string in the code (if it is used more than once).

### Word System

This is broken into a few parts: Words, RandomSubsets, StimWordList, and WordDisplayer.

- **Words** are the basic class that store word information. A Word only stores a single string. A PairedWord stores the basic word string and a paired word string. A CategorizedWord stores the basic word string and a category string. You get the point.
- **RandomSubsets** are fed a list of words, randomizes it, and then allows you to get words based on that randomization. This isn't too important for normal Words (just a shuffle really), but it is very important for things like categorized words since you may want to control how words are shuffled in relation to the categories.
- **StimWordLists** are just classes that hold a list of words associated with their stim states.
- **WordDisplayer** is just the thing that shows the word(s) on screen.

### FRExperimentBase

This is the base for every word recall-based memory experiment. It provides a LOT of base functionality that is useful. All of this functionality can be overridden or changed though.

### MemMapExperiment

This is the main experiment developed by the Herz lab.

## Important Coding Practices

These are the important practices that are critical for all coders to understand and follow:

- Do NOT use *Task.Delay()*. Instead, use *manager.Delay()*. They act exactly the same, but manager.Delay knows how to handle the single-threaded nature of WebGL.

## Acronyms and Terms

Below are the common acronyms and terms used in this project:

### Acronyms

- **EEG**: Electroencephalogram

### Terms

- **Elemem**: CML EEG reading and stimulation control system
