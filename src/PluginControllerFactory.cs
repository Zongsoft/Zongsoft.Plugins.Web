/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2015 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Plugins.Web.
 *
 * Zongsoft.Plugins.Web is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Plugins.Web is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Plugins.Web; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Zongsoft.Plugins;

namespace Zongsoft.Web
{
	public class PluginControllerFactory : IControllerFactory
	{
		#region 常量定义
		private const string ROOT_CONTROLLERS_PATH = "/Workspace/Web/Controllers";
		#endregion

		#region 私有变量
		private Zongsoft.Plugins.PluginContext _pluginContext;
		#endregion

		#region 构造函数
		public PluginControllerFactory(Zongsoft.Plugins.PluginContext pluginContext)
		{
			if(pluginContext == null)
				throw new ArgumentNullException("pluginContext");

			_pluginContext = pluginContext;
		}
		#endregion

		#region 接口成员
		public IController CreateController(RequestContext requestContext, string controllerName)
		{
			var node = this.GetControllerNode(requestContext, controllerName);

			if(node == null || node.NodeType == PluginTreeNodeType.Empty)
				throw new HttpException(404, "Not found." + Environment.NewLine + requestContext.HttpContext.Request.RawUrl);

			var value = node.UnwrapValue(ObtainMode.Alway, this);

			if(value != null && typeof(Delegate).IsAssignableFrom(value.GetType()))
				value = ((Delegate)value).DynamicInvoke();

			return value as IController;
		}

		public void ReleaseController(IController controller)
		{
			IDisposable disposable = controller as IDisposable;

			if(disposable != null)
				disposable.Dispose();
		}

		System.Web.SessionState.SessionStateBehavior IControllerFactory.GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
		{
			var node = this.GetControllerNode(requestContext, controllerName);

			if(node == null || node.NodeType == PluginTreeNodeType.Empty)
				return System.Web.SessionState.SessionStateBehavior.Default;

			Type controllerType = node.ValueType;

			if(controllerType == null)
				return System.Web.SessionState.SessionStateBehavior.Default;

			var attribute = controllerType.GetCustomAttributes(typeof(SessionStateAttribute), inherit: true)
									.OfType<SessionStateAttribute>()
									.FirstOrDefault();

			return (attribute != null) ? attribute.Behavior : System.Web.SessionState.SessionStateBehavior.Default;
		}
		#endregion

		#region 私有方法
		private PluginTreeNode GetControllerNode(RequestContext requestContext, string controllerName)
		{
			Route route = requestContext.RouteData.Route as Route;

			if(route == null)
				return null;

			var controllerPath = string.Empty;
			var area = VirtualPathHelper.GetArea(requestContext.RouteData);

			if(string.IsNullOrWhiteSpace(area))
				controllerPath = PluginPath.Combine(ROOT_CONTROLLERS_PATH, controllerName);
			else
				controllerPath = PluginPath.Combine(ROOT_CONTROLLERS_PATH, area, controllerName);

			requestContext.RouteData.DataTokens["area"] = area;
			requestContext.RouteData.DataTokens["controller.path"] = controllerPath;

			return _pluginContext.PluginTree.Find(controllerPath);
		}
		#endregion
	}
}
