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

Note that the list of results in the report are not guarenteed to only be unique values.  Thus long-running commands and tests can result in large reports.  You'll want the program or script consuming `report.xml` to convert the results to a unique list before attempting to calculate metrics.
