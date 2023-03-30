using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
//using Microsoft.PowerCommands.Commands;
//using Microsoft.PowerCommands.Extensions;
//using Microsoft.PowerCommands.Services;
using Microsoft.VisualStudio.Shell;

namespace Tasler.RenewedPowerCommands.OptionPages
{
	public class CommandsControl : UserControl
	{
		public CommandsPage OptionPage
		{
			get
			{
				return this.optionPage;
			}
			set
			{
				this.optionPage = value;
			}
		}

		public CommandsControl()
		{
			this.InitializeComponent();
		}

		private void CommandsControl_Load(object sender, EventArgs e)
		{
			this.commandManagerService = this.optionPage.Site.GetService<SCommandManagerService, ICommandManagerService>();
			this.items = (from command in this.commandManagerService.GetRegisteredCommands()
			orderby command.GetType().Name
			select new RowItem
			{
				Command = command.CommandID,
				CommandText = CommandsControl.GetDisplayName(command.GetType()),
				Enabled = this.OptionPage.IsCommandEnabled(command.CommandID.Guid, command.CommandID.ID)
			}).ToList<RowItem>();
			this.gridVisibility.DataSource = this.items;
			this.gridVisibility.Columns[0].Width = 200;
			this.gridVisibility.Columns[0].ReadOnly = true;
		}

		private void gridVisibility_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 1)
			{
				RowItem rowItem = this.gridVisibility.CurrentRow.DataBoundItem as RowItem;
				this.optionPage.RemoveDisabledCommand(rowItem.Command.ID);
				if (!rowItem.Enabled)
				{
					this.optionPage.AddDisabledCommand(rowItem.Command.ID);
				}
				DynamicCommand.TelemetrySession.PostEvent("VS/PPT-PowerCommands/OpitonChanged", new object[]
				{
					"VS.PPT-PowerCommands.OpitonChanged.OptionName",
					rowItem.CommandText,
					"VS.PPT-PowerCommands.OpitonChanged.OptionValue",
					rowItem.Enabled
				});
			}
		}

		private void gridVisibility_MouseLeave(object sender, EventArgs e)
		{
			this.gridVisibility.EndEdit();
		}

		internal static string GetDisplayName(Type command)
		{
			string result = string.Empty;
			DisplayNameAttribute displayNameAttribute = TypeDescriptor.GetAttributes(command).OfType<DisplayNameAttribute>().FirstOrDefault<DisplayNameAttribute>();
			if (displayNameAttribute != null)
			{
				result = displayNameAttribute.DisplayName;
			}
			return result;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private ICommandManagerService commandManagerService;
	}
}