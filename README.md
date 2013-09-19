cstools
=========

cslint
---------

cslint is a C# linting tool that checks for common issues in C# code and then outputs the results as a JSON object.

Policies
~~~~~~~~~~~~

The following policies are defined in cslint.  At the moment cslint doesn't have a way of configuring which policies are enabled, so the best option is to comment out each of the policy checkers that you want disabled and rebuild cslint.

  * Class name does not match filename
  * More than one public class defined in file
  * Nested public class defined

csunit
----------

csunit is a C# unit testing tool.  It's a generic interface to various unit test frameworks and is capable of executing various runners over multiple assemblies and combining the result.  This is useful if your unit test suite is spread across multiple unit test frameworks.

cscover
----------

cscover is a code coverage tool.  It instruments (rewrites) a list of assemblies and executes a command, producing an XML report of all lines of code executed in the instrumented assemblies.

You can use cscover with something like the following command:

```
mono --debug ~/Projects/Redpoint/CSharpLinter/cscover/bin/Debug/cscover.exe -o report.xml -c /usr/bin/mono -a "/home/james/Projects/Redpoint/Tychaia/packages/xunit.runners.1.9.1/tools/xunit.console.clr4.exe Tychaia.Globals.Tests.dll" -w "$(pwd)" --copy-to="$(pwd)" Tychaia*.dll
```

In this case, it outputs the report to `report.xml` by running `/usr/bin/mono` with the specified arguments (-a), in the specified working directory (-w).  It copies the `cscover.exe` and `cslib.dll` to the --copy-to directory before running the command (instrumented assemblies depend on these two files).  Finally, all files matching `Tychaia\*.dll` are instrumented (this can just be a generic list of assemblies).

cscover produces a report like this:

```
<?xml version="1.0" encoding="utf-8"?>
<report>
  <total>144</total>
  <instrumented start="16" end="16" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultArrayPool.cs" />
  <instrumented start="17" end="17" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultArrayPool.cs" />
  <instrumented start="18" end="18" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultArrayPool.cs" />
  <instrumented start="19" end="19" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultArrayPool.cs" />
  <instrumented start="22" end="22" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultArrayPool.cs" />
  <instrumented start="23" end="23" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultArrayPool.cs" />
  <instrumented start="24" end="24" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultArrayPool.cs" />
  <instrumented start="27" end="27" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultArrayPool.cs" />
  <instrumented start="28" end="28" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultArrayPool.cs" />
  <instrumented start="29" end="29" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultArrayPool.cs" />
  <instrumented start="30" end="30" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultArrayPool.cs" />
  <instrumented start="31" end="31" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultArrayPool.cs" />
  ...
  <instrumented start="19" end="19" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/TychaiaGlobalIoCModule.cs" />
  <instrumented start="20" end="20" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/TychaiaGlobalIoCModule.cs" />
  <executed start="12" end="12" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultPositionScaleTranslation.cs" />
  <executed start="14" end="14" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultPositionScaleTranslation.cs" />
  <executed start="15" end="15" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultPositionScaleTranslation.cs" />
  <executed start="12" end="12" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultChunkSizePolicy.cs" />
  <executed start="27" end="27" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultChunkSizePolicy.cs" />
  <executed start="16" end="16" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultPositionScaleTranslation.cs" />
  <executed start="19" end="19" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultPositionScaleTranslation.cs" />
  <executed start="20" end="20" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultPositionScaleTranslation.cs" />
  <executed start="21" end="21" file="/home/james/Projects/Redpoint/Tychaia/Tychaia.Globals/Implementations/DefaultPositionScaleTranslation.cs" />
</report>
```

`total` is the total lines that have been instrumented, while the `instrumented` and `executed` elements detail exactly what lines have been instrumented and which lines have been executed during program execution.
