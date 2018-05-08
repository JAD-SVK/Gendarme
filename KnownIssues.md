Known issues
============

Failed tests
------------

* Test.Framework
  * StackEntryAnalysisTest
    * Failed tests:
        * TestMultipleCatch
        * TestTryCatchFinally
      * Reason: unknown, compiler optimalization?
      * Severity: high
      * Fix: unknown
* Test.Rules.Concurrency
  * DoNotLockOnThisOrTypesTest
    * StaticType
    * Type
    * This
  * ProtectCallToEventDelegatesTest
    * Good
    * GenericBad
* Test.Rules.Correctness
  * CheckParametersNullityInVisibleMethodsTest
    * Failed test: MoreGenericCases
  * TypesWithNativeFieldsShouldBeDisposableTest
    * Failed test: TestNativeFieldsArray
    * Severity: low
* Test.Rules.Performance
  * AvoidConcatenatingCharsTest
    * Failed tests: `ConcatObject` _(part 1)_ and `ConcatObject_Array` _(Fixed)_
    * Failed tested methods in `AvoidConcatenatingCharsTest` class: Object_Locals, Object_Parameters, Object_Fields, Object_StaticFields, Object_Mixed_4
    * Reason: Seems like the code was fixed by Visual Studio
  * UseIsOperatorTest
    * Failed tests: `Return` _(Fail)_
    * Reason: Seems like the code was fixed by Visual Studio
  * ReviewLinqMethodTest
    * Failed test: Cases
    * Failed tested methods: `CanUseAny.Bad3a()`, `CanUseAny.Bad3b()`
* Test.Rules.Interoperability
  * DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest
    * Failed 17 test methods
    * Seems like there is a problem in detecting anonymous delegates _(but not only anonymous)_
* Test.Rules.Maintainability
  * AvoidUnnecessarySpecializationTest
    * Failed test: GenericMethod
    * Reason: Problem in detecting typed parameter `<T>` specialization
    * Failed test: InterfaceOfInterface
    * State: do not detect unnecessary specialization
  * AvoidComplexMethodsTest
    * Failed test: CyclomaticComplexityMeasurementTest
    * Reason: probably different compiler than planed, rule works probably without problem
* Test.Rules.Smells
  * AvoidCodeDuplicatedInSameClassTest
    * Failed test: SuccesOnNonDuplicatedInSwitchsLoadingByFieldsTest
    * Reason: probably different compiler than planed
  * AvoidSwitchStatementsTest
    * Failed tests: FailOnMethodWithSwitchTest, FailOnSwitchWithStringsTest, SuccessOnMethodWithoutSwitchAndGeneratorTest
    * Reason: probably different compiler than planed



## Incorrect rule behaviour

* Gendarme.Rules.Correctness
  * CheckParametersNullityInVisibleMethodsRule
    * Current state: check only for single `T Parse(string, ...)` *(first parameter string, other parameters are ignored)* and single `bool TryParse(string, ..., out T)` *(first parameter string, last parameter parsed value, other parameters are ignored)* where `T` is the declaring type.
    * Update proposition:
      * Pair `Parse` and `TryParse` method with the same parameters.
      * Add support also for `ParseExact` and `TryParseExact`.
      * Consider adding support for `T TryParse(string s, ..., T defaultValue)` mode.

## Possible changes

* Gendarme.Rules.Naming
  * UseCorrectSuffixRule
    * Change: add "Node" suffix

Update

*Libraries with incomplete behaviour:*

* NUnit
  * Add rules:
    * test fixture class without test methods
    * test methods attributes (and other) without test fixture attribute
    * support for native Visual Studio unit tests _(all rules as for **NUnit**)_

## Verify

* All weird/strange searches for nested types and full name comparisons

  e.g.:

  ```C#
  if (self.IsNested) {
    int spos = name.LastIndexOf ('/');
    if (spos == -1)
      return false;
    // GetFullName could be optimized away but it's a fairly uncommon case
    return (nameSpace + "." + name == self.GetFullName ());
  }
  ```

* Gendarme.Rules.Smells.AvoidLongMethodsRule _(function: IsAutogeneratedByTools)_
