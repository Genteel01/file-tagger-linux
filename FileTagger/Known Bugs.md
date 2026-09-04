Known bugs:
- Empty number fields in list panel show a validation warning
- List selection clears when a scrollbar is clicked
- When the list is horizontally smaller than the window (either via a large window or shrinking the columns),
the columns end up unaligned
- Double clicking to edit a list field sometimes messes up the horizontal scroll of the ScrollViewer
  - Something to do with the BringIntoViewOnFocusChange part I think
- Clicking from the TrackList to the Previous/Next Image buttons causes an issue
  - Tracklist ScrollViewFocusLost gets called, causing SelectionChanged to be called, resetting SelectedImageIndex to 0
  - The Commands on the buttons get called BEFORE this happens, meaning they change SelectedImageIndex before it gets reset