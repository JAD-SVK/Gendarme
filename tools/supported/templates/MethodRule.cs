﻿//
// $rootnamespace$.$safeitemname$
//
// Authors:
//	$name$ <$email$>
//
// Copyright (C) $year$ $name$
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace $rootnamespace$ {

	/// <summary>
	/// TODO: Add a summary of the rule.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// TODO: Add an example where the rule would fail.
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// TODO: Show how to fix the bad example.
	/// </code>
	/// </example>

	// TODO: Describe the problem and solution
	[Problem ("")]
	[Solution ("")]
	public class $safeitemname$ : Rule, IMethodRule {
		public RuleResult CheckMethod (MethodDefinition method)
		{
			// TODO: Write the rule.
			return RuleResult.Success;
		}
	}
}
