﻿using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tasler.RenewedPowerCommands.Common;
using Tasler.RenewedPowerCommands.Linq;

namespace Tasler.RenewedPowerCommands.Commands
{
	[Guid("")]
	[DisplayName("Remove and Sort Usings")]
	internal class RemoveSortUsingsCommand : DynamicCommand
	{
		public RemoveSortUsingsCommand(IServiceProvider serviceProvider) : base(serviceProvider, new EventHandler(RemoveSortUsingsCommand.OnExecute), new CommandID(typeof(RemoveSortUsingsCommand).GUID, 0xDBE))
		{
		}

		protected override bool CanExecute(OleMenuCommand command)
		{
			if (base.CanExecute(command))
			{
				Project project = DynamicCommand.Dte.SelectedItems.Item(1).Project;
				if (project == null)
				{
					return RemoveSortUsingsCommand.IsAtLeastOneCSharpProject();
				}
				if (project.Kind == "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}")
				{
					return true;
				}
			}
			return false;
		}

		private static void OnExecute(object sender, EventArgs e)
		{
			Project project = DynamicCommand.Dte.SelectedItems.Item(1).Project;
			if (project != null && project.Kind == "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}")
			{
				RemoveSortUsingsCommand.ProcessProject(project);
				return;
			}
			new ProjectIterator(DynamicCommand.Dte.Solution).Where((Project prj) => prj.Kind == "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}").ForEach(delegate (Project prj)
			{
				RemoveSortUsingsCommand.ProcessProject(prj);
			});
		}

		private static bool IsAtLeastOneCSharpProject()
		{
			return new ProjectIterator(DynamicCommand.Dte.Solution).FirstOrDefault((Project prj) => prj.Kind == "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") != null;
		}

		private static void ProcessProject(Project project)
		{
			if (project != null)
			{
				if (DTEHelper.CompileProject(project) != 0)
				{
					new ErrorListWindow(DynamicCommand.ServiceProvider).Show();
					return;
				}
				RunningDocumentTable source = new RunningDocumentTable(DynamicCommand.ServiceProvider);
				List<string> alreadyOpenFiles = source.Select((RunningDocumentInfo info) => info.Moniker).ToList<string>();
				string fileName;
				Func<string, bool> <> 9__3;
				new ProjectItemIterator(project.ProjectItems).Where((ProjectItem item) => item.FileCodeModel != null).ForEach(delegate (ProjectItem item)
				{
					fileName = item.get_FileNames(1);
					Window window = DynamicCommand.Dte.OpenFile("{7651A703-06E5-11D1-8EBD-00A0C90F26EA}", fileName);
					window.Activate();
					try
					{
						DynamicCommand.Dte.ExecuteCommand("Edit.RemoveAndSort", string.Empty);
					}
					catch (COMException)
					{
					}
					IEnumerable<string> alreadyOpenFiles = alreadyOpenFiles;
					Func<string, bool> predicate;
					if ((predicate = <> 9__3) == null)
					{
						predicate = (<> 9__3 = ((string file) => file.Equals(fileName, StringComparison.OrdinalIgnoreCase)));
					}
					if (alreadyOpenFiles.SingleOrDefault(predicate) != null)
					{
						DynamicCommand.Dte.ActiveDocument.Save(fileName);
						return;
					}
					window.Close(1);
				});
			}
		}

		public const uint cmdidRemoveSortUsingsCommand = 0xDBEU;
	}
}
