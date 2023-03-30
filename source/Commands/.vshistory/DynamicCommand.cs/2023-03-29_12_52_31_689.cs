using System;
using System.ComponentModel.Design;
using EnvDTE;
using Tasler.RenewedPowerCommands.Extensions;
using Tasler.RenewedPowerCommands.OptionPages;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using IServiceProvider = System.IServiceProvider;

namespace Tasler.RenewedPowerCommands.Commands
{
	// Token: 0x02000030 RID: 48
	public abstract class DynamicCommand : OleMenuCommand
	{
		protected static IServiceProvider ServiceProvider => DynamicCommand.serviceProvider;

		protected static DTE Dte
		{
			get
			{
				if (DynamicCommand.dte == null)
				{
					DynamicCommand.dte = DynamicCommand.ServiceProvider.GetService<DTE>();
				}
				return DynamicCommand.dte;
			}
		}

		// Token: 0x0600015C RID: 348 RVA: 0x00005D10 File Offset: 0x00003F10
		public static Document GetActiveEditorDocument()
		{
			IVsTextView vsTextView;
			IVsTextLines vsTextLines;
			if (ErrorHandler.Succeeded(((IVsTextManager)DynamicCommand.ServiceProvider.GetService(typeof(SVsTextManager))).GetActiveView(0, null, ref vsTextView)) && ErrorHandler.Succeeded(vsTextView.GetBuffer(ref vsTextLines)))
			{
				IExtensibleObject extensibleObject = vsTextLines as IExtensibleObject;
				if (extensibleObject != null)
				{
					object obj;
					extensibleObject.GetAutomationObject("Document", null, ref obj);
					Document document = obj as Document;
					if (document != null)
					{
						return document;
					}
				}
			}
			return DynamicCommand.Dte.ActiveDocument;
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x0600015D RID: 349 RVA: 0x00005D86 File Offset: 0x00003F86
		protected static PowerCommandsPackage PowerCommandsPackage
		{
			get
			{
				if (DynamicCommand.powerCommandsPackage == null)
				{
					DynamicCommand.powerCommandsPackage = DynamicCommand.ServiceProvider.GetService<PowerCommandsPackage>();
				}
				return DynamicCommand.powerCommandsPackage;
			}
		}

		// Token: 0x0600015E RID: 350 RVA: 0x00005DA3 File Offset: 0x00003FA3
		public DynamicCommand(IServiceProvider provider, EventHandler onExecute, CommandID id) : base(onExecute, id)
		{
			base.BeforeQueryStatus += this.OnBeforeQueryStatus;
			DynamicCommand.serviceProvider = provider;
		}

		// Token: 0x0600015F RID: 351 RVA: 0x00005DC8 File Offset: 0x00003FC8
		protected void OnBeforeQueryStatus(object sender, EventArgs e)
		{
			OleMenuCommand oleMenuCommand = sender as OleMenuCommand;
			oleMenuCommand.Enabled = (oleMenuCommand.Visible = (oleMenuCommand.Supported = this.CanExecute(oleMenuCommand)));
		}

		// Token: 0x06000160 RID: 352 RVA: 0x00005DFB File Offset: 0x00003FFB
		protected virtual bool CanExecute(OleMenuCommand command)
		{
			return DynamicCommand.PowerCommandsPackage.CommandsPage.IsCommandEnabled(command.CommandID.Guid, command.CommandID.ID);
		}

		// Token: 0x06000161 RID: 353 RVA: 0x00005E24 File Offset: 0x00004024
		public override void Invoke(object inArg, IntPtr outArg, OLECMDEXECOPT options)
		{
			try
			{
				base.Invoke(inArg, outArg, options);
			}
			finally
			{
				DynamicCommand.TelemetrySession.PostEvent("VS/PPT-PowerCommands/CommandExecuted", new object[]
				{
					"VS.PPT-PowerCommands.CommandExecuted.CommandName",
					CommandsControl.GetDisplayName(base.GetType())
				});
			}
		}

		// Token: 0x0400007A RID: 122
		private static DTE dte;

		// Token: 0x0400007B RID: 123
		private static IServiceProvider serviceProvider;

		// Token: 0x0400007C RID: 124
		private static PowerCommandsPackage powerCommandsPackage;

		// Token: 0x0400007D RID: 125
		private static ITelemetrySession _telemetrySession;
	}
}