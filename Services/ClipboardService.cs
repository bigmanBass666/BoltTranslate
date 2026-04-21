using System.Threading;
using System.Windows.Forms;
using BoltTranslate.Services.NativeInterop;

namespace BoltTranslate.Services;

public interface IClipboardService
{
    string? GetSelectedTextViaClipboard();
}

public class ClipboardService : IClipboardService
{
    public string? GetSelectedTextViaClipboard()
    {
        string savedText = "";
        string selectedText = "";

        var clipboardOp = new Thread(() =>
        {
            var seqBefore = Win32Api.GetClipboardSequenceNumber();

            try { savedText = Clipboard.GetText(); } catch { }

            var foregroundWnd = Win32Api.GetForegroundWindow();

            Thread.Sleep(50);
            SimulateKeyUp(Win32Api.VK_CONTROL);
            SimulateKeyUp(Win32Api.VK_SHIFT);
            SimulateKeyUp(Win32Api.VK_MENU);
            Thread.Sleep(30);

            SimulateKeyDown(Win32Api.VK_CONTROL);
            SimulateKeyDown(Win32Api.VK_C);
            SimulateKeyUp(Win32Api.VK_C);
            SimulateKeyUp(Win32Api.VK_CONTROL);

            for (int i = 0; i < AppConstants.ClipboardMaxPollCount; i++)
            {
                Thread.Sleep(AppConstants.ClipboardPollInterval);
                if (Win32Api.GetClipboardSequenceNumber() != seqBefore)
                    break;
            }

            try { selectedText = Clipboard.GetText(); } catch { }

            var seqFinal = Win32Api.GetClipboardSequenceNumber();
            if (seqFinal == seqBefore)
            {
                selectedText = "";
                Win32Api.SetForegroundWindow(foregroundWnd);
                return;
            }

            if (selectedText == savedText)
            {
                selectedText = "";
                RestoreClipboard(savedText);
                Win32Api.SetForegroundWindow(foregroundWnd);
                return;
            }

            RestoreClipboard(savedText);
            Win32Api.SetForegroundWindow(foregroundWnd);
        });

        clipboardOp.SetApartmentState(ApartmentState.STA);
        clipboardOp.Start();
        clipboardOp.Join(8000);

        return string.IsNullOrEmpty(selectedText) ? null : selectedText;
    }

    private void RestoreClipboard(string text)
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                    Clipboard.Clear();
                else
                    Clipboard.SetText(text);
                return;
            }
            catch { Thread.Sleep(AppConstants.ClipboardRestoreRetryDelayMs); }
        }
    }

    private void SimulateKeyDown(byte vk) => Win32Api.keybd_event(vk, 0, 0, 0);
    private void SimulateKeyUp(byte vk) => Win32Api.keybd_event(vk, 0, Win32Api.KEYEVENTF_KEYUP, 0);
}
