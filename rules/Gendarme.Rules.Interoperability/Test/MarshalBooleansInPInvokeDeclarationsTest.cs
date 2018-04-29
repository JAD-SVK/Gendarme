﻿//
// Unit tests for MarshalBooleansInPInvokeDeclarationsRule.
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Runtime.InteropServices;

using Gendarme.Rules.Interoperability;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Interoperability {

	[TestFixture]
	public class MarshalBooleansInPInvokeDeclarationsTest : MethodRuleTestFixture<MarshalBooleansInPInvokeDeclarationsRule> {

		[Test]
		public void DoesNotApply ()
		{
			// not pinvokes
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			AssertRuleDoesNotApply (SimpleMethods.GeneratedCodeMethod);
		}

		[DllImport ("liberty.so")]
		[return: MarshalAs (UnmanagedType.Bool)]
		private static extern bool GoodBoolReturn ();

		[DllImport ("liberty.so")]
		private static extern void GoodOutBool ([MarshalAs (UnmanagedType.U1)] out bool b);

		[DllImport ("liberty.so")]
		private static extern void GoodBoolTwo ([MarshalAs (UnmanagedType.Bool)] bool b1,
			[MarshalAs (UnmanagedType.U1)] ref bool b2);

		[Test]
		public void Ok ()
		{
			// no boolean parameters
			AssertRuleSuccess (SimpleMethods.ExternalMethod);

			AssertRuleSuccess<MarshalBooleansInPInvokeDeclarationsTest> ("GoodBoolReturn");
			AssertRuleSuccess<MarshalBooleansInPInvokeDeclarationsTest> ("GoodOutBool");
			AssertRuleSuccess<MarshalBooleansInPInvokeDeclarationsTest> ("GoodBoolTwo");
		}

		[DllImport ("liberty.so")]
		private static extern bool BadBoolReturn ();

		[DllImport ("liberty.so")]
		private static extern void BadOutBool (out bool b);

		[DllImport ("liberty.so")]
		private static extern void BadBoolTwo (bool b1, ref bool b2);

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<MarshalBooleansInPInvokeDeclarationsTest> ("BadBoolReturn", 1);
			AssertRuleFailure<MarshalBooleansInPInvokeDeclarationsTest> ("BadOutBool", 1);
			AssertRuleFailure<MarshalBooleansInPInvokeDeclarationsTest> ("BadBoolTwo", 2);
		}
	}
}
