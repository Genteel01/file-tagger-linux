Known bugs:
- After using an edit field on the sidebar, three issues happen
  1. Double tapping a field in the list to edit it, then switching to another field, causes the second field to appear
  to be focused
     - It can not be type in in this state, but it has the caret and the :focus styles
     - It fixes itself upon clicking it again
  2. The \<keep> option in the dropdown appears twice
  3. The \<keep> option in the dropdown appears whenever two things are selected, regardless of if they are the same
  4. This was already happening before the creation of LabelledDropdown