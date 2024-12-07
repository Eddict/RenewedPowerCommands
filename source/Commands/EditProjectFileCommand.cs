using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Tasler.RenewedPowerCommands.Extensions;
using Tasler.RenewedPowerCommands.Linq;
using VSLangProj;

namespace Tasler.RenewedPowerCommands.Commands
{
    [Guid("888DA324-B21F-4658-B663-F22884A3AF1D")]
    [DisplayName("Edit Project File")]
    internal class EditProjectFileCommand : DynamicCommand
    {
        public EditProjectFileCommand(IServiceProvider serviceProvider)
            : base(serviceProvider,
                  EditProjectFileCommand.OnExecute,
                  new CommandID(typeof(EditProjectFileCommand).GUID, c_cmdidEditProjectCommand))
        {
        }

        protected override bool CanExecute(OleMenuCommand command)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (base.CanExecute(command))
            {
                Project project = DynamicCommand.Dte.SelectedItems.Item(1).Project;
                if (project != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static void OnExecute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Project project = DynamicCommand.Dte.SelectedItems.Item(1).Project;
            if (project != null)
            {
                string fullName = project.FullName;
                if (File.Exists(fullName))
                {
                    try
                    {
                        Dte.ExecuteCommand("Project.UnloadProject", string.Empty);
                        Window window = Dte.OpenFile(EnvDTE.Constants.vsViewKindTextView, fullName);
                        window.Visible = true;
                        window.Activate();
                    }
                    catch (COMException)
                    {
                    }
                }
            }
        }
        public const int c_cmdidEditProjectCommand = 0x0121;
    }
}