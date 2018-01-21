//
// Gendarme.Rules.BadPractice.DoNotForgetNotImplementedMethodsRule
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2008 Cedric Vivier
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

using System;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule checks for short methods that throw a <c>System.NotImplementedException</c> 
	/// exception. It's likely a method that has not yet been implemented and should not be
	/// forgotten by the developer before a release.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// private void Save ()
	/// {
	///	throw new NotImplementedException ("pending final format");
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method looks like it is not implemented or is incomplete.")]
	[Solution ("Implement the method and/or make sure it's limitations are well documented.")]
	[EngineDependency (typeof (OpCodeEngine))]
	// part of CA1065 is implemented here instead of DoNotThrowInUnexpectedLocationRule so we need to be
	// able to suppress it too, with the same [SuppressMessage], even if this rule has a larger scope
	[FxCopCompatibility ("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
	public class DoNotForgetNotImplementedMethodsRule : Rule, IMethodRule {

		// contains NEWOBJ and THROW instructions
		private static OpCodeBitmask bitmask = new OpCodeBitmask (0x0, 0x84000000000000, 0x0, 0x0);

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply if there's no IL code
			// or if it was generated by the compiler or tools
			if (!method.HasBody || method.IsGeneratedMethodBody ())
				return RuleResult.DoesNotApply;

			// is there a Newobj *and* a Throw instruction in this method
			if (!bitmask.IsSubsetOf (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			int n = 0;
			bool cond_branch = false;
			foreach (Instruction inst in method.Body.Instructions) {
				// check if the code is linear or with branches
				if (FlowControl.Cond_Branch == inst.OpCode.FlowControl)
					cond_branch = true;

				// if there are branch and it's long enough then assume it is implemented
				if (cond_branch && (++n > 10))
					break;

				// check for "throw new NotImplementedException (...)"
				if (inst.OpCode.Code != Code.Newobj)
					continue;
				MethodReference ctor = (MethodReference) inst.Operand;
				if (!ctor.DeclaringType.IsNamed ("System", "NotImplementedException"))
					continue;
				if (inst.Next.OpCode.Code != Code.Throw)
					continue;

				// the defect is more severe if the method is visible outside it's assembly
				Severity severity = method.IsPublic ? Severity.High : Severity.Medium;
				Runner.Report (method, severity, Confidence.Normal);
				return RuleResult.Failure;
			}

			return RuleResult.Success;
		}

		public bool SkipGeneratedGuiMethods
		{
			get
			{
				return false;
			}
		}

#if false
		public void GenerateBitmask ()
		{
			OpCodeBitmask bitmask = new OpCodeBitmask ();
			bitmask.Set (Code.Newobj);
			bitmask.Set (Code.Throw);
			Console.WriteLine (bitmask);
		}
#endif
	}
}