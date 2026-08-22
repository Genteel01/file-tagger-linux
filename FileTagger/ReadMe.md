Known bugs:
- After using an edit field on the sidebar
  1. Double tapping a field in the list to edit it, then switching to another field in the same row, causes the second
  field to appear to be focused
     - It can not be typed in in this state, but it has the caret and the :focus styles
     - It fixes itself upon clicking it again
  2. Finishing editing via clicking off the list entirely doesn't update the dropdowns in the edit panel