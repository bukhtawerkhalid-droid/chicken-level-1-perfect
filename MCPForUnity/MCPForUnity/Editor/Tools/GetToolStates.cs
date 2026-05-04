using System.Collections.Generic;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Services;
using Newtonsoft.Json.Linq;

namespace MCPForUnity.Editor.Tools
{
    /// <summary>
    /// Returns the enabled/disabled state of all registered MCP tools.
    /// This command is used by the MCP server to sync tool visibility.
    /// </summary>
    [McpForUnityTool("get_tool_states")]
    public static class GetToolStates
    {
        public static object HandleCommand(JObject @params)
        {
            var allTools = MCPServiceLocator.ToolDiscovery.DiscoverAllTools();

            var states = new Dictionary<string, bool>(allTools.Count);
            foreach (var tool in allTools)
            {
                states[tool.Name] = MCPServiceLocator.ToolDiscovery.IsToolEnabled(tool.Name);
            }

            return new SuccessResponse("Tool states retrieved.", states);
        }
    }
}
