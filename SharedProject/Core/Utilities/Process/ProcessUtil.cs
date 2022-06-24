﻿using System;
using CliWrap;
using System.Linq;
using CliWrap.Buffered;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IProcessUtil))]
	internal class ProcessUtil : IProcessUtil
	{
		public async Task<ExecuteResponse> ExecuteAsync(ExecuteRequest request, CancellationToken cancellationToken)
		{
			var commandTask = Cli
			.Wrap(request.FilePath)
			.WithArguments(request.Arguments)
			.WithValidation(CommandResultValidation.None)
			.WithWorkingDirectory(request.WorkingDirectory)
			.ExecuteBufferedAsync(cancellationToken);

			BufferedCommandResult result = null;
			try
            {
				result = await commandTask;
			}
			catch(OperationCanceledException) // CliWrap does not use the same ct
            {
				cancellationToken.ThrowIfCancellationRequested();
            }

			var exitCode = result.ExitCode;

			var outputList = new List<string>();

			var directOutput = string.Join(Environment.NewLine, new[]
			{
				result.StandardOutput,
				Environment.NewLine,
				result.StandardError
			}
			.Where(x => !string.IsNullOrWhiteSpace(x)))
			.Trim('\r', '\n')
			.Trim();

			if (!string.IsNullOrWhiteSpace(directOutput))
			{
				outputList.Add(directOutput);
			}

			var output = string.Join(Environment.NewLine, outputList)
			.Trim('\r', '\n')
			.Trim();

			return new ExecuteResponse
			{
				ExitCode = exitCode,
				ExitTime = result.ExitTime,
				RunTime = result.RunTime,
				StartTime = result.StartTime,
				Output = output
			};
		}
		
	}

}
