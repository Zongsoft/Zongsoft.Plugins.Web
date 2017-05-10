/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2017 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Collections.Generic;
using System.Web.Routing;
using System.Web.Http;

namespace Zongsoft.Web.Routing
{
	public class PluginRouteProvider : IRouteProvider
	{
		#region 常量定义
		private const string ROUTING_PATH = "/Workbench/Web/Routes";
		#endregion

		#region 私有变量
		private string _path;
		private Zongsoft.Plugins.PluginContext _context;
		#endregion

		#region 构造函数
		public PluginRouteProvider(Zongsoft.Plugins.PluginContext context)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			_context = context;
			_path = ROUTING_PATH;
		}
		#endregion

		#region 公共属性
		public string Path
		{
			get
			{
				return _path;
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				_path = value;
			}
		}
		#endregion

		#region 公共方法
		public IEnumerable<IRoute> GetRoutes()
		{
			var node = _context.PluginTree.Find(_path);

			if(node == null)
				yield break;

			foreach(var child in node.Children)
			{
				if(child.NodeType == Zongsoft.Plugins.PluginTreeNodeType.Empty)
					continue;

				var route = child.UnwrapValue<RouteToken>(Zongsoft.Plugins.ObtainMode.Auto);

				if(route != null)
				{
					route.SetRouteFactory(this.ToRoute);
					yield return route;
				}
			}
		}

		public object ToRoute(IRoute route)
		{
			if(route == null)
				throw new ArgumentNullException(nameof(route));

			var token = route as RouteToken;

			if(token == null)
				return null;

			if(string.IsNullOrWhiteSpace(token.Kind))
			{
				if(token.Url != null && token.Url.StartsWith("api", StringComparison.OrdinalIgnoreCase))
					token.Kind = "api";
			}

			switch(token.Kind.ToLowerInvariant())
			{
				case "":
				case "web":
				case null:
					if(token.Handler == null)
					{
						if(token.Ignored)
							token.Handler = new System.Web.Routing.StopRoutingHandler();
						else
							token.Handler = new System.Web.Mvc.MvcRouteHandler();
					}

					return new Route(
						token.Url,
						new RouteValueDictionary(token.Defaults),
						new RouteValueDictionary(token.Constraints),
						new RouteValueDictionary(token.States), (IRouteHandler)token.Handler);
				case "api":
				case "http":
					if(token.Handler == null && token.Ignored)
						token.Handler = new System.Web.Http.Routing.StopRoutingHandler();

					return GlobalConfiguration.Configuration.Routes.CreateRoute(token.Url, token.Defaults, token.Constraints, token.States, (System.Net.Http.HttpMessageHandler)token.Handler);
				default:
					throw new InvalidOperationException($"Not supported '{token.Kind}' route type.");
			}
		}
		#endregion
	}
}
