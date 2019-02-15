/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2013 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.Http.Controllers;

using Zongsoft.Plugins;

namespace Zongsoft.Web.Http
{
	public class PluginHttpControllerDescriptor : System.Web.Http.Controllers.HttpControllerDescriptor
	{
		#region 成员字段
		private PluginTreeNode _controllerNode;
		#endregion

		#region 构造函数
		public PluginHttpControllerDescriptor(HttpConfiguration configuration, PluginTreeNode controllerNode, string actionName, IHttpRouteData route)
		{
			_controllerNode = controllerNode ?? throw new ArgumentNullException(nameof(controllerNode));

			this.Configuration = configuration;
			this.ControllerName = controllerNode.Name;
			this.ControllerType = controllerNode.ValueType;

			if(actionName != null && actionName.Length > 0 && controllerNode.Children.Count > 0)
			{
				foreach(var child in controllerNode.Children)
				{
					if(string.Equals(child.Name, actionName, StringComparison.OrdinalIgnoreCase))
					{
						route.Values["controller"] = controllerNode.Name + "." + child.Name;
						route.Values["controller.path"] = child.FullPath;

						ExchangeAction(route.Values);

						_controllerNode = child;
						this.ControllerName = child.Name;
						this.ControllerType = child.ValueType;

						break;
					}
				}
			}
		}
		#endregion

		#region 公共属性
		public PluginTreeNode ControllerNode
		{
			get
			{
				return _controllerNode;
			}
		}
		#endregion

		#region 重写方法
		public override IHttpController CreateController(System.Net.Http.HttpRequestMessage request)
		{
			return _controllerNode.UnwrapValue(ObtainMode.Alway) as IHttpController;
		}
		#endregion

		#region 私有方法
		private static void ExchangeAction(IDictionary<string, object> routes)
		{
			if(routes.TryGetValue("id", out var value) && IsIdentifier(value as string))
			{
				routes["action"] = value;
				routes["id"] = null;
			}
			else
			{
				routes["action"] = null;
			}
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private static bool IsIdentifier(string text)
		{
			//如果文本为空或者首字符不是下划线也不是字母，则说明该文本为无效的标识符
			if(string.IsNullOrEmpty(text) || (text[0] != '_' && !char.IsLetter(text[0])))
				return false;

			for(int i = 1; i < text.Length; i++)
			{
				if(!char.IsLetterOrDigit(text[i]) && text[i] != '_')
					return false;
			}

			return true;
		}
		#endregion
	}
}
