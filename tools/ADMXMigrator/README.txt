ADMXMigrator is a great help when modyfing the admx file, however there seems to be one bug:

BUG: 
----------
ADMXMigrator removes the following elements from the policies where these are defined.
[...]
      <enabledValue>
        <decimal value="1" />
      </enabledValue>
      <disabledValue>
        <decimal value="0" />
      </disabledValue>
[...]

WORKAROUND:
----------
Make sure deletion of these lines are discarded when checking in changes to the admx file.

trondr, 2020-04-26