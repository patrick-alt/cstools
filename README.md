cslint
======

cslint is a C# linting tool that checks for common issues in C# code and then outputs the results as a JSON object.

Policies
-----------

The following policies are defined in cslint.  At the moment cslint doesn't have a way of configuring which policies are enabled, so the best option is to comment out each of the policy checkers that you want disabled and rebuild cslint.

  * Class name does not match filename
  * More than one public class defined in file
  * Nested public class defined
