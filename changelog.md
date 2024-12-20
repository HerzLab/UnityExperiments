# Changelog

## v1.0.1 (2024-09-03)

Initial Release

## v1.0.2 (2024-09-06)

### General

- Make question about stim happen for readonly sessions

## v1.0.3 (2024-10-22)

### General

- This is a big overhaul of a bunch of things under the hood, in UnityEPL/PsyForge
- UnityEPL 3.0 also was rebranded to PsyForge

### Analysis Considerations

- Use the new "constants and configs" event for getting all configs and experiment constants
- In the "session start" event, the key "experiment version" has been changed to "experiment name"
- Changed all state/status events to be of type TASK_STATUS
- There are now "frameDisplayed" events on any frame where a unity event occurs. This is because there is a difference between when the event occurs and when it actually displays on screen. The "frameDisplayed" event is when it shows on screen.
  - Prior event timings are only accurate to the framerate of the monitor
- Added "play video", "pause video", and "video finished" events for videos
- Added "recording start" and "recording stop" events for audio recording
- Changed "start recall period" and "stop recall period" to "start recognition period" and "stop recognition period" for the recognition period
- Removed "countdown" event because it was a duplicate of the "COUNTDOWN" status event
- Added "old new" that is set to either "old" or "new" to each "old new keys" event
