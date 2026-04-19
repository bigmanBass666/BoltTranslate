using System.Runtime.InteropServices;

namespace TranslateSharp.Services.NativeInterop;

internal static class UiaNative
{
    public static readonly Guid CUIAutomation = new("ff48dba4-60ef-4201-aa87-54103eef594e");
    public static readonly Guid IUIAutomationIID = new("30cbe57d-d9d0-452a-ab13-7ac5ac482598");
    public static readonly Guid IUIAutomationElementIID = new("d22108a7-8ac5-49a5-837b-37bbb3d7591e");
    public static readonly Guid IUIAutomationTextPatternIID = new("32eba289-3583-42c9-9c59-3b6ab9b79904");
    public static readonly Guid IUIAutomationTextRangeIID = new("a543cc6a-f4a7-4d28-81d7-d7b4d1537a0d");

    public const int UIA_TextPatternId = 10024;
    public const int UIA_TextPattern2Id = 10035;

    [DllImport("ole32.dll")]
    public static extern int CoInitialize(IntPtr pvReserved);

    [DllImport("ole32.dll")]
    public static extern void CoUninitialize();

    [DllImport("ole32.dll")]
    public static extern int CoCreateInstance(
        ref Guid rclsid,
        IntPtr pUnkOuter,
        uint dwClsContext,
        ref Guid riid,
        out IntPtr ppv);

    public const uint CLSCTX_ALL = 23;

