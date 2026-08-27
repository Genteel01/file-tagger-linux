Known bugs:
- Empty number fields in list panel show a validation warning
- Clicking from the TrackList to the Previous/Next Image buttons causes an issue
  - Tracklist ScrollViewFocusLost gets called, causing SelectionChanged to be called, resetting SelectedImageIndex to 0
  - The Commands on the buttons get called BEFORE this happens, meaning they change SelectedImageIndex before it gets reset