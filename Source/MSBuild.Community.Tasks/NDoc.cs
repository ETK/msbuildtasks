#region Copyright � 2005 Paul Welter. All rights reserved.
/*
Copyright � 2005 Paul Welter. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Win32;

// $Id$

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Runs the NDoc application.
    /// </summary>
    /// <example>Generated html help file.
    /// <code><![CDATA[
    /// <NDoc Documenter="MSDN" 
    ///     ProjectFilePath="MSBuild.Community.Tasks.ndoc" />
    /// ]]></code>
    /// </example>
    public class NDoc : ToolTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:NDoc"/> class.
        /// </summary>
        public NDoc()
        {
            
        }

        #region Properties
        private string _documenter;

        /// <summary>
        /// Gets or sets the documenter.
        /// </summary>
        /// <value>The documenter.</value>
        /// <remarks>Available documenters are VS.NET_2003, JavaDoc, LaTeX, LinearHtml, MSDN, XML.</remarks>
        [Required]
        public string Documenter
        {
            get { return _documenter; }
            set { _documenter = value; }
        }

        private string _projectFilePath;

        /// <summary>
        /// Gets or sets the project file path.
        /// </summary>
        /// <value>The project file path.</value>
        [Required]
        public string ProjectFilePath
        {
            get { return _projectFilePath; }
            set { _projectFilePath = value; }
        }

        private bool _verbose;

        /// <summary>
        /// Gets or sets a value indicating whether the output is verbose.
        /// </summary>
        /// <value><c>true</c> if verbose; otherwise, <c>false</c>.</value>
        public bool Verbose
        {
            get { return _verbose; }
            set { _verbose = value; }
        } 
        #endregion

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            StringBuilder builder = new StringBuilder();
            
            if (!string.IsNullOrEmpty(_documenter))
                builder.AppendFormat(" -documenter={0}", _documenter);
            if (!string.IsNullOrEmpty(_projectFilePath))
                builder.AppendFormat(" -project=\"{0}\"", _projectFilePath);
            if (_verbose)
                builder.Append(" -verbose");

            return builder.ToString(); ;   
        }

        /// <summary>
        /// Returns the fully qualified path to the executable file.
        /// </summary>
        /// <returns>
        /// The fully qualified path to the executable file.
        /// </returns>
        protected override string GenerateFullPathToTool()
        {
            string ndocPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            ndocPath = Path.Combine(ndocPath, @"NDoc 1.3\bin\net\1.1");

            try
            {
                using(RegistryKey buildKey = Registry.ClassesRoot.OpenSubKey(@"NDoc Project File\shell\build\command"))
                {
                    if (buildKey == null)
                    {
                        Log.LogError("Could not find the NDoc Project File build command. Please make sure NDoc is installed.");
                    }
                    else
                    {
                        ndocPath = buildKey.GetValue(null, ndocPath).ToString();
                        Regex ndocRegex = new Regex("(.+)NDocConsole\\.exe", RegexOptions.IgnoreCase);
                        Match pathMatch = ndocRegex.Match(ndocPath);
                        ndocPath = pathMatch.Groups[1].Value.Replace("\"", "");
                    }
                }                
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }
            
            base.ToolPath = ndocPath;
            return Path.Combine(ToolPath, ToolName);
        }

        /// <summary>
        /// Gets the name of the executable file to run.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the executable file to run.</returns>
        protected override string ToolName
        {
            get { return "NDocConsole.exe"; }
        }

        /// <summary>
        /// Gets the <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.</returns>
        protected override MessageImportance StandardOutputLoggingImportance
        {
            get
            {
                return MessageImportance.Normal;
            }
        }
    }
}