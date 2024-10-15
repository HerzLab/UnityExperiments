# Changelog

## v1.0.1

Initial Release

## v1.0.2

### Analysis Considerations

- Use the new "constants and configs" event for getting all configs and experiment constants
- In the "session start" event, the key "experiment version" has been changed to "experiment name"
- Changed all state/status events to be of type TASK_STATUS
- There are now "frameDisplayed" events on any frame where a unity event occurs. This is because there is a difference between when the event occurs and when it actually displays on screen. The "frameDisplayed" event is when it shows on screen.
  - Prior event timings are only accurate to the framerate of the monitor
- Added "play video", "pause video", and "video finished" events for videos
- Added "recording start" and "recording stop" events for audio recording
