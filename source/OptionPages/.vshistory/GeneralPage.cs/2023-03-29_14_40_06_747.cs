using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace Tasler.RenewedPowerCommands.OptionPages
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[Guid("7FF0673B-4E19-45FD-86E3-09473884C44F")]
	public class GeneralPage : DialogPage
	{
		public bool RemoveAndSortUsingsOnSave { get; set; }

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override IWin32Window Window
		{
			get
			{
				_control = new GeneralControl();
				_control.Location = new Point(0, 0);
				_control.OptionPage = this;
				return _control;
			}
		}

		private GeneralControl _control;
	}
}
