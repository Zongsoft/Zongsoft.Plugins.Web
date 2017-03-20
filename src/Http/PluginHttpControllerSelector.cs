/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2016 Zongsoft Corporation <http://www.zongsoft.com>
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
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
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
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;
using System.Web.Http.Dispatcher;
using System.Web.Http.Controllers;

namespace Zongsoft.Web.Http
{
	public class PluginHttpControllerSelector : IHttpControllerSelector
	{
		#region 常量定义
		private const string ROOT_CONTROLLERS_PATH = "/Workspace/Web/Controllers";
		#endregion

		#region 成员字段
		private Zongsoft.Plugins.PluginContext _pluginContext;
		#endregion

		#region 构造函数
		public PluginHttpControllerSelector(Zongsoft.Plugins.PluginContext pluginContext)
		{
			if(pluginContext == null)
				throw new ArgumentNullException("pluginContext");

			_pluginContext = pluginContext;
		}
		#endregion

		#region 公共方法
		public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
		{
			throw new NotImplementedException();
		}

		public HttpControllerDescriptor SelectController(HttpRequestMessage request)
		{
			var routeData = (IHttpRouteData)request.Properties[System.Web.Http.Hosting.HttpPropertyKeys.HttpRouteDataKey];

			if(routeData == null)
				return null;

			object item;

			//处理路由中action可能与args错位的问题。
			if(routeData.Values.TryGetValue("action", out item) && item != null && item is string)
			{
				foreach(var chr in (string)item)
				{
					//如果action包含非数字、字母及下划线字符则认为其与args错位了
					if(!Char.IsLetterOrDigit(chr) && chr != '_')
					{
						routeData.Values["args"] = item;
						routeData.Values["action"] = null;
						break;
					}
				}
			}

			string areaName, controllerPath;

			if(routeData.Route.DataTokens.TryGetValue("area", out item))
				areaName = (string)item;
			else
				areaName = VirtualPathHelper.GetArea(routeData.Route.RouteTemplate);

			if(string.IsNullOrWhiteSpace(areaName))
				controllerPath = Zongsoft.Plugins.PluginPath.Combine(ROOT_CONTROLLERS_PATH, (string)routeData.Values["controller"]);
			else
				controllerPath = Zongsoft.Plugins.PluginPath.Combine(ROOT_CONTROLLERS_PATH, areaName, (string)routeData.Values["controller"]);

			var node = _pluginContext.PluginTree.Find(controllerPath);

			if(node == null)
				return null;

			routeData.Values["area"] = areaName;
			routeData.Values["controller.path"] = controllerPath;

			var descriptor = new PluginHttpControllerDescriptor(request.GetConfiguration(), node);

			descriptor.Properties["route.area"] = areaName;
			descriptor.Properties["http.method"] = request.Method.Method;

			return descriptor;
		}
		#endregion
	}
}
