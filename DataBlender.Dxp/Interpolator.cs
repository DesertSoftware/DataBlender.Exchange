/* 
//  Copyright Desert Software Solutions, Inc 2018
//  Data Exchange Platform - Data Blender 

// Copyright (c) 2014 Desert Software Solutions Inc. All rights reserved.

// THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY, NON-INFRINGEMENT AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED.  

// IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT 
// OF THE USE OF THIS SOFTWARE, WHETHER OR NOT SUCH DAMAGES WERE FORESEEABLE AND EVEN IF THE AUTHOR IS ADVISED 
// OF THE POSSIBILITY OF SUCH DAMAGES. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using DesertSoftware.Solutions.Dynamix;

namespace DataBlender.Dxp
{
    public class Interpolator
    {
        static private Regex expression = new Regex(@"\{(.*?)\}");

        static public MatchCollection Interpolations(string text) {
            return expression.Matches(text ?? "");
        }

        static public string Interpolate(string text, ValueBag values, Func<string, dynamic, dynamic> evaluator = null) {
            // {colA} -- {colB} ==> colA}, -- , colB}
            var tokens = new Dictionary<string, dynamic>();

            evaluator = evaluator ?? ((source, value) => value);

            // find all of the source names delimited within {} braces. 
            foreach (Match match in Interpolations(text)) {
                var token = match.Value.Trim('{', '}');

                if (!tokens.ContainsKey(token))
                    tokens.Add(token, null);
            }

            // If no braces present, no interpolations detected, return the text as is.
            if (tokens.Count == 0)
                return text;

            // bind the source values to our sources
            foreach (var token in tokens.Keys.ToList()) {
                // if this is a literal assignment the source will begin with a single quote
                var tokenValue = token.StartsWith("'")
                    ? token.Trim("'".ToCharArray())
                    : values[token] ?? "";

                // finally bind the result of the evaluator to our sources 
                tokens[token] = evaluator(token, tokenValue);   // ?? this.default
            }

            // perform the interpolation
            foreach (var token in tokens.Keys)
                text = text.Replace(string.Concat("{", token, "}"), string.Format("{0}", tokens[token]));

            return text;
        }
    }
}
