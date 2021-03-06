include ../common.make

rules_sources =  \
	AvoidConcatenatingCharsRule.cs \
	AvoidLargeNumberOfLocalVariablesRule.cs \
	AvoidLargeStructureRule.cs \
	AvoidLocalDataStoreSlotRule.cs \
	AvoidMethodWithLargeMaximumStackSizeRule.cs \
	AvoidRepetitiveCallsToPropertiesRule.cs \
	AvoidRepetitiveCastsRule.cs \
	AvoidReturningArraysOnPropertiesRule.cs \
	AvoidTypeGetTypeForConstantStringsRule.cs \
	AvoidUncalledPrivateCodeRule.cs \
	AvoidUninstantiatedInternalClassesRule.cs \
	AvoidUnnecessaryOverridesRule.cs \
	AvoidUnneededCallsOnStringRule.cs \
	AvoidUnneededFieldInitializationRule.cs \
	AvoidUnneededUnboxingRule.cs \
	AvoidUnsealedConcreteAttributesRule.cs \
	AvoidUnsealedUninheritedInternalTypeRule.cs \
	AvoidUnusedParametersRule.cs \
	AvoidUnusedPrivateFieldsRule.cs \
	CompareWithEmptyStringEfficientlyRule.cs \
	ConsiderCustomAccessorsForNonVisibleEventsRule.cs \
	DoNotIgnoreMethodResultRule.cs \
	RemoveUnneededFinalizerRule.cs \
	UseSuppressFinalizeOnIDisposableTypeWithFinalizerRule.cs \
	ImplementEqualsTypeRule.cs \
	MathMinMaxCandidateRule.cs \
	OverrideValueTypeDefaultsRule.cs \
	PreferCharOverloadRule.cs \
	PreferInterfaceConstraintOnGenericParameterForPrimitiveInterfaceRule.cs \
	PreferLiteralOverInitOnlyFieldsRule.cs \
	RemoveUnusedLocalVariablesRule.cs \
	ReviewLinqMethodRule.cs \
	UseIsOperatorRule.cs \
	UseStringEmptyRule.cs \
	UseTypeEmptyTypesRule.cs

# turn off optimization for tests since gmcs (unlike MS csc) now turns "" into String.Empty when optimizing is on
EXTRA_TESTS_OPTIONS += -unsafe -o- -d:CODE_ANALYSIS

tests_sources = \
	AvoidConcatenatingCharsTest.cs \
	AvoidLargeNumberOfLocalVariablesTest.cs \
	AvoidLocalDataStoreSlotTest.cs \
	AvoidRepetitiveCallsToPropertiesTest.cs \
	AvoidUnnecessaryOverridesTest.cs \
	AvoidUnneededFieldInitializationTest.cs \
	RemoveUnneededFinalizerTest.cs \
	GlobalSuppressions.cs \
	UseSuppressFinalizeOnIDisposableTypeWithFinalizerTest.cs \
	UseStringEmptyTest.cs \
	UseTypeEmptyTypesTest.cs \
	AvoidUnusedParametersTest.cs \
	CompareWithEmptyStringEfficientlyTest.cs \
	AvoidMethodWithLargeMaximumStackSizeTest.cs \
	AvoidUncalledPrivateCodeTest.cs \
	AvoidUnneededCallsOnStringTest.cs \
	AvoidUninstantiatedInternalClassesTest.cs \
	DoNotIgnoreMethodResultTest.cs \
	AvoidReturningArraysOnPropertiesTest.cs \
	AvoidUnsealedConcreteAttributesTest.cs \
	UseIsOperatorTest.cs \
	AvoidUnsealedUninheritedInternalTypeTest.cs \
	OverrideValueTypeDefaultsTest.cs \
	ImplementEqualsTypeTest.cs \
	MathMinMaxCandidateTest.cs \
	AvoidLargeStructureTest.cs \
	RemoveUnusedLocalVariablesTest.cs \
	AvoidUnneededUnboxingTest.cs \
	ConsiderCustomAccessorsForNonVisibleEventsTest.cs \
	AvoidTypeGetTypeForConstantStringsTest.cs \
	AvoidRepetitiveCastsTest.cs \
	AvoidUnusedPrivateFieldsTest.cs \
	PreferCharOverloadTest.cs \
	PreferInterfaceConstraintOnGenericParameterForPrimitiveInterfaceTest.cs \
	PreferLiteralOverInitOnlyFieldsTest.cs \
	ReviewLinqMethodTest.cs
