﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using static Microsoft.VisualStudio.VSConstants;

using Tasler.RenewedPowerCommands.Common;
using Tasler.RenewedPowerCommands.OptionPages;
using Tasler.RenewedPowerCommands.Services;
using Tasler.RenewedPowerCommands.Extensions;

namespace Tasler.RenewedPowerCommands
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[ProvideMenuResource(1000, 1)]
	[ProvideAutoLoad(EnvDTE.Constants.vsContextNoSolution, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideService   (typeof(SCommandManagerService), ServiceName = "CommandManagerService")]
	[ProvideProfile   (typeof(CommandsPage          ), "RenewedPowerCommands", "Commands", 15600, 1912, true, DescriptionResourceID = 197    )]
	[ProvideOptionPage(typeof(CommandsPage          ), "RenewedPowerCommands", "Commands", 15600, 1912, true, "ToolsOptionsKeywords_Commands")]
	[ProvideProfile   (typeof(GeneralPage           ), "RenewedPowerCommands", "General" , 15600, 4606, true, DescriptionResourceID = 2891   )]
	[ProvideOptionPage(typeof(GeneralPage           ), "RenewedPowerCommands", "General" , 15600, 4606, true, "ToolsOptionsKeywords_General" )]
	[Guid(RenewedPowerCommandsPackage.PackageGuidString)]
	public sealed class RenewedPowerCommandsPackage : AsyncPackage
	{
		public const string PackageGuidString = "BA8DECA4-149F-42B1-B371-454C7D096326";

		#region Package Members

		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			((IServiceContainer)this).AddService(typeof(SCommandManagerService), new ServiceCreatorCallback(this.CreateCommandManagerService), true);
			new CommandSet(this).Initialize();
			try
			{
				foreach (CommandInterceptor commandInterceptor in _saveCommandInterceptors)
				{
					commandInterceptor.BeforeExecute += this.SaveCommandInterceptor_BeforeExecute;
				}
			}
			catch
			{
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			foreach (CommandInterceptor commandInterceptor in _saveCommandInterceptors)
			{
				commandInterceptor.BeforeExecute -= this.SaveCommandInterceptor_BeforeExecute;
				commandInterceptor.Dispose();
			}
		}
		#endregion Package Members

		private void SaveCommandInterceptor_BeforeExecute(object sender, CommandEventArgs args)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (string.Compare(args.CommandGuid, typeof(VSStd97CmdID).GUID.ToString(), true) == 0)
			{
				if (this.GeneralPage.RemoveAndSortUsingsOnSave || this.GeneralPage.FormatOnSave)
				{
					Document activeDocument = this.Dte.ActiveDocument;
					IEnumerable<Document> enumerable;
					if (args.CommandId == 0xE0)
					{
						enumerable = this.GetDocumentsToBeSaved();
					}
					else
					{
						enumerable = new Document[] { activeDocument };
					}

					foreach (Document document in enumerable)
					{
						if (document.ProjectItem.FileCodeModel != null
							&& document.ProjectItem.ContainingProject.IsKind(VSLangProj.PrjKind.prjKindCSharpProject)
							|| document.ProjectItem.ContainingProject.IsKind(VSLangProj.PrjKind.prjKindVBProject)
							|| document.ProjectItem.ContainingProject.IsKind(CpsCsProjectGuid)
							|| document.ProjectItem.ContainingProject.IsKind(CpsVbProjectGuid))
						{
							if (!activeDocument.Equals(document))
							{
								document.Activate();
							}
							if (this.GeneralPage.FormatOnSave)
							{
								this.Dte.ExecuteCommand("Edit.FormatDocument", string.Empty);
							}
							if (this.GeneralPage.RemoveAndSortUsingsOnSave)
							{
								this.Dte.ExecuteCommand("Edit.RemoveAndSort", string.Empty);
							}
							activeDocument.Activate();
						}
					}
				}
			}
		}

		public DTE2 Dte => _dte ?? (_dte = base.GetDialogPage(typeof(DTE)) as DTE2);
		private DTE2 _dte;

		public CommandsPage CommandsPage => _commandsPage ?? (_commandsPage = base.GetDialogPage(typeof(CommandsPage)) as CommandsPage);
		private CommandsPage _commandsPage;

		public GeneralPage GeneralPage => _generalPage ?? (_generalPage = (base.GetDialogPage(typeof(GeneralPage)) as GeneralPage));
		private GeneralPage _generalPage;

		private List<CommandInterceptor> _saveCommandInterceptors = new List<CommandInterceptor>
			{
				new CommandInterceptor(this.Dte, "{5EFC7975-14BC-11CF-9B2B-00AA00573819}", 0x6E),
				new CommandInterceptor(this.Dte, "{5EFC7975-14BC-11CF-9B2B-00AA00573819}", 0x14B),
				new CommandInterceptor(this.Dte, "{5EFC7975-14BC-11CF-9B2B-00AA00573819}", 0x6F),
				new CommandInterceptor(this.Dte, "{5EFC7975-14BC-11CF-9B2B-00AA00573819}", 0xE2),
				new CommandInterceptor(this.Dte, "{5EFC7975-14BC-11CF-9B2B-00AA00573819}", 0xE0)
			};


		private object CreateCommandManagerService(IServiceContainer container, Type serviceType)
		{
			if (container != this)
			{
				return null;
			}
			if (typeof(SCommandManagerService) == serviceType)
			{
				return new CommandManagerService();
			}
			return null;
		}

		private IEnumerable<Document> GetDocumentsToBeSaved()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			new RunningDocumentTable(this);
			return this.Dte.Documents.OfType<Document>().Where((Document document) => !document.IsSaved());
		}

		private const string CpsCsProjectGuid = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";

		private const string CpsVbProjectGuid = "{778DAE3C-4631-46EA-AA77-85C1314464D9}";
	}
}
