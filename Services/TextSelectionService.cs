using System.Runtime.InteropServices;
using TranslateSharp.Services.NativeInterop;

namespace TranslateSharp.Services;

public interface ITextSelectionService
{
    string? GetSelectedText();
}

public class TextSelectionService : ITextSelectionService
{
    private readonly IClipboardService _clipboardService;

    public TextSelectionService(IClipboardService clipboardService)
    {
        _clipboardService = clipboardService;
    }

    public string? GetSelectedText()
    {
        var uiaText = TryGetSelectedTextViaUIA();
        if (!string.IsNullOrEmpty(uiaText))
            return uiaText;

        return _clipboardService.GetSelectedTextViaClipboard();
    }

    private static string? TryGetSelectedTextViaUIA()
    {
        try
        {
            var hr = UiaNative.CoInitialize(IntPtr.Zero);
            bool needsUninitialize = hr == 0;

            try
            {
                IntPtr pAutomation = IntPtr.Zero;
                var clsid = UiaNative.CUIAutomation;
                var iid = UiaNative.IUIAutomationIID;
                hr = UiaNative.CoCreateInstance(ref clsid, IntPtr.Zero, UiaNative.CLSCTX_ALL, ref iid, out pAutomation);
                if (hr != 0 || pAutomation == IntPtr.Zero) return null;

                var automation = Marshal.GetObjectForIUnknown(pAutomation) as UiaNative.IUIAutomation;
                if (automation == null) { Marshal.Release(pAutomation); return null; }

                IntPtr pElement = IntPtr.Zero;
                hr = automation.GetFocusedElement(out pElement);
                if (hr != 0 || pElement == IntPtr.Zero) { Marshal.Release(pAutomation); return null; }

                var element = Marshal.GetObjectForIUnknown(pElement) as UiaNative.IUIAutomationElement;
                if (element == null) { Marshal.Release(pElement); Marshal.Release(pAutomation); return null; }

                IntPtr pPattern = IntPtr.Zero;
                var textPatternGuid = UiaNative.IUIAutomationTextPatternIID;
                hr = element.GetCurrentPatternAs(UiaNative.UIA_TextPatternId, ref textPatternGuid, out pPattern);
                if (hr != 0 || pPattern == IntPtr.Zero) { Marshal.Release(pElement); Marshal.Release(pAutomation); return null; }

                var textPattern = Marshal.GetObjectForIUnknown(pPattern) as UiaNative.IUIAutomationTextPattern;
                if (textPattern == null) { Marshal.Release(pPattern); Marshal.Release(pElement); Marshal.Release(pAutomation); return null; }

                IntPtr[]? ranges = null;
                hr = textPattern.GetSelection(out ranges);
                if (hr != 0 || ranges == null || ranges.Length == 0) { Marshal.Release(pPattern); Marshal.Release(pElement); Marshal.Release(pAutomation); return null; }

                var result = new System.Text.StringBuilder();

                foreach (var pRange in ranges)
                {
                    if (pRange == IntPtr.Zero) continue;
                    var range = Marshal.GetObjectForIUnknown(pRange) as UiaNative.IUIAutomationTextRange;
                    if (range == null) { Marshal.Release(pRange); continue; }

                    string text = "";
                    hr = range.GetText(-1, out text);
                    if (hr == 0 && !string.IsNullOrEmpty(text))
                        result.Append(text);

                    Marshal.Release(pRange);
                }

                Marshal.Release(pPattern);
                Marshal.Release(pElement);
                Marshal.Release(pAutomation);

                var finalText = result.ToString().Trim();
                return string.IsNullOrEmpty(finalText) ? null : finalText;
            }
            finally
            {
                if (needsUninitialize)
                    UiaNative.CoUninitialize();
            }
        }
        catch
        {
            return null;
        }
    }
}
