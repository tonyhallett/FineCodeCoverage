﻿using FineCodeCoverage.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FineCodeCoverage.Impl
{
	internal class CoverageProject
	{
		public string ProjectFolder { get; set; }

		public string TestDllFileInOutputFolder { get; internal set; }
		public string WorkFolder { get; internal set; }
		public string ProjectOutputFolder { get; internal set; }
		public string FailureDescription { get; internal set; }
		public string FailureStage { get; internal set; }
		public bool HasFailed => !string.IsNullOrWhiteSpace(FailureStage) || !string.IsNullOrWhiteSpace(FailureDescription);
		public string ProjectFile { get; internal set; }
		public string ProjectName => Path.GetFileNameWithoutExtension(ProjectFile);
		public string ProjectCoberturaFile { get; internal set; }
		public string TestDllFileInWorkFolder { get; internal set; }

		public AppSettings Settings { get; internal set; }

		public CoverageProject Step(string stepName, Action<CoverageProject> action)
		{
			if (HasFailed)
			{
				return this;
			}

			Logger.Log($"{stepName} ({ProjectName})");

			try
			{
				action(this);
			}
			catch (Exception exception)
			{
				FailureStage = stepName;
				FailureDescription = exception.ToString();
			}

			if (HasFailed)
			{
				Logger.Log($"{stepName} ({ProjectName}) Failed", FailureDescription);
			}
			
			return this;
		}
	}
}
