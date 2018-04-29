﻿//
// Gendarme.Rules.Maintainability.AvoidComplexMethodsRule class
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// 	(C) 2008 Cedric Vivier
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Maintainability {

	/// <summary>
	/// This rule computes the cyclomatic complexity (CC) for every method and reports any method
	/// with a CC over 25 (this limit is configurable). Large CC value often indicate complex
	/// code that is hard to understand and maintain. It's likely that breaking the
	/// method into several methods will help readability. This rule won't report any defects
	/// on code generated by the compiler or by tools.
	/// </summary>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("Methods with a large cyclomatic complexity are hard to understand and maintain.")]
	[Solution ("Simplify the method using refactors like Extract Method.")]
	[FxCopCompatibility ("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class AvoidComplexMethodsRule : Rule, IMethodRule {

		// defaults match fxcop rule http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=1575061&SiteID=1
		// so people using both tools should not see conflicting results
		private const int DefaultSuccessThreshold = 25;

		static OpCodeBitmask ld = new OpCodeBitmask (0xFFFF6C3FC, 0x1B0300000000FFE0, 0x400100FFF800, 0xDE0);

		public AvoidComplexMethodsRule ()
		{
			SuccessThreshold = DefaultSuccessThreshold;
		}

		/// <summary>
		/// Initialize the rule. This is where rule can do it's heavy initialization
		/// since the assemblies to be analyzed are already known (and accessible thru
		/// the runner parameter).
		/// </summary>
		/// <param name="runner">The runner that will execute this rule.</param>
		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// works if only SuccessThreshold is configured in rules.xml
			if (LowThreshold == 0)
				LowThreshold = SuccessThreshold * 2;
			if (MediumThreshold == 0)
				MediumThreshold = SuccessThreshold * 3;
			if (HighThreshold == 0)
				HighThreshold = SuccessThreshold * 4;
		}

		/// <summary>The cyclomatic complexity at which defects begin to be reported.</summary>
		/// <remarks>This defaults to 25 and larger values will mean fewer reported defects.</remarks>
		[DefaultValue (DefaultSuccessThreshold)]
		[Description ("The cyclomatic complexity at which defects are reported.")]
		public int SuccessThreshold { get; set; }

		/// <summary>Methods with cyclomatic complexity less than this will be reported as low severity.</summary>
		/// <remarks>If left as zero then the rule will initialize it to 2*SuccessThreshold.</remarks>
		[DefaultValue (0)]
		[Description ("Methods with cyclomatic complexity less than this will be reported as low severity.")]
		public int LowThreshold { get; set; }

		/// <summary>Methods with cyclomatic complexity less than this (but higher than LowThreshold) will be reported as medium severity.</summary>
		/// <remarks>If left as zero then the rule will initialize it to 3*SuccessThreshold.</remarks>
		[DefaultValue (0)]
		[Description ("Methods with cyclomatic complexity less than this will be reported as medium severity.")]
		public int MediumThreshold { get; set; }

		/// <summary>Methods with cyclomatic complexity less than this (but higher than MediumThreshold) will be reported as high severity.</summary>
		/// <remarks>Methods with cyclomatic complexity greater than this will be reported as critical severity.
		/// If left as zero then the rule will initialize it to 4*SuccessThreshold.</remarks>
		[DefaultValue (0)]
		[Description ("Methods with cyclomatic complexity less than this will be reported as high severity.")]
		public int HighThreshold { get; set; }


		/// <summary>
		/// Check method
		/// </summary>
		/// <param name="method">Method to be chcecked</param>
		/// <returns>Result of the check</returns>
		public RuleResult CheckMethod (MethodDefinition method)
		{
			//does rule apply?
			if (!method.HasBody || method.IsGeneratedMethodBody () || method.IsCompilerControlled)
				return RuleResult.DoesNotApply;

			//yay! rule do apply!

			// quick optimization: if the number of instructions is lower
			// than our SuccessThreshold then it cannot be too complex
			if (method.Body.Instructions.Count < SuccessThreshold)
				return RuleResult.Success;

			int cc = GetCyclomaticComplexity (method);
			if (cc < SuccessThreshold)
				return RuleResult.Success;

			//how's severity?
			Severity sev = GetCyclomaticComplexitySeverity(cc);

			string msg = String.Format (CultureInfo.CurrentCulture, "Method's cyclomatic complexity : {0}.", cc);
			Runner.Report (method, sev, Confidence.High, msg);
			return RuleResult.Failure;
		}

		/// <summary>
		/// Skip methods generated by Visual Studio (and/or other) GUI environments?
		/// </summary>
		public bool SkipGeneratedGuiMethods
		{
			get
			{
				return true;
			}
		}

		public Severity GetCyclomaticComplexitySeverity(int cc)
		{
			// 25 <= CC < 50 is not good but not catastrophic either
			if (cc < LowThreshold)
				return Severity.Low;
			// 50 <= CC < 75 this should be refactored asap
			if (cc < MediumThreshold)
				return Severity.Medium;
			// 75 <= CC < 100 this SHOULD be refactored asap
			if (cc < HighThreshold)
				return Severity.High;
			// CC > 100, don't touch it since it may become a classic in textbooks
			// anyway probably no one can understand it ;-)
			return Severity.Critical;
		}

		static public int GetCyclomaticComplexity (MethodDefinition method)
		{
			if ((method == null) || !method.HasBody)
				return 1;

			if (OpCodeEngine.GetBitmask (method).Get (Code.Switch))
				return GetSwitchCyclomaticComplexity (method);
			else
				return GetFastCyclomaticComplexity (method);
		}

		// the use of 'switch' requires a bit more code so we avoid it unless there are swicth instructions
		static private int GetFastCyclomaticComplexity (MethodDefinition method)
		{
			int cc = 1;
			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.FlowControl) {
				case FlowControl.Branch:
					// detect ternary pattern
					Instruction previous = ins.Previous;
					if ((previous != null) && ld.Get (previous.OpCode.Code))
						cc++;
					break;
				case FlowControl.Cond_Branch:
					cc++;
					break;
				}
			}
			return cc;
		}

		static private int GetSwitchCyclomaticComplexity (MethodDefinition method)
		{
			Instruction previous = null;
			Instruction branch = null;
			int cc = 1;

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.FlowControl) {
				case FlowControl.Branch:
					if (previous == null)
						continue;
					// detect ternary pattern
					previous = ins.Previous;
					if (ld.Get (previous.OpCode.Code))
						cc++;
					// or 'default' (xmcs)
					if (previous.OpCode.FlowControl == FlowControl.Cond_Branch) {
						branch = (previous.Operand as Instruction);
						// branch can be null (e.g. switch -> Instruction[])
						if ((branch != null) && targets.Contains (branch))
							targets.AddIfNew (ins);
					}
					break;
				case FlowControl.Cond_Branch:
					// note: a single switch (C#) with sparse values can be broken into several swicth (IL)
					// that will use the same 'targets' and must be counted only once
					if (ins.OpCode.Code == Code.Switch) {
						AccumulateSwitchTargets (ins);
					} else {
						// some conditional branch can be related to the sparse switch
						branch = (ins.Operand as Instruction);
						previous = branch.Previous;
						if ((previous != null) && !previous.Previous.Is (Code.Switch)) {
							if (!targets.Contains (branch))
								cc++;
						}
					}
					break;
				}
			}
			// count all unique targets (and default if more than one C# switch is used)
			cc += targets.Count;
			targets.Clear ();

			return cc;
		}

		static List<Instruction> targets = new List<Instruction> ();

		static private void AccumulateSwitchTargets (Instruction ins)
		{
			Instruction[] cases = (Instruction[]) ins.Operand;
			foreach (Instruction target in cases) {
				// ignore targets that are the next instructions (xmcs)
				if (target != ins.Next)
					targets.AddIfNew (target);
			}
			// add 'default' branch (if one exists)
			Instruction next = ins.Next;
			if (next.OpCode.FlowControl == FlowControl.Branch) {
				Instruction unc = FindFirstUnconditionalBranchTarget (cases [0]);
				if (unc != next.Operand)
					targets.AddIfNew (next.Operand as Instruction);
			}
		}

		static private Instruction FindFirstUnconditionalBranchTarget (Instruction ins)
		{
			while (ins != null) {
				if (FlowControl.Branch == ins.OpCode.FlowControl)
					return ((Instruction) ins.Operand);

				ins = ins.Next;
			}
			return null;
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask mask = new OpCodeBitmask ();
			mask.Set (Code.Ldarg);
			mask.Set (Code.Ldarg_0);
			mask.Set (Code.Ldarg_1);
			mask.Set (Code.Ldarg_2);
			mask.Set (Code.Ldarg_3);
			mask.Set (Code.Ldarg_S);
			mask.Set (Code.Ldarga);
			mask.Set (Code.Ldarga_S);
			mask.Set (Code.Ldc_I4);
			mask.Set (Code.Ldc_I4_0);
			mask.Set (Code.Ldc_I4_1);
			mask.Set (Code.Ldc_I4_2);
			mask.Set (Code.Ldc_I4_3);
			mask.Set (Code.Ldc_I4_4);
			mask.Set (Code.Ldc_I4_5);
			mask.Set (Code.Ldc_I4_6);
			mask.Set (Code.Ldc_I4_7);
			mask.Set (Code.Ldc_I4_8);
			mask.Set (Code.Ldc_I4_M1);
			mask.Set (Code.Ldc_I4_S);
			mask.Set (Code.Ldc_I8);
			mask.Set (Code.Ldc_R4);
			mask.Set (Code.Ldc_R8);
			mask.Set (Code.Ldelem_Any);
			mask.Set (Code.Ldelem_I);
			mask.Set (Code.Ldelem_I1);
			mask.Set (Code.Ldelem_I2);
			mask.Set (Code.Ldelem_I4);
			mask.Set (Code.Ldelem_I8);
			mask.Set (Code.Ldelem_R4);
			mask.Set (Code.Ldelem_R8);
			mask.Set (Code.Ldelem_Ref);
			mask.Set (Code.Ldelem_U1);
			mask.Set (Code.Ldelem_U2);
			mask.Set (Code.Ldelem_U4);
			mask.Set (Code.Ldelema);
			mask.Set (Code.Ldfld);
			mask.Set (Code.Ldflda);
			mask.Set (Code.Ldftn);
			mask.Set (Code.Ldind_I);
			mask.Set (Code.Ldind_I1);
			mask.Set (Code.Ldind_I2);
			mask.Set (Code.Ldind_I4);
			mask.Set (Code.Ldind_I8);
			mask.Set (Code.Ldind_R4);
			mask.Set (Code.Ldind_R8);
			mask.Set (Code.Ldind_Ref);
			mask.Set (Code.Ldind_U1);
			mask.Set (Code.Ldind_U2);
			mask.Set (Code.Ldind_U4);
			mask.Set (Code.Ldlen);
			mask.Set (Code.Ldloc);
			mask.Set (Code.Ldloc_0);
			mask.Set (Code.Ldloc_1);
			mask.Set (Code.Ldloc_2);
			mask.Set (Code.Ldloc_3);
			mask.Set (Code.Ldloc_S);
			mask.Set (Code.Ldloca);
			mask.Set (Code.Ldloca_S);
			mask.Set (Code.Ldnull);
			mask.Set (Code.Ldobj);
			mask.Set (Code.Ldsfld);
			mask.Set (Code.Ldsflda);
			mask.Set (Code.Ldstr);
			mask.Set (Code.Ldtoken);
			mask.Set (Code.Ldvirtftn);
			Console.WriteLine (mask);
		}
#endif
	}
}

