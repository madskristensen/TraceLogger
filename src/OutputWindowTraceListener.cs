using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace TraceLogger
{
    public class OutputWindowTraceListener : TraceListener
    {
        private IVsOutputWindowPane pane;
        private IVsOutputWindow _outputWindowService;
        private string _name;

        public OutputWindowTraceListener(IVsOutputWindow outputWindowService, string name)
        {
            _outputWindowService = outputWindowService;
            _name = name;
            Trace.Listeners.Add(this);
        }

        private bool EnsurePane()
        {
            if (pane == null)
            {
                Guid guid = Guid.NewGuid();
                _outputWindowService.CreatePane(ref guid, _name, 1, 1);
                _outputWindowService.GetPane(ref guid, out pane);
            }

            return pane != null;
        }

        public override void Write(string message)
        {
            try
            {
                if (EnsurePane())
                {
                    ThreadHelper.JoinableTaskFactory.Run(async delegate
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        pane.OutputString(message);
                    });
                }
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        public override void WriteLine(string message)
        {
            Write(Environment.NewLine + message);
        }
    }
}