    [ComImport, Guid("30cbe57d-d9d0-452a-ab13-74103eef594e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IUIAutomation
    {
        int CompareElements(IntPtr el1, IntPtr el2, out int areSame);
        int CompareRuntimeIds(int[] runtimeId1, int[] runtimeId2, out int areSame);
        int GetRootElement(out IntPtr root);
        int ElementFromHandle(IntPtr hwnd, out IntPtr element);
        int ElementFromPoint(tagPOINT pt, out IntPtr element);
        int GetFocusedElement(out IntPtr element);
        int GetRootElementBuildCache(IntPtr cacheRequest, out IntPtr root);
        int ElementFromHandleBuildCache(IntPtr hwnd, IntPtr cacheRequest, out IntPtr element);
        int ElementFromPointBuildCache(tagPOINT pt, IntPtr cacheRequest, out IntPtr element);
        int GetFocusedElementBuildCache(IntPtr cacheRequest, out IntPtr element);
        int CreateTreeWalker(IntPtr condition, out IntPtr walker);
        int ControlViewWalker(out IntPtr walker);
        int ContentViewWalker(out IntPtr walker);
        int RawViewWalker(out IntPtr walker);
        int RawViewCondition(out IntPtr condition);
        int ControlViewCondition(out IntPtr condition);
        int ContentViewCondition(out IntPtr condition);
        int CreateCacheRequest(out IntPtr cacheRequest);
        int CreateTrueCondition(out IntPtr condition);
        int CreateFalseCondition(out IntPtr condition);
        int CreatePropertyCondition(int propertyId, IntPtr value, out IntPtr condition);
        int CreatePropertyConditionEx(int propertyId, IntPtr value, int flags, out IntPtr condition);
        int CreateAndCondition(IntPtr cond1, IntPtr cond2, out IntPtr condition);
        int CreateAndConditionFromArray(IntPtr[] conditions, out IntPtr condition);
        int CreateAndConditionFromNativeArray(IntPtr conditions, int conditionCount, out IntPtr condition);
        int CreateOrCondition(IntPtr cond1, IntPtr cond2, out IntPtr condition);
        int CreateOrConditionFromArray(IntPtr[] conditions, out IntPtr condition);
        int CreateOrConditionFromNativeArray(IntPtr conditions, int conditionCount, out IntPtr condition);
        int CreateNotCondition(IntPtr condition, out IntPtr notCondition);
        int AddAutomationEventHandler(int eventId, IntPtr element, int scope, IntPtr cacheRequest, IntPtr handler);
        int RemoveAutomationEventHandler(int eventId, IntPtr element, IntPtr handler);
        int AddPropertyChangedEventHandler(IntPtr element, int scope, IntPtr cacheRequest, IntPtr handler, int[] properties);
        int RemovePropertyChangedEventHandler(IntPtr element, IntPtr handler);
        int AddStructureChangedEventHandler(IntPtr element, int scope, IntPtr cacheRequest, IntPtr handler);
        int RemoveStructureChangedEventHandler(IntPtr element, IntPtr handler);
        int AddFocusChangedEventHandler(IntPtr cacheRequest, IntPtr handler);
        int RemoveFocusChangedEventHandler(IntPtr handler);
        int RemoveAllEventHandlers();
        int IntNativeArrayToSafeArray(int[] array, out IntPtr safeArray);
        int IntSafeArrayToNativeArray(IntPtr safeArray, out int[] array, out int arrayCount);
        int RectToVariant(tagRECT rc, out IntPtr var);
        int VariantToRect(IntPtr var, out tagRECT rc);
        int SafeArrayToRectNativeArray(IntPtr safeArray, out tagRECT[] rects, out int rectCount);
        int CreateProxyFactoryEntry(IntPtr factory, out IntPtr factoryEntry);
        int ProxyFactoryMapping(out IntPtr mapping);
        int GetPropertyProgrammaticName(int property, out string name);
        int GetPatternProgrammaticName(int pattern, out string name);
        int PollForPotentialSupportedPatterns(IntPtr element, out int[] patternIds, out string[] patternNames);
        int PollForPotentialSupportedProperties(IntPtr element, out int[] propertyIds, out string[] propertyNames);
        int CheckNotSupported(IntPtr value, out int isNotSupported);
        int ReservedNotSupportedValue(out IntPtr notSupportedValue);
        int ReservedMixedAttributeValue(out IntPtr mixedAttributeValue);
        int ElementFromIAccessible(IntPtr accessible, int childId, out IntPtr element);
        int ElementFromIAccessibleBuildCache(IntPtr accessible, int childId, IntPtr cacheRequest, out IntPtr element);
    }

    [ComImport, Guid("d22108a7-8ac5-49a5-837b-37bbb3d7591e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IUIAutomationElement
    {
        int SetFocus();
        int GetRuntimeId(out int[] runtimeId);
        int FindFirst(int scope, IntPtr condition, out IntPtr found);
        int FindAll(int scope, IntPtr condition, out IntPtr found);
        int FindFirstBuildCache(int scope, IntPtr condition, IntPtr cacheRequest, out IntPtr found);
        int FindAllBuildCache(int scope, IntPtr condition, IntPtr cacheRequest, out IntPtr found);
        int BuildUpdatedCache(IntPtr cacheRequest, out IntPtr updatedElement);
        int GetCurrentPropertyValue(int propertyId, out IntPtr retVal);
        int GetCurrentPropertyValueEx(int propertyId, int ignoreDefaultValue, out IntPtr retVal);
        int GetCachedPropertyValue(int propertyId, out IntPtr retVal);
        int GetCachedPropertyValueEx(int propertyId, int ignoreDefaultValue, out IntPtr retVal);
        int GetCurrentPatternAs(int patternId, ref Guid riid, out IntPtr patternObject);
        int GetCachedPatternAs(int patternId, ref Guid riid, out IntPtr patternObject);
        int GetCurrentPattern(int patternId, out IntPtr patternObject);
        int GetCachedPattern(int patternId, out IntPtr patternObject);
        int GetCachedParent(out IntPtr parent);
        int GetCachedChildren(out IntPtr children);
        int CurrentProcessId(out int pid);
        int CurrentControlType(out int controlType);
        int CurrentLocalizedControlType(out string localizedControlType);
        int CurrentName(out string name);
        int CurrentAcceleratorKey(out string acceleratorKey);
        int CurrentAccessKey(out string accessKey);
        int CurrentHasKeyboardFocus(out int hasKeyboardFocus);
        int CurrentIsKeyboardFocusable(out int isKeyboardFocusable);
        int CurrentIsEnabled(out int isEnabled);
        int CurrentAutomationId(out string automationId);
        int CurrentClassName(out string className);
        int CurrentHelpText(out string helpText);
        int CurrentCulture(out int culture);
        int CurrentIsControlElement(out int isControlElement);
        int CurrentIsContentElement(out int isContentElement);
        int CurrentIsPassword(out int isPassword);
        int CurrentNativeWindowHandle(out IntPtr nativeWindowHandle);
        int CurrentItemType(out string itemType);
        int CurrentIsOffscreen(out int isOffscreen);
        int CurrentOrientation(out int orientation);
        int CurrentFrameworkId(out string frameworkId);
        int CurrentIsRequiredForForm(out int isRequiredForForm);
        int CurrentItemStatus(out string itemStatus);
        int CurrentBoundingRectangle(out tagRECT boundingRectangle);
        int CurrentLabeledBy(out IntPtr labeledBy);
        int CurrentAriaRole(out string ariaRole);
        int CurrentAriaProperties(out string ariaProperties);
        int CurrentIsDataValidForForm(out int isDataValidForForm);
        int CurrentControllerFor(out IntPtr controllerFor);
        int CurrentDescribedBy(out IntPtr describedBy);
        int CurrentFlowsTo(out IntPtr flowsTo);
        int CurrentProviderDescription(out string providerDescription);
    }

    [ComImport, Guid("32eba289-3583-42c9-9c59-3b6ab9b79904"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IUIAutomationTextPattern
    {
        int RangeFromPoint(tagPOINT pt, out IntPtr range);
        int RangeFromChild(IntPtr child, out IntPtr range);
        int GetSelection(out IntPtr[] ranges);
        int GetVisibleRanges(out IntPtr[] ranges);
        int DocumentRange(out IntPtr range);
        int SupportedTextSelection(out int supportedTextSelection);
    }

    [ComImport, Guid("a543cc6a-f4a7-4d28-81d7-d7b4d1537a0d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IUIAutomationTextRange
    {
        int Clone(out IntPtr clonedRange);
        int Compare(IntPtr range, out int areSame);
        int CompareEndpoints(int srcEndPoint, IntPtr targetRange, int targetEndPoint, out int compValue);
        int ExpandToEnclosingUnit(int unit);
        int FindAttribute(int attr, IntPtr val, int backward, out IntPtr found);
        int FindText(string text, int backward, int ignoreCase, out IntPtr found);
        int GetAttributeValue(int attr, out IntPtr value);
        int GetBoundingRectangles(out double[] boundingRects);
        int GetEnclosingElement(out IntPtr element);
        int GetText(int maxLength, out string text);
        int Move(int unit, int count, out int moved);
        int MoveEndpointByUnit(int endpoint, int unit, int count, out int moved);
        int MoveEndpointByRange(int srcEndPoint, IntPtr targetRange, int targetEndPoint);
        int Select();
        int AddToSelection();
        int RemoveFromSelection();
        int ScrollIntoView(int alignToTop);
        int GetChildren(out IntPtr children);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tagPOINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tagRECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
