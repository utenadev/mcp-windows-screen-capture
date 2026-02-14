using System.Runtime.InteropServices;
using System.Text;

namespace WindowsDesktopUse.App;

/// <summary>
/// UI Automation service for extracting structured text from windows
/// </summary>
public sealed class AccessibilityService
{
    private const int MaxDepth = 10;

    /// <summary>
    /// Extract text from a window in Markdown format
    /// </summary>
    public string ExtractWindowText(IntPtr hwnd, bool includeButtons = false)
    {
        try
        {
            Console.Error.WriteLine($"[Accessibility] Extracting text from window: {hwnd}");

            var element = GetElementFromHandle(hwnd);
            if (element == null)
            {
                Console.Error.WriteLine("[Accessibility] Failed to get element from handle");
                return "";
            }

            var sb = new StringBuilder();
            ExtractTextRecursive(element, sb, 0, includeButtons);

            var result = sb.ToString().Trim();
            Console.Error.WriteLine($"[Accessibility] Extracted {result.Length} characters");
            return result;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Accessibility] Error extracting text: {ex.Message}");
            return "";
        }
    }

    private void ExtractTextRecursive(object element, StringBuilder sb, int depth, bool includeButtons)
    {
        if (depth > MaxDepth || element == null)
            return;

        try
        {
            var info = GetElementInfo(element);
            if (info == null)
                return;

            // Check control type and format accordingly
            var controlType = info.ControlType;
            var name = info.Name ?? "";

            switch (controlType)
            {
                case "TitleBar":
                case "Header":
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var prefix = new string('#', Math.Min(depth + 1, 6));
                        sb.AppendLine($"{prefix} {name}");
                    }
                    break;

                case "ListItem":
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        sb.AppendLine($"- {name}");
                    }
                    break;

                case "Text":
                case "Edit":
                case "Document":
                    var text = GetTextValue(element);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.AppendLine(text);
                    }
                    break;

                case "Button":
                    if (includeButtons && !string.IsNullOrWhiteSpace(name))
                    {
                        sb.AppendLine($"[ Button: {name} ]");
                    }
                    break;

                default:
                    // For other controls, just add name if meaningful
                    if (!string.IsNullOrWhiteSpace(name) && name.Length > 1)
                    {
                        // Check if it's likely a URL bar
                        if (IsUrlBar(element, name))
                        {
                            sb.AppendLine($"[ URL: {name} ]");
                        }
                    }
                    break;
            }

            // Traverse children
            var children = GetChildren(element);
            foreach (var child in children)
            {
                ExtractTextRecursive(child, sb, depth + 1, includeButtons);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Accessibility] Error at depth {depth}: {ex.Message}");
        }
    }

    private bool IsUrlBar(object element, string name)
    {
        try
        {
            var lowerName = name.ToLowerInvariant();
            return lowerName.Contains("address") || 
                   lowerName.Contains("search bar") ||
                   lowerName.Contains("url") ||
                   (name.StartsWith("http", StringComparison.OrdinalIgnoreCase) && name.Contains("."));
        }
        catch
        {
            return false;
        }
    }

    #region COM Interop for UI Automation

    [ComImport, Guid("30cbe57d-d9d0-452a-ab13-7ac5ac4825ee")]
    private class CUIAutomation
    {
    }

    [Guid("9f6e2884-5c67-4dbb-9780-1b08078dc10a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IUIAutomation
    {
        object ElementFromHandle(IntPtr hwnd);
    }

    [Guid("d2210847-6547-4b84-93d7-d5b7f2e505f4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IUIAutomationElement
    {
        int CurrentControlType { get; }
        string CurrentName { get; }
        string CurrentClassName { get; }
        object GetCachedChildren();
        object GetCachedParent();
        object GetCurrentPattern(int patternId);
    }

    private static object? GetElementFromHandle(IntPtr hwnd)
    {
        try
        {
            var automation = new CUIAutomation() as IUIAutomation;
            return automation?.ElementFromHandle(hwnd);
        }
        catch
        {
            return null;
        }
    }

    private record ElementInfo(string? ControlType, string? Name, string? ClassName);

    private static ElementInfo? GetElementInfo(object element)
    {
        try
        {
            var elem = element as IUIAutomationElement;
            if (elem == null)
                return null;

            var controlTypeId = elem.CurrentControlType;
            var name = elem.CurrentName ?? "";
            var className = elem.CurrentClassName ?? "";

            var controlType = GetControlTypeName(controlTypeId);

            return new ElementInfo(controlType, name, className);
        }
        catch
        {
            return null;
        }
    }

    private static string? GetControlTypeName(int typeId)
    {
        return typeId switch
        {
            50000 => "Button",
            50001 => "Calendar",
            50002 => "CheckBox",
            50003 => "ComboBox",
            50004 => "Edit",
            50005 => "Hyperlink",
            50006 => "Image",
            50007 => "ListItem",
            50008 => "List",
            50009 => "Menu",
            50010 => "MenuBar",
            50011 => "MenuItem",
            50012 => "ProgressBar",
            50013 => "RadioButton",
            50014 => "ScrollBar",
            50015 => "Slider",
            50016 => "Spinner",
            50017 => "StatusBar",
            50018 => "Tab",
            50019 => "TabItem",
            50020 => "Text",
            50021 => "ToolBar",
            50022 => "ToolTip",
            50023 => "Tree",
            50024 => "TreeItem",
            50025 => "Custom",
            50026 => "Group",
            50027 => "Thumb",
            50028 => "DataGrid",
            50029 => "DataItem",
            50030 => "Document",
            50031 => "SplitButton",
            50032 => "Window",
            50033 => "Pane",
            50034 => "Header",
            50035 => "HeaderItem",
            50036 => "Table",
            50037 => "TitleBar",
            50038 => "Separator",
            50039 => "SemanticZoom",
            50040 => "AppBar",
            _ => $"Type{typeId}"
        };
    }

    private static string GetTextValue(object element)
    {
        try
        {
            var elem = element as IUIAutomationElement;
            if (elem == null)
                return "";

            // Try to get ValuePattern (for edit controls)
            // Pattern ID for ValuePattern is 10002
            try
            {
                var pattern = elem.GetCurrentPattern(10002);
                if (pattern != null)
                {
                    // Try to get Value property
                    // For simplicity, just return the name
                    return elem.CurrentName ?? "";
                }
            }
            catch { }

            // Fallback to name
            return elem.CurrentName ?? "";
        }
        catch
        {
            return "";
        }
    }

    private static List<object> GetChildren(object element)
    {
        var children = new List<object>();
        try
        {
            // Use TreeWalker to get children
            // For simplicity, we'll use a basic approach
            // In a full implementation, we'd use IUIAutomationTreeWalker

            // For now, return empty list as COM interop for children is complex
            // A proper implementation would need more interop definitions
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Accessibility] Error getting children: {ex.Message}");
        }
        return children;
    }

    #endregion
}
